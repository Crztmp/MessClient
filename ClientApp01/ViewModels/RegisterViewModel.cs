using ClientApp01.Models;
using ClientApp01.Services;
using ClientApp01.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Security.Principal;
using System.DirectoryServices.AccountManagement;

namespace ClientApp01.ViewModels
{
    public partial class RegisterViewModel : ObservableObject
    {
        public class RegistrationType
        {
            public string DisplayName { get; set; }
            public int Type { get; set; } // 0 - Regular, 1 - AD, 2 - Current Windows
        }

        private readonly AuthService _authService;
        private readonly INavigationService _navigationService;

        [ObservableProperty]
        private string _username;

        [ObservableProperty]
        private string _password;

        [ObservableProperty]
        private string _confirmPassword;

        [ObservableProperty]
        private string _email;

        [ObservableProperty]
        private string _firstName;

        [ObservableProperty]
        private string _lastName;

        [ObservableProperty]
        private string _errorMessage;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private bool _isUsernameEnabled = true;

        [ObservableProperty]
        private bool _showPasswordFields = true;

        [ObservableProperty]
        private RegistrationType _selectedRegistrationType;
        [ObservableProperty]
        private string _adPassword;

        [ObservableProperty]
        private bool _showRegularPasswordFields = true;

        [ObservableProperty]
        private bool _showADPasswordField = false;

        public ObservableCollection<RegistrationType> RegistrationTypes { get; } = new ObservableCollection<RegistrationType>
        {
            new RegistrationType { DisplayName = "Обычный пользователь", Type = 0 },
            new RegistrationType { DisplayName = "Доменный пользователь AD", Type = 1 },
            new RegistrationType { DisplayName = "Текущий пользователь Windows", Type = 2 }
        };

        public RegisterViewModel(AuthService authService, INavigationService navigationService)
        {
            _authService = authService;
            _navigationService = navigationService;
            SelectedRegistrationType = RegistrationTypes[0];
        }

        partial void OnSelectedRegistrationTypeChanged(RegistrationType value)
        {
            switch (value.Type)
            {
                case 0: // Regular
                    IsUsernameEnabled = true;
                    ShowRegularPasswordFields = true;
                    ShowADPasswordField = false;
                    break;
                case 1: // AD
                    IsUsernameEnabled = true;
                    ShowRegularPasswordFields = false;
                    ShowADPasswordField = true;
                    break;
                case 2: // Current Windows
                    IsUsernameEnabled = false;
                    ShowRegularPasswordFields = false;
                    ShowADPasswordField = false;
                    SetCurrentWindowsUser();
                    break;
            }
        }

        private void SetCurrentWindowsUser()
        {
            try
            {
                var currentUser = WindowsIdentity.GetCurrent();
                Username = currentUser.Name;
            }
            catch
            {
                ErrorMessage = "Не удалось получить текущего пользователя Windows";
            }
        }

        [RelayCommand]
        private async Task RegisterAsync()
        {
            if (SelectedRegistrationType.Type == 0 && Password != ConfirmPassword)
            {
                ErrorMessage = "Пароли не совпадают";
                return;
            }
            if (SelectedRegistrationType.Type == 1 && string.IsNullOrEmpty(AdPassword))
            {
                ErrorMessage = "Введите пароль AD";
                return;
            }
            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                AuthResponse result;
                switch (SelectedRegistrationType.Type)
                {
                    case 0: // Regular
                        result = await _authService.RegisterAsync(
                            Username, Password, Email, FirstName, LastName);
                        break;
                    case 1: // AD
                        result = await _authService.RegisterADUserAsync(
                            Username, AdPassword, Email, FirstName, LastName);
                        break;
                    case 2: // Current Windows
                        result = await _authService.RegisterWindowsUserAsync(
                            Username, Email, FirstName, LastName);
                        break;
                    default:
                        result = new AuthResponse { Error = "Неизвестный тип регистрации" };
                        //ClientApp01.Services.AuthResponse
                        break;
                }

                if (result.Success)
                {
                    //_navigationService.NavigateToMainWindow();
                    var currentWindowClose = new CurrentWindowCloseDto
                    {
                        Close = true
                    };
                    _navigationService.NavigateToMainWindow(currentWindowClose);
                }
                else
                {
                    ErrorMessage = result.Error ?? "Ошибка регистрации";
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void NavigateToLogin()
        {
            var currentWindowClose = new CurrentWindowCloseDto
            {
                Close = true
            };
            _navigationService.NavigateToLogin(currentWindowClose);
            //var loginWindow = new LoginWindow();
            //loginWindow.Show();
            //Application.Current.Windows[0]?.Close();
        }
    }
}