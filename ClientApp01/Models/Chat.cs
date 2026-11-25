using System;
using System.Security.RightsManagement;

namespace ClientApp01.Models
{
    public class Chat
    {
        public int ChatId { get; set; }
        public string ChatName { get; set; } = string.Empty;
        public string? ChatDescription { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreationDate { get; set; }
        public string LastMessage { get; set; } = string.Empty;
        public DateTime LastMessageTime { get; set; }
        public int ParticipantsCount { get; set; }
        public int ChatType { get; set; }
        public string DisplayName =>
            !string.IsNullOrEmpty(ChatName) ? ChatName : $"Чат #{ChatId}";
        public string DisplayInfo => $"Количество участников #{ParticipantsCount}";
    }
}