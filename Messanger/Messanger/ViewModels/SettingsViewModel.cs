using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Messanger.Services;

namespace Messanger.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _api;

        private string _targetLang = "DE";
        private string _message = "";
        private bool _isBusy;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int UserId { get; }

        public SettingsViewModel(int userId)
        {
            UserId = userId;
            _api = new ApiService();

            SaveCommand = new Command(async () => await SaveAsync(), () => !IsBusy);
        }

        public string TargetLang
        {
            get => _targetLang;
            set
            {
                if (_targetLang == value) return;
                _targetLang = value;
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
                ((Command)SaveCommand).ChangeCanExecute();
            }
        }

        public ICommand SaveCommand { get; }

        public async Task InitialLadenAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                Message = "";

                var s = await _api.HoleUserSettingsAsync(UserId);
                TargetLang = (s.TargetLang ?? "DE").Trim().ToUpper();
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

        private async Task SaveAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                Message = "";

                var saved = await _api.SpeichereUserSettingsAsync(UserId, TargetLang);
                TargetLang = (saved.TargetLang ?? "DE").Trim().ToUpper();

                Message = "Gespeichert!";
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

        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}