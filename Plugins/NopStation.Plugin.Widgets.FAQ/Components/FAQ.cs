using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Infrastructure;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Widgets.FAQ.Models;

namespace NopStation.Plugin.Widgets.FAQ.Components
{
    public class FAQViewComponent : NopStationViewComponent
    {
        private readonly FAQSettings _faqSettings;

        public FAQViewComponent(FAQSettings faqSettings)
        {
            _faqSettings = faqSettings;
        }

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            if (!_faqSettings.EnablePlugin)
                return Content("");

            if (!_faqSettings.IncludeInFooter && !_faqSettings.IncludeInTopMenu)
                return Content("");

            if (widgetZone != PublicWidgetZones.Footer)
            {
                if (!_faqSettings.IncludeInTopMenu)
                    return Content("");

                var model = new PublicInfoModel();
                model.TopMenu = true;
                return View(model);
            }
            else
            {
                if (!_faqSettings.IncludeInFooter)
                    return Content("");

                var model = new PublicInfoModel();
                model.FooterElementSelector = _faqSettings.FooterElementSelector;
                return View(model);
            }
        }
    }
}
