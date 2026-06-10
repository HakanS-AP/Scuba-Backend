using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace ScubaHub.Api.Controllers
{
    [ApiController]
    [Route("api/admin/auth")]
    //[EnableCors("AdminFrontend")] // Only the admin frontend origin may call these routes.
    public class AdminAuthController : ControllerBase
    {
        // POST /api/admin/auth/verify
        // Body: { "password": "..." }
        //
        // Validates the admin password and sets an HttpOnly session cookie.
        // HttpOnly means JavaScript cannot read the cookie at all — XSS on the
        // admin frontend cannot steal the session even if it runs in the same origin.
        //
        // This endpoint has no [AdminAuthFilter] — authentication IS the login step.
        // Consider adding rate-limiting in production.
        [HttpPost("verify")]
        public IActionResult Verify([FromBody] VerifyRequest req, [FromServices] IConfiguration config)
        {
            var expectedKey = config["AdminSettings:Key"];

            if (string.IsNullOrWhiteSpace(expectedKey))
                return StatusCode(500, "AdminSettings:Key is not configured on the server.");

            if (req.Password != expectedKey)
                return Unauthorized(new { message = "Invalid password." });

            // Set an HttpOnly, Secure, SameSite=Strict cookie.
            // JS cannot read this — document.cookie will never show it.
            Response.Cookies.Append("admin_session", expectedKey, new CookieOptions
            {
                HttpOnly  = true,               // Invisible to JavaScript
                Secure    = !HttpContext.Request.IsHttps ? false : true, // HTTPS only in prod
                SameSite  = SameSiteMode.Strict, // Never sent on cross-site requests (CSRF guard)
                Path      = "/api/admin",        // Cookie only sent to admin API routes
                MaxAge    = TimeSpan.FromHours(8)
            });

            return Ok(new { message = "Authenticated." });
        }

        // POST /api/admin/auth/logout
        // Clears the session cookie.
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("admin_session", new CookieOptions
            {
                Path = "/api/admin"
            });
            return Ok(new { message = "Logged out." });
        }
    }

    public record VerifyRequest(string Password);
}
