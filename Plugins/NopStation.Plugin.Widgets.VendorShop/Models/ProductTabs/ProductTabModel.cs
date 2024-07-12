using System.Collections.Generic;
using Nop.Web.Models.Media;

namespace NopStation.Plugin.Widgets.VendorShop.Models.ProductTabs
{
    public class ProductTabModel
    {
        public ProductTabModel()
        {
            Picture = new PictureModel();
            Items = new List<ProductTabItemModel>();
        }

        public int Id { get; set; }

        public string Title { get; set; }

        public bool DisplayTitle { get; set; }

        public PictureModel Picture { get; set; }

        public string CustomUrl { get; set; }

        public bool AutoPlay { get; set; }

        public string CustomCssClass { get; set; }

        public bool Loop { get; set; }

        public int Margin { get; set; }

        public int StartPosition { get; set; }

        public bool Center { get; set; }

        public bool Nav { get; set; }

        public bool LazyLoad { get; set; }

        public int LazyLoadEager { get; set; }

        public int AutoPlayTimeout { get; set; }

        public bool AutoPlayHoverPause { get; set; }

        public bool RTL { get; set; }

        public IList<ProductTabItemModel> Items { get; set; }
    }
}
