namespace SkiApi.Models;

public class SelectionLog
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    // Местоположение (опционально)
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    // Погода
    public string WeatherSource { get; set; } = string.Empty; // open-meteo / manual
    public double AirTemp { get; set; }
    public double Humidity { get; set; }
    public double EffectiveHumidity { get; set; }
    public double? WindSpeed { get; set; }
    public bool? IsSunny { get; set; }

    // Условия
    public string SnowType { get; set; } = string.Empty;
    public string TrackType { get; set; } = string.Empty;
    public string Style { get; set; } = string.Empty;

    // Выбранные мази (хранятся как JSON-массивы ID)
    public string? SelectedGlideBaseIds { get; set; }
    public string? SelectedGlideFinishIds { get; set; }
    public string? SelectedGripIds { get; set; }

    // Обратная связь
    public int? Rating { get; set; }         // 1-10
    public string? Review { get; set; }      // отзыв
    public string? DeviceHash { get; set; }  // анонимный ID устройства
    public bool NotificationSent { get; set; } = false;
}

