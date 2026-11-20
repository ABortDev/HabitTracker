using System.Text;
using System.Text.Json;
using Habit_Tracker_Project.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Habit_Tracker_Project.Services
{
    public class TelegramBotService : BackgroundService
    {
        public const string LinkCachePrefix = "tg-link:";

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _config;
        private readonly ILogger<TelegramBotService> _logger;
        private long _offset;

        public TelegramBotService(
            IServiceScopeFactory scopeFactory,
            IHttpClientFactory httpClientFactory,
            IMemoryCache cache,
            IConfiguration config,
            ILogger<TelegramBotService> logger)
        {
            _scopeFactory = scopeFactory;
            _httpClientFactory = httpClientFactory;
            _cache = cache;
            _config = config;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var token = _config["Telegram:BotToken"];
            if (string.IsNullOrWhiteSpace(token))
            {
                _logger.LogWarning("Telegram BotToken is missing. Bot is disabled.");
                return;
            }

            var client = _httpClientFactory.CreateClient();
            var api = $"https://api.telegram.org/bot{token}";

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var url = $"{api}/getUpdates?timeout=30&offset={_offset}";
                    using var response = await client.GetAsync(url, stoppingToken);
                    var json = await response.Content.ReadAsStringAsync(stoppingToken);
                    using var doc = JsonDocument.Parse(json);

                    if (!doc.RootElement.GetProperty("ok").GetBoolean())
                    {
                        await Task.Delay(3000, stoppingToken);
                        continue;
                    }

                    foreach (var update in doc.RootElement.GetProperty("result").EnumerateArray())
                    {
                        _offset = update.GetProperty("update_id").GetInt64() + 1;

                        if (!update.TryGetProperty("message", out var message))
                            continue;
                        if (!message.TryGetProperty("text", out var textEl))
                            continue;

                        var text = textEl.GetString() ?? "";
                        var chatId = message.GetProperty("chat").GetProperty("id").GetInt64();

                        var code = ExtractLinkCode(text);
                        if (code == null)
                        {
                            await SendAsync(client, api, chatId,
                                "Send /start and the code from your Habit Tracker profile.",
                                stoppingToken);
                            continue;
                        }

                        if (!_cache.TryGetValue(LinkCachePrefix + code, out string? userId)
                            || string.IsNullOrEmpty(userId))
                        {
                            await SendAsync(client, api, chatId,
                                "Code is invalid or expired. Generate a new one on the website.",
                                stoppingToken);
                            continue;
                        }

                        using var scope = _scopeFactory.CreateScope();
                        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId, stoppingToken);
                        if (user == null)
                            continue;

                        user.TelegramChatId = chatId;
                        await db.SaveChangesAsync(stoppingToken);
                        _cache.Remove(LinkCachePrefix + code);

                        await SendAsync(client, api, chatId,
                            "Telegram is connected. You will get habit reminders here.",
                            stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Telegram polling failed");
                    await Task.Delay(5000, stoppingToken);
                }
            }
        }

        private static string? ExtractLinkCode(string text)
        {
            var parts = text.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var raw = parts.Length >= 2 && parts[0].StartsWith("/start", StringComparison.OrdinalIgnoreCase)
                ? parts[1]
                : parts[0];

            raw = raw.Trim().ToUpperInvariant();
            return raw.StartsWith("HT-") ? raw : null;
        }

        public static async Task SendAsync(
            HttpClient client,
            string api,
            long chatId,
            string text,
            CancellationToken ct)
        {
            var payload = JsonSerializer.Serialize(new { chat_id = chatId, text });
            using var content = new StringContent(payload, Encoding.UTF8, "application/json");
            await client.PostAsync($"{api}/sendMessage", content, ct);
        }
    }
}