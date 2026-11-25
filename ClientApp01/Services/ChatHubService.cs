using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration; // Добавьте эту директиву
using System;
using System.Threading.Tasks;
using ClientApp01.Models;
using System.Configuration;

namespace ClientApp01.Services
{
    public class ChatHubService
    {
        private readonly string _hubUrl;
        private HubConnection _connection;

        public ChatHubService(IConfiguration config)
        {
            _hubUrl = config["ChatHubUrl"];
        }

        public event Action<string> ConnectionStatusChanged;
        public event Action<Message> MessageReceived;
        public event Action<Message> PrivateMessageReceived;
        public ChatHubService() { }
        public async Task ConnectAsync()
        {
            _connection = new HubConnectionBuilder()
                .WithUrl(_hubUrl) // Используем URL из конфигурации
                .Build();

            ConfigureConnectionEvents();
            _connection.On<Message>("ReceivePrivateMessage", message =>
            {
                PrivateMessageReceived?.Invoke(message);
            });

            _connection.On<Message>("ReceiveMessage", message =>
            {
                MessageReceived?.Invoke(message);
            });

            await _connection.StartAsync();
        }

        private void ConfigureConnectionEvents()
        {
            _connection.Closed += async (error) =>
            {
                ConnectionStatusChanged?.Invoke("Disconnected");
                await Task.Delay(5000);
                await ConnectAsync();
            };

            _connection.Reconnected += (connectionId) =>
            {
                ConnectionStatusChanged?.Invoke("Reconnected");
                return Task.CompletedTask;
            };
        }

        public async Task SendMessageAsync(Message message)
        {
            try
            {
                if (_connection?.State == HubConnectionState.Connected)
                {
                    await _connection.InvokeAsync("SendMessage", message.ChatId, message);
                    //await _connection.InvokeAsync("SendMessage", message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message: {ex.Message}");
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_connection != null)
            {
                await _connection.DisposeAsync();
            }
        }
    }
}