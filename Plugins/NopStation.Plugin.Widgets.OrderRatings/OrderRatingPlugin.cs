using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Services;
using NopStation.Plugin.Widgets.OrderRatings.Areas.Admin.Components;
using NopStation.Plugin.Widgets.OrderRatings.Components;

namespace NopStation.Plugin.Widgets.OrderRatings
{
    public class OrderRatingPlugin : BasePlugin, IWidgetPlugin, INopStationPlugin, IAdminMenuPlugin
    {
        public bool HideInWidgetList => true;

        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly OrderRatingSettings _orderRatingSettings;
        private readonly IPermissionService _permissionService;
        private readonly IWebHelper _webHelper;
        private readonly INopStationCoreService _nopStationCoreService;

        public OrderRatingPlugin(ISettingService settingService,
            ILocalizationService localizationService,
            OrderRatingSettings orderRatingSettings,
            IPermissionService permissionService,
            IWebHelper webHelper,
            INopStationCoreService nopStationCoreService)
        {
            _settingService = settingService;
            _localizationService = localizationService;
            _orderRatingSettings = orderRatingSettings;
            _permissionService = permissionService;
            _webHelper = webHelper;
            _nopStationCoreService = nopStationCoreService;
        }

        public Type GetWidgetViewComponent(string widgetZone)
        {
            if (widgetZone.Equals(PublicWidgetZones.HomepageBeforeCategories))
                return typeof(OrderRatingHomepageViewComponent);
            if (widgetZone.Equals(AdminWidgetZones.OrderDetailsBlock))
                return typeof(OrderRatingViewComponent);

            return typeof(OrderRatingDetailsViewComponent);
        }

        public Task<IList<string>> GetWidgetZonesAsync()
        {
            var orderDetailsPageWidgetZone = PublicWidgetZones.OrderDetailsPageOverview;
            if (string.IsNullOrWhiteSpace(_orderRatingSettings.OrderDetailsPageWidgetZone))
                orderDetailsPageWidgetZone = _orderRatingSettings.OrderDetailsPageWidgetZone;

            return Task.FromResult<IList<string>>(new List<string> 
            { 
                PublicWidgetZones.HomepageBeforeCategories, 
                orderDetailsPageWidgetZone,
                AdminWidgetZones.OrderDetailsBlock
            });
        }

        public override async Task InstallAsync()
        {
            await _settingService.SaveSettingAsync(new OrderRatingSettings()
            {
                OpenOrderRatingPopupInHomepage = true,
                ShowOrderRatedDateInDetailsPage = true,
                OrderDetailsPageWidgetZone = PublicWidgetZones.OrderDetailsPageOverview
            });

            await this.InstallPluginAsync(new OrderRatingPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await this.UninstallPluginAsync(new OrderRatingPermissionProvider());
            await base.UninstallAsync();
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            var list = new List<KeyValuePair<string, string>>
            {
                new("Admin.NopStation.OrderRatings.Menu.OrderRating", "Order rating"),
                new("Admin.NopStation.OrderRatings.Menu.Configuration", "Configuration"),

                new("Admin.NopStation.OrderRatings.OrderRating.Info", "Order rating info"),
                new("Admin.NopStation.OrderRatings.OrderRating.Fields.Rating", "Rating"),
                new("Admin.NopStation.OrderRatings.OrderRating.Fields.Rating.Hint", "The customer's order rating."),
                new("Admin.NopStation.OrderRatings.OrderRating.Fields.Note", "Note"),
                new("Admin.NopStation.OrderRatings.OrderRating.Fields.Note.Hint", "The order rating note."),
                new("Admin.NopStation.OrderRatings.OrderRating.Fields.RatedOn", "Rated on"),
                new("Admin.NopStation.OrderRatings.OrderRating.Fields.RatedOn.Hint", "The date/time that the rating was created."),

                new("Admin.NopStation.OrderRatings.Configuration", "Order rating settings"),
                new("Admin.NopStation.OrderRatings.Configuration.Fields.OpenOrderRatingPopupInHomepage", "Open order rating popup in homepage"),
                new("Admin.NopStation.OrderRatings.Configuration.Fields.ShowOrderRatedDateInDetailsPage", "Show order rated date in details page"),
                new("Admin.NopStation.OrderRatings.Configuration.Fields.OrderDetailsPageWidgetZone", "Order details page widget zone"),
                new("Admin.NopStation.OrderRatings.Configuration.Fields.OpenOrderRatingPopupInHomepage.Hint", "Open order rating popup in homepage."),
                new("Admin.NopStation.OrderRatings.Configuration.Fields.ShowOrderRatedDateInDetailsPage.Hint", "Show order rated date in details page."),
                new("Admin.NopStation.OrderRatings.Configuration.Fields.OrderDetailsPageWidgetZone.Hint", "Order details page widget zone."),

                new("NopStation.OrderRatings.Fields.OrderId", "Order#"),
                new("NopStation.OrderRatings.Fields.Rating", "Rating"),
                new("NopStation.OrderRatings.Fields.Rating.Bad", "Bad"),
                new("NopStation.OrderRatings.Fields.Rating.Excellent", "Excellent"),
                new("NopStation.OrderRatings.Rating.Bad", "Bad"),
                new("NopStation.OrderRatings.Rating.NotGood", "Not good"),
                new("NopStation.OrderRatings.Rating.Average", "Average"),
                new("NopStation.OrderRatings.Rating.Good", "Good"),
                new("NopStation.OrderRatings.Rating.Excellent", "Excellent"),

                new("NopStation.OrderRatings.Fields.Note", "Note"),
                new("NopStation.OrderRatings.Rating.SubmitButton", "Submit"),
                new("NopStation.OrderRatings.OrderDetails.RatedOn", "Rated On: {0}"),
                new("NopStation.OrderRatings.OrderDetails.CreatedOn", "Order placed on: {0}"),
                new("NopStation.OrderRatings.OrderDetails.YourRate", "You have rated {0} for this order (#{1})")
            };

            return list;
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menu = new SiteMapNode()
            {
                Visible = true,
                IconClass = "far fa-dot-circle",
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.OrderRatings.Menu.OrderRating")
            };

            if (await _permissionService.AuthorizeAsync(OrderRatingPermissionProvider.ManageConfiguration))
            {
                var settings = new SiteMapNode()
                {
                    Visible = true,
                    IconClass = "far fa-circle",
                    Url = "~/Admin/OrderRating/Configure",
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.OrderRatings.Menu.Configuration"),
                    SystemName = "OrderRating.Configuration"
                };
                menu.ChildNodes.Add(settings);
            }

            var documentation = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                Url = "https://www.nop-station.com/order-rating-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=order-rating",
                Visible = true,
                IconClass = "far fa-circle",
                OpenUrlInNewTab = true
            };
            menu.ChildNodes.Add(documentation);

            await _nopStationCoreService.ManageSiteMapAsync(rootNode, menu, NopStationMenuType.Plugin);
        }

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/OrderRating/Configure";
        }
    }
}
