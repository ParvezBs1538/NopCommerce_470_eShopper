using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Widgets.PaldeskChat.Models;
using Nop.Services.Common;
using Nop.Services.Customers;

namespace NopStation.Plugin.Widgets.PaldeskChat.Components
{
    public class PaldeskChatViewComponent : NopStationViewComponent
    {
        #region Fields

        private readonly PaldeskChatSettings _paldeskChatSettings;
        private readonly ICustomerService _customerService;
        private readonly IWorkContext _workContext;
        private readonly IGenericAttributeService _genericAttributeService;

        #endregion

        #region Ctor

        public PaldeskChatViewComponent(PaldeskChatSettings paldeskChatSettings,
            ICustomerService customerService,
            IWorkContext workContext,
            IGenericAttributeService genericAttributeService)
        {
            _paldeskChatSettings = paldeskChatSettings;
            _customerService = customerService;
            _workContext = workContext;
            _genericAttributeService = genericAttributeService;
        }

        #endregion

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            if (!_paldeskChatSettings.EnablePlugin)
                return Content("");

            var model = new PublicPaldeskChatModel()
            {
                Key = _paldeskChatSettings.Key,
                SettingModeId = (int)_paldeskChatSettings.SettingMode,
                Script = _paldeskChatSettings.Script,
                ConfigureWithCustomerDataIfLoggedIn = _paldeskChatSettings.ConfigureWithCustomerDataIfLoggedIn
            };

            var customer = await _workContext.GetCurrentCustomerAsync();
            if (_paldeskChatSettings.ConfigureWithCustomerDataIfLoggedIn && await _customerService.IsRegisteredAsync(customer))
            {
                model.IsRegistered = true;
                model.CustomerEmail = customer.Email;
                model.CustomerUsername = customer.Username;
                model.CustomerGuid = customer.CustomerGuid;
                model.CustomerFirstName = customer.FirstName;
                model.CustomerLastName = customer.LastName;
                model.CustomerPhoneNumber = customer.Phone;
            }

            return View("~/Plugins/NopStation.Plugin.Widgets.PaldeskChat/Views/PublicInfo.cshtml", model);
        }
    }
}
