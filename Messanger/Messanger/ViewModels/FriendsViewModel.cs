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
    public class FriendsViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;

        private readonly Command _refreshCommand;
        private readonly Command _freundHinzufuegenCommand;
        private readonly Command<User> _anfrageAnnehmenCommand;
        private readonly Command<User> _anfrageAblehnenCommand;
        private readonly Command<User> _openChatCommand;

        private string _freundKeyEingabe = "";
        private string _message = "";
        private bool _isBusy;

        public event PropertyChangedEventHandler PropertyChanged;

        public int UserId { get; }
        public string Username { get; }
        public string Email { get; }

        public ObservableCollection<User> Freunde { get; } = new ObservableCollection<User>();
        public ObservableCollection<User> Anfragen { get; } = new ObservableCollection<User>();

        public FriendsViewModel(int userId, string username, string email)
        {
            _apiService = new ApiService();

            UserId = userId;
            Username = username;
            Email = email;

            _refreshCommand = new Command(async () => await RefreshAsync(), () => !IsBusy);

            _freundHinzufuegenCommand = new Command(async () => await FreundHinzufuegenAsync(), () => !IsBusy);

            _anfrageAnnehmenCommand = new Command<User>(
                async (u) => await AnfrageAnnehmenAsync(u),
                (u) => !IsBusy);

            _anfrageAblehnenCommand = new Command<User>(
                async (u) => await AnfrageAblehnenAsync(u),
                (u) => !IsBusy);

            _openChatCommand = new Command<User>(
                async (u) => await OpenChatAsync(u),
                (u) => !IsBusy);
        }

        public string AngemeldeterAnzeigeText => $"{Username} ({Email})";

        public string FreundKeyEingabe
        {
            get => _freundKeyEingabe;
            set
            {
                if (_freundKeyEingabe == value) return;
                _freundKeyEingabe = value;
                OnPropertyChanged();
            }
        }

        public string Message
        {
            get => _message;
            set
            {
                if (_message == value) return;
                _message = value;
                OnPropertyChanged();
            }
        }

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (_isBusy == value) return;

                _isBusy = value;
                OnPropertyChanged();

                _refreshCommand.ChangeCanExecute();
                _freundHinzufuegenCommand.ChangeCanExecute();
                _anfrageAnnehmenCommand.ChangeCanExecute();
                _anfrageAblehnenCommand.ChangeCanExecute();
                _openChatCommand.ChangeCanExecute();
            }
        }

        public ICommand RefreshCommand => _refreshCommand;
        public ICommand FreundHinzufuegenCommand => _freundHinzufuegenCommand;
        public ICommand AnfrageAnnehmenCommand => _anfrageAnnehmenCommand;
        public ICommand AnfrageAblehnenCommand => _anfrageAblehnenCommand;
        public ICommand OpenChatCommand => _openChatCommand;

        public Task InitialLadenAsync() => RefreshAsync();

        private async Task RefreshAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                Message = "";

                await RefreshCoreAsync();
            }
            catch (Exception ex)
            {
                Message = "Fehler: " + ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task RefreshCoreAsync()
        {
            var freundeListe = await _apiService.HoleFreundeAsync(UserId);
            var anfragenListe = await _apiService.HoleAnfragenAsync(UserId);

            Freunde.Clear();
            foreach (var f in freundeListe)
                Freunde.Add(f);

            Anfragen.Clear();
            foreach (var a in anfragenListe)
                Anfragen.Add(a);
        }

        private async Task FreundHinzufuegenAsync()
        {
            if (IsBusy) return;

            Message = "";

            if (string.IsNullOrWhiteSpace(FreundKeyEingabe))
            {
                Message = "Bitte Freund-Key eingeben.";
                return;
            }

            try
            {
                IsBusy = true;

                await _apiService.SendeFreundschaftsanfrageAsync(UserId, FreundKeyEingabe.Trim());

                FreundKeyEingabe = "";
                Message = "Anfrage gesendet.";

                await RefreshCoreAsync();
            }
            catch (Exception ex)
            {
                Message = "Fehler: " + ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task AnfrageAnnehmenAsync(User user)
        {
            if (IsBusy) return;
            if (user == null) return;

            try
            {
                IsBusy = true;
                Message = "";

                await _apiService.BestaetigeFreundschaftAsync(UserId, user.Id);

                await RefreshCoreAsync();
            }
            catch (Exception ex)
            {
                Message = "Fehler: " + ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task AnfrageAblehnenAsync(User user)
        {
            if (IsBusy) return;
            if (user == null) return;

            try
            {
                IsBusy = true;
                Message = "";

                await _apiService.LehneFreundschaftAsync(UserId, user.Id);

                await RefreshCoreAsync();
            }
            catch (Exception ex)
            {
                Message = "Fehler: " + ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task OpenChatAsync(User friend)
        {
            if (friend == null) return;

            var url =
                $"ChatPage" +
                $"?userId={UserId}" +
                $"&username={Uri.EscapeDataString(Username)}" +
                $"&email={Uri.EscapeDataString(Email)}" +
                $"&friendUserId={friend.Id}" +
                $"&friendUsername={Uri.EscapeDataString(friend.Username ?? "")}";

            await Shell.Current.GoToAsync(url);
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
