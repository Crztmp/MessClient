using ClientApp01.Services;
using ClientApp01.ViewModels;
using ClientApp01.Models;
using ClientApp01.Converters;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;

namespace ClientApp01
{
    public class ViewModelLocator
    {
        public static ViewModelLocator Instance { get; } = new ViewModelLocator();

        public LoginViewModel LoginViewModel => App.Services.GetRequiredService<LoginViewModel>();
        public MainViewModel MainViewModel => App.Services.GetRequiredService<MainViewModel>();
        public RegisterViewModel RegisterViewModel => App.Services.GetRequiredService<RegisterViewModel>();
        public LoginSettingsViewModel LoginSettingsViewModel => App.Services.GetRequiredService<LoginSettingsViewModel>();
        
        private T GetService<T>() where T : class
        {
            //if (Application.Current is App app)
            var app = Application.Current as App;
            if (app != null)
            {
    
     //           return app.ServiceProvider.GetRequiredService<T>();
            }
            throw new InvalidOperationException("Application.Current is not App");
        }
        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            // Регистрация сервисов
            services.AddSingleton<AppState>(); 
            services.AddSingleton<ApiService>();
            services.AddSingleton<ChatHubService>();
            services.AddSingleton<AuthService>();
            services.AddSingleton<INavigationService, NavigationService>();

            // Регистрация ViewModels
            services.AddTransient<MainViewModel>();
            services.AddTransient<LoginViewModel>();
            services.AddTransient<RegisterViewModel>();

            return services.BuildServiceProvider();
        }
    }
}