using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using ScubaHub.Api.Services;

namespace ScubaHub.Api.Controllers
{
    [ApiController]
    [Route("api")]
    [EnableCors("PublicFrontend")] // Only the public frontend may call these routes.
    public class WeatherController : ControllerBase
    {
        private readonly WeatherDataStore _store;

        public WeatherController(WeatherDataStore store)
        {
            _store = store;
        }

        // GET /api/weatherforecast
        [HttpGet("weatherforecast")]
        public IReadOnlyList<WeatherForecast> GetWeatherForecast()
        {
            return _store.GetAll();
        }
    }
}
