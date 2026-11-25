using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows;
using ClientApp01.Models;
using ClientApp01.Services;
using System.Windows.Input;
using System.Threading.Tasks;
using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Build.Framework;
using System.Security.RightsManagement;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Windows.Interop;
using static System.Net.Mime.MediaTypeNames;
using ClientApp01.Views;
using System.Windows.Controls;
using System.Windows.Media;
using static MaterialDesignThemes.Wpf.Theme;
using System.Windows.Threading;
//using ClientApp01.Assets.Styles;

namespace ClientApp01.ViewModels
{
    public partial class MainViewModel : ObservableObject        
    {
        private readonly ILogger<MainViewModel> _logger;
        private readonly ApiService _apiService;
        private readonly ChatHubService _hubService;
        private readonly AppState _appState;
        private readonly INavigationService _navigationService;
        
        
        private DispatcherTimer _messagePollingTimer;
        private DateTime _lastMessageTime = DateTime.MinValue;

        [ObservableProperty]
        private User? _currentUser;

        [ObservableProperty]
        private ObservableCollection<Contact> _chats = new();

        //[ObservableProperty]
        //private ObservableCollection<Contact> _contacts;

        [ObservableProperty]
        private Contact _selectedChat;

        [ObservableProperty]
        private Contact? _selectedContact;

        [ObservableProperty]
        private ObservableCollection<Contact> _contacts = new();

        [ObservableProperty]
        private ObservableCollection<Message> _messages = new();

        [ObservableProperty]
        private string _currentMessage;
        [ObservableProperty]
        private int _selectedTabIndex;
        private int _currentChatId = 0;
        private DateTime _currentChatLastMessageTime = DateTime.MinValue;

        partial void OnSelectedTabIndexChanged(int value)
        {
            if (value == 0) // Вкладка чатов
            {
                _ = LoadChatsAsync();
            }
            else if (value == 1) // Вкладка контактов
            {
                _ = LoadContactsAsync();
            }
        }

        public bool HasMessage => !string.IsNullOrEmpty(CurrentMessage);

        public ICommand SendCommand { get; }

        public MainViewModel(ApiService apiService,
                           ChatHubService hubService,
                           AppState appState,
                           INavigationService navigationService,
                           ILogger<MainViewModel> logger)
        {
            _apiService = apiService;
            _hubService = hubService;
            _appState = appState;
            _navigationService = navigationService;
            _logger = logger;

            if (_appState.CurrentUser == null)
            {
                _logger.LogError("MainViewModel.MainViewModel CurrentUser is null in MainViewModel constructor");
                //_navigationService.NavigateToLogin();
                var currentWindowClose = new CurrentWindowCloseDto
                {
                    Close = true
                };
                _navigationService.NavigateToLogin(currentWindowClose);
                return;
            }

            CurrentUser = _appState.CurrentUser;
            Contacts = new ObservableCollection<Contact>();
            Chats = new ObservableCollection<Contact>();
            _logger.LogInformation("MainViewModel. initialized  for user: {Username}", _appState.CurrentUser.Username);

            SendCommand = new RelayCommand(SendMessage);
            LoadInitialDataAsync();
            InitializeMessagePolling();
            App.Current.Exit += (s, e) => _messagePollingTimer?.Stop();// Остановка таймера при выходе
        }
        

        public async Task LoadContactsAsync()
        {
            try
            {
                if (_appState.CurrentUser == null) return;

                var contacts = await _apiService.GetContactsAsync(_appState.CurrentUser.Id);

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    Contacts.Clear();
                    foreach (var contact in contacts)
                    {
                        Contacts.Add(contact);
                    }

                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading contacts");
            }
        }

