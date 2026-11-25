using ClientApp01.Models;
using ClientApp01.Services;
using ClientApp01.Views;
using ClientApp01.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.Logging;
using Microsoft.Build.Framework;
using System.Security.Principal;
using System.DirectoryServices.AccountManagement;
using System.Windows.Navigation;


namespace ClientApp01.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly AuthService _authService;
        private readonly ILogger<LoginViewModel> _logger;
        private readonly AppState _appState;
        private readonly INavigationService _navigationService;


        [ObservableProperty]
        private string _username;

        [ObservableProperty]
        private string _password;

        [ObservableProperty]
        private bool _isLoading;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasError))]
        private string _errorMessage;
        [ObservableProperty]
        private bool _useCurrentWindowsUser;

        [ObservableProperty]
        private bool _isManualLoginEnabled = true;
        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        public LoginViewModel(
        AuthService authService,
        ILogger<LoginViewModel> logger,
        AppState appState,
        INavigationService navigationService)
        {
            _authService = authService;
            _logger = logger;
            _appState = appState;
            _navigationService = navigationService;
        }
        [RelayCommand]
        private void NavigateToRegister()
        {

            var currentWindowClose = new CurrentWindowCloseDto
            {
                Close = true
            };
            _navigationService.NavigateToRegister(currentWindowClose);

            //Application.Current.MainWindow.Hide();
            //_logger.LogInformation("NavigateToRegister");
            //var registerWindow = new RegisterWindow();
            //_logger.LogInformation("NavigateToRegister2");
            //registerWindow.Show();
            //_logger.LogInformation("NavigateToRegister3");
        }
        [RelayCommand]
        private void OpenSettings()
        {
            var settingsWindow = new LoginSettingsWindow();
            settingsWindow.Owner = Application.Current.MainWindow;
            settingsWindow.ShowDialog();
        }
        [RelayCommand]
        private void CheckCurrentUser()
        {
            if (!UseCurrentWindowsUser)
            {
                IsManualLoginEnabled = true;
                return;
            }

            try
            {
                // Check if current user is a domain user
                using (var context = new PrincipalContext(ContextType.Domain))
                {
                    var currentUser = WindowsIdentity.GetCurrent();
                    var domainUser = UserPrincipal.FindByIdentity(context, currentUser.Name);

                    if (domainUser == null)
                    {
                        ErrorMessage = "Текущий пользователь не является доменным";
                        UseCurrentWindowsUser = false;
                        IsManualLoginEnabled = true;
                        return;
                    }

                    // Set the username and disable manual input
                    Username = currentUser.Name;
                    IsManualLoginEnabled = false;
                    ErrorMessage = string.Empty;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking current Windows user");
                ErrorMessage = "Ошибка при проверке текущего пользователя Windows";
                UseCurrentWindowsUser = false;
                IsManualLoginEnabled = true;
            }
        }
        [RelayCommand]
        private async Task LoginAsync()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            //_logger = logger;

            try
            {
                // Сначала проверяем доступность сервера
                _logger.LogInformation("CrzC_201");
                var pingResult = await _authService.PingServerAsync();
                if (!pingResult)
                {
                    ErrorMessage = "Сервер не доступен";
                    IsLoading = false;
                    return;
                }

                AuthResponse result;

                if (UseCurrentWindowsUser)
                {
                    // Для текущего пользователя Windows
                    _logger.LogInformation("CrzC_2031 - Windows integrated login");
                    result = await _authService.LoginWindowsIntegratedAsync(Username);

                    if (result.Success && _appState.CurrentUser != null)
                    {
                        _appState.CurrentUser.IsActiveDirectory = true;
                    }
                }
                else
                {
                    // Проверяем, является ли пользователь доменным
                    _logger.LogInformation("CrzC_202");
                    var userInfo = await _authService.CheckUserTypeAsync(Username);

                    _logger.LogInformation("CrzC_203");
                    AuthResponse authResponse;
                    if (userInfo.IsActiveDirectory)
                    {
                        // Для AD пользователей
                        _logger.LogInformation("CrzC_2031");
                        authResponse = await _authService.LoginADAsync(Username, Password);
                        if (authResponse.Success && _appState.CurrentUser != null)
                        {
                            _appState.CurrentUser.IsActiveDirectory = true;
                        }
                    }

                    else
                    {
                        // Для обычных пользователей
                        _logger.LogInformation("CrzC_2032");
                        authResponse = await _authService.LoginAsync(Username, Password);
                    }
                    result = authResponse;
                }
                _logger.LogInformation("CrzC_204");
                if (result.Success)
                {
                    var currentWindowClose = new CurrentWindowCloseDto
                    {
                        Close = true
                    };
                    _navigationService.NavigateToMainWindow(currentWindowClose);

                    //Application.Current.MainWindow.Hide();
                    //var mainWindow = new MainWindow();
                    //Application.Current.MainWindow = mainWindow;
                    //mainWindow.Show();
                    //foreach (Window window in Application.Current.Windows)
                    //{
                    //    if (window is LoginWindow)
                    //    {
                    //        window.Close();
                    //        break;
                    //    }
                    //}
                }
                else
                {
                    _logger.LogInformation("CrzC_2042");
                    ErrorMessage = result.Error ?? "Ошибка входа";
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation("CrzC_210");
                ErrorMessage = $"Ошибка: {ex.Message}";
            }
            finally
            {
                _logger.LogInformation("CrzC_220");
                IsLoading = false;
            }
        }
    }
}