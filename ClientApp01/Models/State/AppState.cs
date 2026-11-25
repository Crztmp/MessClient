using CommunityToolkit.Mvvm.ComponentModel;
using ClientApp01;
using ClientApp01.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ClientApp01.Models
{
    public partial class AppState : ObservableObject
    {
        [ObservableProperty]
        private User _currentUser;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsAuthenticated))]
        private string _authToken;

        [ObservableProperty]
        //private List<Contact> _contacts = new();
        private ObservableCollection<Contact> _contacts = new();

        [ObservableProperty]
        //private List<Chat> _chats = new();
        private ObservableCollection<Chat> _chats = new();

        [ObservableProperty]
        private ObservableCollection<Message> _messages = new();

        public bool IsAuthenticated => !string.IsNullOrEmpty(AuthToken);
        
        public void ClearState()
        {
            AuthToken = null;
            CurrentUser = null;
            Contacts.Clear();
            Chats.Clear();
        }
    }
}