        public async Task LoadChatsAsync()
        {
            try
            {
                //IsLoading = true;
                if (_appState.CurrentUser == null) return;

                var chats = await _apiService.GetChatsAsync(_appState.CurrentUser.Id);

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    Chats.Clear();
                    foreach (var chat in chats)
                    {
                        //chat.ContactName ??= string.Empty;
                        Chats.Add(chat);
                    }

                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading chats");
            }
            finally
            {
                //IsLoading = false;
            }
        }
        private async Task LoadInitialDataAsync()
        {
            try
            {
                _logger.LogInformation("MainViewModel.LoadInitialDataAsync - Starting");
                _logger.LogInformation($"CurrentUser ID: {CurrentUser?.Id}");
               
                // Загружаем данные для активной вкладки
                if (SelectedTabIndex == 0)
                {
                    await LoadChatsAsync();
                }
                else
                {
                    await LoadContactsAsync();
                }
              

                // Подключаемся к SignalR
                await _hubService.ConnectAsync();
                _hubService.MessageReceived += OnMessageReceived;
                _logger.LogInformation("MainViewModel.LoadInitialDataAsync - Completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MainViewModel.LoadInitialDataAsync - Error");
                throw;
            }
        }

        private async void SendMessage()
        {
            if (string.IsNullOrEmpty(CurrentMessage)) return;
            if (_appState.CurrentUser == null) return;

            try
            {
                int? chatId = null;

                // Если выбран контакт
                if (SelectedContact != null)
                {
                    // Если у контакта нет ChatID, создаем новый чат
                    if (!SelectedContact.ChatId.HasValue)
                    {

                        var result = await _apiService.AddContactAsync(new AddContactDto
                        {
                            ContactUserId = SelectedContact.ContactUserId,
                        });
                        var contactid = SelectedContact.ContactId;
                        _logger.LogInformation("SelectedContact.ContactId: {SelectedContact.ContactId}", SelectedContact.ContactId);
                        _ = LoadContactsAsync();
                        
                        var contact = Contacts.FirstOrDefault(c => c.ContactId == contactid);
                        if (contact != null)
                        {
                            if (contact.ChatId.HasValue)
                            {
                                chatId = contact.ChatId;
                            }
                        }
                    }
                    else if (SelectedContact.ChatId.HasValue)
                    {
                        chatId = SelectedContact.ChatId;
                    }
                    
                }
                // Если выбран чат
                else if (SelectedChat != null)
                {
                    chatId = SelectedChat.ChatId;
                }

                if (chatId.HasValue)
                {
                    var sent = await _apiService.SendChatMessageAsync(
                        chatId.Value,
                        CurrentMessage,
                        _appState.CurrentUser.Id);

                    if (sent)
                    {
                        CurrentMessage = string.Empty;
                        await LoadMessagesForChatAsync(chatId.Value);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message");
                MessageBox.Show($"Ошибка отправки сообщения: {ex.Message}");
            }
        }

        private void OnMessageReceived(Message message)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                message.IsMyMessage = message.SenderId == _appState.CurrentUser?.Id;
                Messages.Add(message);
                _lastMessageTime = message.SendDate;

                // Обновляем последнее сообщение в списке чатов
                var chat = Chats.FirstOrDefault(c => c.ChatId == message.ChatId);
                if (chat != null)
                {
                    //chat.LastMessage = $"{message.SenderName}: {message.MessageText}";
                    chat.LastMessageTime = message.SendDate;
                }
            });
        }
        [RelayCommand]
        private async Task CreateChat()
        {
            var dialog = new CreateChatDialog();
            if (dialog.ShowDialog() == true && dialog.DataContext is CreateChatDialogViewModel vm)
            {

            }
            _ = LoadChatsAsync();
        }

