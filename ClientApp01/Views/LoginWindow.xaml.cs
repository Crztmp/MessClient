using Newtonsoft.Json.Linq;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;

namespace ClientApp01.Views
{
    public partial class LoginWindow : Window
    {
        public TextBox UsernameTextBox { get; private set; }
        //public PasswordBox PasswordBox { get; private set; }
        public LoginWindow()
        {

            InitializeComponent();
            // Load last username
            var appSettingsPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
            if (File.Exists(appSettingsPath))
            {
                var settings = JObject.Parse(File.ReadAllText(appSettingsPath));
                if (settings["LastUsername"] != null)
                {
                    UsernameBox.Text = settings["LastUsername"].ToString();
                    PasswordBox.Focus();
                }
            }
            PasswordBox.PasswordChanged += PasswordBox_PasswordChanged;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModels.LoginViewModel vm)
            {
                vm.Password = PasswordBox.Password;
            }
        }
    }
}
