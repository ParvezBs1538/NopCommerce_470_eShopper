using System;
using Nop.Core;

namespace NopStation.Plugin.Widgets.NopChat.Domains
{
    public partial class NopChatMessage : BaseEntity
    {
        public string Text { get; set; }
        public DateTime? DateCreated { get; set; }
        public int CustomerId { get; set; }
        public int VendorCustomerId { get; set; }
        public int VendorId { get; set; }
        public bool IsVendorResponse { get; set; }
        public bool IsChecked { get; set; }
    }
}