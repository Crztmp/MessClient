using System;
using System.Collections.Generic;

namespace ClientApp01.Models
{
    public class User
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string AvatarUrl { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsActiveDirectory { get; set; }
        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;
        public DateTime? LastOnline { get; set; }
        public string DomainName { get; set; }
        public DateTime? LastADSyncDate { get; set; }
        //public ICollection<ChatUser> Chats { get; set; }

        public string DisplayName =>
            !string.IsNullOrEmpty(FirstName) && !string.IsNullOrEmpty(LastName)
                ? $"{FirstName} {LastName}"
                : Username;
    }
}