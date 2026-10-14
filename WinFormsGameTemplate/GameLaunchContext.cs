namespace WinFormsGameTemplate
{
    public sealed record GameLaunchContext(int? SessionId, string? Token)
    {
        public static GameLaunchContext Empty { get; } = new(null, null);

        public bool IsPortalSession =>
            SessionId is > 0 && !string.IsNullOrWhiteSpace(Token);

        public static GameLaunchContext From(string[] args)
        {
            int? sessionId = ParseSessionId(
                Environment.GetEnvironmentVariable("LINGVO_GAME_SESSION_ID"));
            string? token = Environment.GetEnvironmentVariable(
                "LINGVO_GAME_SESSION_TOKEN");

            foreach (var argument in args)
            {
                if (argument.StartsWith("--session=", StringComparison.OrdinalIgnoreCase))
                {
                    sessionId = ParseSessionId(argument["--session=".Length..]);
                }
                else if (argument.StartsWith("--token=", StringComparison.OrdinalIgnoreCase))
                {
                    token = argument["--token=".Length..];
                }
                else if (Uri.TryCreate(argument, UriKind.Absolute, out var launchUri))
                {
                    var query = ParseQuery(launchUri.Query);
                    if (query.TryGetValue("session", out var rawSessionId))
                        sessionId = ParseSessionId(rawSessionId);
                    if (query.TryGetValue("token", out var rawToken))
                        token = rawToken;
                }
            }

            return new GameLaunchContext(sessionId, token);
        }

        private static int? ParseSessionId(string? value)
        {
            return int.TryParse(value, out var sessionId) && sessionId > 0
                ? sessionId
                : null;
        }

        private static Dictionary<string, string> ParseQuery(string query)
        {
            return query
                .TrimStart('?')
                .Split('&', StringSplitOptions.RemoveEmptyEntries)
                .Select(part => part.Split('=', 2))
                .Where(parts => parts.Length == 2)
                .ToDictionary(
                    parts => Uri.UnescapeDataString(parts[0]),
                    parts => Uri.UnescapeDataString(parts[1]),
                    StringComparer.OrdinalIgnoreCase);
        }
    }
}
