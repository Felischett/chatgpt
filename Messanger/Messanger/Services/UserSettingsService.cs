using System.Net.Http.Json;
using Models;

namespace Messanger.Services
{
    public class UserSettingsService
    {
        private readonly HttpClient _http;

        public UserSettingsService(HttpClient http)
        {
            _http = http;
        }

        public async Task<UserSettings> GetAsync(int userId)
        {
            var res = await _http.GetAsync($"api/settings/{userId}");
            var json = await res.Content.ReadAsStringAsync();

            if (!res.IsSuccessStatusCode)
                throw new Exception($"Settings laden fehlgeschlagen (HTTP {(int)res.StatusCode}): {json}");

            var settings = await res.Content.ReadFromJsonAsync<UserSettings>();
            return settings ?? new UserSettings { UserId = userId, TargetLang = "DE" };
        }

        public async Task<UserSettings> SaveAsync(int userId, string targetLang)
        {
            var payload = new { targetLang = targetLang };

            var res = await _http.PutAsJsonAsync($"api/settings/{userId}", payload);
            var json = await res.Content.ReadAsStringAsync();

            if (!res.IsSuccessStatusCode)
                throw new Exception($"Settings speichern fehlgeschlagen (HTTP {(int)res.StatusCode}): {json}");

            var settings = await res.Content.ReadFromJsonAsync<UserSettings>();
            return settings ?? new UserSettings { UserId = userId, TargetLang = targetLang };
        }
    }
}