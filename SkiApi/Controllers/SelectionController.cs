using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkiApi.Data;
using SkiApi.Models;
using SkiApi.Services;
using System.Text.Json;

namespace SkiApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SelectionController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly WeatherService _weather;
    private readonly NotificationService _notifier;

    public SelectionController(AppDbContext context, WeatherService weather, NotificationService notifier)
    {
        _context = context;
        _weather = weather;
        _notifier = notifier;
    }

    [HttpPost]
    public async Task<IActionResult> SelectWaxes([FromBody] SelectionRequest request)
    {
        if (request == null) return BadRequest("Request is empty");

        // 0. Погода из API (если есть координаты)
        bool weatherFromApi = false;
        if (request.Latitude.HasValue && request.Longitude.HasValue)
        {
            var weather = await _weather.GetWeatherAsync(request.Latitude.Value, request.Longitude.Value);
            if (weather != null)
            {
                request.AirTemp ??= weather.AirTemp;
                request.Humidity ??= weather.Humidity;
                request.WindSpeed ??= weather.WindSpeed;
                request.IsSunny ??= weather.IsSunny;
                weatherFromApi = true;
            }
        }

        if (!request.AirTemp.HasValue || !request.Humidity.HasValue)
            return BadRequest("Не удалось определить температуру и влажность.");

        double airTemp = request.AirTemp.Value;
        double humidity = request.Humidity.Value;
        double? windSpeed = request.WindSpeed;

        // 1. Корректировка влажности
        double effectiveHumidity = humidity;
        if (windSpeed.HasValue && windSpeed.Value >= 3) effectiveHumidity -= 10;
        if (request.IsSunny == true) effectiveHumidity += 10;
        effectiveHumidity = Math.Clamp(effectiveHumidity, 0, 100);

        // 2. ПОДБОР ЛЫЖ (сначала, чтобы знать про камус)
        var allSkis = await _context.SkiModels
            .Include(s => s.StoneGrind)
            .ToListAsync();

        var skiResults = allSkis
            .Where(s => s.Style == request.Style)
            .Select(s => new
            {
                Ski = s,
                Score = CalculateSkiScore(s, airTemp, effectiveHumidity, request.SnowType, request.TrackType)
            })
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .ToList();

        // 3. Проверка: есть ли среди подходящих лыж пара с камусом?
        // Если да — мази держания для классики не обязательны.
// Grip не нужен, только если ВСЕ подходящие лыжи с камусом (или стиль не Classic)
bool allSkiHaveSkin = skiResults.Any() && skiResults.All(x => x.Ski.HasSkin);
bool gripOptional = allSkiHaveSkin && request.Style == "Classic";

// Показываем grip, если: классика И (не все лыжи с камусом)
bool showGrip = request.Style == "Classic" && !allSkiHaveSkin;


        // 4. ПОДБОР МАЗЕЙ
        var glideCategories = new List<string> { "Glide", "Powder" };
        var gripCategories = new List<string> { "Grip" };


var categories = new List<string>(glideCategories);
if (showGrip)
    categories.AddRange(gripCategories);


        var allWaxes = await _context.Waxes.Where(w => categories.Contains(w.Category)).ToListAsync();

        bool Matches(Wax w) =>
            airTemp >= Math.Min(w.TempMin, w.TempMax) &&
            airTemp <= Math.Max(w.TempMin, w.TempMax) &&
            effectiveHumidity >= w.HumidityMin &&
            effectiveHumidity <= w.HumidityMax &&
            (w.SnowType == "All" || w.SnowType == request.SnowType) &&
            (w.TrackType == "All" || w.TrackType == request.TrackType);

        var matched = allWaxes.Where(Matches).ToList();

        var glideBase   = matched.Where(w => w.Category == "Glide" && w.Type == "Base").ToList();
        var glideFinish = matched.Where(w => w.Category == "Powder" || (w.Category == "Glide" && w.Type == "Finish")).ToList();
        var gripBase    = matched.Where(w => w.Category == "Grip").ToList();

        // 5. Логирование
        var log = new SelectionLog
        {
            Timestamp = DateTime.UtcNow,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            WeatherSource = weatherFromApi ? "open-meteo" : "manual",
            AirTemp = airTemp,
            Humidity = humidity,
            EffectiveHumidity = effectiveHumidity,
            WindSpeed = windSpeed,
            IsSunny = request.IsSunny,
            SnowType = request.SnowType,
            TrackType = request.TrackType,
            Style = request.Style,
            SelectedGlideBaseIds   = JsonSerializer.Serialize(glideBase.Select(w => w.Id)),
            SelectedGlideFinishIds = JsonSerializer.Serialize(glideFinish.Select(w => w.Id)),
            SelectedGripIds        = JsonSerializer.Serialize(gripBase.Select(w => w.Id)),
            DeviceHash = request.DeviceHash
        };
        _context.SelectionLogs.Add(log);
        await _context.SaveChangesAsync();

        // 6. Формирование ответа
        object ToDto(Wax w) => new
        {
            w.Id, w.Name, w.Brand, w.Category, w.Type,
            w.TempMin, w.TempMax, w.HumidityMin, w.HumidityMax, w.Notes, w.Warnings
        };

        object SkiToDto(dynamic x) => new
        {
            x.Ski.Id, x.Ski.Brand, x.Ski.Model, x.Ski.Year, x.Ski.Style,
            x.Ski.Length, x.Ski.Profile,
            x.Ski.ProfileTempMin, x.Ski.ProfileTempMax,
            x.Ski.StiffnessValue, x.Ski.StiffnessLabel,
            x.Ski.CamberHeightMm,
            x.Ski.HasSkin,
            x.Ski.Notes, x.Ski.PersonalNotes,
            stoneGrind = x.Ski.StoneGrind == null ? null : new
            {
                x.Ski.StoneGrind.Id, x.Ski.StoneGrind.Name,
                x.Ski.StoneGrind.TempMin, x.Ski.StoneGrind.TempMax,
                x.Ski.StoneGrind.SnowTypes, x.Ski.StoneGrind.TrackType,
                x.Ski.StoneGrind.Notes
            },
            matchScore = x.Score
        };

        // Формируем список предупреждений
        var notesList = new List<string>
        {
            "glideBase — наносится первым (основание)",
            "glideFinish — наносится поверх основания (финишный слой)",
            "skis — отсортированы по степени совпадения (matchScore). Выбор — за спортсменом.",
            "Вес спортсмена не учитывается автоматически — жёсткость (stiffnessValue) отображается для справки."
        };


if (gripOptional)
{
    notesList.Add("⚡ Все найденные лыжи с камусом — мази держания не требуются.");
}
else if (request.Style == "Classic" && skiResults.Any(x => x.Ski.HasSkin))
{
    notesList.Add("💡 Среди найденных лыж есть пара с камусом — для неё мази держания не нужны, для остальных нужны.");
}
else
{
    notesList.Add("grip — мази держания для классического хода");
}


        var result = new
        {
            selectionId = log.Id,
            weatherSource = log.WeatherSource,
            input = new
            {
                request.Latitude, request.Longitude,
                airTemp, humidity, effectiveHumidity,
                snowType = request.SnowType,
                trackType = request.TrackType,
                style = request.Style,
                windSpeed,
                isSunny = request.IsSunny
            },
            glideBase    = glideBase.Select(ToDto),
            glideFinish  = glideFinish.Select(ToDto),
            grip         = gripBase.Select(ToDto),
            gripOptional,     // true = можно не использовать (камус)
            skis         = skiResults.Select(SkiToDto),
            totalMatched = matched.Count,
            skiCount     = skiResults.Count,
            notes = notesList
        };

        return Ok(result);
    }

    /// <summary>
    /// Считает степень совпадения лыжи с условиями (0-100).
    /// </summary>
    private int CalculateSkiScore(SkiModel ski, double airTemp, double effectiveHumidity,
                                   string snowType, string trackType)
    {
        int score = 0;

        // 1. Эпюра (профиль) — температурный диапазон
        if (ski.ProfileTempMin.HasValue && ski.ProfileTempMax.HasValue)
        {
            double pMin = Math.Min(ski.ProfileTempMin.Value, ski.ProfileTempMax.Value);
            double pMax = Math.Max(ski.ProfileTempMin.Value, ski.ProfileTempMax.Value);

            if (airTemp >= pMin && airTemp <= pMax)
            {
                score += 30;
                double mid = (pMin + pMax) / 2;
                double halfRange = (pMax - pMin) / 2;
                if (halfRange > 0)
                {
                    double deviation = Math.Abs(airTemp - mid);
                    score += (int)(10 * (1 - deviation / halfRange));
                }
                else score += 10;
            }
            else if (airTemp >= pMin - 3 && airTemp <= pMax + 3) score += 15;
            else if (airTemp >= pMin - 5 && airTemp <= pMax + 5) score += 5;
        }
        else
        {
            score += 10;
        }

        // 2. Штайншлифт
        if (ski.StoneGrind != null)
        {
            var sg = ski.StoneGrind;

            if (sg.TempMin.HasValue && sg.TempMax.HasValue)
            {
                double sMin = Math.Min(sg.TempMin.Value, sg.TempMax.Value);
                double sMax = Math.Max(sg.TempMin.Value, sg.TempMax.Value);

                if (airTemp >= sMin && airTemp <= sMax) score += 30;
                else if (airTemp >= sMin - 2 && airTemp <= sMax + 2) score += 20;
                else if (airTemp >= sMin - 5 && airTemp <= sMax + 5) score += 10;
                else score -= 10;
            }

            var snowTypes = (sg.SnowTypes ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries)
                                                .Select(s => s.Trim()).ToList();
            if (snowTypes.Contains("All") || snowTypes.Contains(snowType)) score += 20;
            else score -= 15;

            if (sg.TrackType == "All" || sg.TrackType == trackType) score += 15;
            else score -= 10;
        }
        else
        {
            score += 5;
        }

        return Math.Max(score, 0);
    }

    [HttpPost("{id}/rate")]
    public async Task<IActionResult> RateSelection(int id, [FromBody] RatingRequest request)
    {
        var log = await _context.SelectionLogs.FindAsync(id);
        if (log == null) return NotFound($"Selection log #{id} not found");
        if (request.Rating < 1 || request.Rating > 10)
            return BadRequest("Rating must be between 1 and 10");

        log.Rating = request.Rating;
        if (!string.IsNullOrWhiteSpace(request.Review)) log.Review = request.Review;
        await _context.SaveChangesAsync();

        bool notificationSent = false;
        if (request.Rating <= 3 && !log.NotificationSent)
        {
            notificationSent = await _notifier.SendLowRatingAlertAsync(log);
            if (notificationSent)
            {
                log.NotificationSent = true;
                await _context.SaveChangesAsync();
            }
        }

        return Ok(new { selectionId = log.Id, log.Rating, log.Review, notificationSent });
    }
}

public class SelectionRequest
{
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public double? AirTemp { get; set; }
    public double? SnowTemp { get; set; }
    public double? Humidity { get; set; }
    public double? WindSpeed { get; set; }
    public bool? IsSunny { get; set; }
    public string SnowType { get; set; } = "All";
    public string TrackType { get; set; } = "All";
    public string Style { get; set; } = "Classic";
    public string? DeviceHash { get; set; }
}

public class RatingRequest
{
    public int Rating { get; set; }
    public string? Review { get; set; }
}

