using Nop.Services.Configuration;
using Nop.Web.Framework.Menu;
using Nop.Services.Plugins;
using Nop.Services.Localization;
using Nop.Core;
using Nop.Services.Cms;
using System.Collections.Generic;
using Nop.Web.Framework.Infrastructure;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using System.Threading.Tasks;
using NopStation.Plugin.Misc.Core.Services;
using Nop.Services.Security;
using NopStation.Plugin.Misc.Core;
using System;
using NopStation.Plugin.Widgets.CancelOrder.Components;

namespace NopStation.Plugin.Widgets.CancelOrder
{
    public class CancelOrderPlugin : BasePlugin, IWidgetPlugin, IAdminMenuPlugin, INopStationPlugin
    {
        private readonly IWebHelper _webHelper;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly CancelOrderSettings _cancelOrderSettings;
        private readonly IPermissionService _permissionService;
        private readonly INopStationCoreService _nopStationCoreService;

        public bool HideInWidgetList => false;

        public CancelOrderPlugin(IWebHelper webHelper,
            ISettingService settingService,
            ILocalizationService localizationService,
            CancelOrderSettings cancelOrderSettings,
            IPermissionService permissionService,
            INopStationCoreService nopStationCoreService)
        {
            _webHelper = webHelper;
            _settingService = settingService;
            _localizationService = localizationService;
            _cancelOrderSettings = cancelOrderSettings;
            _permissionService = permissionService;
            _nopStationCoreService = nopStationCoreService;
        }

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/CancelOrder/Configure";
        }

        public override async Task InstallAsync()
        {
            var settings = new CancelOrderSettings()
            {
                WidgetZone = PublicWidgetZones.OrderDetailsPageBottom,
                CancellableOrderStatuses = new List<int>()
                {
                    (int)OrderStatus.Pending,
                    (int)OrderStatus.Processing
                },
                CancellablePaymentStatuses = new List<int>()
                {
                    (int)PaymentStatus.Pending
                },
                CancellableShippingStatuses= new List<int>()
                {
                    (int)ShippingStatus.NotYetShipped
                }
            };
            await _settingService.SaveSettingAsync(settings);

            await this.InstallPluginAsync(new CancelOrderPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await this.UninstallPluginAsync(new CancelOrderPermissionProvider());
            await base.UninstallAsync();
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.CancelOrder.Menu.CancelOrder"),
                Visible = true,
                IconClass = "far fa-dot-circle",
            };

            if (await _permissionService.AuthorizeAsync(CancelOrderPermissionProvider.ManageCancelOrder))
            {
                var categoryIconItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.CancelOrder.Menu.Configuration"),
                    Url = "~/Admin/CancelOrder/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "CancelOrder.Configure"
                };
                menuItem.ChildNodes.Add(categoryIconItem);
            }
            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/cancel-order-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=cancel-order",
                    Visible = true,
                    IconClass = "far fa-circle",
                    OpenUrlInNewTab = true
                };
                menuItem.ChildNodes.Add(documentation);
            }
                
            await _nopStationCoreService.ManageSiteMapAsync(rootNode, menuItem, NopStationMenuType.Plugin);
        }

        public Type GetWidgetViewComponent(string widgetZone)
        {
            return typeof(CancelOrderViewComponent);
        }

        public Task<IList<string>> GetWidgetZonesAsync()
        {
            var wz = string.IsNullOrWhiteSpace(_cancelOrderSettings.WidgetZone) ? PublicWidgetZones.OrderDetailsPageBottom : _cancelOrderSettings.WidgetZone;
            return Task.FromResult<IList<string>>(new List<string> { wz });
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            var list = new List<KeyValuePair<string, string>>
            {
                new("Admin.NopStation.CancelOrder.Menu.CancelOrder", "Cancel order"),
                new("Admin.NopStation.CancelOrder.Menu.Configuration", "Configuration"),
                new("NopStation.CancelOrder.OrderCancelledByCustomer", "Order cancelled by customer"),
                new("NopStation.CancelOrder.InvalidRequest", "Invalid order cancel request."),
                new("NopStation.CancelOrder.OrderNotFound", "Order not found!"),
                new("NopStation.CancelOrder.Button", "Cancel order"),
                new("NopStation.CancelOrder.Confirm", "Are you sure to cancel order?"),
                new("Admin.NopStation.CancelOrder.Configuration", "Cancel order settings"),
                new("Admin.NopStation.CancelOrder.Configuration.Fields.WidgetZone", "Widget zone"),
                new("Admin.NopStation.CancelOrder.Configuration.Fields.WidgetZone.Hint", "Specify widget zone where cancel button will be displayed in order details page."),
                new("Admin.NopStation.CancelOrder.Configuration.Fields.CancellableOrderStatuses", "Cancellable order statuses"),
                new("Admin.NopStation.CancelOrder.Configuration.Fields.CancellableOrderStatuses.Hint", "Specify cancellable order statuses."),
                new("Admin.NopStation.CancelOrder.Configuration.Fields.CancellablePaymentStatuses", "Cancellable payment statuses"),
                new("Admin.NopStation.CancelOrder.Configuration.Fields.CancellablePaymentStatuses.Hint", "Specify cancellable payment statuses."),
                new("Admin.NopStation.CancelOrder.Configuration.Fields.CancellableShippingStatuses", "Cancellable shipping statuses"),
                new("Admin.NopStation.CancelOrder.Configuration.Fields.CancellableShippingStatuses.Hint", "Specify cancellable shipping statuses.")
            };

            return list;
        }
    }
}
