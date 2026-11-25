using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
//using ClientApp01;
using ClientApp01.Services;
using ClientApp01.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace ClientApp01.Views
{
    /// <summary>
    /// Логика взаимодействия для LoginSettingsWindow.xaml
    /// </summary>
    public partial class LoginSettingsWindow : Window
    {
        public LoginSettingsWindow()
        {
            InitializeComponent();
            DataContext = new LoginSettingsViewModel(App.Configuration, App.Services.GetRequiredService<INavigationService>());
        }
    }
}
