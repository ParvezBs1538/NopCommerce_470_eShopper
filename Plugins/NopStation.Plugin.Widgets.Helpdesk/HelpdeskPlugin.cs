using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Services.Cms;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Services;
using NopStation.Plugin.Widgets.Helpdesk.Components;

namespace NopStation.Plugin.Widgets.Helpdesk
{
    public class HelpdeskPlugin : BasePlugin, IAdminMenuPlugin, IWidgetPlugin, INopStationPlugin
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly HelpdeskSettings _ticketsSettings;
        private readonly IPermissionService _permissionService;
        private readonly INopStationCoreService _nopStationCoreService;
        private readonly IWebHelper _webHelper;

        #endregion

        #region Ctor

        public HelpdeskPlugin(ILocalizationService localizationService,
            HelpdeskSettings ticketsSettings,
            IPermissionService permissionService,
            INopStationCoreService nopStationCoreService,
            IWebHelper webHelper)
        {
            _localizationService = localizationService;
            _ticketsSettings = ticketsSettings;
            _permissionService = permissionService;
            _nopStationCoreService = nopStationCoreService;
            _webHelper = webHelper;
        }

        #endregion

        public bool HideInWidgetList => false;

        public Type GetWidgetViewComponent(string widgetZone)
        {
            if (widgetZone == _ticketsSettings.OrderPageWidgetZone)
                return typeof(HelpdeskOrderViewComponent);

            return typeof(HelpdeskNavigationViewComponent);
        }

