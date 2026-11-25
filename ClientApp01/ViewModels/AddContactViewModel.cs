using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using ClientApp01.Models;
using ClientApp01.Services;
using ClientApp01.ViewModels;
using System.Threading.Tasks;
using System;
using System.Diagnostics;
using ClientApp01.Views;
using Microsoft.Extensions.Logging;

namespace ClientApp01.ViewModels
{
    public partial class AddContactViewModel : ObservableObject
    {
        private readonly ApiService _apiService;
        private readonly AppState _appState;
        private readonly ILogger<ApiService> _logger;

        [ObservableProperty]
        private string _searchText;

        [ObservableProperty]
        private ObservableCollection<User> _searchResults = new();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanAdd))]
        private User _selectedUser;

        [ObservableProperty]
        private string _customName;

        //public bool CanAdd => SelectedUser != null;
        public bool CanAdd => SelectedUser != null &&
                     !_appState.Contacts.Any(c => c.ContactUserId == SelectedUser?.Id);

        public AddContactViewModel(ApiService apiService, AppState appState)
        {
            _apiService = apiService;
            _appState = appState;
        }

        [RelayCommand]
        private async Task Search()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                SearchResults.Clear();
                return;
            }

            var users = await _apiService.SearchUsersAsync(SearchText);
            SearchResults = new ObservableCollection<User>(users);
            OnPropertyChanged(nameof(CanAdd));
        }

        [RelayCommand]
        private async Task AddContact()
        {
            if (SelectedUser == null || _appState.CurrentUser == null) return;

            try
            {
                // Проверяем, есть ли уже такой контакт
                if (_appState.Contacts.Any(c => c.ContactUserId == SelectedUser.Id))
                {
                    MessageBox.Show("Этот пользователь уже есть в ваших контактах");
                    return;
                }
                _logger.LogInformation("AddContact request: {SelectedUser.Id}", SelectedUser.Id);
                var response = await _apiService.AddContactAsync(new AddContactDto
                {
                    //UserId = _appState.CurrentUser.Id,
                    ContactUserId = SelectedUser.Id,
                    CustomName = CustomName
                });

                if (response)
                {
                    MessageBox.Show("Контакт успешно добавлен");
                    // Обновляем список контактов
                    await LoadContactsAsync();
                    if (Application.Current.Windows.OfType<AddContactDialog>().FirstOrDefault() is AddContactDialog dialog)
                    {
                        dialog.DialogResult = true;
                        dialog.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении контакта: {ex.Message}");
            }
        }
        private async Task LoadContactsAsync()
        {
            try
            {
                if (_appState.CurrentUser == null) return;

                var contacts = await _apiService.GetContactsAsync(_appState.CurrentUser.Id);
                _appState.Contacts.Clear();
                foreach (var contact in contacts)
                {
                    _appState.Contacts.Add(contact);
                }
            }
            catch (Exception ex)
            {
                // Логирование ошибки
                Debug.WriteLine($"Error loading contacts: {ex.Message}");
            }
        }
    }
}