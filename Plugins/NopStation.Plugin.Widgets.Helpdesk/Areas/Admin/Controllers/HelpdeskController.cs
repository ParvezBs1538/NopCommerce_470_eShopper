using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Widgets.Helpdesk.Areas.Admin.Factories;
using NopStation.Plugin.Widgets.Helpdesk.Areas.Admin.Models;

namespace NopStation.Plugin.Widgets.Helpdesk.Areas.Admin.Controllers
{
    public class HelpdeskController : NopStationAdminController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly IHelpdeskModelFactory _helpdeskModelFactory;
        private readonly IPermissionService _permissionService;

        #endregion

        #region Ctor

        public HelpdeskController(ILocalizationService localizationService,
            INotificationService notificationService,
            IStoreContext storeContext,
            ISettingService settingService,
            IHelpdeskModelFactory helpdeskModelFactory,
            IPermissionService permissionService)
        {
            _localizationService = localizationService;
            _notificationService = notificationService;
            _storeContext = storeContext;
            _settingService = settingService;
            _helpdeskModelFactory = helpdeskModelFactory;
            _permissionService = permissionService;
        }

        #endregion

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(HelpdeskPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var model = await _helpdeskModelFactory.PrepareConfigurationModelAsync();
            return View("~/Plugins/NopStation.Plugin.Widgets.Helpdesk/Areas/Admin/Views/Helpdesk/Configure.cshtml", model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(HelpdeskPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var settings = await _settingService.LoadSettingAsync<HelpdeskSettings>(storeScope);
            settings = model.ToSettings(settings);

            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.SendEmailOnNewTicket, model.SendEmailOnNewTicket_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.SendEmailOnNewResponse, model.SendEmailOnNewResponse_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.SendEmailsTo, model.SendEmailsTo_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.EmailAccountId, model.EmailAccountId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.AllowCustomerToSetPriority, model.AllowCustomerToSetPriority_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.DefaultTicketPriorityId, model.DefaultTicketPriorityId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.AllowCustomerToUploadAttachmentInResponse, model.AllowCustomerToUploadAttachmentInResponse_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.AllowCustomerToUploadAttachmentInTicket, model.AllowCustomerToUploadAttachmentInTicket_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.EnableTicketDepartment, model.EnableTicketDepartment_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.TicketDepartmentRequired, model.TicketDepartmentRequired_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.DefaultTicketDepartmentId, model.DefaultTicketDepartmentId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.EnableTicketCategory, model.EnableTicketCategory_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.TicketCategoryRequired, model.TicketCategoryRequired_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.DefaultTicketCategoryId, model.DefaultTicketCategoryId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.ShowMenuInCustomerNavigation, model.ShowMenuInCustomerNavigation_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.NavigationWidgetZone, model.NavigationWidgetZone_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.AllowCustomerToCreateTicketFromOrderPage, model.AllowCustomerToCreateTicketFromOrderPage_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.OrderPageWidgetZone, model.OrderPageWidgetZone_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.MinimumTicketCreateInterval, model.MinimumTicketCreateInterval_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.MinimumResponseCreateInterval, model.MinimumResponseCreateInterval_OverrideForStore, storeScope, false);

            await _settingService.ClearCacheAsync();
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

            return RedirectToAction("Configure");
        }
    }
}