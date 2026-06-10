namespace ScubaHub.Api.Services
{
    // Singleton in-memory store — shared between the public and admin controllers.
    // Data resets when the app restarts. Swap this for a database-backed
    // repository when you're ready to add persistence.
    public class WeatherDataStore
    {
        private static readonly string[] DefaultSummaries =
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild",
            "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly List<WeatherForecast> _forecasts;
        private readonly object _lock = new();

        public WeatherDataStore()
        {
            _forecasts = Enumerable.Range(1, 5).Select(index => new WeatherForecast(
                DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                Random.Shared.Next(-20, 55),
                DefaultSummaries[Random.Shared.Next(DefaultSummaries.Length)]
            )).ToList();
        }

        public IReadOnlyList<WeatherForecast> GetAll()
        {
            lock (_lock) return _forecasts.AsReadOnly();
        }

        public WeatherForecast? Update(int index, int temperatureC, string? summary)
        {
            lock (_lock)
            {
                if (index < 0 || index >= _forecasts.Count) return null;

                _forecasts[index] = _forecasts[index] with
                {
                    TemperatureC = temperatureC,
                    Summary      = summary
                };

                return _forecasts[index];
            }
        }
    }
}
