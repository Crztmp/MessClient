using ClientApp01.Models;
using ClientApp01.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.DirectoryServices;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using ClientApp01.Views;

namespace ClientApp01.ViewModels
{
    public partial class CreateChatDialogViewModel : ObservableObject
    {
        private readonly ApiService _apiService;
        private readonly AppState _appState;
        private readonly ILogger<ApiService> _logger;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanCreate))]
        private string _chatName = string.Empty;

        [ObservableProperty]
        private string _chatDescription = string.Empty;

        public bool CanCreate => !string.IsNullOrWhiteSpace(ChatName);



        public CreateChatDialogViewModel(ApiService apiService, AppState appState, ILogger<ApiService> logger)
        {
            _apiService = apiService;
            _appState = appState;
            _logger = logger;
        }

        [RelayCommand]
        private async Task Create()
        {
            if (string.IsNullOrWhiteSpace(ChatName))
            {
                return;
            }
            if (ChatName != null)
            {
                _logger.LogInformation("AddContact request: ChatName = {ChatName}, ChatDescription = {ChatDescription}", ChatName, ChatDescription);
                var response = await _apiService.CreatePublicChatAsync(new CreateChatDto
                {
                    ChatName = _chatName,
                    ChatDescription = _chatDescription
                });
                if (response != null) 
                {
                    ChatName = null;
                    ChatDescription = null;
                    
                }
            }

            // Закрываем окно только после успешного добавления
            Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (Window window in Application.Current.Windows)
                {
                    if (window is CreateChatDialog dialog)
                    {
                        dialog.DialogResult = true;
                        dialog.Close();
                        break;
                    }
                }
            });


        }

        [RelayCommand]
        private void Cancel()
        {

        }
    }
}