using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using Nop.Core;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.ScheduleTasks;
using Nop.Services.Cms;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Plugins;
using Nop.Services.ScheduleTasks;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Services;
using NopStation.Plugin.Widgets.AbandonedCarts.Components;
using NopStation.Plugin.Widgets.AbandonedCarts.Services.Messages;

namespace NopStation.Plugin.Widgets.AbandonedCarts
{
    public class AbandonedCartsPlugin : BasePlugin, IAdminMenuPlugin, IWidgetPlugin, INopStationPlugin
    {
        #region Properties

        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;
        private readonly IScheduleTaskService _scheduleTaskService;
        private readonly IMessageTemplateService _messageTemplateService;
        private readonly INopStationCoreService _nopStationCoreService;
        private readonly IPermissionService _permissionService;

        #endregion

        #region Ctors

        public AbandonedCartsPlugin(ILocalizationService localizationService,
            IWebHelper webHelper,
            IScheduleTaskService scheduleTaskService,
            IMessageTemplateService messageTemplateService,
            INopStationCoreService nopStationCoreService,
            IPermissionService permissionService)
        {
            _localizationService = localizationService;
            _webHelper = webHelper;
            _scheduleTaskService = scheduleTaskService;
            _messageTemplateService = messageTemplateService;
            _nopStationCoreService = nopStationCoreService;
            _permissionService = permissionService;
        }

        #endregion

        #region Methods

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/WidgetsAbandonedCarts/Configure";
        }

        public bool HideInWidgetList => false;

        public Type GetWidgetViewComponent(string widgetZone)
        {
            return typeof(AbandonedCartViewComponent);
        }

        public Task<IList<string>> GetWidgetZonesAsync()
        {
            return Task.FromResult<IList<string>>(new List<string> { AdminWidgetZones.CustomerDetailsBlock, AdminWidgetZones.MaintenanceDetailsBlock });
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Plugins.Widgets.AbandonedCarts.Menu.AbandonedCarts"),
                Visible = true,
                IconClass = "far fa-dot-circle",
            };

            if (await _permissionService.AuthorizeAsync(AbandonedCartsPermissionProvider.ManageAbandonedCarts))
            {
                var configItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Plugins.Widgets.AbandonedCarts.Menu.Configuration"),
                    Url = "~/Admin/WidgetsAbandonedCarts/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "WidgetsAbandonedCarts.Configuration"
                };
                menuItem.ChildNodes.Add(configItem);

