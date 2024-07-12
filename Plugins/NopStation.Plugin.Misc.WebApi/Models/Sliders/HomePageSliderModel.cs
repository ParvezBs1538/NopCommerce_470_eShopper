using System.Collections.Generic;

namespace NopStation.Plugin.Misc.WebApi.Models.Sliders
{
    public class HomePageSliderModel
    {
        public HomePageSliderModel()
        {
            Sliders = new List<SliderModel>();
        }

        public bool IsEnabled { get; set; }

        public IList<SliderModel> Sliders { get; set; }


        public class SliderModel : BaseApiEntityModel
        {
            public string ImageUrl { get; set; }

            public int SliderType { get; set; }

            public int EntityId { get; set; }
        }
    }
}
