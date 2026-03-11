using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Models;

namespace Messanger.Services
{
    public class ApiService
    {
        private readonly HttpClient client;
        private const string baseUrl = "https://localhost:7246/";

        public ApiService()
        {
            client = new HttpClient { BaseAddress = new Uri(baseUrl) };
        }

        public async Task<bool> RegisterAsync(string username, string email, string password)
        {
            var payload = new { Username = username, Email = email, Password = password };
            var response = await client.PostAsJsonAsync("api/Users/register", payload);

            if (response.IsSuccessStatusCode) return true;

            var statusCode = (int)response.StatusCode;
            var reason = response.ReasonPhrase ?? "";
            var content = await response.Content.ReadAsStringAsync();

            throw new Exception($"Registrierung fehlgeschlagen (HTTP {statusCode} {reason}). Server-Antwort: {content}");
        }

        // NEU: Login nur mit Email + Passwort
        public async Task<User?> LoginAsync(string email, string password)
        {
            var payload = new { Email = email, Password = password };
            var response = await client.PostAsJsonAsync("api/Users/login", payload);

            if (!response.IsSuccessStatusCode)
            {
                var statusCode = (int)response.StatusCode;
                var reason = response.ReasonPhrase ?? "";
                var content = await response.Content.ReadAsStringAsync();
                throw new Exception($"Login fehlgeschlagen (HTTP {statusCode} {reason}). Server-Antwort: {content}");
            }

            return await response.Content.ReadFromJsonAsync<User>();
        }

        // ALT (falls wo anders noch verwendet) -> leitet auf neuen Login um
        public Task<User?> LoginAsync(string username, string email, string password)
        {
            return LoginAsync(email, password);
        }

        public async Task<List<User>> HoleFreundeAsync(int userId)
        {
            var response = await client.GetAsync($"api/Friends/{userId}/accepted");
            if (!response.IsSuccessStatusCode) throw await baueApiExceptionAsync("Freunde laden fehlgeschlagen", response);

            return await response.Content.ReadFromJsonAsync<List<User>>() ?? new List<User>();
        }

        public async Task<List<User>> HoleAnfragenAsync(int userId)
        {
            var response = await client.GetAsync($"api/Friends/{userId}/requests");
            if (!response.IsSuccessStatusCode) throw await baueApiExceptionAsync("Anfragen laden fehlgeschlagen", response);

            return await response.Content.ReadFromJsonAsync<List<User>>() ?? new List<User>();
        }

        public async Task SendeFreundschaftsanfrageAsync(int userId, string freundKey)
        {
            var payload = new { UserId = userId, FriendKey = freundKey };
            var response = await client.PostAsJsonAsync("api/Friends/request", payload);

            if (!response.IsSuccessStatusCode) throw await baueApiExceptionAsync("Freundschaftsanfrage fehlgeschlagen", response);
        }

        public async Task BestaetigeFreundschaftAsync(int userId, int friendUserId)
        {
            var payload = new { UserId = userId, FriendUserId = friendUserId };
            var response = await client.PostAsJsonAsync("api/Friends/accept", payload);

            if (!response.IsSuccessStatusCode) throw await baueApiExceptionAsync("Freundschaft bestätigen fehlgeschlagen", response);
        }

        public async Task LehneFreundschaftAsync(int userId, int friendUserId)
        {
            var payload = new { UserId = userId, FriendUserId = friendUserId };
            var response = await client.PostAsJsonAsync("api/Friends/reject", payload);

            if (!response.IsSuccessStatusCode) throw await baueApiExceptionAsync("Freundschaft ablehnen fehlgeschlagen", response);
        }

        public async Task<List<Models.Messages>> HoleChatAsync(int userId, int friendUserId, int take = 200)
        {
            var route = $"api/Messages/conversation?userId={userId}&friendUserId={friendUserId}&take={take}";
            var response = await client.GetAsync(route);

            if (!response.IsSuccessStatusCode) throw await baueApiExceptionAsync("Chat laden fehlgeschlagen", response);

            return await response.Content.ReadFromJsonAsync<List<Models.Messages>>() ?? new List<Models.Messages>();
        }

        public async Task SendeMessageAsync(int senderId, int empfaengerId, string text)
        {
            var payload = new Models.Messages
            {
                SenderId = senderId,
                EmpfaengerId = empfaengerId,
                Message = text
            };

            var response = await client.PostAsJsonAsync("api/Messages/send", payload);

            if (!response.IsSuccessStatusCode) throw await baueApiExceptionAsync("Message senden fehlgeschlagen", response);
        }

        private async Task<Exception> baueApiExceptionAsync(string titel, HttpResponseMessage response)
        {
            var statusCode = (int)response.StatusCode;
            var reason = response.ReasonPhrase ?? "";
            var content = await response.Content.ReadAsStringAsync();

            return new Exception($"{titel} (HTTP {statusCode} {reason}). Server-Antwort: {content}");
        }

        public async Task<UserSettings> HoleUserSettingsAsync(int userId)
        {
            var response = await client.GetAsync($"api/Settings/{userId}");
            if (!response.IsSuccessStatusCode) throw await baueApiExceptionAsync("Settings laden fehlgeschlagen", response);

            return await response.Content.ReadFromJsonAsync<UserSettings>()
                   ?? new UserSettings { UserId = userId, TargetLang = "DE" };
        }

        public async Task<UserSettings> SpeichereUserSettingsAsync(int userId, string targetLang)
        {
            var payload = new { targetLang = targetLang };
            var response = await client.PutAsJsonAsync($"api/Settings/{userId}", payload);
            if (!response.IsSuccessStatusCode) throw await baueApiExceptionAsync("Settings speichern fehlgeschlagen", response);

            return await response.Content.ReadFromJsonAsync<UserSettings>()
                   ?? new UserSettings { UserId = userId, TargetLang = (targetLang ?? "DE") };
        }

        public async Task<string> TranslateViaBackendAsync(string text, string targetLang)
        {
            var payload = new { text = text, targetLang = targetLang };
            var response = await client.PostAsJsonAsync("api/translate", payload);

            if (!response.IsSuccessStatusCode) throw await baueApiExceptionAsync("Übersetzen fehlgeschlagen", response);

            var result = await response.Content.ReadFromJsonAsync<TranslateResponse>();
            return result?.TranslatedText ?? "";
        }

        private class TranslateResponse
        {
            public string TranslatedText { get; set; } = "";
        }
    }
}
