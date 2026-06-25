using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace ScubaHub.Api.Controllers
{
    [ApiController]
    [Route("api/admin/auth")]
    [EnableCors("AdminFrontend")]
    public class AdminAuthController : ControllerBase
    {
        // POST /api/admin/auth/verify
        // Validates the admin password and sets an HttpOnly session cookie.
        [HttpPost("verify")]
        public IActionResult Verify([FromBody] VerifyRequest req, [FromServices] IConfiguration config)
        {
            var expectedKey = config["AdminSettings:Key"];

            if (string.IsNullOrWhiteSpace(expectedKey))
                return StatusCode(500, "AdminSettings:Key is not configured on the server.");

            if (req.Password != expectedKey)
                return Unauthorized(new { message = "Invalid password." });

            Response.Cookies.Append("admin_session", expectedKey, new CookieOptions
            {
                HttpOnly = true,
                // IsHttps is reliable here because UseForwardedHeaders in Program.cs
                // reads the X-Forwarded-Proto header set by Azure's load balancer.
                Secure   = HttpContext.Request.IsHttps,
                SameSite = SameSiteMode.Strict,
                Path     = "/api/admin",
                MaxAge   = TimeSpan.FromHours(8)
            });

            return Ok(new { message = "Authenticated." });
        }

        // POST /api/admin/auth/logout
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("admin_session", new CookieOptions { Path = "/api/admin" });
            return Ok(new { message = "Logged out." });
        }
    }

    public record VerifyRequest(string Password);
}