                var abandonedList = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Plugins.Widgets.AbandonedCarts.Menu.AbandonedCartsList"),
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "WidgetsAbandonedCarts.List",
                    ControllerName = "WidgetsAbandonedCarts",
                    ActionName = "List",
                    RouteValues = new RouteValueDictionary { { "area", AreaNames.ADMIN } }
                };
                menuItem.ChildNodes.Add(abandonedList);
            }
            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/abandoned-carts-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=abandoned-carts",
                    Visible = true,
                    IconClass = "far fa-circle",
                    OpenUrlInNewTab = true
                };
                menuItem.ChildNodes.Add(documentation);
            }

            await _nopStationCoreService.ManageSiteMapAsync(rootNode, menuItem, NopStationMenuType.Plugin);
        }

        public override async Task InstallAsync()
        {
            if (await _scheduleTaskService.GetTaskByTypeAsync("NopStation.Plugin.Widgets.AbandonedCarts.TaskService.UpdateAbandonedCartsTask") == null)
            {
                await _scheduleTaskService.InsertTaskAsync(new ScheduleTask
                {
                    Enabled = true,
                    Name = "Update Abandoned Carts Status",
                    Seconds = 300,           // 5 minutes.
                    StopOnError = false,
                    Type = "NopStation.Plugin.Widgets.AbandonedCarts.TaskService.UpdateAbandonedCartsTask",
                    LastEnabledUtc = DateTime.UtcNow,
                });
            }
            if (await _scheduleTaskService.GetTaskByTypeAsync("NopStation.Plugin.Widgets.AbandonedCarts.TaskService.DeleteAbandonedCartsTask") == null)
            {
                await _scheduleTaskService.InsertTaskAsync(new ScheduleTask
                {
                    Enabled = true,
                    Name = "Delete Abandoned Deleted Carts",
                    Seconds = 86400,           // 24 hours.
                    StopOnError = false,
                    Type = "NopStation.Plugin.Widgets.AbandonedCarts.TaskService.DeleteAbandonedCartsTask",
                    LastEnabledUtc = DateTime.UtcNow,
                });
            }
            await AddTemplatesAsync();
            await this.InstallPluginAsync(new AbandonedCartsPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            //schedule task
            var task = await _scheduleTaskService.GetTaskByTypeAsync("NopStation.Plugin.Widgets.AbandonedCarts.TaskService.UpdateAbandonedCartsTask");
            if (task != null)
                await _scheduleTaskService.DeleteTaskAsync(task);
            task = await _scheduleTaskService.GetTaskByTypeAsync("NopStation.Plugin.Widgets.AbandonedCarts.TaskService.DeleteAbandonedCartsTask");
            if (task != null)
                await _scheduleTaskService.DeleteTaskAsync(task);

            var messageTemplates = await _messageTemplateService.GetMessageTemplatesByNameAsync(AbandonedCartMessageTemplateSystemNames.ABANDONED_CARTS_CUSTOMER_NOTIFICATION);
            foreach (var messageTemplate in messageTemplates)
            {
                await _messageTemplateService.DeleteMessageTemplateAsync(messageTemplate);
            }

            await _localizationService.DeleteLocaleResourcesAsync("Plugins.Widgets.AbandonedCarts");

            await this.UninstallPluginAsync(new AbandonedCartsPermissionProvider());
            await base.UninstallAsync();
        }

        private async Task AddTemplatesAsync()
        {
            //message template
            var cartsReminderEmailTemplate = new MessageTemplate
            {
                Name = AbandonedCartMessageTemplateSystemNames.ABANDONED_CARTS_CUSTOMER_NOTIFICATION,
                Subject = $"Abandoned-cart reminder from %Store.Name%.",
                Body = $"{Environment.NewLine}<a href=\"%Store.URL%cart\">%Store.Name%</a>{Environment.NewLine}<br />" +
                $"{Environment.NewLine}<br />{Environment.NewLine}Hello %Customer.FullName%,{Environment.NewLine}<br />" +
                $"{Environment.NewLine}Hope you are doing well. You have added some products to your shopping cart. But haven't placed the orders. <br />" +
                $"{Environment.NewLine}Would you like to get them? Place order today!<br />{Environment.NewLine}<br />" +
                $"{Environment.NewLine}Below is the summary of the products you have added to the cart.{Environment.NewLine}<br />" +
                $"{Environment.NewLine}%Product(s)%<br />{Environment.NewLine}<br />{Environment.NewLine}Thank you for your time!<br />" +
                $"{Environment.NewLine}<br />{Environment.NewLine}Do not want to get notifications for Abandoned Carts? " +
                $"<a href=\"%Store.URL%AbandonmentSubscription/Unsubscribe?returnurl=%Customer.jwtToken%\">Unsubscribe</a>{Environment.NewLine}",
                IsActive = true,
                EmailAccountId = 1
            };

            await _messageTemplateService.InsertMessageTemplateAsync(cartsReminderEmailTemplate);
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            var list = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Plugins.Widgets.AbandonedCarts.Menu.AbandonedCarts", "Abandoned Carts"),
                new KeyValuePair<string, string>("Plugins.Widgets.AbandonedCarts.Menu.AbandonedCartsList", "Abandoned Carts List"),
                new KeyValuePair<string, string>("Plugins.Widgets.AbandonedCarts.Menu.Configuration", "Configuration"),

                new KeyValuePair<string, string>("Plugins.Widgets.AbandonedCarts.Fields.IsEnabled", "Is enabled?"),
                new KeyValuePair<string, string>("Plugins.Widgets.AbandonedCarts.Fields.IsEnabled.Hint", "Determine Is Enabled or not."),
                new KeyValuePair<string, string>("Plugins.Widgets.AbandonedCarts.Fields.AbandonmentCutOffTime", "Consider abandoned cart cut-off time"),
                new KeyValuePair<string, string>("Plugins.Widgets.AbandonedCarts.Fields.AbandonmentCutOffTime.Hint", "Enter abandoned cart cut-off time to be considered in minutes."),
                new KeyValuePair<string, string>("Plugins.Widgets.AbandonedCarts.Fields.NotificationSendingCondition", "Choose notification sending condition"),
                new KeyValuePair<string, string>("Plugins.Widgets.AbandonedCarts.Fields.NotificationSendingCondition.Hint", "Select the notification sending condition."),
                new KeyValuePair<string, string>("Plugins.Widgets.AbandonedCarts.Fields.IsEnabledFirstNotification", "Is enabled first notification?"),
                new KeyValuePair<string, string>("Plugins.Widgets.AbandonedCarts.Fields.IsEnabledFirstNotification.Hint", "Determine is enabled first notification or not."),
                new KeyValuePair<string, string>("Plugins.Widgets.AbandonedCarts.Fields.DurationAfterFirstAbandonment", "Duration after first abandonment"),
                new KeyValuePair<string, string>("Plugins.Widgets.AbandonedCarts.Fields.DurationAfterFirstAbandonment.Hint", "Enter duration after first abandonment in minutes."),
                new KeyValuePair<string, string>("Plugins.Widgets.AbandonedCarts.Fields.IsEnabledSecondNotification", "Is enabled second notification?"),
                new KeyValuePair<string, string>("Plugins.Widgets.AbandonedCarts.Fields.IsEnabledSecondNotification.Hint", "Determine is enabled second notification or not."),
                new KeyValuePair<string, string>("Plugins.Widgets.AbandonedCarts.Fields.DurationAfterSecondAbandonment", "Duration after second abandonment"),
                new KeyValuePair<string, string>("Plugins.Widgets.AbandonedCarts.Fields.DurationAfterSecondAbandonment.Hint", "Enter duration after second abandonment in minutes."),
                new KeyValuePair<string, string>("Plugins.Widgets.AbandonedCarts.Fields.CustomerOnlineCutoffTime", "Customer online cut-off time"),
                new KeyValuePair<string, string>("Plugins.Widgets.AbandonedCarts.Fields.CustomerOnlineCutoffTime.Hint", "Enter customer online cut-off time in minutes."),
                new KeyValuePair<string, string>("Plugins.Widgets.AbandonedCarts.Fields.DontSendSameDay", "Don't send on the same day"),
                new KeyValuePair<string, string>("Plugins.Widgets.AbandonedCarts.Fields.DontSendSameDay.Hint", "Enable not to send notification on the same day."),

                new KeyValuePair<string, string>("Enums.NopStation.Plugin.Widgets.AbandonedCarts.Domain.AbandonedType.AllAbandoned", "All Abandoned"),
                new KeyValuePair<string, string>("Enums.NopStation.Plugin.Widgets.AbandonedCarts.Domain.AbandonedType.AnyAbandoned", "Any Abandoned"),
                new KeyValuePair<string, string>("Enums.NopStation.Plugin.Widgets.AbandonedCarts.Domain.CustomerAbandonmentStatus.Subscribed", "Subscribed"),
                new KeyValuePair<string, string>("Enums.NopStation.Plugin.Widgets.AbandonedCarts.Domain.CustomerAbandonmentStatus.Unsubscribed", "Unsubscribed"),
                new KeyValuePair<string, string>("Enums.NopStation.Plugin.Widgets.AbandonedCarts.Domain.AbandonedStatus.AllCarts", "All Carts"),
                new KeyValuePair<string, string>("Enums.NopStation.Plugin.Widgets.AbandonedCarts.Domain.AbandonedStatus.InAction", "In Action"),
                new KeyValuePair<string, string>("Enums.NopStation.Plugin.Widgets.AbandonedCarts.Domain.AbandonedStatus.Abandoned", "Abandoned"),
                new KeyValuePair<string, string>("Enums.NopStation.Plugin.Widgets.AbandonedCarts.Domain.AbandonedStatus.Recovered", "Recovered"),
                new KeyValuePair<string, string>("Enums.NopStation.Plugin.Widgets.AbandonedCarts.Domain.AbandonedStatus.Deleted", "Deleted"),

                new KeyValuePair<string, string>("Plugins.Widgets.AbandonedCarts.Fields.Customer", "Customer"),
                new KeyValuePair<string, string>("Plugins.Widgets.AbandonedCarts.Fields.NotificationSentFrequency", "Notification sent frequency"),
                new KeyValuePair<string, string>("Plugins.Widgets.AbandonedCarts.Fields.Status", "Status"),
                new KeyValuePair<string, string>("Plugins.Widgets.AbandonedCarts.Fields.SubscriptionStatus", "Subscription Status"),
                new KeyValuePair<string, string>("Plugins.Widgets.AbandonedCarts.Fields.Status.Hint", "Select Status"),
                new KeyValuePair<string, string>("Plugins.Widgets.AbandonedCarts.Fields.Token", "Token"),
                new KeyValuePair<string, string>("Plugins.Widgets.AbandonedCarts.Fields.LastNotificationSentOn", "Last notification sent on"),
                new KeyValuePair<string, string>("Plugins.Widgets.AbandonedCarts.Fields.UnsubscribedOnUtc", "Unsubscribed on utc"),
                new KeyValuePair<string, string>("Plugins.Widgets.AbandonedCarts.Fields.ShoppingCartItem", "Shopping cart item"),
                new KeyValuePair<string, string>("Plugins.Widgets.AbandonedCarts.Fields.Product", "Product"),
                new KeyValuePair<string, string>("Plugins.Widgets.AbandonedCarts.Fields.Quantity", "Quantity"),
                new KeyValuePair<string, string>("Plugins.Widgets.AbandonedCarts.Fields.UnitPrice", "Unit Price"),
                new KeyValuePair<string, string>("Plugins.Widgets.AbandonedCarts.Fields.TotalPrice", "Total Price"),
                new KeyValuePair<string, string>("Plugins.Widgets.AbandonedCarts.Fields.ProductSku", "Product sku"),
                new KeyValuePair<string, string>("Plugins.Widgets.AbandonedCarts.Fields.OrderItem", "Order item"),
                new KeyValuePair<string, string>("Plugins.Widgets.AbandonedCarts.Fields.TotalItems", "Total Items"),
                new KeyValuePair<string, string>("Plugins.Widgets.AbandonedCarts.Fields.StatusChangedOn", "Status changed on"),
                new KeyValuePair<string, string>("Plugins.Widgets.AbandonedCarts.Fields.FirstNotificationSentOn", "First notification sent on"),
                new KeyValuePair<string, string>("Plugins.Widgets.AbandonedCarts.Fields.SecondNotificationSentOn", "Second notification sent on"),
                new KeyValuePair<string, string>("Plugins.Widgets.AbandonedCarts.Fields.IsSoftDeleted", "Is soft deleted?"),
                new KeyValuePair<string, string>("Plugins.Widgets.AbandonedCarts.Fields.ProductName", "Product name"),
                new KeyValuePair<string, string>("Plugins.Widgets.AbandonedCarts.Fields.ProductQuantity", "Quantity"),
                new KeyValuePair<string, string>("Plugins.Widgets.AbandonedCarts.Fields.LastActivityBefore", "Last activity before"),
                new KeyValuePair<string, string>("Plugins.Widgets.AbandonedCarts.Fields.LastActivityBefore.Hint", "Last activity of Abandoned Carts before selected date will be deleted."),
                new KeyValuePair<string, string>("Plugins.Widgets.AbandonedCarts.Fields.DeletingMaintenance", "Deleting abandonment carts from Abandoned Carts Plugin"),
                new KeyValuePair<string, string>("Plugins.Widgets.AbandonedCarts.Fields.TotalDeleted", "{0} items were deleted"),
                new KeyValuePair<string, string>("Plugins.Widgets.AbandonedCarts.Unsubscribe", "Unsubscribe"),
                new KeyValuePair<string, string>("Plugins.Widgets.AbandonedCarts.Fields.StartDate", "Start Date"),
                new KeyValuePair<string, string>("Plugins.Widgets.AbandonedCarts.Fields.StartDate.Hint", "The start date for the search (last activity date of a product in abandoned carts)."),
                new KeyValuePair<string, string>("Plugins.Widgets.AbandonedCarts.Fields.EndDate", "End Date"),
                new KeyValuePair<string, string>("Plugins.Widgets.AbandonedCarts.Fields.EndDate.Hint", "The end date for the search (last activity date of a product in abandoned carts)."),
            };
            return list;
        }

        #endregion
    }
}
