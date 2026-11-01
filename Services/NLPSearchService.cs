using System.Net.Http.Json;
using System.Text.Json;

namespace IntelligentInventoryDashboard.Services
{
    /// <summary>
    /// Turns a natural-language query (e.g. "Hondas under 25000 that sat here for more than 60 days")
    /// into structured <see cref="VehicleQueryFilters"/> using the OpenAI gpt-4o-mini model with
    /// JSON-object response mode.
    ///
    /// The API key is read from configuration (User Secrets) under the key "OpenAI:ApiKey".
    /// When the key is missing, or the request fails for any reason, the service degrades
    /// gracefully to plain keyword matching.
    /// </summary>
    public class NLPSearchService
    {
        private const string OpenAiChatUrl = "https://api.openai.com/v1/chat/completions";

        private static readonly string SystemPrompt =
            "You translate free-text vehicle inventory searches into JSON search filters. " +
            "Output JSON only. Use this exact schema: " +
            "{\"keyword\": string|null, \"make\": string|null, \"model\": string|null, " +
            "\"maxPrice\": number|null, \"minDays\": number|null}. " +
            "Rules: \"make\" is the vehicle brand (e.g. Honda, Toyota); \"model\" is the specific model name; " +
            "\"maxPrice\" is the price ceiling in whatever currency units the user implies; " +
            "\"minDays\" is the minimum number of days in stock the user asks for (e.g. \"older than 90 days\" -> 90); " +
            "\"keyword\" captures any other free-text the user wants matched against chassis number, make, or model. " +
            "Use null for fields the user did not specify.";

        private readonly HttpClient _http;
        private readonly IConfiguration _config;
        private readonly ILogger<NLPSearchService> _logger;

        public NLPSearchService(HttpClient http, IConfiguration config, ILogger<NLPSearchService> logger)
        {
            _http = http;
            _config = config;
            _logger = logger;
        }

        private string? ApiKey => _config["OpenAI:ApiKey"];

        /// <summary>
        /// Extracts structured filters from the user's query. Never returns null; on any
        /// failure it falls back to a keyword-only filter.
        /// </summary>
        public async Task<VehicleQueryFilters> ExtractFiltersAsync(string? query, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return new VehicleQueryFilters();
            }

            var apiKey = ApiKey;
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                _logger.LogInformation("OpenAI:ApiKey not configured (User Secrets); using keyword fallback");
                return KeywordFallback(query);
            }

            try
            {
                var payload = new
                {
                    model = "gpt-4o-mini",
                    response_format = new { type = "json_object" },
                    messages = new object[]
                    {
                        new { role = "system", content = SystemPrompt },
                        new { role = "user", content = query }
                    },
                    max_tokens = 300,
                    temperature = 0
                };

                using var request = new HttpRequestMessage(HttpMethod.Post, OpenAiChatUrl);
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
                request.Content = JsonContent.Create(payload);

                using var response = await _http.SendAsync(request, cancellationToken);
                response.EnsureSuccessStatusCode();

                var root = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);
                var content = root
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                return TryParseFilters(content) ?? KeywordFallback(query);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or InvalidOperationException or JsonException)
            {
                _logger.LogWarning(ex, "OpenAI NLP request failed; falling back to keyword matching");
                return KeywordFallback(query);
            }
        }

        /// <summary>Parses the model's JSON content into filters; null when it cannot be trusted.</summary>
        private static VehicleQueryFilters? TryParseFilters(string? content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return null;
            }

            try
            {
                using var document = JsonDocument.Parse(content.Trim());
                var root = document.RootElement;

                if (root.ValueKind != JsonValueKind.Object)
                {
                    return null;
                }

                var filters = new VehicleQueryFilters
                {
                    Keyword = ReadString(root, "keyword"),
                    Make = ReadString(root, "make"),
                    Model = ReadString(root, "model")
                };

                if (root.TryGetProperty("maxPrice", out var maxPrice) &&
                    maxPrice.ValueKind == JsonValueKind.Number &&
                    maxPrice.TryGetDecimal(out var maxPriceValue))
                {
                    filters.MaxPrice = maxPriceValue;
                }

                if (root.TryGetProperty("minDays", out var minDays) &&
                    minDays.ValueKind == JsonValueKind.Number &&
                    minDays.TryGetInt32(out var minDaysValue))
                {
                    filters.MinDays = minDaysValue;
                }

                return filters;
            }
            catch (JsonException)
            {
                return null;
            }
        }

        private static string? ReadString(JsonElement root, string propertyName)
        {
            if (root.TryGetProperty(propertyName, out var element) &&
                element.ValueKind == JsonValueKind.String)
            {
                var value = element.GetString()?.Trim();
                return string.IsNullOrEmpty(value) ? null : value;
            }

            return null;
        }

        private static VehicleQueryFilters KeywordFallback(string query) => new()
        {
            Keyword = query.Trim()
        };
    }
}
