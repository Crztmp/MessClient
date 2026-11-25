using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ClientApp01.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System.IO;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.DirectoryServices.AccountManagement;

namespace ClientApp01.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private readonly AppState _appState;
        private readonly string _baseUrl;
        private readonly ILogger<AuthService> _logger;

        public AuthService(IHttpClientFactory httpClientFactory, AppState appState,
                  IConfiguration config, ILogger<AuthService> logger)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
            _appState = appState;
            _baseUrl = config["ApiBaseUrl"];
            _logger = logger;
        }

        public async Task<bool> PingServerAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/ping");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
        public async Task<UserTypeInfo> CheckUserTypeAsync(string username)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/auth/checkuser?username={Uri.EscapeDataString(username)}");

                if (!response.IsSuccessStatusCode)
                {
                    return new UserTypeInfo { Exists = false };
                }

                return await response.Content.ReadFromJsonAsync<UserTypeInfo>();
            }
            catch
            {
                return new UserTypeInfo { Exists = false };
            }
        }

        public class UserTypeInfo
        {
            public bool Exists { get; set; }
            public bool IsActiveDirectory { get; set; }
        }
        public async Task<AuthResponse> LoginADAsync(string domainUsername, string password)
        {
            try
            {
                _logger.LogInformation("AuthService.LoginADAsync CrzC_010");
                // Проверка формата имени пользователя
                if (!domainUsername.Contains('\\'))
                {
                    _logger.LogInformation("AuthService.LoginADAsync CrzC_0101");
                    return new AuthResponse { Error = "Domain username should be in format 'DOMAIN\\username'" };
                }

                // Отправка запроса на сервер для аутентификации AD
                var response = await _httpClient.PostAsJsonAsync(
                $"{_baseUrl}/api/auth/login/ad",
                    new { username = domainUsername, password });

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return new AuthResponse { Error = $"Login failed: {errorContent}" };
                }

                // Обработка успешного ответа
                var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
                if (result != null)
                {
                    _appState.AuthToken = result.Token;
                    //_appState.CurrentUser = result.User;
                    _appState.CurrentUser = new User
                    {
                        Id = result.User?.Id,
                        Username = domainUsername,
                        Email = result.User?.Email,
                        FirstName = result.User?.FirstName,
                        LastName = result.User?.LastName,
                        IsActiveDirectory = true
                    };
                }

                return result ?? new AuthResponse { Error = "Invalid response from server" };
            }
            catch (Exception ex)
            {
                return new AuthResponse { Error = $"Login error: {ex.Message}" };
            }
        }

        public async Task<AuthResponse> RegisterAsync(string username, string password, string email, string firstName, string lastName)
        {
            var response = await _httpClient.PostAsJsonAsync(
            $"{_baseUrl}/api/auth/register",
                new
                {
                    Username = username,
                    Password = password,
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName
                });

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return new AuthResponse { Error = error };
            }

            return await LoginAsync(username, password);
        }
        public async Task<AuthResponse> RegisterADUserAsync(string username, string password,
            string email, string firstName, string lastName)
        {
            try
            {
                // Verify AD credentials
                var domainParts = username.Split('\\');
                if (domainParts.Length != 2)
                {
                    return new AuthResponse { Error = "Неверный формат имени пользователя. Используйте DOMAIN\\username" };
                }

                var domain = domainParts[0];
                var userPrincipalName = domainParts[1];

                // Verify AD credentials
                using (var context = new PrincipalContext(ContextType.Domain, domain))
                {
                    if (!context.ValidateCredentials(userPrincipalName, password))
                    {
                        return new AuthResponse { Error = "Неверные учетные данные AD" };
                    }

                    var user = UserPrincipal.FindByIdentity(context, username);
                    if (user == null)
                    {
                        return new AuthResponse { Error = "Доменный пользователь не найден" };
                    }
                }

                // Register the user
                var response = await _httpClient.PostAsJsonAsync(
                    $"{_baseUrl}/api/auth/register/ad",
                    new
                    {
                        Username = username,
                        Email = email,
                        FirstName = firstName,
                        LastName = lastName,
                        Password = password // Send password for server-side verification
                    });

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return new AuthResponse { Error = error };
                }

                return await LoginADAsync(username, password);
            }
            catch (Exception ex)
            {
                return new AuthResponse { Error = $"Ошибка регистрации AD: {ex.Message}" };
            }
        }

        public async Task<AuthResponse> RegisterWindowsUserAsync(string username, string email,
            string firstName, string lastName)
        {
            try
            {
                // Register without password
                var response = await _httpClient.PostAsJsonAsync(
                    $"{_baseUrl}/api/auth/register/windows",
                    new
                    {
                        Username = username,
                        Email = email,
                        FirstName = firstName,
                        LastName = lastName
                    });

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return new AuthResponse { Error = error };
                }

                return await LoginWindowsIntegratedAsync(username);
            }
            catch (Exception ex)
            {
                return new AuthResponse { Error = $"Ошибка регистрации Windows: {ex.Message}" };
            }
        }

        public async Task<AuthResponse> LoginAsync(string username, string password)
        {
            try
            {
                _logger.LogInformation("AuthService.LoginAsync Attempting login for user: {Username}", username);

                // Отправка запроса на сервер для аутентификации
                var response = await _httpClient.PostAsJsonAsync(
                    $"{_baseUrl}/api/auth/login",
                    new { username, password });

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("AuthService.LoginAsync Login failed for user {Username}. Status: {StatusCode}, Error: {Error}",
                        username, response.StatusCode, errorContent);
                    return new AuthResponse { Error = "Invalid credentials" };
                }

                // Обработка успешного ответа
                var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
                if (result != null)
                {
                    _appState.AuthToken = result.Token;
                    _appState.CurrentUser = new User
                    {
                        Id = result.User.Id,
                        Username = result.User.Username,
                        Email = result.User.Email,
                        FirstName = result.User.FirstName,
                        LastName = result.User.LastName
                        //IsActiveDirectory = result.User.IsActiveDirectory
                    };
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", _appState.AuthToken);
                }

                _logger.LogInformation("AuthService.LoginAsync User {Username} logged in successfully", username);
                return result ?? new AuthResponse { Error = "Invalid response" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AuthService.LoginAsync Error during login for user {Username}", username);
                return new AuthResponse { Error = $"Login error: {ex.Message}" };
            }
        }
        public async Task<AuthResponse> LoginWindowsIntegratedAsync(string windowsUsername)
        {
            try
            {
                _logger.LogInformation("AuthService.LoginWindowsIntegratedAsync Attempting Windows integrated login for user: {Username}", windowsUsername);

                // Create request with Windows credentials
                var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/api/auth/login/windows");
                request.Content = JsonContent.Create(new { username = windowsUsername });

                // Add current Windows credentials to the request
                request.Headers.Add("Authorization", $"Windows {windowsUsername}");

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("LoginWindowsIntegratedAsync failed with status {StatusCode}: {Error}",
                        response.StatusCode, errorContent);
                    return new AuthResponse { Error = $"Login failed: {errorContent}" };
                }

                var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
                if (result != null)
                {
                    _appState.AuthToken = result.Token;
                    _appState.CurrentUser = new User
                    {
                        Id = result.User?.Id,
                        Username = windowsUsername,
                        Email = result.User?.Email,
                        FirstName = result.User?.FirstName,
                        LastName = result.User?.LastName,
                        IsActiveDirectory = true
                    };
                }

                return result ?? new AuthResponse { Error = "Invalid response from server" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LoginWindowsIntegratedAsync error");
                return new AuthResponse { Error = $"Login error: {ex.Message}" };
            }
        }

        public void Logout()
        {
            _appState.AuthToken = null;
            _appState.CurrentUser = null;
        }
    }

    //public class AuthResponse
    //{
    //    public bool Success => !string.IsNullOrEmpty(Token);
    //    public string Token { get; set; }
    //    public User User { get; set; }
    //    public List<ContactInfo> Contacts { get; set; }
    //    public List<ChatInfo> Chats { get; set; }
    //    public string Error { get; set; }
    //}

    public class ContactInfo
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string ContactName { get; set; }
    }

    public class ChatInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Type { get; set; }
    }
}