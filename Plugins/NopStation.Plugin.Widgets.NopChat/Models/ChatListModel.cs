using System;

namespace NopStation.Plugin.Widgets.NopChat.Models
{
    public record ChatListModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string AvatarURL { get; set; }
        public DateTime? LastMesageDate { get; set; }
        public int NumberOfMessages { get; set; }
    }
}
