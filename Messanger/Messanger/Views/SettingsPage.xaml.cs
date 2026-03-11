using Messanger.ViewModels;

namespace Messanger.Views
{
    [QueryProperty(nameof(UserId), "userId")]
    public partial class SettingsPage : ContentPage
    {
        private int _userId;

        public SettingsPage()
        {
            InitializeComponent();
        }

        public int UserId
        {
            get => _userId;
            set
            {
                _userId = value;
                TryInitViewModel();
            }
        }

        private void TryInitViewModel()
        {
            if (_userId <= 0) return;
            if (BindingContext is SettingsViewModel) return;

            var vm = new SettingsViewModel(_userId);
            BindingContext = vm;

            _ = vm.InitialLadenAsync();
        }

        private async void HomeWhenClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(FriendsPage));
        }
    }
}