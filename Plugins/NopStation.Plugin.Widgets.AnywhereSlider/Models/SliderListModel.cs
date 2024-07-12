using System.Collections.Generic;
using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.AnywhereSlider.Models
{
    public record SliderListModel : BaseNopModel
    {
        public SliderListModel()
        {
            Sliders = new List<SliderOverviewModel>();
        }

        public List<SliderOverviewModel> Sliders { get; set; }

        public record SliderOverviewModel : BaseNopEntityModel
        {
            public bool ShowBackgroundPicture { get; set; }

            public string BackgroundPictureUrl { get; set; }
        }
    }
}
