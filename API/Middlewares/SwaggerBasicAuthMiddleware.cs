using System.Net;
using System.Text;

namespace API.Middlewares
{
    public class SwaggerBasicAuthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;

        public SwaggerBasicAuthMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Check if the request is for Swagger UI or Swagger JSON
            if (context.Request.Path.StartsWithSegments("/swagger") ||
                context.Request.Path.Value == "/")
            {
                // Get credentials from configuration (environment variables in production)
                var swaggerUsername = _configuration["Swagger:Username"] ?? "admin";
                var swaggerPassword = _configuration["Swagger:Password"] ?? "admin123";

                // Check if basic auth is enabled (can be disabled in development)
                var authEnabled = _configuration.GetValue<bool>("Swagger:AuthEnabled", true);

                if (authEnabled)
                {
                    string authHeader = context.Request.Headers["Authorization"];

                    if (authHeader != null && authHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
                    {
                        // Extract credentials
                        var encodedCredentials = authHeader.Substring("Basic ".Length).Trim();
                        var credentialBytes = Convert.FromBase64String(encodedCredentials);
                        var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':', 2);

                        var username = credentials[0];
                        var password = credentials[1];

                        // Validate credentials
                        if (username == swaggerUsername && password == swaggerPassword)
                        {
                            await _next.Invoke(context);
                            return;
                        }
                    }

                    // Return authentication required response
                    context.Response.Headers["WWW-Authenticate"] = "Basic realm=\"Swagger UI\"";
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    await context.Response.WriteAsync("Swagger UI access requires authentication.");
                    return;
                }
            }

            await _next.Invoke(context);
        }
    }
}
