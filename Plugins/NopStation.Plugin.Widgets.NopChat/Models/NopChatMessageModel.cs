using System;
using System.Collections.Generic;
using Nop.Core;

namespace NopStation.Plugin.Widgets.NopChat.Models
{
    public class NopChatMessageModel : BaseEntity
    {
        public string Text { get; set; }
        public DateTime? DateCreated { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public int VendorCustomerId { get; set; }
        public string VendorCustomerName { get; set; }
        public int VendorId { get; set; }
        public string VendorName { get; set; }
        public bool IsVendorResponse { get; set; }
        public bool IsChecked { get; set; }
        public string VendorAvatar { get; set; }
        public string CustomerAvatar { get; set; }

        public IList<ChatListModel> ContactList { get; set; }
    }
}
