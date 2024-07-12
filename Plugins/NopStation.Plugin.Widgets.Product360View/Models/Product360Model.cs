using System.Collections.Generic;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Models;
using Nop.Web.Models.Media;

namespace NopStation.Plugin.Widgets.Product360View.Models
{
    /// <summary>
    /// Represents a product 360 model
    /// </summary>
    public partial record Product360Model : BaseNopEntityModel
    {
        #region Ctor

        public Product360Model()
        {
            AddPictureModel = new ProductPictureModel();
            ProductPictureSearchModel = new Picture360SearchModel();
            ImageSetting360Model = new ImageSetting360Model();
            PictureUrls = new List<string>();
            PanoramaPictureUrls = new List<string>();
            PictureModels = new List<PictureModel>();
        }

        #endregion

        #region Properties

        public ProductPictureModel AddPictureModel { get; set; }
        public Picture360SearchModel ProductPictureSearchModel { get; set; }
        public ImageSetting360Model ImageSetting360Model { get; set; }
        public List<string> PictureUrls { get; set; }
        public List<string> PanoramaPictureUrls { get; set; }
        public List<PictureModel> PictureModels { get; set; }

        #endregion
    }
}
