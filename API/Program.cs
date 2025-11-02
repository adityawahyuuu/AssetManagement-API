using API.Configuration;
using API.Data;
using API.Middlewares;
using API.Repositories.Register;
using API.Repositories.Room;
using API.Services.Email;
using API.Services.Jwt;
using API.Services.Otp;
using API.Services.PasswordHashing;
using API.Services.PasswordReset;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

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

            // Add services to the container
            builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly, includeInternalTypes: true);

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(builder.Configuration.GetConnectionString("AssetManagementConnection"));
            });

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder
                        .AllowAnyOrigin()      // Allow any origin (0.0.0.0)
                        .AllowAnyMethod()      // Allow GET, POST, PUT, DELETE, etc.
                        .AllowAnyHeader();     // Allow any headers
                });

                // OR: More restrictive (recommended for production)
                options.AddPolicy("AllowLocal", builder =>
                {
                    builder
                        .WithOrigins(
                            "http://localhost:3000",
                            "http://localhost:5000",
                            "http://127.0.0.1:3000"
                        )
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();  // Allow cookies/auth
                });
            });

            builder.Services.AddScoped<IRegisterRepository, RegisterRepository>();
            builder.Services.AddScoped<IRoomRepository, RoomRepository>();
            builder.Services.AddAutoMapper(typeof(Program).Assembly);
            builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
            builder.Services.AddScoped<IOtpService, OtpService>();
            builder.Services.AddScoped<IPasswordResetService, PasswordResetService>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<IJwtService, JwtService>();
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
            });

            builder.Services.AddAuthorization();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            app.UseCors("AllowLocal");  // or "AllowAll"

            app.UseMiddleware<GlobalExceptionHandler>();

            // Configure the HTTP request pipeline
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
