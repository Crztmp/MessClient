using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ClientApp01.Models;
using ClientApp01.Services;
using System.Collections.ObjectModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ClientApp01.ViewModels;
using System.Diagnostics;
using CommunityToolkit.Mvvm.Input;

namespace ClientApp01.Views
{
    public partial class MainWindow : Window
    {
        private readonly ApiService _apiService;
        private readonly AppState _appState;
        private readonly ILogger<MainWindow> _logger;

        public MainWindow()
        {
            InitializeComponent();

            // Получаем сервисы из контейнера зависимостей
            _apiService = App.Services.GetRequiredService<ApiService>();
            _appState = App.Services.GetRequiredService<AppState>();
            _logger = App.Services.GetRequiredService<ILogger<MainWindow>>();

            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
            Loaded += (s, e) => Debug.WriteLine($"DataContext is {DataContext?.GetType().Name}");
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Загружаем данные пользователя
            await LoadUserDataAsync();
        }
        private async void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Вызываем Logout через ViewModel
            if (DataContext is MainViewModel vm)
            {
                vm.LogoutAddClose();
            }

            // Закрываем все окна приложения
            Application.Current.Shutdown();
        }

        private async Task LoadUserDataAsync()
        {
            if (_appState.CurrentUser != null)
            {
                // Обновляем отображение текущего пользователя
                if (DataContext is ViewModels.MainViewModel vm)
                {
                    vm.CurrentUser = _appState.CurrentUser;
                }
            }
        }

        private async Task LoadChatsAsync()
        {
            try
            {
                var chats = await _apiService.GetChatsAsync(_appState.CurrentUser?.Id);

                if (DataContext is ViewModels.MainViewModel vm)
                {
                    vm.Chats.Clear();
                    foreach (var chat in chats)
                    {
                        vm.Chats.Add(chat);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading chats");
            }
        }

        private void UpdateCurrentUserDisplay()
        {
            // Обновляем отображение текущего пользователя в UI
            if (_appState.CurrentUser != null && DataContext is ViewModels.MainViewModel vm)
            {
                var displayName = !string.IsNullOrEmpty(vm.CurrentUser.FirstName) &&
                                 !string.IsNullOrEmpty(vm.CurrentUser.LastName)
                    ? $"{vm.CurrentUser.FirstName} {vm.CurrentUser.LastName}"
                    : vm.CurrentUser.Username;

                CurrentUserDisplayName.Text = displayName;
                CurrentUserUsername.Text = _appState.CurrentUser.Username;
            }
        }

        private void UpdateSelectedContactDisplay()
        {
            if (DataContext is ViewModels.MainViewModel vm && vm.SelectedContact != null)
            {
                var displayName = vm.SelectedContact.DisplayName;

                //SelectedContactDisplayName.Text = displayName;
            }
        }

        private void ContactsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateSelectedContactDisplay();
        }
        private void ContactsList_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (sender is ListBox listBox &&
                listBox.DataContext is MainViewModel vm &&
                listBox.SelectedItem is Contact selectedContact)
            {
                Debug.WriteLine($"Context menu opening for contact: {selectedContact.DisplayName}");
            }
        }
    }

}
