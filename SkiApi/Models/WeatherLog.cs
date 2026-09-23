namespace SkiApi.Models;

public class WeatherLog
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public double AirTemp { get; set; }
    public double? SnowTemp { get; set; }
    public double Humidity { get; set; }
    public string SnowType { get; set; } = string.Empty;
    public string TrackType { get; set; } = string.Empty;
    public string? CloudCover { get; set; }
    public double? WindSpeed { get; set; }
}

