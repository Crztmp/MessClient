using ClientApp01.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace ClientApp01.Views.Controls
{
    public class ContactItemStyleSelector : StyleSelector
    {
        public Style NonAddedStyle { get; set; }
        public Style DefaultStyle { get; set; }

        public override Style SelectStyle(object item, DependencyObject container)
        {
            if (item is Contact contact)
            {
                return contact.Added == false ? NonAddedStyle : DefaultStyle;
            }
            return DefaultStyle;
        }
    }
}
