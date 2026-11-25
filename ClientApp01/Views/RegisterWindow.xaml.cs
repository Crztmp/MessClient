using System.Windows;

namespace ClientApp01.Views
{
    public partial class RegisterWindow : Window
    {
        public RegisterWindow()
        {
            InitializeComponent();
            PasswordBox.PasswordChanged += PasswordBox_PasswordChanged;
            ConfirmPasswordBox.PasswordChanged += ConfirmPasswordBox_PasswordChanged;
            AdPasswordBox.PasswordChanged += AdPasswordBox_PasswordChanged;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModels.RegisterViewModel vm)
            {
                vm.Password = PasswordBox.Password;
            }
        }
        private void AdPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModels.RegisterViewModel vm)
            {
                vm.AdPassword = AdPasswordBox.Password;
            }
        }
        private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModels.RegisterViewModel vm)
            {
                vm.ConfirmPassword = ConfirmPasswordBox.Password;
            }
        }
    }
}