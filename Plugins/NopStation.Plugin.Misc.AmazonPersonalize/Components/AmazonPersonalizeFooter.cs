using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Misc.Core.Components;

namespace NopStation.Plugin.Misc.AmazonPersonalize.Components
{
    public class AmazonPersonalizeFooterViewComponent : NopStationViewComponent
    {
        #region Fields

        private readonly AmazonPersonalizeSettings _amazonPersonalizeSettings;

        #endregion Fields

        #region Ctor

        public AmazonPersonalizeFooterViewComponent(AmazonPersonalizeSettings amazonPersonalizeSettings)
        {
            _amazonPersonalizeSettings = amazonPersonalizeSettings;
        }

        #endregion Ctor

        #region Methods

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            if (!_amazonPersonalizeSettings.EnableAmazonPersonalize)
                return Content("");

            return View();
        }

        #endregion Methods
    }
}