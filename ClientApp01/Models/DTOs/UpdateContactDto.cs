using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientApp01.Models
{
    public class UpdateContactDto
    {
        public int ContactId { get; set; }
        public string CustomName { get; set; }
        public bool? IsIgnored { get; set; }
        public bool? DoNotDisturb { get; set; }
        public bool? IsBlocked { get; set; }
    }
}
