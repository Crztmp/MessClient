using ClientApp01.Services;
using ClientApp01.Views;
using ClientApp01.Models;
//using ClientApp01.Converters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using System.IO;
using System;
using System.Windows;
using ClientApp01.ViewModels;
using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;

namespace ClientApp01
{
    public partial class App : Application
    {
        //public IServiceProvider ServiceProvider { get; private set; }
        public static IServiceProvider Services { get; private set; }
        public static IConfiguration Configuration { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            // Загрузка ресурсов Material Design перед инициализацией
            var materialAssembly = typeof(MaterialDesignThemes.Wpf.ButtonAssist).Assembly;
            var resourceLocator = new Uri(
                "pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Light.xaml",
                UriKind.Absolute);

            var resourceDict = new ResourceDictionary { Source = resourceLocator };
            Current.Resources.MergedDictionaries.Add(resourceDict);

            base.OnStartup(e);
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            Configuration = builder.Build();

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            Services = serviceCollection.BuildServiceProvider();

            var loginWindow = Services.GetRequiredService<LoginWindow>();
            loginWindow.Show();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Configure logging
            services.AddLogging(configure =>
            {
                configure.AddConsole();
                configure.AddDebug();
                configure.SetMinimumLevel(LogLevel.Debug);
            });
            // HTTP-клиент
            services.AddHttpClient("ApiClient", client =>
            {
                client.BaseAddress = new Uri(Configuration["ApiBaseUrl"]);
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
            });
            services.AddSingleton(Configuration);
            // Сервисы
            //services.AddSingleton<IConfiguration>(Configuration);
            services.AddSingleton<AppState>();
            services.AddSingleton<INavigationService, NavigationService>();

            // Репозитории/сервисы
            services.AddTransient<ApiService>();
            services.AddTransient<AuthService>();
            services.AddTransient<ChatHubService>();
            //services.AddSingleton<INavigationService, NavigationService>();

            // ViewModels
            services.AddTransient<LoginViewModel>();
            services.AddTransient<MainViewModel>(); 
            services.AddSingleton<ViewModelLocator>();
            services.AddTransient<RegisterViewModel>();

            // Окна
            services.AddTransient<LoginWindow>();
            services.AddTransient<MainWindow>();
            services.AddTransient<RegisterWindow>();

            services.AddSingleton<ILogger<AuthService>>(provider =>
            provider.GetRequiredService<ILoggerFactory>().CreateLogger<AuthService>());
            services.AddSingleton<ILogger<ApiService>>(provider =>
            provider.GetRequiredService<ILoggerFactory>().CreateLogger<ApiService>());
        }
    }
}