        [RelayCommand]
        private async Task AddContactAsync()
        {
            try
            {
                var dialog = new AddContactDialog();
                if (dialog.ShowDialog() == true && dialog.DataContext is AddContactViewModel vm)
                {
                    var result = await _apiService.AddContactAsync(new AddContactDto
                    {
                        ContactUserId = vm.SelectedUser.Id,
                        CustomName = vm.CustomName
                    });

                    if (result)
                    {
                        await LoadContactsAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding contact");
                MessageBox.Show("Ошибка при добавлении контакта");
            }
        }
        [RelayCommand]
        private async Task AddChatAsync()
        {
            try
            {
                var dialog = new AddChatDialog();
                if (dialog.ShowDialog() == true && dialog.DataContext is AddChatDialogViewModel vm)
                {
                    if (vm.SelectedChat?.ChatId != null) 
                    {
                        var result = await _apiService.AddChatAsync(new AddChatDto
                        {
                            ChatId = vm.SelectedChat.ChatId,
                            CustomName = vm.CustomName
                            
                        });
                    }
                    _ = LoadChatsAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding chat");
                MessageBox.Show("Ошибка при поиске чата");
            }
        }

        // Удаление контакта
        [RelayCommand]
        private async Task DeleteContactAsync(Contact contact)
        {
            try
            {
                if (contact == null) return;

                var result = await _apiService.DeleteContactAsync(contact.ContactId);
                if (result)
                {
                    await LoadContactsAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting contact");
                MessageBox.Show("Ошибка при удалении контакта");
            }
        }
        partial void OnSelectedChatChanged(Contact? value)
        {
            

            if (value != null)
            {
                _logger.LogInformation("ChatId", value.ChatId);
                //SelectedContact = null;
                _ = LoadMessagesForChatAsync(value.ChatId.Value);
            }

        }
        partial void OnSelectedContactChanged(Contact? value)
        {
            if (value != null)
            {

                if (value.ChatId.HasValue)
                {
                    _logger.LogInformation("ContactId", value.ContactUserId);
                    //SelectedChat = null;
                    _ = LoadMessagesForChatAsync(value.ChatId.Value);
                }
                else
                {
                    Messages.Clear();
                    _currentChatLastMessageTime = DateTime.MinValue;
                    _currentChatId = 0;
                }
            }
        }
        private void ScrollToLastMessage()
        {
            try
            {
                _logger.LogInformation("Attempting to scroll to last message");

                if (Messages.Count == 0)
                {
                    _logger.LogInformation("No messages to scroll to");
                    return;
                }

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    // Получаем ScrollViewer через Binding
                    if (System.Windows.Application.Current.MainWindow is MainWindow mainWindow)
                    {
                        var scrollViewer = mainWindow.MessagesScrollViewer;
                        if (scrollViewer != null)
                        {
                            _logger.LogInformation("ScrollViewer found, scrolling to end");
                            scrollViewer.ScrollToEnd();
                        }
                        else
                        {
                            _logger.LogWarning("ScrollViewer is null");
                        }
                    }
                    else
                    {
                        _logger.LogWarning("MainWindow is not set or not of type MainWindow");
                    }
                }, DispatcherPriority.Render);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ScrollToLastMessage");
            }
        }
        private async Task LoadMessagesForChatAsync(int chatId)
        {
            try
            {
                _logger.LogInformation("Loading messages for chat {ChatId}", chatId);
                if (_currentChatId != chatId)
                {
                    Messages.Clear();
                    _currentChatId = chatId;
                    _currentChatLastMessageTime = DateTime.MinValue;
                }

                var _startTime = _currentChatLastMessageTime;
                if (_currentChatLastMessageTime == DateTime.MinValue)
                {
                    _startTime = DateTime.UtcNow.AddDays(-30);
                }
                _logger.LogInformation("Loading messages From {_startTime}", _startTime);
                var messages = await _apiService.GetChatMessagesAsync(chatId, _startTime, DateTime.UtcNow);
                //_currentChatLastMessageTime = DateTime.UtcNow;
                


                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    foreach (var msg in messages.OrderBy(m => m.SendDate))
                    {
                        msg.SendDate = msg.SendDate.ToLocalTime(); 
                        Messages.Add(msg);
                        _currentChatLastMessageTime = msg.SendDate;
                    }
                    ScrollToLastMessage();
                });

                _logger.LogInformation("Loaded {Count} messages for chat {ChatId}", messages.Count, chatId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading messages for chat {ChatId}", chatId);
            }
        }
        [RelayCommand]
        public async Task ShowContactChatProperties(Contact contact)
        {
            _logger.LogInformation("ShowContactChatProperties command executed for DisplayName:" +
                "{contact.DisplayName}, IsIgnored: {contact.IsIgnored}, DoNotDisturb: {contact.DoNotDisturb}",
                contact.DisplayName, contact.IsIgnored, contact.DoNotDisturb);
            var dialog = new ContactChatPropertiesDialog();
            var vm = new ContactChatPropertiesViewModel(_apiService, contact);

            dialog.DataContext = vm;
            dialog.ShowDialog();
        }
        [RelayCommand]
        public async Task ShowContactProperties(Contact contact)
        {
            _logger.LogInformation("ShowContactProperties command executed for DisplayName:" +
                "{contact.DisplayName}, IsIgnored: {contact.IsIgnored}, DoNotDisturb: {contact.DoNotDisturb}, IsBlocked: {contact.IsBlocked}", 
                contact.DisplayName, contact.IsIgnored, contact.DoNotDisturb, contact.IsBlocked);
            var dialog = new ContactPropertiesDialog();
            var vm = new ContactPropertiesViewModel(_apiService, contact);

            dialog.DataContext = vm;
            dialog.ShowDialog();
        }
        [RelayCommand]
        private async Task IgnoreContact(Contact contact)
        {
            try
            {
                // Implement ignore logic
                MessageBox.Show($"Contact {contact.DisplayName} will be ignored");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ignoring contact");
                MessageBox.Show("Error ignoring contact");
            }
        }

        [RelayCommand]
        private async Task SetDoNotDisturb(Contact contact)
        {
            try
            {
                // Implement DND logic
                MessageBox.Show($"Do Not Disturb mode set for {contact.DisplayName}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting Do Not Disturb");
                MessageBox.Show("Error setting Do Not Disturb");
            }
        }

        [RelayCommand]
        private async Task BlockContact(Contact contact)
        {
            try
            {
                var result = await _apiService.UpdateContactAsync(new UpdateContactDto
                {
                    ContactId = contact.ContactId,
                    CustomName = contact.ContactName,
                    IsBlocked = true
                });

                if (result)
                {
                    MessageBox.Show($"Contact {contact.DisplayName} blocked");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error blocking contact");
                MessageBox.Show("Error blocking contact");
            }
        }

        [RelayCommand]
        private async Task ClearContactHistory(Contact contact)
        {
            try
            {
                if (contact.ChatId.HasValue)
                {
                    // Call API to clear chat history
                    MessageBox.Show($"Chat history with {contact.DisplayName} cleared");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing contact history");
                MessageBox.Show("Error clearing contact history");
            }
        }

        [RelayCommand]
        private async Task SetChatDoNotDisturb(Contact contact)
        {
            // Implement chat DND
            MessageBox.Show($"Do Not Disturb set for chat {contact.DisplayName}");
        }

        [RelayCommand]
        private async Task ClearChatHistory(Contact contact)
        {
            // Implement clear chat history
            MessageBox.Show($"History cleared for chat {contact.DisplayName}");
        }

        [RelayCommand]
        private async Task LeaveChat(Chat chat)
        {
            // Implement leave chat
            MessageBox.Show($"Left chat {chat.DisplayName}");
        }
        [RelayCommand]
        public void Logout()
        {
            try
            {
                _logger.LogInformation("Logging out user: {Username}", _appState.CurrentUser?.Username);

                // Очищаем состояние приложения
                _appState.ClearState();

                // Очищаем локальные коллекции
                Chats.Clear();
                Contacts.Clear();
                Messages.Clear();

                // Навигация на окно входа
                var currentWindowClose = new CurrentWindowCloseDto
                {
                    Close = true
                };
                _navigationService.NavigateToLogin(currentWindowClose);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                MessageBox.Show("Ошибка при выходе из системы");
            }
        }
        public void LogoutAddClose()
        {
            try
            {
                _logger.LogInformation("Logging out user: {Username}", _appState.CurrentUser?.Username);

                // Очищаем состояние приложения
                _appState.ClearState();

                // Очищаем локальные коллекции
                Chats.Clear();
                Contacts.Clear();
                Messages.Clear();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                MessageBox.Show("Ошибка при выходе из системы");
            }
        }
        private void InitializeMessagePolling()
        {
            _messagePollingTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(60) // Опрос каждые xx секунд
            };

            _messagePollingTimer.Tick += async (s, e) => await PollForNewMessages();
            _messagePollingTimer.Start();
        }

        private async Task PollForNewMessages()
        {
            if (_currentChatId != 0)
            {
                LoadMessagesForChatAsync(_currentChatId);
            }
        }
    }
}