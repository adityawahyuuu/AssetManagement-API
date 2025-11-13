using API.Configuration;
using API.Data;
using API.Middlewares;
using API.Repositories.Asset;
using API.Repositories.AssetCategory;
using API.Repositories.Register;
using API.Repositories.Room;
using API.Services.Email;
using API.Services.Jwt;
using API.Services.Otp;
using API.Services.PasswordHashing;
using API.Services.PasswordReset;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Load .env file BEFORE creating builder (Development only)
            // This ensures environment variables are available when configuration is built
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
            if (environment == "Development")
            {
                var envFilePath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
                if (File.Exists(envFilePath))
                {
                    DotNetEnv.Env.Load(envFilePath);
                    // No need to call AddEnvironmentVariables() - WebApplication.CreateBuilder does this automatically
                }
            }

            var builder = WebApplication.CreateBuilder(args);
            // Note: builder already includes environment variables in configuration by default

            // Configure options from appsettings
            builder.Services.Configure<DatabaseOptions>(
                builder.Configuration.GetSection(DatabaseOptions.SectionName));
            builder.Services.Configure<PasswordHashingOptions>(
                builder.Configuration.GetSection(PasswordHashingOptions.SectionName));
            builder.Services.Configure<ValidationOptions>(
                builder.Configuration.GetSection(ValidationOptions.SectionName));
            builder.Services.Configure<OtpOptions>(
                builder.Configuration.GetSection(OtpOptions.SectionName));
            builder.Services.Configure<EmailOptions>(
                builder.Configuration.GetSection(EmailOptions.SectionName));
            builder.Services.Configure<PasswordResetOptions>(
                builder.Configuration.GetSection(PasswordResetOptions.SectionName));
            builder.Services.Configure<JwtOptions>(
                builder.Configuration.GetSection(JwtOptions.SectionName));

            // Configure forwarded headers for deployment behind reverse proxy (Render, etc.)
            // Only accept forwarded headers in Production to avoid security issues in Development
            if (builder.Environment.IsProduction())
            {
                builder.Services.Configure<ForwardedHeadersOptions>(options =>
                {
                    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                    // Accept forwarded headers from any source (required for cloud platforms like Render)
                    // In production, we trust the cloud platform's proxy
                    options.KnownNetworks.Clear();
                    options.KnownProxies.Clear();
                });
            }

            // Add services to the container
            builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly, includeInternalTypes: true);

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(builder.Configuration.GetConnectionString("AssetManagementConnection"));
            });

            builder.Services.AddScoped<IRegisterRepository, RegisterRepository>();
            builder.Services.AddScoped<IRoomRepository, RoomRepository>();
            builder.Services.AddScoped<IAssetRepository, AssetRepository>();
            builder.Services.AddScoped<IAssetCategoryRepository, AssetCategoryRepository>();
            builder.Services.AddAutoMapper(typeof(Program).Assembly);
            builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
            builder.Services.AddScoped<IOtpService, OtpService>();
            builder.Services.AddScoped<IPasswordResetService, PasswordResetService>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<IJwtService, JwtService>();

            // Configure CORS to allow frontend
            var corsOrigins = builder.Configuration["Cors:AllowedOrigins"]
                ?? "http://localhost:3000,http://localhost:3001";
            var allowedOrigins = corsOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(origin => origin.Trim())
                .ToArray();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy.WithOrigins(allowedOrigins)
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                });
            });

            builder.Services.AddControllers();

            // Configure JWT Authentication
            var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>();
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret)),
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtOptions.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                // Add detailed logging for debugging JWT issues
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                        logger.LogError(context.Exception, "JWT Authentication failed: {Message}", context.Exception.Message);

                        if (context.Exception is SecurityTokenExpiredException)
                        {
                            logger.LogWarning("Token expired at {ExpiredAt}", ((SecurityTokenExpiredException)context.Exception).Expires);
                        }

                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                        var userId = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                        logger.LogInformation("JWT Token validated successfully for user {UserId}", userId);
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                        logger.LogWarning("JWT Challenge: Error={Error}, ErrorDescription={ErrorDescription}",
                            context.Error, context.ErrorDescription);
                        return Task.CompletedTask;
                    },
                    OnMessageReceived = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();

                        if (!string.IsNullOrEmpty(authHeader))
                        {
                            logger.LogInformation("Authorization header received: {HeaderPreview}",
                                authHeader.Substring(0, Math.Min(20, authHeader.Length)) + "...");

                            // Extract token - handle both "Bearer <token>" and "<token>" formats
                            if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                            {
                                // Standard format: "Bearer <token>"
                                context.Token = authHeader.Substring("Bearer ".Length).Trim();
                                logger.LogInformation("Token extracted with Bearer prefix, length: {TokenLength}", context.Token.Length);
                            }
                            else if (!authHeader.Contains(" "))
                            {
                                // Direct token without Bearer prefix (some clients send this way)
                                context.Token = authHeader.Trim();
                                logger.LogInformation("Token extracted without Bearer prefix, length: {TokenLength}", context.Token.Length);
                            }
                            else
                            {
                                logger.LogWarning("Authorization header format not recognized: {Header}",
                                    authHeader.Substring(0, Math.Min(30, authHeader.Length)));
                            }
                        }
                        else
                        {
                            logger.LogWarning("No Authorization header found in request");
                        }

                        return Task.CompletedTask;
                    }
                };
            });

            builder.Services.AddAuthorization();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "Asset Management API",
                    Version = "v1",
                    Description = "API for managing dormitory assets"
                });

                // Add JWT Authentication to Swagger
                c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Enter your token below (without 'Bearer' prefix).",
                    Name = "Authorization",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT"
                });

                c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
                {
                    {
                        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                        {
                            Reference = new Microsoft.OpenApi.Models.OpenApiReference
                            {
                                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            var app = builder.Build();

            app.UseMiddleware<GlobalExceptionHandler>();

            // Configure forwarded headers for HTTPS behind reverse proxy
            // Must be early in the pipeline, before UseHttpsRedirection
            // In Development: Uses default secure settings (ignores forwarded headers)
            // In Production: Accepts forwarded headers from Render's proxy
            if (app.Environment.IsProduction())
            {
                app.UseForwardedHeaders();
            }

            // Add Basic Authentication for Swagger UI
            app.UseMiddleware<SwaggerBasicAuthMiddleware>();

            // Configure the HTTP request pipeline
            // Enable Swagger in all environments for testing purposes
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Asset Management API v1");
                c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
                c.DocumentTitle = "Asset Management API - Swagger UI";
                c.DefaultModelsExpandDepth(-1); // Hide schemas section by default
                c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None); // Collapse all endpoints by default
                c.DisplayRequestDuration(); // Show request duration
            });

            app.UseHttpsRedirection();

            // CORS must be before Authentication and Authorization
            app.UseCors("AllowFrontend");

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
