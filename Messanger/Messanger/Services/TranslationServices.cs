using System.Net.Http.Json;
using System.Text.Json;

namespace Messanger.Services
{
    public class TranslationService
    {
        private readonly HttpClient _http;

        public TranslationService(HttpClient http)
        {
            _http = http;
        }

        public async Task<string> TranslateAsync(string text, string targetLang)
        {
            if (string.IsNullOrWhiteSpace(text))
                return "";

            if (string.IsNullOrWhiteSpace(targetLang))
                targetLang = "DE";

            var payload = new TranslateRequest
            {
                Text = text,
                TargetLang = targetLang
            };

            var res = await _http.PostAsJsonAsync("api/translate", payload);

            var json = await res.Content.ReadAsStringAsync();

            if (!res.IsSuccessStatusCode)
                throw new Exception($"Translate Backend Fehler (HTTP {(int)res.StatusCode}): {json}");

            var data = JsonSerializer.Deserialize<TranslateResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return data?.TranslatedText ?? "";
        }

        private sealed class TranslateRequest
        {
            public string Text { get; set; } = "";
            public string TargetLang { get; set; } = "DE";
        }

        private sealed class TranslateResponse
        {
            public string TranslatedText { get; set; } = "";
        }
    }
}