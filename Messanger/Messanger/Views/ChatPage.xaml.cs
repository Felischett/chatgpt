using Messanger.ViewModels;

namespace Messanger.Views
{
    [QueryProperty(nameof(UserId), "userId")]
    [QueryProperty(nameof(Username), "username")]
    [QueryProperty(nameof(Email), "email")]
    [QueryProperty(nameof(FriendUserId), "friendUserId")]
    [QueryProperty(nameof(FriendUsername), "friendUsername")]
    public partial class ChatPage : ContentPage
    {
        private int _userId;
        private string _username;
        private string _email;

        private int _friendUserId;
        private string _friendUsername;

        public ChatPage()
        {
            InitializeComponent();
        }

        public int UserId
        {
            get => _userId;
            set { _userId = value; TryInit(); }
        }

        public string Username
        {
            get => _username;
            set { _username = value; TryInit(); }
        }

        public string Email
        {
            get => _email;
            set { _email = value; TryInit(); }
        }

        public int FriendUserId
        {
            get => _friendUserId;
            set { _friendUserId = value; TryInit(); }
        }

        public string FriendUsername
        {
            get => _friendUsername;
            set { _friendUsername = value; TryInit(); }
        }

        private void TryInit()
        {
            if (_userId <= 0) return;
            if (string.IsNullOrWhiteSpace(_username)) return;
            if (string.IsNullOrWhiteSpace(_email)) return;

            if (_friendUserId <= 0) return;
            if (string.IsNullOrWhiteSpace(_friendUsername)) return;

            if (BindingContext is ChatViewModel) return;

            var vm = new ChatViewModel(_userId, _username, _email, _friendUserId, _friendUsername);
            BindingContext = vm;

            _ = vm.InitialLadenAsync();
        }

        private async void OnFreundeClicked(object sender, EventArgs e)
        {
            if (_userId <= 0) return;

            var url =
                $"FriendsPage" +
                $"?userId={_userId}" +
                $"&username={Uri.EscapeDataString(_username ?? "")}" +
                $"&email={Uri.EscapeDataString(_email ?? "")}";

            await Shell.Current.GoToAsync(url);
        }
    }
}
