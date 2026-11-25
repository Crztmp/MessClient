
using ClientApp01.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;
using ClientApp01.Views;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;

namespace ClientApp01.ViewModels
{
    public partial class LoginSettingsViewModel : ObservableObject
    {
        private readonly IConfiguration _config;
        private readonly INavigationService _navigationService;

        [ObservableProperty]
        private string _serverAddress;

        [ObservableProperty]
        private string _selectedProtocol;

        [ObservableProperty]
        private string _port;

        public ObservableCollection<string> Protocols { get; } = new() { "http", "https" };

        public LoginSettingsViewModel(IConfiguration config, INavigationService navigationService)
        {
            _config = config;
            _navigationService = navigationService;
            LoadSettings();
        }

        private void LoadSettings()
        {
            try
            {
                var baseUrl = _config["ApiBaseUrl"];
                if (!string.IsNullOrEmpty(baseUrl))
                {
                    var uri = new Uri(baseUrl);
                    ServerAddress = uri.Host;
                    // нормализуем протокол
                    SelectedProtocol = uri.Scheme.ToLowerInvariant();
                    Port = uri.Port.ToString();
                }
                else
                {
                    ServerAddress = "localhost";
                    SelectedProtocol = "http";
                    Port = "5278";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки настроек: {ex.Message}");
                ServerAddress = "localhost";
                SelectedProtocol = "http";
                Port = "5278";
            }
        }

        [RelayCommand]
        private void Save()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ServerAddress) ||
                    string.IsNullOrWhiteSpace(SelectedProtocol) ||
                    string.IsNullOrWhiteSpace(Port))
                {
                    MessageBox.Show("Все поля должны быть заполнены");
                    return;
                }

                // SelectedProtocol теперь всегда чистая строка "http"/"https"
                var protocol = SelectedProtocol.Trim().ToLowerInvariant();
                if (protocol != "http" && protocol != "https")
                {
                    MessageBox.Show("Протокол должен быть http или https");
                    return;
                }

                if (!int.TryParse(Port, out var port) || port <= 0 || port > 65535)
                {
                    MessageBox.Show("Некорректный порт");
                    return;
                }

                var newUrl = $"{protocol}://{ServerAddress}:{port}";
                var json = JsonConvert.SerializeObject(new
                {
                    ChatHubUrl = $"{newUrl}/chatHub",
                    ApiBaseUrl = newUrl,
                    DefaultTheme = _config["DefaultTheme"] ?? "Light"
                }, Formatting.Indented);

                var appSettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
                File.WriteAllText(appSettingsPath, json);

                Application.Current.Windows.OfType<LoginSettingsWindow>().FirstOrDefault()?.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения настроек: {ex.Message}");
            }
        }

        [RelayCommand]
        private void Cancel()
        {
            Application.Current.Windows.OfType<LoginSettingsWindow>().FirstOrDefault()?.Close();
        }
    }

}
