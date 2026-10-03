using System.Text.Json;
using System.Text.Json.Serialization;

namespace SkiApi.Services;

public class WeatherService
{
    private readonly HttpClient _http;

    public WeatherService(IHttpClientFactory httpClientFactory)
    {
        _http = httpClientFactory.CreateClient("Weather");
        _http.BaseAddress = new Uri("https://api.open-meteo.com/");
    }

    /// <summary>
    /// Получает погоду для указанных координат.
    /// dayOffset: 0 = сегодня, 1 = завтра.
    /// hour: 0-23 (по местному времени). null = текущий час.
    /// </summary>
    public async Task<WeatherData?> GetWeatherAsync(double lat, double lon, int dayOffset = 0, int? hour = null)
    {
        try
        {
            var latStr = lat.ToString(System.Globalization.CultureInfo.InvariantCulture);
            var lonStr = lon.ToString(System.Globalization.CultureInfo.InvariantCulture);

            var url = $"v1/forecast?latitude={latStr}&longitude={lonStr}" +
                      "&hourly=temperature_2m,relative_humidity_2m,wind_speed_10m,cloud_cover" +
                      "&forecast_days=2" +
                      "&timezone=auto" +
                      "&wind_speed_unit=ms";

            var response = await _http.GetAsync(url);
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<OpenMeteoResponse>(json);

            if (data?.Hourly == null || data.Hourly.Time == null || data.Hourly.Time.Count == 0)
                return null;

            // Если час не задан — берём текущий час (по местному времени сервера, 
            // но корректнее — по времени в ответе API)
            int targetHour = hour ?? DateTime.Now.Hour;

            // Ищем индекс в массиве: время формата "2026-10-04T09:00"
            // dayOffset определяет нужный день
            var targetDay = DateTime.Today.AddDays(dayOffset);
            var targetDateTimeString = targetDay.ToString("yyyy-MM-dd") + $"T{targetHour:D2}:00";

            int idx = data.Hourly.Time.FindIndex(t => t == targetDateTimeString);

            // Если точного совпадения нет — берём ближайшее время
            if (idx < 0)
            {
                // Пытаемся найти по часу внутри нужного дня
                var dayPrefix = targetDay.ToString("yyyy-MM-dd");
                idx = data.Hourly.Time.FindIndex(t => t.StartsWith(dayPrefix));
                if (idx < 0) return null;

                // Сдвигаем на нужный час
                idx = Math.Min(idx + targetHour, data.Hourly.Time.Count - 1);
            }

            return new WeatherData
            {
                AirTemp = data.Hourly.Temperature[idx],
                Humidity = data.Hourly.Humidity[idx],
                WindSpeed = data.Hourly.WindSpeed[idx],
                IsSunny = data.Hourly.CloudCover[idx] < 30,
                Hour = targetHour,
                DayOffset = dayOffset
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"WeatherService error: {ex.Message}");
            return null;
        }
    }
}

public class WeatherData
{
    public double AirTemp { get; set; }
    public double Humidity { get; set; }
    public double WindSpeed { get; set; }
    public bool IsSunny { get; set; }
    public int Hour { get; set; }
    public int DayOffset { get; set; }
}

internal class OpenMeteoResponse
{
    [JsonPropertyName("hourly")]
    public OpenMeteoHourly? Hourly { get; set; }
}

internal class OpenMeteoHourly
{
    [JsonPropertyName("time")]
    public List<string> Time { get; set; } = new();

    [JsonPropertyName("temperature_2m")]
    public List<double> Temperature { get; set; } = new();

    [JsonPropertyName("relative_humidity_2m")]
    public List<double> Humidity { get; set; } = new();

    [JsonPropertyName("wind_speed_10m")]
    public List<double> WindSpeed { get; set; } = new();

    [JsonPropertyName("cloud_cover")]
    public List<double> CloudCover { get; set; } = new();
}
