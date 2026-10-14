using System.Text;
using System.Text.Json;

namespace WinFormsGameTemplate;

public sealed class API
{
    readonly HttpClient _httpClient;
    static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public API()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = BuildApiBaseAddress(LoadSetting("LINGVO_API_URL")
                ?? "http://game.рудзынг.рф/api"),
            Timeout = TimeSpan.FromSeconds(10)
        };
    }

    static Uri BuildApiBaseAddress(string baseUrl)
    {
        baseUrl = baseUrl.Trim().TrimEnd('/');
        if (!baseUrl.EndsWith("/api", StringComparison.OrdinalIgnoreCase))
            baseUrl += "/api";
        return new Uri(baseUrl + "/", UriKind.Absolute);
    }

    static string? LoadSetting(string name)
    {
        var value = Environment.GetEnvironmentVariable(name);
        if (!string.IsNullOrWhiteSpace(value))
            return value;

        var envPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".env");
        if (!File.Exists(envPath))
            return null;

        foreach (var rawLine in File.ReadAllLines(envPath))
        {
            var line = rawLine.Trim();
            if (line.StartsWith(name + "=", StringComparison.Ordinal))
                return line[(name.Length + 1)..].Trim().Trim('"', '\'');
        }

        return null;
    }

    public async Task<GameSessionResponse> ResolveGameSessionAsync(
        GameLaunchContext launchContext)
    {
        if (launchContext.SessionId is not > 0 || string.IsNullOrWhiteSpace(launchContext.Token))
            throw new InvalidOperationException("Launcher не передал игровую сессию и токен.");
        if (!int.TryParse(LoadSetting("LINGVO_GAME_ID"), out var expectedGameId)
            || expectedGameId <= 0)
            throw new InvalidOperationException("LINGVO_GAME_ID не настроен для этой игры.");

        using var content = new StringContent(
            JsonSerializer.Serialize(new
            {
                gameSessionId = launchContext.SessionId.Value,
                token = launchContext.Token
            }),
            Encoding.UTF8,
            "application/json");
        using var response = await _httpClient.PostAsync("game/session/resolve", content);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Игровая сессия отклонена: {responseContent}");

        var result = JsonSerializer.Deserialize<GameSessionResponse>(responseContent, JsonOptions)
            ?? throw new InvalidOperationException("Портал вернул пустой ответ игровой сессии.");
        if (!result.valid || string.IsNullOrWhiteSpace(result.userId))
            throw new InvalidOperationException("Портал вернул некорректную игровую сессию.");
        if (result.gameId != expectedGameId)
            throw new InvalidOperationException("Игровая сессия выдана для другой игры.");

        return result;
    }

    public async Task<ScoreResponse> AddScoreAsync(
        GameLaunchContext launchContext,
        int amount)
    {
        if (launchContext.SessionId is not > 0 || string.IsNullOrWhiteSpace(launchContext.Token))
            throw new InvalidOperationException("Игра запущена без игровой сессии портала.");
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Количество баллов должно быть положительным.");

        using var content = new StringContent(
            JsonSerializer.Serialize(new
            {
                gameSessionId = launchContext.SessionId.Value,
                score = amount,
                token = launchContext.Token
            }),
            Encoding.UTF8,
            "application/json");
        using var response = await _httpClient.PostAsync("score", content);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Ошибка начисления баллов: {responseContent}");

        return JsonSerializer.Deserialize<ScoreResponse>(responseContent, JsonOptions)
            ?? throw new InvalidOperationException("Портал вернул пустой ответ начисления баллов.");
    }
}

public sealed class GameSessionResponse
{
    public bool valid { get; set; }
    public int gameId { get; set; }
    public string userId { get; set; } = string.Empty;
    public string userName { get; set; } = string.Empty;
    public int totalUserPoints { get; set; }
}

public sealed class ScoreResponse
{
    public string userId { get; set; } = string.Empty;
    public int newTotalPoints { get; set; }
}
