using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using ScubaHub.Api.Filters;
using ScubaHub.Api.Services;

namespace ScubaHub.Api.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [EnableCors("AdminFrontend")] // Only the admin frontend origin may call these routes.
    [AdminAuthFilter]             // Validates the HttpOnly session cookie on every request.
    public class AdminController : ControllerBase
    {
        private readonly WeatherDataStore _store;

        public AdminController(WeatherDataStore store)
        {
            _store = store;
        }

        // GET /api/admin/session
        // Lightweight check — returns 200 if the session cookie is valid, 401 if not.
        [HttpGet("session")]
        public IActionResult GetSession() => Ok(new { authenticated = true });

        // GET /api/admin/weatherforecast
        [HttpGet("weatherforecast")]
        public IReadOnlyList<WeatherForecast> GetWeatherForecast()
        {
            return _store.GetAll();
        }

        // PUT /api/admin/weatherforecast/{index}
        // Updates a single forecast entry. Returns the updated record.
        [HttpPut("weatherforecast/{index:int}")]
        public IActionResult UpdateWeatherForecast(int index, [FromBody] UpdateForecastRequest req)
        {
            var updated = _store.Update(index, req.TemperatureC, req.Summary);

            if (updated is null)
                return NotFound(new { message = $"No forecast entry at index {index}." });

            return Ok(updated);
        }
    }

    public record UpdateForecastRequest(int TemperatureC, string? Summary);
}
