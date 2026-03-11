using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Messanger.Services;
using Microsoft.Maui.Controls;

namespace Messanger.ViewModels
{
    public class RegistrationViewModel : INotifyPropertyChanged
    {
        private string username;
        private string email;
        private string password;
        private string passwordRepeat;
        private string message;
        private bool isBusy;

        private readonly ApiService apiService;
        private readonly Command registerCommand;

        public event PropertyChangedEventHandler PropertyChanged;

        public RegistrationViewModel()
        {
            apiService = new ApiService();

            registerCommand = new Command(
              async () => await RegisterAsync(),
              () => !IsBusy);

            Message = string.Empty;
        }

        public string Username
        {
            get => username;
            set {
                if (username == value)
                {
                    return;
                }

                username = value;
                OnPropertyChanged();
            }
        }

        public string Email
        {
            get => email;
            set
            {
                if (email == value)
                {
                    return;
                }
                email = value;
                OnPropertyChanged();
            }
        }

        public string Password
        {
            get => password;
            set
            {
                if (password == value)
                {
                    return;
                }

                password = value;
                OnPropertyChanged();
            }
        }

        public string PasswordRepeat
        {
            get => passwordRepeat;
            set
            {
                if (passwordRepeat == value)
                {
                    return;
                }

                passwordRepeat = value;
                OnPropertyChanged();
            }
        }

        public string Message
        {
            get => message;
            set
            {
                if (message == value)
                {
                    return;
                }

                message = value;
                OnPropertyChanged();
            }
        }

        public bool IsBusy
        {
            get => isBusy;
            set
            {
                if (isBusy == value)
                {
                    return;
                }

                isBusy = value;
                OnPropertyChanged();
                registerCommand.ChangeCanExecute();
            }
        }

        public ICommand RegisterCommand => registerCommand;

        private async Task RegisterAsync()
        {
            if (IsBusy)
            {
                return;
            }

            Message = string.Empty;

            if (string.IsNullOrWhiteSpace(Username))
            {
                Message = "Bitte einen Benutzernamen eingeben.";
                return;
            }

            if (string.IsNullOrWhiteSpace(Email))
            {
                Message = "Bitte eine E-Mail-Adresse eingeben.";
                return;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                Message = "Bitte ein Passwort eingeben.";
                return;
            }

            if (Password != PasswordRepeat)
            {
                Message = "Die Passwörter stimmen nicht überein.";
                return;
            }

            try
            {
                IsBusy = true;

                var success =
                  await apiService.RegisterAsync(Username, Email, Password);

                if (success)
                {
                    Message = "Registrierung erfolgreich. ";

                    Password = string.Empty;
                    PasswordRepeat = string.Empty;
                }
                else
                {
                    Message = "Registrierung fehlgeschlagen. ";
                }
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

        private void OnPropertyChanged(
          [CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(
              this,
              new PropertyChangedEventArgs(propertyName));
        }
    }
}
