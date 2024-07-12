using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Services;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Menu;
using Nop.Services.ScheduleTasks;
using Nop.Core.Domain.ScheduleTasks;

namespace NopStation.Plugin.Misc.AutoCancelOrder
{
    public class AutoCancelOrderPlugin : BasePlugin, INopStationPlugin, IAdminMenuPlugin, IMiscPlugin
    {
        #region Fields

        private readonly IWebHelper _webHelper;
        private readonly INopStationCoreService _nopStationCoreService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IScheduleTaskService _scheduleTaskService;

        #endregion

        #region Ctor

        public AutoCancelOrderPlugin(IWebHelper webHelper,
            INopStationCoreService nopStationCoreService,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            ISettingService settingService,
            IScheduleTaskService scheduleTaskService)
        {
            _webHelper = webHelper;
            _nopStationCoreService = nopStationCoreService;
            _localizationService = localizationService;
            _permissionService = permissionService;
            _settingService = settingService;
            _scheduleTaskService = scheduleTaskService;
        }

        #endregion

        #region Methods

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/AutoCancelOrder/Configure";
        }

        public override async Task InstallAsync()
        {
            if (await _scheduleTaskService.GetTaskByTypeAsync(AutoCancelOrderDefaults.AutoCancelOrderTask) == null)
            {
                await _scheduleTaskService.InsertTaskAsync(new ScheduleTask
                {
                    Enabled = true,
                    Seconds = 300,
                    Name = AutoCancelOrderDefaults.AutoCancelOrderTaskName,
                    Type = AutoCancelOrderDefaults.AutoCancelOrderTask,
                });
            }

            await _settingService.SaveSettingAsync(new AutoCancelOrderSettings()
            {
                EnablePlugin = true,
                ApplyOnOrderStatuses = new List<int>
                {
                    (int)OrderStatus.Pending,
                    (int)OrderStatus.Processing
                },
                ApplyOnShippingStatuses = new List<int>
                {
                    (int)ShippingStatus.NotYetShipped
                }
            });

            await this.InstallPluginAsync(new AutoCancelOrderPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            if (await _scheduleTaskService.GetTaskByTypeAsync(AutoCancelOrderDefaults.AutoCancelOrderTask) is ScheduleTask scheduleTask)
                await _scheduleTaskService.DeleteTaskAsync(scheduleTask);

            await this.UninstallPluginAsync(new AutoCancelOrderPermissionProvider());
            await base.UninstallAsync();
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.AutoCancelOrder.Menu.AutoCancelOrder"),
                Visible = true,
                IconClass = "far fa-dot-circle",
            };

            if (await _permissionService.AuthorizeAsync(AutoCancelOrderPermissionProvider.ManageConfiguration))
            {
                var configItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.AutoCancelOrder.Menu.Configuration"),
                    Url = "~/Admin/AutoCancelOrder/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "AutoCancelOrder.Configuration"
                };
                menuItem.ChildNodes.Add(configItem);
            }
            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/auto-cancel-order-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=auto-cancel-order",
                    Visible = true,
                    IconClass = "far fa-circle",
                    OpenUrlInNewTab = true
                };
                menuItem.ChildNodes.Add(documentation);
            }

            await _nopStationCoreService.ManageSiteMapAsync(rootNode, menuItem, NopStationMenuType.Plugin);
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            var list = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Admin.NopStation.AutoCancelOrder.Menu.Configuration", "Configuration"),
                new KeyValuePair<string, string>("Admin.NopStation.AutoCancelOrder.Menu.AutoCancelOrder", "Auto cancel order"),

                new KeyValuePair<string, string>("Admin.NopStation.AutoCancelOrder.Configuration", "Auto cancel order settings"),
                new KeyValuePair<string, string>("Admin.NopStation.AutoCancelOrder.Configuration.Fields.EnablePlugin", "Enable plugin"),
                new KeyValuePair<string, string>("Admin.NopStation.AutoCancelOrder.Configuration.Fields.EnablePlugin.Hint", "Determines whether the plugin is enabled or not."),
                new KeyValuePair<string, string>("Admin.NopStation.AutoCancelOrder.Configuration.Fields.SendNotificationToCustomer", "Send notification to customer"),
                new KeyValuePair<string, string>("Admin.NopStation.AutoCancelOrder.Configuration.Fields.SendNotificationToCustomer.Hint", "Determines whether to send cancel order notification to customer or not."),
                new KeyValuePair<string, string>("Admin.NopStation.AutoCancelOrder.Configuration.Fields.ApplyOnOrderStatuses", "Apply on order statuses"),
                new KeyValuePair<string, string>("Admin.NopStation.AutoCancelOrder.Configuration.Fields.ApplyOnOrderStatuses.Hint", "Define apply on order statuses."),
                new KeyValuePair<string, string>("Admin.NopStation.AutoCancelOrder.Configuration.Fields.ApplyOnShippingStatuses", "Apply on shipping statuses"),
                new KeyValuePair<string, string>("Admin.NopStation.AutoCancelOrder.Configuration.Fields.ApplyOnShippingStatuses.Hint", "Define apply on order shipping statuses."),
                new KeyValuePair<string, string>("Admin.NopStation.AutoCancelOrder.Configuration.Fields.ApplyOnPaymentMethods", "Apply on payment methods"),
                new KeyValuePair<string, string>("Admin.NopStation.AutoCancelOrder.Configuration.Fields.ApplyOnPaymentMethods.Hint", "Define apply on payment methods."),

                new KeyValuePair<string, string>("Admin.NopStation.AutoCancelOrder.Configuration.PaymentMethod", "Payment method"),
                new KeyValuePair<string, string>("Admin.NopStation.AutoCancelOrder.Configuration.Offset", "Offset"),
                new KeyValuePair<string, string>("Admin.NopStation.AutoCancelOrder.Configuration.Offset.Error", "Value of offset can not be null or decimal point"),
                new KeyValuePair<string, string>("Admin.NopStation.AutoCancelOrder.Configuration.Unit", "Unit"),

                new KeyValuePair<string, string>("NopStation.AutoCancelOrder.OrderNote", "Order has been cancelled automatically by \"Auto cancel order\" plugin."),
            };

            return list;
        }

        #endregion
    }
}
