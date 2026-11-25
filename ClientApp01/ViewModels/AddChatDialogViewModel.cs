using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClientApp01.Models;
using ClientApp01.Services;
using ClientApp01.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Security.AccessControl;
using Microsoft.Extensions.Logging;
using System.Windows;
using Microsoft.Build.Framework;

namespace ClientApp01.ViewModels
{
    public partial class AddChatDialogViewModel : ObservableObject
    {
        private readonly ApiService _apiService;
        private readonly AppState _appState;
        private readonly ILogger<ApiService> _logger;

        [ObservableProperty]
        private string _searchText;

        [ObservableProperty]
        private ObservableCollection<Chat> _searchResults = new();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanSelect))]
        private Chat _selectedChat;

        [ObservableProperty]
        private string _CustomName;


        public bool CanSelect => SelectedChat != null;
     
        public AddChatDialogViewModel(ApiService apiService, AppState appState, ILogger<ApiService> logger)
        {
            _apiService = apiService;
            _appState = appState;
            _logger = logger;
        }

        [RelayCommand]
        private async Task Search()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                SearchResults.Clear();
                return;
            }


            var chats = await _apiService.SearchChatsAsync(SearchText);
            SearchResults = new ObservableCollection<Chat>(chats);
            OnPropertyChanged(nameof(CanSelect));
        }

        [RelayCommand]
        private async Task AddChat()
        {
            if (SelectedChat == null) return;
            // Проверяем, есть ли уже такой чат
            if (_appState.Contacts.Any(c => c.UserId == _appState.CurrentUser.Id && c.ChatId == SelectedChat.ChatId))
            {

                return;
            }
            if (SelectedChat != null)
            {
                _logger.LogInformation("AddContact request: {SelectedChat.ChatId}", SelectedChat.ChatId);
                var response = await _apiService.AddChatAsync(new AddChatDto
                {
                    ChatId = SelectedChat.ChatId,
                    CustomName = CustomName
                });
                // Закрываем окно только после успешного добавления
                Application.Current.Dispatcher.Invoke(() =>
                {
                    foreach (Window window in Application.Current.Windows)
                    {
                        if (window is AddChatDialog dialog)
                        {
                            dialog.DialogResult = true;
                            dialog.Close();
                            break;
                        }
                    }
                });
            }


        }
    }
}
