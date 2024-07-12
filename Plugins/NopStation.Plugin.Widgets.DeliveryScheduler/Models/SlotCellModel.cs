using System;
using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.DeliveryScheduler.Models
{
    public record SlotCellModel : BaseNopModel
    {
        public int SlotId { get; set; }

        public string SlotName { get; set; }

        public int Capacity { get; set; }

        public DateTime SlotDate { get; set; }
    }
}
