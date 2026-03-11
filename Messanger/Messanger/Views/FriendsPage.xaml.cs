using Messanger.ViewModels;

namespace Messanger.Views
{
    [QueryProperty(nameof(UserId), "userId")]
    [QueryProperty(nameof(Username), "username")]
    [QueryProperty(nameof(Email), "email")]
    public partial class FriendsPage : ContentPage
    {
        private int _userId;
        private string _username;
        private string _email;

        public FriendsPage()
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

        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                TryInitViewModel();
            }
        }

        public string Email
        {
            get => _email;
            set
            {
                _email = value;
                TryInitViewModel();
            }
        }

        private void TryInitViewModel()
        {
            if (_userId <= 0) return;
            if (string.IsNullOrWhiteSpace(_username)) return;
            if (string.IsNullOrWhiteSpace(_email)) return;

            if (BindingContext is FriendsViewModel) return;

            var vm = new FriendsViewModel(_userId, _username, _email);
            BindingContext = vm;

            _ = vm.InitialLadenAsync();
        }

        private async void OnSettingsClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync($"{nameof(SettingsPage)}?userId={UserId}");
        }
    }
}