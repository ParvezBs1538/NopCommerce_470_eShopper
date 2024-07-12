using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Misc.Core.Components;
using Nop.Web.Factories;
using System.Threading.Tasks;

namespace NopStation.Plugin.Widgets.PopupLogin.Components
{
    public class PopupLoginViewComponent : NopStationViewComponent
    {
        private readonly PopupLoginSettings _popupLoginSettings;
        private readonly ICustomerModelFactory _customerModelFactory;

        public PopupLoginViewComponent(PopupLoginSettings popupLoginSettings,
            ICustomerModelFactory customerModelFactory)
        {
            _popupLoginSettings = popupLoginSettings;
            _customerModelFactory = customerModelFactory;
        }

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            if (!_popupLoginSettings.EnablePopupLogin)
                return Content("");

            var model = await _customerModelFactory.PrepareLoginModelAsync(null);
            return View(model);
        }
    }
}
