using API.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    [Route("api/test-auth")]
    [ApiController]
    public class TestAuthController : ControllerBase
    {
        [HttpGet("check")]
        [Authorize]
        public IActionResult CheckAuth()
        {
            var claims = User.Claims.Select(c => new
            {
                Type = c.Type,
                Value = c.Value
            });

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var name = User.FindFirst(ClaimTypes.Name)?.Value;

            return Ok(new
            {
                type = ResponseMessages.Success,
                message = "Authentication check successful",
                data = new
                {
                    isAuthenticated = User.Identity?.IsAuthenticated,
                    userName = User.Identity?.Name,
                    userId = userId,
                    email = email,
                    name = name,
                    claims = claims,
                    authType = User.Identity?.AuthenticationType
                }
            });
        }

        [HttpGet("public")]
        public IActionResult PublicEndpoint()
        {
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();
            var hasAuthHeader = !string.IsNullOrEmpty(authHeader);

            string authHeaderPreview = null;
            if (hasAuthHeader)
            {
                authHeaderPreview = authHeader.Length > 50
                    ? authHeader.Substring(0, 50) + "..."
                    : authHeader;
            }

            return Ok(new
            {
                type = ResponseMessages.Success,
                message = "Public endpoint - no authentication required",
                data = new
                {
                    hasAuthHeader = hasAuthHeader,
                    authHeaderPreview = authHeaderPreview,
                    timestamp = DateTime.UtcNow,
                    corsHeaders = new
                    {
                        origin = Request.Headers["Origin"].FirstOrDefault(),
                        method = Request.Method
                    }
                }
            });
        }

        [HttpGet("token-info")]
        [Authorize]
        public IActionResult GetTokenInfo()
        {
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();

            if (string.IsNullOrEmpty(authHeader))
            {
                return BadRequest(new
                {
                    type = ResponseMessages.Failed,
                    message = "No authorization header found"
                });
            }

            var token = authHeader.Replace("Bearer ", "");
            var tokenPreview = token.Length > 50 ? token.Substring(0, 50) + "..." : token;

            var claims = User.Claims.ToDictionary(c => c.Type, c => c.Value);

            return Ok(new
            {
                type = ResponseMessages.Success,
                message = "Token information retrieved",
                data = new
                {
                    tokenPreview = tokenPreview,
                    tokenLength = token.Length,
                    claims = claims,
                    issuedAt = claims.ContainsKey("iat") ? claims["iat"] : null,
                    expiresAt = claims.ContainsKey("exp") ? claims["exp"] : null,
                    issuer = claims.ContainsKey("iss") ? claims["iss"] : null,
                    audience = claims.ContainsKey("aud") ? claims["aud"] : null
                }
            });
        }

        [HttpOptions("check")]
        public IActionResult OptionsCheck()
        {
            return Ok();
        }

        [HttpOptions("token-info")]
        public IActionResult OptionsTokenInfo()
        {
            return Ok();
        }
    }
}
