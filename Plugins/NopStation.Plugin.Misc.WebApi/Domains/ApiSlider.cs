using System;
using Nop.Core;

namespace NopStation.Plugin.Misc.WebApi.Domains
{
    public partial class ApiSlider : BaseEntity
    {
        public int PictureId { get; set; }

        public DateTime? ActiveStartDateUtc { get; set; }

        public DateTime? ActiveEndDateUtc { get; set; }

        public int SliderTypeId { get; set; }

        public int EntityId { get; set; }

        public int DisplayOrder { get; set; }

        public DateTime CreatedOnUtc { get; set; }


        public SliderType SliderType
        {
            get => (SliderType)SliderTypeId;
            set => SliderTypeId = (int)value;
        }
    }
}
