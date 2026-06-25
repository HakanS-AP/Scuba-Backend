using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
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
            builder.Services.AddSingleton<ScubaHub.Api.Services.WeatherDataStore>();

            // ── CORS ──────────────────────────────────────────────────────────
            // Origins are read from config — no code change needed when you add
            // a custom domain or a new environment.
            //
            // Set these in Azure Portal → App Service → Configuration → Application Settings:
            //   CorsOrigins__Public__0  =  https://<your-public-app>.azurewebsites.net
            //   CorsOrigins__Admin__0   =  https://<your-admin-app>.azurewebsites.net
            //
            // Add __1, __2, ... for additional origins (e.g. when custom domains go live).
            var publicOrigins = builder.Configuration
                                       .GetSection("CorsOrigins:Public")
                                       .Get<string[]>() ?? [];

            var adminOrigins  = builder.Configuration
                                       .GetSection("CorsOrigins:Admin")
                                       .Get<string[]>() ?? [];

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("PublicFrontend", policy =>
                    policy.WithOrigins(publicOrigins)
                          .AllowAnyHeader()
                          .AllowAnyMethod());

                // AllowCredentials() is required for the HttpOnly session cookie.
                options.AddPolicy("AdminFrontend", policy =>
                    policy.WithOrigins(adminOrigins)
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials());
            });

            var app = builder.Build();

            // ── Azure forwarded headers ───────────────────────────────────────
            // Azure App Service terminates SSL at its load balancer and forwards
            // requests to the app over plain HTTP internally.
            // Without this middleware HttpContext.Request.IsHttps is always false,
            // which breaks the Secure cookie flag on the admin session cookie.
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedProto
            });

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseCors();
            app.MapControllers();

            app.Run();
        }
    }
}
