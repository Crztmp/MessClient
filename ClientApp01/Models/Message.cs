using System;

namespace ClientApp01.Models
{
    public class Message
    {
        public int MessageID { get; set; }
        public string MessageText { get; set; }
        public DateTime SendDate { get; set; }
        public string SenderId { get; set; }
        public string RecipientId { get; set; }
        public int ChatId { get; set; }
        public string SenderName { get; set; }
        public string SenderFullName { get; set; }
        public bool IsMyMessage { get; set; }
        public bool IsRead { get; set; }
        public bool IsEdited { get; set; }
        public string AttachmentURL { get; set; }
    }
}