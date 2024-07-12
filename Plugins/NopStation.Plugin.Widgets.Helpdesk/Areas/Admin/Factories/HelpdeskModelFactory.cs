using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Services;
using Nop.Services.Configuration;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using NopStation.Plugin.Widgets.Helpdesk.Areas.Admin.Models;
using NopStation.Plugin.Widgets.Helpdesk.Domains;
using NopStation.Plugin.Widgets.Helpdesk.Services;

namespace NopStation.Plugin.Widgets.Helpdesk.Areas.Admin.Factories
{
    public class HelpdeskModelFactory : IHelpdeskModelFactory
    {
        #region Fields

        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly IDepartmentService _departmentService;
        private readonly IStaffService _staffService;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;

        #endregion

        #region Ctor

        public HelpdeskModelFactory(IStoreContext storeContext,
            ISettingService settingService,
            IDepartmentService departmentService,
            IStaffService staffService,
            IBaseAdminModelFactory baseAdminModelFactory)
        {
            _storeContext = storeContext;
            _settingService = settingService;
            _departmentService = departmentService;
            _staffService = staffService;
            _baseAdminModelFactory = baseAdminModelFactory;
        }

        #endregion

        #region Methods

        public async Task<ConfigurationModel> PrepareConfigurationModelAsync()
        {
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var settings = await _settingService.LoadSettingAsync<HelpdeskSettings>(storeScope);

            var model = settings.ToSettingsModel<ConfigurationModel>();
            model.ActiveStoreScopeConfiguration = storeScope;

            await _baseAdminModelFactory.PrepareEmailAccountsAsync(model.AvailableEmailAccounts, false);
            model.AvailableDepartments = (await _departmentService.GetAllHelpdeskDepartmentsAsync())
                .Select(x => new SelectListItem()
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                }).ToList();
            model.AvailablePriorities = (await TicketPriority.High.ToSelectListAsync()).ToList();
            model.AvailableCategories = (await TicketCategory.Order.ToSelectListAsync()).ToList();
            model.AvailableNavigationWidgetZones = new List<SelectListItem>()
            {
                new SelectListItem()
                {
                    Value = "account_navigation_before",
                    Text = "account_navigation_before"
                },
                new SelectListItem()
                {
                    Value = "account_navigation_after",
                    Text = "account_navigation_after"
                }
            };
            model.AvailableOrderPageWidgetZones = new List<SelectListItem>()
            {
                new SelectListItem()
                {
                    Value = "orderdetails_page_top",
                    Text = "orderdetails_page_top"
                },
                new SelectListItem()
                {
                    Value = "orderdetails_page_overview",
                    Text = "orderdetails_page_overview"
                },
                new SelectListItem()
                {
                    Value = "orderdetails_page_beforeproducts",
                    Text = "orderdetails_page_beforeproducts"
                },
                new SelectListItem()
                {
                    Value = "orderdetails_product_line",
                    Text = "orderdetails_product_line"
                },
                new SelectListItem()
                {
                    Value = "orderdetails_page_afterproducts",
                    Text = "orderdetails_page_afterproducts"
                },
                new SelectListItem()
                {
                    Value = "orderdetails_page_bottom",
                    Text = "orderdetails_page_bottom"
                }
            };

            if (storeScope == 0)
                return model;

            model.SendEmailOnNewTicket_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.SendEmailOnNewTicket, storeScope);
            model.SendEmailOnNewResponse_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.SendEmailOnNewResponse, storeScope);
            model.SendEmailsTo_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.SendEmailsTo, storeScope);
            model.EmailAccountId_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.EmailAccountId, storeScope);
            model.AllowCustomerToSetPriority_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.AllowCustomerToSetPriority, storeScope);
            model.AllowCustomerToUploadAttachmentInResponse_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.AllowCustomerToUploadAttachmentInResponse, storeScope);
            model.AllowCustomerToUploadAttachmentInTicket_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.AllowCustomerToUploadAttachmentInTicket, storeScope);
            model.EnableTicketDepartment_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.EnableTicketDepartment, storeScope);
            model.TicketDepartmentRequired_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.TicketDepartmentRequired, storeScope);
            model.DefaultTicketDepartmentId_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.DefaultTicketDepartmentId, storeScope);
            model.EnableTicketCategory_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.EnableTicketCategory, storeScope);
            model.TicketCategoryRequired_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.TicketCategoryRequired, storeScope);
            model.DefaultTicketCategoryId_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.DefaultTicketCategoryId, storeScope);
            model.ShowMenuInCustomerNavigation_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.ShowMenuInCustomerNavigation, storeScope);
            model.NavigationWidgetZone_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.NavigationWidgetZone, storeScope);
            model.AllowCustomerToCreateTicketFromOrderPage_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.AllowCustomerToCreateTicketFromOrderPage, storeScope);
            model.OrderPageWidgetZone_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.OrderPageWidgetZone, storeScope);
            model.MinimumTicketCreateInterval_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.MinimumTicketCreateInterval, storeScope);
            model.MinimumResponseCreateInterval_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.MinimumResponseCreateInterval, storeScope);

            return model;
        }

        #endregion
    }
}
