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
                // The admin SPA (scuba-admin.azurewebsites.net) and this API are on
                // different sites — *.azurewebsites.net is on the Public Suffix List,
                // so each app is its own registrable domain. A cross-site cookie is
                // only stored and sent when it is SameSite=None AND Secure.
                Secure   = true,
                SameSite = SameSiteMode.None,
                Path     = "/api/admin",
                MaxAge   = TimeSpan.FromHours(8)
            });

            return Ok(new { message = "Authenticated." });
        }

        // POST /api/admin/auth/logout
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("admin_session", new CookieOptions
            {
                Path     = "/api/admin",
                Secure   = true,
                SameSite = SameSiteMode.None
            });
            return Ok(new { message = "Logged out." });
        }
    }

    public record VerifyRequest(string Password);
}
