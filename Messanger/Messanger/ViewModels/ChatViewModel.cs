using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Messanger.Services;
using Microsoft.Maui.Controls;
using Models;

namespace Messanger.ViewModels
{
    public class ChatViewModel : INotifyPropertyChanged
    {
        // Diese Felder müssen definiert sein:
        private readonly ApiService _apiService;
        private string _textEingabe = "";
        private string _message = "";
        private bool _isBusy;
        private string _targetLang = "DE";

        public event PropertyChangedEventHandler PropertyChanged;

        public int UserId { get; }
        public string Username { get; }
        public string Email { get; }
        public int FriendUserId { get; }
        public string FriendUsername { get; }

        public ObservableCollection<Models.Messages> ChatMessages { get; } = new ObservableCollection<Models.Messages>();

        public ChatViewModel(int userId, string username, string email, int friendUserId, string friendUsername)
        {
            _apiService = new ApiService(); // Initialisierung des Services
            UserId = userId;
            Username = username;
            Email = email;
            FriendUserId = friendUserId;
            FriendUsername = friendUsername;

            SendCommand = new Command(async () => await SendAsync(), () => !IsBusy);
            TranslateCommand = new Command<Models.Messages>(async (m) => await TranslateAsync(m), (m) => !IsBusy && m != null);
        }

        public ICommand SendCommand { get; }
        public ICommand TranslateCommand { get; }
        public string TitleText => $"Chat mit {FriendUsername}";

        public string TextEingabe
        {
            get => _textEingabe;
            set { _textEingabe = value; OnPropertyChanged(); }
        }

        public string Message
        {
            get => _message;
            set { _message = value; OnPropertyChanged(); }
        }

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnPropertyChanged();
                ((Command)SendCommand).ChangeCanExecute();
                ((Command<Models.Messages>)TranslateCommand).ChangeCanExecute();
            }
        }

        public async Task InitialLadenAsync()
        {
            await LoadSettingsAsync();
            await RefreshCoreAsync();
        }

        private async Task LoadSettingsAsync()
        {
            try
            {
                var s = await _apiService.HoleUserSettingsAsync(UserId);
                _targetLang = (s?.TargetLang ?? "DE").Trim().ToUpper();
            }
            catch { _targetLang = "DE"; }
        }

        // DEIN CODE AUS DEM SCREENSHOT (JETZT KORRIGIERT)
        private async Task RefreshCoreAsync()
        {
            try
            {
                var list = await _apiService.HoleChatAsync(UserId, FriendUserId, 200);
                ChatMessages.Clear();
                foreach (var m in list)
                {
                    m.TranslatedText = "";
                    // Logik: Wenn die SenderId meine eigene UserId ist, ist es meine Nachricht
                    m.IsOwnMessage = (m.SenderId == UserId);
                    ChatMessages.Add(m);
                }
            }
            catch (Exception ex)
            {
                Message = "Fehler beim Laden: " + ex.Message;
            }
        }

        private async Task SendAsync()
        {
            if (IsBusy) return;
            var text = TextEingabe?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(text)) return;
            try
            {
                IsBusy = true;
                await _apiService.SendeMessageAsync(UserId, FriendUserId, text);
                TextEingabe = "";
                await RefreshCoreAsync();
            }
            catch (Exception ex) { Message = "Fehler: " + ex.Message; }
            finally { IsBusy = false; }
        }

        private async Task TranslateAsync(Models.Messages msg)
        {
            if (IsBusy || msg == null) return;
            try
            {
                IsBusy = true;
                var translated = await _apiService.TranslateViaBackendAsync(msg.Message, _targetLang);
                msg.TranslatedText = translated;

                int index = ChatMessages.IndexOf(msg);
                if (index >= 0) ChatMessages[index] = msg;
            }
            catch (Exception ex) { Message = "Fehler: " + ex.Message; }
            finally { IsBusy = false; }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}