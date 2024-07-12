using NopStation.Plugin.Widgets.ProductQuestionAnswer.Areas.Admin.Models;

namespace NopStation.Plugin.Widgets.ProductQuestionAnswer.Models
{
    public class ProductQAPublicInfoModel
    {
        #region Ctor
        public ProductQAPublicInfoModel()
        {
            ProductQAConfigurationModel = new ConfigurationModel();
            ProductQAModel = new ProductQAModel();
        }
        #endregion

        #region Properties

        public ConfigurationModel ProductQAConfigurationModel { get; set; }
        public ProductQAModel ProductQAModel { get; set; }
        public bool IsAccessToAskQuestion { get; set; }
        public bool IsQuestionAsAAnonymous { get; set; }

        #endregion
    }
}
