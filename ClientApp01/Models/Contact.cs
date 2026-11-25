using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientApp01.Models
{
    public class Contact
    {
        public int ContactId { get; set; }
        public string? ContactUserId { get; set; }
        public bool? Added { get; set; }
        public bool? IsBlocked { get; set; }
        public bool? IsIgnored { get; set; }
        public bool? DoNotDisturb { get; set; }
        public DateTime? AddedDate { get; set; }
        public string? UserId { get; set; } 
        public string? ContactName { get; set; }
        public User? ContactUser { get; set; }
        public Chat? Chat { get; set; }
        public int? ChatId { get; set; }
        public byte? ChatType { get; set; }
        public bool? IsDeleted { get; set; }
        public DateTime? LastMessageLoadedDate { get; set; }
        public DateTime? LastMessageTime { get; set; }
        public string? CustomDisplayName { get; set; }

        public string DisplayName =>
            !string.IsNullOrEmpty(CustomDisplayName) ? CustomDisplayName :
            (!string.IsNullOrEmpty(ContactName) ? ContactName :
            (!string.IsNullOrEmpty(Chat.DisplayName) ? Chat.DisplayName :
            (ContactUser?.Username ?? "Unknown")));

    //    public string DisplayName =>
    //!string.IsNullOrEmpty(CustomDisplayName) ? CustomDisplayName :
    //(!string.IsNullOrEmpty(ContactName) ? ContactName :
    //(!string.IsNullOrEmpty(Chat.DisplayName) ? Chat.DisplayName :
    //(ContactUser?.Username ?? "Unknown")));
    }
}
