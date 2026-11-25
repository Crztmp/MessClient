using ClientApp01.Models;
using ClientApp01.Views;
using System.Linq;
using System.Windows;
using static System.Net.Mime.MediaTypeNames;

namespace ClientApp01.Services
{
    public interface INavigationService
    {
        void NavigateToMainWindow(CurrentWindowCloseDto? currentWindowClose);
        void NavigateToLogin(CurrentWindowCloseDto? currentWindowClose);
        void NavigateToRegister(CurrentWindowCloseDto? currentWindowClose);
    }

    public class NavigationService : INavigationService
    {
        public void NavigateToMainWindow(CurrentWindowCloseDto? currentWindowClose)
        {
            var currentWindow = System.Windows.Application.Current.MainWindow;
            var mainWindow = new MainWindow();
            System.Windows.Application.Current.MainWindow = mainWindow; // Устанавливаем как главное окно
            if (currentWindowClose.Close.HasValue)
            {
                if (currentWindowClose.Close.Value == true)
                {
                    currentWindow.Close();
                }
                else
                {
                    currentWindow.Hide();
                }
            }
            mainWindow.Show();
        }
        public void NavigateToLogin(CurrentWindowCloseDto? currentWindowClose)
        {
            var currentWindow = System.Windows.Application.Current.MainWindow;
            var LoginWindow = new LoginWindow();
            System.Windows.Application.Current.MainWindow = LoginWindow; // Устанавливаем как главное окно
            if (currentWindowClose.Close.HasValue)
            {
                if (currentWindowClose.Close.Value == true)
                {
                    currentWindow.Close();
                }
                else 
                {
                    currentWindow.Hide();
                }
            }
            LoginWindow.Show();
        }

        public void NavigateToRegister(CurrentWindowCloseDto? currentWindowClose)
        {
            var currentWindow = System.Windows.Application.Current.MainWindow;
            //System.Windows.Application.Current.MainWindow.Hide();
            var registerWindow = new RegisterWindow();
            System.Windows.Application.Current.MainWindow = registerWindow; // Устанавливаем как главное окно
            if (currentWindowClose.Close.HasValue)
            {
                if (currentWindowClose.Close.Value == true)
                {
                    currentWindow.Close();
                }
                else
                {
                    currentWindow.Hide();
                }
            }
            registerWindow.Show();
        }
        private void CloseAllWindows()
        {
            var windows = System.Windows.Application.Current.Windows.OfType<Window>().ToList();
            foreach (var window in windows)
            {
                if (window != System.Windows.Application.Current.MainWindow)
                {
                    window.Close();
                }
            }
            System.Windows.Application.Current.MainWindow?.Close();
        }
    }
}