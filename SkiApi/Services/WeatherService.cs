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

    public async Task<WeatherData?> GetWeatherAsync(double lat, double lon)
    {
        try
        {
            var latStr = lat.ToString(System.Globalization.CultureInfo.InvariantCulture);
            var lonStr = lon.ToString(System.Globalization.CultureInfo.InvariantCulture);

            var url = $"v1/forecast?latitude={latStr}&longitude={lonStr}" +
                      "&current=temperature_2m,relative_humidity_2m,wind_speed_10m,cloud_cover" +
                      "&wind_speed_unit=ms";

            var response = await _http.GetAsync(url);
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<OpenMeteoResponse>(json);

            if (data?.Current == null) return null;

            return new WeatherData
            {
                AirTemp = data.Current.Temperature,
                Humidity = data.Current.Humidity,
                WindSpeed = data.Current.WindSpeed,
                IsSunny = data.Current.CloudCover < 30
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
}

internal class OpenMeteoResponse
{
    [JsonPropertyName("current")]
    public OpenMeteoCurrent? Current { get; set; }
}

internal class OpenMeteoCurrent
{
    [JsonPropertyName("temperature_2m")]
    public double Temperature { get; set; }

    [JsonPropertyName("relative_humidity_2m")]
    public double Humidity { get; set; }

    [JsonPropertyName("wind_speed_10m")]
    public double WindSpeed { get; set; }

    [JsonPropertyName("cloud_cover")]
    public double CloudCover { get; set; }
}
	
