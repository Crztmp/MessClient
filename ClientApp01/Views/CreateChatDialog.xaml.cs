using ClientApp01.Models;
using ClientApp01.Services;
using ClientApp01.ViewModels;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
using Microsoft.Extensions.Logging;

namespace ClientApp01.Views
{

    public partial class CreateChatDialog : Window
    //private readonly ILogger _logger;
    {
        public CreateChatDialog()
        {
            InitializeComponent();
            DataContext = new CreateChatDialogViewModel(
                App.Services.GetRequiredService<ApiService>(),
                App.Services.GetRequiredService<AppState>(),
                App.Services.GetRequiredService<ILogger<ApiService>>());
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}