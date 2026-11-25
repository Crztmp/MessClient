using ClientApp01.Models;
using ClientApp01.Services;
using ClientApp01.ViewModels;
using Microsoft.Extensions.DependencyInjection;
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

namespace ClientApp01.Views
{
    public partial class AddContactDialog : Window
    {
        public AddContactDialog()
        {
            InitializeComponent();
            DataContext = new AddContactViewModel(
                App.Services.GetRequiredService<ApiService>(),
                App.Services.GetRequiredService<AppState>());
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
        private void Add_Click(object sender, RoutedEventArgs e) => DialogResult = true;
    }
}
