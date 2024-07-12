using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Widgets.NopChat.Areas.Admin.Factories;
using NopStation.Plugin.Widgets.NopChat.Areas.Admin.Models;
using NopStation.Plugin.Widgets.NopChat.Hubs;
using NopStation.Plugin.Widgets.NopChat.Models;
using NopStation.Plugin.Widgets.NopChat.Services;

namespace NopStation.Plugin.Widgets.NopChat.Areas.Admin.Controllers
{
    public class NopChatAdminController : NopStationAdminController
    {
        #region Fields

        private readonly IWorkContext _workContext;
        private readonly NopChatHub _nopChatHub;
        private readonly INopChatMessageService _nopChatMessageService;
        private readonly INopChatModelFactory _nopChatModelFactory;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;

        #endregion

        #region Ctor

        public NopChatAdminController(IWorkContext workContext,
            NopChatHub nopChatHub,
            INopChatMessageService nopChatMessageService,
            INopChatModelFactory nopChatModelFactory,
            ISettingService settingService,
            IStoreContext storeContext)
        {
            _workContext = workContext;
            _nopChatHub = nopChatHub;
            _nopChatMessageService = nopChatMessageService;
            _nopChatModelFactory = nopChatModelFactory;
            _settingService = settingService;
            _storeContext = storeContext;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Configure()
        {
            var customer = _workContext.GetCurrentCustomerAsync();
            ViewBag.CustomerName = customer.Result.Username;

            var model = await _nopChatModelFactory.PrepareConfigurationModelAsync();

            return View("~/Plugins/NopStation.Plugin.Widgets.NopChat/Areas/Admin/Views/NopChatAdmin/Configure.cshtml", model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();

            var nopChatSettings = await _settingService.LoadSettingAsync<NopChatSettings>(storeScope);
            nopChatSettings = model.ToSettings(nopChatSettings);

            await _settingService.SaveSettingOverridablePerStoreAsync(nopChatSettings, x => x.Logo, model.Logo_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(nopChatSettings, x => x.OpenOnMonday, model.OpenOnMonday_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(nopChatSettings, x => x.OpenOnTuesday, model.OpenOnTuesday_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(nopChatSettings, x => x.OpenOnWednesday, model.OpenOnWednesday_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(nopChatSettings, x => x.OpenOnThursday, model.OpenOnThursday_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(nopChatSettings, x => x.OpenOnFriday, model.OpenOnFriday_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(nopChatSettings, x => x.OpenOnSaturday, model.OpenOnSaturday_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(nopChatSettings, x => x.OpenOnSunday, model.OpenOnSunday_OverrideForStore, storeScope, false);

            await _settingService.ClearCacheAsync();

            return RedirectToAction("Configure");
        }

        public async Task<IActionResult> AdminChatBox(int id)
        {
            var customerId = (await _workContext.GetCurrentCustomerAsync()).Id;
            var vendorId = (await _workContext.GetCurrentCustomerAsync()).VendorId;
            var model = new NopChatMessageModel
            {
                VendorId = vendorId,
                VendorCustomerId = customerId,
                ContactList = await _nopChatMessageService.GetVendorChatListListAsync(vendorId)
            };

            return View("~/Plugins/NopStation.Plugin.Widgets.NopChat/Areas/Admin/Views/NopChatAdmin/AdminChatBox.cshtml", model);
        }

        #endregion
    }
}
