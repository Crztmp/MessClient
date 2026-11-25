using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientApp01.Models
{
    public class UserDataResponse
    {
        public User User { get; set; }
        public List<Contact> Contacts { get; set; }
        public List<ChatInfoDto> Chats { get; set; }
    }

    public class ChatInfoDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Type { get; set; }
    }
}
