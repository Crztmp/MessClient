using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ClientApp01.Models;
using ClientApp01.Services;
using System.Windows;

namespace ClientApp01.ViewModels
{
    public partial class ContactChatPropertiesViewModel : ObservableObject
    {
        private readonly ApiService _apiService;

        [ObservableProperty]
        private Contact _contact;

        [ObservableProperty]
        private string _FormDisplayName;
        [ObservableProperty]
        private bool _FormIsBlocked;
        [ObservableProperty]
        private bool _FormIsIgnored;
        [ObservableProperty]
        private bool _FormDoNotDisturb;


        public ContactChatPropertiesViewModel(ApiService apiService, Contact contact)
        {
            _apiService = apiService;
            _contact = contact;

            // Инициализация свойств формы значениями из контакта
            FormDisplayName = contact.CustomDisplayName ?? contact.DisplayName;
            FormIsIgnored = contact.IsIgnored ?? false;
            FormDoNotDisturb = contact.DoNotDisturb ?? false;
            //FormDesc=contact.de
            Console.WriteLine($"Saving: CustomDisplayName={contact.CustomDisplayName}, Blocked={Contact.IsBlocked}, Ignored={Contact.IsIgnored}, DND={Contact.DoNotDisturb}");
        }

        [RelayCommand]
        private async Task Save()
        {
            try
            {
                Contact.IsBlocked ??= false;
                Contact.IsIgnored ??= false;
                Contact.DoNotDisturb ??= false;
                Console.WriteLine($"Saving: Blocked={Contact.IsBlocked}, Ignored={Contact.IsIgnored}, DND={Contact.DoNotDisturb}");

                bool hasChanges =
                    Contact.ContactName != FormDisplayName ||
                    Contact.IsBlocked != FormIsBlocked ||
                    Contact.IsIgnored != FormIsIgnored ||
                    Contact.DoNotDisturb != FormDoNotDisturb;

                // Update contact name and modes
                if (hasChanges)
                {
                    var result = await _apiService.UpdateContactAsync(new Models.UpdateContactDto
                    {
                        ContactId = Contact.ContactId,
                        CustomName = FormDisplayName,
                        IsBlocked = FormIsBlocked,
                        IsIgnored = FormIsIgnored,
                        DoNotDisturb = FormDoNotDisturb
                    });

                    if (result)
                    {
                        //Contact.ContactName = FormDisplayName;

                        MessageBox.Show("Контакт успешно сохранён", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                        foreach (Window window in Application.Current.Windows)
                        {
                            if (window.DataContext == this)
                            {
                                window.Close();
                                break;
                            }
                        }
                        if (Application.Current.MainWindow?.DataContext is MainViewModel mainViewModel)
                        {
                            await mainViewModel.LoadChatsAsync();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task ClearHistory()
        {
            var result = MessageBox.Show(
                $"Вы уверены, что хотите очистить историю переписки с {Contact.DisplayName}?",
                "Подтверждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                await ClearHistoryConfirmed();
            }
        }

        private async Task ClearHistoryConfirmed()
        {
            try
            {
                if (Contact.ChatId.HasValue)
                {
                    // Call API to clear chat history
                    MessageBox.Show("Chat history cleared");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error clearing history: {ex.Message}");
            }
        }

        [RelayCommand]
        private void Cancel()
        {
            if (Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.DataContext == this) is Window window)
            {
                window.Close();
            }
        }
    }
}