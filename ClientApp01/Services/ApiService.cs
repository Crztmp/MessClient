using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using ClientApp01.Models;
//using ClientApp01.Models.DTOs;
using ClientApp01.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ClientApp01.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly ILogger<ApiService> _logger;
        private readonly AppState _appState;

        public ApiService(HttpClient httpClient, IConfiguration config, ILogger<ApiService> logger, AppState appState)
        {
            _httpClient = httpClient;
            _baseUrl = config["ApiBaseUrl"];
            _logger = logger;
            _appState = appState;
            // Set base address if not already set
            if (_httpClient.BaseAddress == null)
            {
                _httpClient.BaseAddress = new Uri(_baseUrl);
            }

           
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            // Подписываемся на изменение токена
            _appState.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(AppState.AuthToken))
                {
                    UpdateAuthHeader();
                }
            };

            // Инициализируем заголовок при создании
            UpdateAuthHeader();
        }
        private void UpdateAuthHeader()
        {
            if (!string.IsNullOrEmpty(_appState.AuthToken))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _appState.AuthToken);
            }
            else
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }

        public async Task<List<Contact>> GetContactsAsync(string userId)
        {
            try
            {
                // Ensure we have the auth token
                if (string.IsNullOrEmpty(_appState.AuthToken))
                {
                    _logger.LogWarning("No auth token available");
                    return new List<Contact>();
                }

                // Add authorization header if token exists
                if (!string.IsNullOrEmpty(_appState.AuthToken))
                {
                    _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _appState.AuthToken);
                }

                var response = await _httpClient.GetAsync($"{_baseUrl}/api/contacts/getprived");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to load contacts. Status: {StatusCode}, Response: {Response}",
                        response.StatusCode, await response.Content.ReadAsStringAsync());
                    return new List<Contact>();
                }

                var content = await response.Content.ReadAsStringAsync();
                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore
                };
                var contacts = JsonConvert.DeserializeObject<List<Contact>>(content);
                return contacts ?? new List<Contact>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading contacts");
                return new List<Contact>();
            }
        }

        public async Task<List<Contact>> GetChatsAsync(string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(_appState.AuthToken))
                {
                    _logger.LogWarning("No auth token available for chats request");
                    return new List<Contact>();
                }
                _logger.LogInformation("Requesting chats from {Url}", $"{_baseUrl}/api/chat");
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _appState.AuthToken);
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/contacts/getchats");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("ApiService. Failed to load chats. Status: {StatusCode}", response.StatusCode);
                    return new List<Contact>();
                }
                var content = await response.Content.ReadAsStringAsync();

                var contacts = JsonConvert.DeserializeObject<List<Contact>>(content);
                return contacts ?? new List<Contact>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ApiService. Error loading chats");
                return new List<Contact>();
            }
        }
        public async Task<bool> AddContactAsync(AddContactDto dto)
        {
            try
            {
                _logger.LogInformation("Sending AddContact request: {dto.ContactUserId}", dto.ContactUserId);
                var json = JsonConvert.SerializeObject(dto);
                _logger.LogInformation("Sending AddContact request: {Json}", json);

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUrl}/api/contacts/AddPrivedContact", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("AddContact failed: {StatusCode}, {Error}",
                        response.StatusCode, errorContent);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AddContactAsync");
                return false;
            }
        }
        public async Task<bool> AddChatAsync(AddChatDto dto)
        {
            try
            {
                _logger.LogInformation("Sending AddChat request: {dto.ChatId}", dto.ChatId);
                var json = JsonConvert.SerializeObject(dto);
                _logger.LogInformation("Sending AddCchat request: {Json}", json);

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUrl}/api/chat/AddPublicChat", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("AddChat failed: {StatusCode}, {Error}",
                        response.StatusCode, errorContent);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AddContactAsync");
                return false;
            }
        }

        public async Task<bool> DeleteContactAsync(int contactId)
        {
            var response = await _httpClient.DeleteAsync($"{_baseUrl}/api/contacts/{contactId}");
            return response.IsSuccessStatusCode;
        }

        //public async Task<bool> UpdateContactAsync(UpdateContactDto dto)
        //{
        //    var response = await _httpClient.PutAsJsonAsync($"{_baseUrl}/api/contacts/{dto.ContactId}", dto);
        //    return response.IsSuccessStatusCode;
        //}
        public async Task<bool> UpdateContactAsync(UpdateContactDto dto)
        {
            try
            {
                _logger.LogInformation("Sending UpdateContact request: {dto.ContactUserId}", dto.ContactId);
                var json = JsonConvert.SerializeObject(dto);
                _logger.LogInformation("Sending AddContact request: {Json}", json);

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUrl}/api/contacts/UpdatePrivedContact", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("UpdateContact failed: {StatusCode}, {Error}",
                        response.StatusCode, errorContent);
                    return false;
                }
                //await LoadContactsAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AddContactAsync");
                return false;
            }
        }
        public async Task<List<User>> SearchUsersAsync(string searchText)
        {
            try
            {
                var response = await _httpClient.GetAsync(
                    $"{_baseUrl}/api/contacts/search?term={Uri.EscapeDataString(searchText)}");

                if (!response.IsSuccessStatusCode)
                    return new List<User>();

                var content = await response.Content.ReadAsStringAsync();
                var users = JsonConvert.DeserializeObject<List<User>>(content);
                return users ?? new List<User>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching users");
                return new List<User>();
            }
        }
        public async Task<List<Chat>> SearchChatsAsync(string searchText)
        {
            try
            {
                var response = await _httpClient.GetAsync(
                    $"{_baseUrl}/api/chat/search?term={Uri.EscapeDataString(searchText)}");

                if (!response.IsSuccessStatusCode)
                    return new List<Chat>();

                var content = await response.Content.ReadAsStringAsync();
                var chats = JsonConvert.DeserializeObject<List<Chat>>(content);
                return chats ?? new List<Chat>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching chats");
                return new List<Chat>();
            }
        }
        public async Task<bool> CreatePublicChatAsync(CreateChatDto dto)
        {
            try
            {
                _logger.LogInformation("Sending AddChat request: ChatName={dto.ChatName}, ChatDescription={dto.ChatDescription}", dto.ChatName, dto.ChatDescription);
                var json = JsonConvert.SerializeObject(dto);
                _logger.LogInformation("Sending AddCchat request: {Json}", json);

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUrl}/api/chat/createPublic", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("AddChat failed: {StatusCode}, {Error}",
                        response.StatusCode, errorContent);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating chat");
                return false;
            }
        }
        public async Task<List<Message>> GetChatMessagesAsync(
            int chatId,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int count = 100)
        {
            try
            {
                _logger.LogInformation("Loading messages for chat {ChatId}", chatId);
                var url = $"{_baseUrl}/api/messages/GetChatMessages?chatId={chatId}&count={count}";

                if (fromDate.HasValue)
                {
                    url += $"&fromDate={Uri.EscapeDataString(fromDate.Value.ToString("o"))}";
                }

                if (toDate.HasValue)
                    url += $"&toDate={Uri.EscapeDataString(toDate.Value.ToString("o"))}";

                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to load messages for chat {ChatId}. Status: {StatusCode}, Response: {Response}",
                        chatId, response.StatusCode, await response.Content.ReadAsStringAsync());
                    return new List<Message>();
                }

                //var content = await response.Content.ReadAsStringAsync();
                var messages = await response.Content.ReadFromJsonAsync<List<Message>>();
                _logger.LogDebug("Loaded {Count} messages for chat {ChatId}", messages?.Count, chatId);
                return messages ?? new List<Message>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading messages for chat {ChatId}", chatId);
                return new List<Message>();
            }
        }
        public async Task<bool> SendChatMessageAsync(int chatId, string messageText, string userId)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/api/messages/SendMessage",
                    new { ChatID = chatId, MessageText = messageText });

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error sending chat message: {Error}", error);
                    return false;
                }

                return true;
                //return await response.Content.ReadFromJsonAsync<Message>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending chat message");
                return false;
            }
        }

        public async Task<bool> SendPrivateMessageAsync(int chatId, string messageText, string userId)
        {
            try
            {
                _logger.LogInformation("Attempting to send private message to {ContactId}", chatId);

                // 1. Находим ID приватного чата
                var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/api/messages/SendMessage",
                    new { ChatID = chatId, MessageText = messageText });

                // 2. Отправляем сообщение в найденный чат
                return true;
                //return await SendChatMessageAsync(chatId, messageText, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending private message");
                return false;
            }
        }
        public void Logout()
        {
            try
            {
                // Очищаем заголовок авторизации
                _httpClient.DefaultRequestHeaders.Authorization = null;

                // Очищаем состояние приложения
                _appState.AuthToken = null;
                _appState.CurrentUser = null;
                _appState.Contacts.Clear();
                _appState.Chats.Clear();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                throw;
            }
        }
    }
}