        public Task<IList<string>> GetWidgetZonesAsync()
        {
            var list = new List<string>();

            if (_ticketsSettings.AllowCustomerToCreateTicketFromOrderPage)
                list.Add(_ticketsSettings.OrderPageWidgetZone);
            if (_ticketsSettings.ShowMenuInCustomerNavigation)
                list.Add(_ticketsSettings.NavigationWidgetZone);

            return Task.FromResult<IList<string>>(list);
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menu = new SiteMapNode()
            {
                Visible = true,
                IconClass = "far fa-dot-circle",
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.Helpdesk.Menu.Helpdesk")
            };

            #region Tickets

            if (await _permissionService.AuthorizeAsync(HelpdeskPermissionProvider.ManageTickets))
            {
                var campaign = new SiteMapNode()
                {
                    Visible = true,
                    IconClass = "far fa-circle",
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Helpdesk.Menu.Tickets"),
                    Url = $"{_webHelper.GetStoreLocation()}Admin/Ticket/List",
                    SystemName = "Tickets"
                };
                menu.ChildNodes.Add(campaign);
            }

            #endregion

            #region Categories

            if (await _permissionService.AuthorizeAsync(HelpdeskPermissionProvider.ManageCategories))
            {
                var notificationTemplate = new SiteMapNode()
                {
                    Visible = true,
                    IconClass = "far fa-circle",
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Helpdesk.Menu.Departments"),
                    Url = $"{_webHelper.GetStoreLocation()}Admin/HelpdeskDepartment/List",
                    SystemName = "Helpdesk.Departments"
                };
                menu.ChildNodes.Add(notificationTemplate);
            }

            #endregion

            #region Staffs

            if (await _permissionService.AuthorizeAsync(HelpdeskPermissionProvider.ManageStaffs))
            {
                var queue = new SiteMapNode
                {
                    Visible = true,
                    IconClass = "far fa-circle",
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Helpdesk.Menu.Staffs"),
                    Url = $"{_webHelper.GetStoreLocation()}Admin/HelpdeskStaff/List",
                    SystemName = "Helpdesk.Staffs"
                };
                menu.ChildNodes.Add(queue);
            }

            #endregion

            #region Configuration

            if (await _permissionService.AuthorizeAsync(HelpdeskPermissionProvider.ManageConfiguration))
            {
                var queue = new SiteMapNode
                {
                    Visible = true,
                    IconClass = "far fa-circle",
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Helpdesk.Menu.Configuration"),
                    Url = $"{_webHelper.GetStoreLocation()}Admin/Helpdesk/Configure",
                    SystemName = "Helpdesk.Configuration"
                };
                menu.ChildNodes.Add(queue);
            }

            #endregion

            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/help-desk-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=help-desk",
                    Visible = true,
                    IconClass = "far fa-circle",
                    OpenUrlInNewTab = true
                };
                menu.ChildNodes.Add(documentation);
            }

            await _nopStationCoreService.ManageSiteMapAsync(rootNode, menu, NopStationMenuType.Plugin);
        }

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/Helpdesk/Configure";
        }

        public override async Task InstallAsync()
        {
            await this.InstallPluginAsync(new HelpdeskPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await this.UninstallPluginAsync(new HelpdeskPermissionProvider());
            await base.UninstallAsync();
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            var list = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Tickets.List.Email", "Email"),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Tickets.List.Email.Hint", "Search By Email"),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Tickets.List.Category", "Category"),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Tickets.List.Category.Hint", "Select Category To Search"),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Tickets.List.Priority", "Priority"),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Tickets.List.Priority.Hint", "Select Priority To Search"),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Tickets.List.Status", "Status"),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Tickets.List.Status.Hint", "Select Status To Search"),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Tickets.List.CreatedFrom", "Created From"),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Tickets.List.CreatedFrom.Hint", "Search By Created From"),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Tickets.List.CreatedTo", "Created To"),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Tickets.List.CreatedTo.Hint", "Search By Created To"),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Tickets.List.AvailableCategory.All", "All"),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Tickets.List.AvailablePriority.All", "All"),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Tickets.List.AvailableStatus.All", "All"),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Responses.Fields.CreateBy", "Created by"),

                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Menu.Helpdesk", "Helpdesk"),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Menu.Tickets", "Tickets"),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Menu.Departments", "Departments"),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Menu.Staffs", "Staffs"),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Menu.Configuration", "Configuration"),

                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Departments.Fields.Name", "Name"),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Departments.Fields.Name.Hint", "The department name."),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Departments.Fields.Email", "Email"),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Departments.Fields.Email.Hint", "The department email."),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Departments.Fields.DisplayOrder", "Display order"),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Departments.Fields.DisplayOrder.Hint", "The department display order. 1 represents the first item in the list."),

                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Responses.Fields.Body", "Body"),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Responses.Fields.Body.Hint", "The response body."),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Responses.Fields.DownloadId", "Attachment"),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Responses.Fields.DownloadId.Hint", "The ticket response attachment."),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Responses.Fields.DisplayToCustomer", "Display to customer"),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Responses.Fields.DisplayToCustomer.Hint", "Determines whether to display to customer or not."),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Responses.Fields.CreatedBy", "Created by"),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Responses.Fields.CreatedBy.Hint", "Response created by."),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Responses.Fields.CreatedOn", "Created on"),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Responses.Fields.CreatedOn.Hint", "The create date of response."),

                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Staffs.Fields.Name", "Name"),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Staffs.Fields.Name.Hint", "The staff name."),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Staffs.Fields.Email", "Email"),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Staffs.Fields.Email.Hint", "The staff email."),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Staffs.Fields.DisplayOrder", "Display order"),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Staffs.Fields.DisplayOrder.Hint", "The staff display order. 1 represents the first item in the list."),

                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Tickets.Fields.Name", "Name"),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Tickets.Fields.Name.Hint", "The customer name."),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Tickets.Fields.TicketGuid", "Ticket guid"),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Tickets.Fields.TicketGuid.Hint", "The ticket guid."),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Tickets.Fields.Subject", "Subject"),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Tickets.Fields.Subject.Hint", "The ticket subject."),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Tickets.Fields.Body", "Body"),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Tickets.Fields.Body.Hint", "The ticket body."),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Tickets.Fields.Email", "Email"),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Tickets.Fields.Email.Hint", "The customer email."),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Tickets.Fields.PhoneNumber", "Phone number"),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Tickets.Fields.PhoneNumber.Hint", "The customer phone number."),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Tickets.Fields.DepartmentId", "Department"),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Tickets.Fields.DepartmentId.Hint", "The ticket department."),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Tickets.Fields.StaffId", "Staff"),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Tickets.Fields.StaffId.Hint", "The ticket staff."),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Tickets.Fields.StoreId", "Store"),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Tickets.Fields.StoreId.Hint", "The ticket store."),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Tickets.Fields.CustomerId", "Customer"),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Tickets.Fields.CustomerId.Hint", "The customer."),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Tickets.Fields.CategoryId", "Category"),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Tickets.Fields.CategoryId.Hint", "The ticket category."),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Tickets.Fields.OrderId", "Order ID"),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Tickets.Fields.OrderId.Hint", "The order ID."),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Tickets.Fields.ProductId", "Product ID"),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Tickets.Fields.ProductId.Hint", "The product ID."),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Tickets.Fields.StatusId", "Status"),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Tickets.Fields.StatusId.Hint", "The ticket status."),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Tickets.Fields.DownloadId", "Attachment"),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Tickets.Fields.DownloadId.Hint", "The ticket attachment."),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Tickets.Fields.PriorityId", "Priority"),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Tickets.Fields.PriorityId.Hint", "The ticket priority."),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Tickets.Fields.CreatedBy", "Created by"),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Tickets.Fields.CreatedBy.Hint", "The ticket created by."),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Tickets.Fields.CreatedOn", "Created on"),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Tickets.Fields.CreatedOn.Hint", "The create date of ticket."),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Tickets.Fields.UpdatedOn", "Updated on"),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Tickets.Fields.UpdatedOn.Hint", "The update date of ticket."),

                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Configuration", "Helpdesk settings"),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Departments.List", "Departments"),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Staffs.List", "Staffs"),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Tickets.List", "Tickets"),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Departments.AddNew", "Add new department"),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Staffs.AddNew", "Add new staff"),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Tickets.AddNew", "Add new ticket"),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Departments.EditDepartmentDetails", "Edit department details"),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Staffs.EditStaffDetails", "Edit staff details"),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Tickets.EditTicketDetails", "Edit ticket details"),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Departments.BackToList", "back to department list"),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Staffs.BackToList", "back to staff list"),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Tickets.BackToList", "back to ticket list"),

                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Tickets.Info", "Info"),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Tickets.Responses", "Responses"),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Departments.Fields.Name.Required", "The name is required."),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Staffs.Fields.Name.Required", "The name is required."),

                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Tickets.Fields.Name.Required", "The name is required."),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Tickets.Fields.Body.Required", "The body is required."),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Tickets.Fields.Email.Required", "The email is required."),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Tickets.Fields.PhoneNumber.Required", "The phone number is required."),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Tickets.Fields.Subject.Required", "The subject is required."),

                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Configuration.Fields.SendEmailOnNewTicket", "Send email on new ticket"),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Configuration.Fields.SendEmailOnNewTicket.Hint", "Check to send email on create new ticket."),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Configuration.Fields.SendEmailOnNewResponse", "Send email on new comment"),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Configuration.Fields.SendEmailOnNewResponse.Hint", "Check to send email on create new comment."),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Configuration.Fields.SendEmailsTo", "Send emails to"),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Configuration.Fields.SendEmailsTo.Hint", "Send emails to."),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Configuration.Fields.EmailAccountId", "Email account"),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Configuration.Fields.EmailAccountId.Hint", "Select email account."),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Configuration.Fields.AllowCustomerToSetPriority", "Allow customer to set priority"),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Configuration.Fields.AllowCustomerToSetPriority.Hint", "Check to allow customer to set priority."),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Configuration.Fields.DefaultTicketPriorityId", "Default ticket priority"),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Configuration.Fields.DefaultTicketPriorityId.Hint", "Set default ticket priority."),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Configuration.Fields.AllowCustomerToUploadAttachmentInTicket", "Allow customer to upload attachment in ticket"),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Configuration.Fields.AllowCustomerToUploadAttachmentInTicket.Hint", "Check to allow customer to upload attachment in ticket."),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Configuration.Fields.AllowCustomerToUploadAttachmentInResponse", "Allow customer to upload attachment in response"),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Configuration.Fields.AllowCustomerToUploadAttachmentInResponse.Hint", "Check to allow customer to upload attachment in response."),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Configuration.Fields.EnableTicketDepartment", "Enable ticket department"),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Configuration.Fields.EnableTicketDepartment.Hint", "Check to enable ticket department."),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Configuration.Fields.TicketDepartmentRequired", "Ticket department required"),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Configuration.Fields.TicketDepartmentRequired.Hint", "Check to mark ticket department as required."),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Configuration.Fields.DefaultTicketDepartmentId", "Default ticket department"),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Configuration.Fields.DefaultTicketDepartmentId.Hint", "The default ticket department."),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Configuration.Fields.EnableTicketCategory", "Enable ticket category"),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Configuration.Fields.EnableTicketCategory.Hint", "Check to enable ticket category."),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Configuration.Fields.TicketCategoryRequired", "Ticket category required"),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Configuration.Fields.TicketCategoryRequired.Hint", "Check to mark ticket category as required."),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Configuration.Fields.DefaultTicketCategoryId", "Default ticket category"),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Configuration.Fields.DefaultTicketCategoryId.Hint", "The default ticket category."),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Configuration.Fields.ShowMenuInCustomerNavigation", "Show menu in customer navigation"),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Configuration.Fields.ShowMenuInCustomerNavigation.Hint", "Check to show menu in customer navigation."),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Configuration.Fields.NavigationWidgetZone", "Navigation widget zone"),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Configuration.Fields.NavigationWidgetZone.Hint", "The navigation widget zone."),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Configuration.Fields.AllowCustomerToCreateTicketFromOrderPage", "Allow customer to create ticket from order page"),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Configuration.Fields.AllowCustomerToCreateTicketFromOrderPage.Hint", "Check to allow customer to create ticket from order page."),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Configuration.Fields.OrderPageWidgetZone", "Order page widget zone"),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Configuration.Fields.OrderPageWidgetZone.Hint", "The order page widget zone."),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Configuration.Fields.MinimumTicketCreateInterval", "Minimum ticket create interval"),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Configuration.Fields.MinimumTicketCreateInterval.Hint", "Enter minimum ticket create interval in seconds."),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Configuration.Fields.MinimumResponseCreateInterval", "Minimum response create interval"),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Configuration.Fields.MinimumResponseCreateInterval.Hint", "Enter minimum response create interval in seconds."),

                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Departments.Created", "Department has been created successfully."),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Departments.Updated", "Department has been updated successfully."),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Departments.Deleted", "Department has been deleted successfully."),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Staffs.Created", "Staff has been created successfully."),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Staffs.Updated", "Staff has been updated successfully."),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Staffs.Deleted", "Staff has been deleted successfully."),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Tickets.Created", "Ticket has been created successfully."),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Tickets.Updated", "Ticket has been updated successfully."),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Tickets.Deleted", "Ticket has been deleted successfully."),

                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Responses.AddTitle", "Add new response"),
                new KeyValuePair<string, string>("Admin.NopStation.Helpdesk.Responses.AddButton", "Add response"),

                new KeyValuePair<string, string>("NopStation.Helpdesk.Tickets.View", "View"),
                new KeyValuePair<string, string>("NopStation.Helpdesk.Account.MyTickets", "My tickets"),
                new KeyValuePair<string, string>("NopStation.Helpdesk.Account.CreateNewTicket", "Create new ticket"),
                new KeyValuePair<string, string>("NopStation.Helpdesk.Navigation.MyTickets", "My tickets"),
                new KeyValuePair<string, string>("NopStation.Helpdesk.Tickets.ListIsEmpty", "Ticket list is empty."),
                new KeyValuePair<string, string>("NopStation.Helpdesk.Tickets.CreateTicket", "Create ticket"),

                new KeyValuePair<string, string>("NopStation.Helpdesk.Tickets.Name", "Name"),
                new KeyValuePair<string, string>("NopStation.Helpdesk.Tickets.Email", "Email"),
                new KeyValuePair<string, string>("NopStation.Helpdesk.Tickets.PhoneNumber", "Phone number"),
                new KeyValuePair<string, string>("NopStation.Helpdesk.Tickets.TicketGuid", "Ticket guid"),
                new KeyValuePair<string, string>("NopStation.Helpdesk.Tickets.Subject", "Subject"),
                new KeyValuePair<string, string>("NopStation.Helpdesk.Tickets.Body", "Body"),
                new KeyValuePair<string, string>("NopStation.Helpdesk.Tickets.Department", "Department"),
                new KeyValuePair<string, string>("NopStation.Helpdesk.Tickets.Category", "Category"),
                new KeyValuePair<string, string>("NopStation.Helpdesk.Tickets.Order", "Order"),
                new KeyValuePair<string, string>("NopStation.Helpdesk.Tickets.Product", "Product"),
                new KeyValuePair<string, string>("NopStation.Helpdesk.Tickets.Status", "Status"),
                new KeyValuePair<string, string>("NopStation.Helpdesk.Tickets.Priority", "Priority"),
                new KeyValuePair<string, string>("NopStation.Helpdesk.Tickets.Download", "Attachment"),
                new KeyValuePair<string, string>("NopStation.Helpdesk.Tickets.CreatedOn", "Created on"),
                new KeyValuePair<string, string>("NopStation.Helpdesk.Tickets.UpdatedOn", "Updated on"),

                new KeyValuePair<string, string>("NopStation.Helpdesk.Tickets.Name.Required", "The 'Name' is required."),
                new KeyValuePair<string, string>("NopStation.Helpdesk.Tickets.Body.Required", "The 'Body' is required."),
                new KeyValuePair<string, string>("NopStation.Helpdesk.Tickets.Subject.Required", "The 'Subject' is required."),
                new KeyValuePair<string, string>("NopStation.Helpdesk.Tickets.Email.Required", "The 'Email' is required."),
                new KeyValuePair<string, string>("NopStation.Helpdesk.Tickets.PhoneNumber.Required", "The 'Phone number' is required."),
                new KeyValuePair<string, string>("NopStation.Helpdesk.Tickets.SubmitButton", "Submit ticket"),
                new KeyValuePair<string, string>("NopStation.Helpdesk.Account.TicketDetails", "Ticket details"),
                new KeyValuePair<string, string>("NopStation.Helpdesk.Tickets.DownloadAttachment", "Download attachment"),
                new KeyValuePair<string, string>("NopStation.Helpdesk.Tickets.CreatedByCustomer", "CreatedBy"),
                new KeyValuePair<string, string>("NopStation.Helpdesk.Tickets.Deparment", "Department"),

                new KeyValuePair<string, string>("NopStation.Helpdesk.TicketResponses.Body.Required", "Response body is required."),
                new KeyValuePair<string, string>("NopStation.Helpdesk.TicketResponses.Body", "Reply"),
                new KeyValuePair<string, string>("NopStation.Helpdesk.TicketResponse.SubmitButton", "Submit"),
                new KeyValuePair<string, string>("NopStation.Helpdesk.TicketResponses.Download", "Attachment"),

                new KeyValuePair<string, string>("NopStation.Helpdesk.Tickets.Email.Subject", "New ticket created"),
                new KeyValuePair<string, string>("NopStation.Helpdesk.Tickets.Email.Body", "A new ticket has been created by {0}.<br/>Click <a href=\"{1}\">here</a> to check details.<br/><br/><br/>"),
                new KeyValuePair<string, string>("NopStation.Helpdesk.TicketResponses.Email.Subject", "New ticket response added"),
                new KeyValuePair<string, string>("NopStation.Helpdesk.TicketResponses.Email.Body", "A new ticket response has been added by {0}.<br/>Click <a href=\"{1}\">here</a> to check details.<br/><br/><br/>"),

                new KeyValuePair<string, string>("NopStation.Helpdesk.Tickets.MinTicketCreateInterval", "Please wait several seconds before creating a new ticket (already created another ticket several seconds ago)."),
                new KeyValuePair<string, string>("NopStation.Helpdesk.TicketResponses.MinResponseCreateInterval", "Please wait several seconds before creating a new response (already created another response several seconds ago)."),
            };

            return list;
        }
    }
}