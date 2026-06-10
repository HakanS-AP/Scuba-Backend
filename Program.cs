using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ScubaHub.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddOpenApi();
            builder.Services.AddControllers();

            // Shared in-memory data store — one instance for the lifetime of the app.
            builder.Services.AddSingleton<ScubaHub.Api.Services.WeatherDataStore>();

            // ── CORS ──────────────────────────────────────────────────────────
            // Two separate policies — public and admin routes never share origins.
            //
            // Why this matters: CORS is browser-enforced. If the public frontend
            // origin were allowed on admin routes, an XSS script injected into
            // scubahub.com could make requests to /api/admin/* and the browser
            // would not block it (only the missing X-Admin-Key would stop it).
            // By restricting admin routes to the admin origin, the browser rejects
            // those requests before they even leave the tab.
            //
            // Note: CORS does not protect against direct HTTP calls (e.g. curl).
            // The AdminAuthFilter (X-Admin-Key header) is the server-side guard.
            // Your Azure VNet + ZTNA policy is the network-layer guard.
            // builder.Services.AddCors(options =>
            // {
            //     // Applied to public endpoints — never includes the admin origin.
            //     options.AddPolicy("PublicFrontend", policy =>
            //     {
            //         policy.WithOrigins(
            //                 "http://localhost:5173",  // dev
            //                 "https://scubahub.com"    // prod
            //               )
            //               .AllowAnyHeader()
            //               .AllowAnyMethod();
            //     });

            //     // Applied to admin endpoints — never includes the public origin.
            //     options.AddPolicy("AdminFrontend", policy =>
            //     {
            //         policy.WithOrigins(
            //                 "http://localhost:5174",       // dev
            //                 "https://admin.scubahub.com"  // prod
            //               )
            //               .AllowAnyHeader()
            //               .AllowAnyMethod()
            //               .AllowCredentials(); // Required for HttpOnly cookie auth
            //     });
            // });

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            // UseCors() with no policy name enables per-controller policy selection.
            //app.UseCors();
            app.MapControllers();

            app.Run();
        }
    }
}
