using SkiApi.Models;
using System.Net;
using System.Net.Mail;

namespace SkiApi.Services;

public class NotificationService
{
    private readonly HttpClient _telegramHttp;
    private readonly IConfiguration _config;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(IHttpClientFactory httpClientFactory, IConfiguration config, ILogger<NotificationService> logger)
    {
        _telegramHttp = httpClientFactory.CreateClient("Telegram");
        _config = config;
        _logger = logger;
    }

    public async Task<bool> SendLowRatingAlertAsync(SelectionLog log)
    {
        var text = $@"⚠️ Низкая оценка подбора ⚠️

📊 Оценка: {log.Rating}/10
🆔 ID: {log.Id}
📝 Отзыв: {log.Review ?? "(без отзыва)"}

🌡 Температура: {log.AirTemp}°C
💧 Влажность: {log.Humidity}% (с поправкой: {log.EffectiveHumidity}%)
💨 Ветер: {log.WindSpeed?.ToString() ?? "—"} м/с
☀️ Солнце: {(log.IsSunny == true ? "да" : "нет")}

❄️ Снег: {log.SnowType}
🏔 Трасса: {log.TrackType}
🎿 Стиль: {log.Style}

📍 Координаты: {log.Latitude?.ToString() ?? "—"}, {log.Longitude?.ToString() ?? "—"}";

        var telegramTask = SendTelegramAsync(text, log.Id);
        var emailTask = SendEmailAsync("SKI-S: Низкая оценка подбора", text);

        await Task.WhenAll(telegramTask, emailTask);

        bool telegramOk = await telegramTask;
        bool emailOk = await emailTask;

        _logger.LogInformation($"Уведомления: Telegram={telegramOk}, Email={emailOk}");

        return telegramOk || emailOk;
    }

    private async Task<bool> SendTelegramAsync(string text, int logId)
    {
        var token = _config["Telegram:Token"];
        var chatId = _config["Telegram:ChatId"];

        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(chatId) ||
            token.StartsWith("ТОКЕН") || chatId.StartsWith("ВАШ"))
        {
            _logger.LogWarning("Telegram token/chatId не настроены — уведомление пропущено");
            return false;
        }

        try
        {
            var url = $"https://api.telegram.org/bot{token}/sendMessage";
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("chat_id", chatId),
                new KeyValuePair<string, string>("text", text)
            });

            var response = await _telegramHttp.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation($"Telegram-уведомление отправлено для log #{logId}");
                return true;
            }

            var error = await response.Content.ReadAsStringAsync();
            _logger.LogError($"Telegram API error: {response.StatusCode} — {error}");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Ошибка отправки в Telegram: {ex.Message}");
            return false;
        }
    }

    private async Task<bool> SendEmailAsync(string subject, string body)
    {
        var server = _config["Email:SmtpServer"];
        var port = int.Parse(_config["Email:SmtpPort"] ?? "587");
        var sender = _config["Email:SenderEmail"];
        var password = _config["Email:SenderPassword"];
        var recipient = _config["Email:RecipientEmail"];

        if (string.IsNullOrEmpty(server) || string.IsNullOrEmpty(sender) ||
            string.IsNullOrEmpty(password) || string.IsNullOrEmpty(recipient))
        {
            _logger.LogWarning("Email-настройки не заполнены — уведомление пропущено");
            return false;
        }

        try
        {
            using var client = new SmtpClient(server, port)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(sender, password)
            };

            var message = new MailMessage(sender, recipient, subject, body);
            await client.SendMailAsync(message);

            _logger.LogInformation($"Email-уведомление отправлено на {recipient}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Ошибка отправки Email: {ex.Message}");
            return false;
        }
    }
}

