using System.Net.Http.Headers;
using System.Text.Json;

namespace WebApi_Server.Services
{
    public class DeeplTranslationService
    {
        private readonly HttpClient _http;

        // DeepL Free endpoint + Key (nur Backend!)
        private const string DeeplEndpoint = "https://api-free.deepl.com/v2/translate";
        private const string DeeplApiKey = "1f6273ad-e70a-4af6-a910-d0e2f0d58e1e:fx";

        public DeeplTranslationService(HttpClient http)
        {
            _http = http;
        }

        public async Task<string> TranslateAsync(string text, string targetLang)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            if (string.IsNullOrWhiteSpace(targetLang))
                targetLang = "DE";

            using var req = new HttpRequestMessage(HttpMethod.Post, DeeplEndpoint);

            req.Headers.Authorization = new AuthenticationHeaderValue("DeepL-Auth-Key", DeeplApiKey);

            req.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["text"] = text,
                ["target_lang"] = targetLang
            });

            using var res = await _http.SendAsync(req);

            var json = await res.Content.ReadAsStringAsync();

            if (!res.IsSuccessStatusCode)
                throw new Exception($"DeepL Fehler (HTTP {(int)res.StatusCode}): {json}");

            var data = JsonSerializer.Deserialize<DeeplResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var translated = data?.Translations?.FirstOrDefault()?.Text;

            return translated ?? "";
        }

        private sealed class DeeplResponse
        {
            public List<DeeplTranslation>? Translations { get; set; }
        }

        private sealed class DeeplTranslation
        {
            public string? Text { get; set; }
        }
    }
}