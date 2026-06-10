using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ScubaHub.Api.Filters
{
    /// <summary>
    /// Applied to any controller or action that requires admin access.
    ///
    /// Reads the admin session from an HttpOnly cookie (admin_session).
    /// Because the cookie is HttpOnly, JavaScript can never read or steal it —
    /// even if XSS runs in the admin frontend's origin.
    ///
    /// Security layers (all three must be active in production):
    ///
    ///   Layer 1 — Network (Azure):
    ///     VNet Integration + Private Endpoint on the App Service. Only VNet
    ///     traffic reaches admin.scubahub.com. ZTNA (e.g. Microsoft Entra
    ///     Private Access) gates who gets into the VNet at all.
    ///
    ///   Layer 2 — CORS:
    ///     Admin routes only accept requests from the admin origin
    ///     (admin.scubahub.com). The public frontend cannot even initiate a
    ///     browser request to /api/admin/* — the browser blocks it.
    ///
    ///   Layer 3 — This filter:
    ///     Validates the HttpOnly cookie on every admin request, server-side.
    ///     Protects against direct HTTP calls that bypass CORS entirely.
    /// </summary>
    public class AdminAuthFilter : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var config = context.HttpContext.RequestServices
                                .GetRequiredService<IConfiguration>();

            var expectedKey = config["AdminSettings:Key"];

            if (string.IsNullOrWhiteSpace(expectedKey))
            {
                context.Result = new ObjectResult("AdminSettings:Key is not configured on the server.")
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
                return;
            }

            // Read from the HttpOnly cookie — JS cannot access this value.
            context.HttpContext.Request.Cookies.TryGetValue("admin_session", out var sessionValue);

            if (string.IsNullOrEmpty(sessionValue) || sessionValue != expectedKey)
            {
                context.Result = new UnauthorizedResult();
            }
        }
    }
}
