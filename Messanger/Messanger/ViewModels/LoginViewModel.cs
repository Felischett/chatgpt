using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Messanger.Services;
using Messanger.Views;
using Models;

namespace Messanger.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private string email = "";
        private string password = "";
        private string message = "";
        private bool isBusy;

        private readonly ApiService apiService;
        private readonly Command loginCommand;

        public event PropertyChangedEventHandler PropertyChanged;

        public LoginViewModel()
        {
            apiService = new ApiService();

            loginCommand = new Command(
                async () => await LoginAsync(),
                () => !IsBusy);

            Message = string.Empty;
        }

        public string Email
        {
            get => email;
            set
            {
                if (email == value) return;
                email = value;
                OnPropertyChanged();
            }
        }

        public string Password
        {
            get => password;
            set
            {
                if (password == value) return;
                password = value;
                OnPropertyChanged();
            }
        }

        public string Message
        {
            get => message;
            set
            {
                if (message == value) return;
                message = value;
                OnPropertyChanged();
            }
        }

        public bool IsBusy
        {
            get => isBusy;
            set
            {
                if (isBusy == value) return;
                isBusy = value;
                OnPropertyChanged();
                loginCommand.ChangeCanExecute();
            }
        }

        public ICommand LoginCommand => loginCommand;

        private async Task LoginAsync()
        {
            if (IsBusy) return;

            Message = string.Empty;

            if (string.IsNullOrWhiteSpace(Email))
            {
                Message = "Bitte E-Mail eingeben.";
                return;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                Message = "Bitte Passwort eingeben.";
                return;
            }

            try
            {
                IsBusy = true;

                User? user = await apiService.LoginAsync(Email, Password);

                if (user == null)
                {
                    Message = "Login fehlgeschlagen.";
                    return;
                }

                var url =
                    $"{nameof(FriendsPage)}" +
                    $"?userId={user.Id}" +
                    $"&username={Uri.EscapeDataString(user.Username)}" +
                    $"&email={Uri.EscapeDataString(user.Email)}";

                await Shell.Current.GoToAsync(url);
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

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
