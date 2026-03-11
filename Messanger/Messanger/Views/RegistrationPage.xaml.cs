using Messanger.ViewModels;

namespace Messanger.Views
{
    public partial class RegistrationPage : ContentPage
    {
        public RegistrationPage()
        {
            InitializeComponent();
            BindingContext = new RegistrationViewModel();
        }

        private void OnUsernameCompleted(object sender, EventArgs e)
        {
            EmailEntry.Focus();
        }

        private void OnEmailCompleted(object sender, EventArgs e)
        {
            PasswordEntry.Focus();
        }

        private void OnPasswordCompleted(object sender, EventArgs e)
        {
            if (BindingContext is RegistrationViewModel vm && vm.RegisterCommand.CanExecute(null))
            {
                vm.RegisterCommand.Execute(null);
            }
        }

        private async void OnLoginTapped(object sender, EventArgs e)
        {
            // Zurück zur LoginPage (pop)
            await Shell.Current.GoToAsync("..");
        }
    }
}
