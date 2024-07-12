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
using NopStation.Plugin.Widgets.OCarousels.Components;
using NopStation.Plugin.Widgets.OCarousels.Domains;
using NopStation.Plugin.Widgets.OCarousels.Helpers;
using NopStation.Plugin.Widgets.OCarousels.Services;

namespace NopStation.Plugin.Widgets.OCarousels
{
    public class OCarouselPlugin : BasePlugin, IWidgetPlugin, IAdminMenuPlugin, INopStationPlugin
    {
        #region Fields

        public bool HideInWidgetList => false;

        private readonly IWebHelper _webHelper;
        private readonly INopStationCoreService _nopStationCoreService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IOCarouselService _carouselService;

        #endregion

        #region Ctor

        public OCarouselPlugin(IWebHelper webHelper,
            INopStationCoreService nopStationCoreService,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            ISettingService settingService,
            IOCarouselService carouselService)
        {
            _webHelper = webHelper;
            _nopStationCoreService = nopStationCoreService;
            _localizationService = localizationService;
            _permissionService = permissionService;
            _settingService = settingService;
            _carouselService = carouselService;
        }

        #endregion

        #region Utilities

        private async void CreateSampleDataAsync()
        {
            var carouselSettings = new OCarouselSettings()
            {
                EnableOCarousel = true
            };
            await _settingService.SaveSettingAsync(carouselSettings);

            var carousel1 = new OCarousel()
            {
                Active = true,
                AutoPlay = true,
                AutoPlayHoverPause = true,
                AutoPlayTimeout = 3000,
                CreatedOnUtc = DateTime.UtcNow,
                DataSourceTypeEnum = DataSourceTypeEnum.HomePageCategories,
                DisplayTitle = true,
                Loop = true,
                LazyLoad = true,
                Name = "Featured Categories",
                Nav = true,
                UpdatedOnUtc = DateTime.UtcNow,
                NumberOfItemsToShow = 10,
                Title = "Featured Categories",
                WidgetZoneId = 2
            };
            await _carouselService.InsertCarouselAsync(carousel1);

            var carousel2 = new OCarousel()
            {
                Active = true,
                AutoPlay = true,
                AutoPlayHoverPause = true,
                AutoPlayTimeout = 3000,
                CreatedOnUtc = DateTime.UtcNow,
                DataSourceTypeEnum = DataSourceTypeEnum.NewProducts,
                DisplayTitle = true,
                Loop = true,
                LazyLoad = true,
                Name = "New Products",
                Nav = true,
                UpdatedOnUtc = DateTime.UtcNow,
                NumberOfItemsToShow = 10,
                Title = "New Products",
                WidgetZoneId = 3
            };
            await _carouselService.InsertCarouselAsync(carousel2);

            var carousel3 = new OCarousel()
            {
                Active = true,
                AutoPlay = true,
                AutoPlayHoverPause = true,
                AutoPlayTimeout = 3000,
                CreatedOnUtc = DateTime.UtcNow,
                DataSourceTypeEnum = DataSourceTypeEnum.BestSellers,
                DisplayTitle = true,
                Loop = true,
                LazyLoad = true,
                Name = "Best Sellers",
                Nav = true,
                UpdatedOnUtc = DateTime.UtcNow,
                NumberOfItemsToShow = 10,
                Title = "Best Sellers",
                WidgetZoneId = 4
            };
            await _carouselService.InsertCarouselAsync(carousel3);

            var carousel4 = new OCarousel()
            {
                Active = true,
                AutoPlay = true,
                AutoPlayHoverPause = true,
                AutoPlayTimeout = 3000,
                CreatedOnUtc = DateTime.UtcNow,
                DataSourceTypeEnum = DataSourceTypeEnum.Manufacturers,
                DisplayTitle = true,
                Loop = true,
                LazyLoad = true,
                Name = "Manufacturers",
                Nav = true,
                UpdatedOnUtc = DateTime.UtcNow,
                NumberOfItemsToShow = 10,
                Title = "Manufacturers",
                WidgetZoneId = 6
            };
            await _carouselService.InsertCarouselAsync(carousel4);
        }

        #endregion

        #region Methods

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/OCarousel/Configure";
        }

        public Type GetWidgetViewComponent(string widgetZone)
        {
            if (widgetZone == PublicWidgetZones.Footer)
                return typeof(OCarouselFooterViewComponent);

            return typeof(OCarouselViewComponent);
        }

        public Task<IList<string>> GetWidgetZonesAsync()
        {
            var widgetZones = OCarouselHelper.GetCustomWidgetZones();
            widgetZones.Add(PublicWidgetZones.Footer);
            return Task.FromResult<IList<string>>(widgetZones);
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            if (await _permissionService.AuthorizeAsync(OCarouselPermissionProvider.ManageOCarousels))
            {
                var menuItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.OCarousels.Menu.OCarousel"),
                    Visible = true,
                    IconClass = "far fa-dot-circle",
                };

                var listItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.OCarousels.Menu.Carousels"),
                    Url = "~/Admin/OCarousel/List",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "OCarousels"
                };
                menuItem.ChildNodes.Add(listItem);

                var configItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.OCarousels.Menu.Configuration"),
                    Url = "~/Admin/OCarousel/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "OCarousels.Configuration"
                };
                menuItem.ChildNodes.Add(configItem);

                if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
                {
                    var documentation = new SiteMapNode()
                    {
                        Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                        Url = "https://www.nop-station.com/ocarousel-documentation?utm_source=admin-panel?utm_source=admin-panel&utm_medium=products&utm_campaign=ocarousel",
                        Visible = true,
                        IconClass = "far fa-circle",
                        OpenUrlInNewTab = true
                    };
                    menuItem.ChildNodes.Add(documentation);
                }

                await _nopStationCoreService.ManageSiteMapAsync(rootNode, menuItem, NopStationMenuType.Plugin);
            }
        }

        public override async Task InstallAsync()
        {
            CreateSampleDataAsync();
            await this.InstallPluginAsync(new OCarouselPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await this.UninstallPluginAsync(new OCarouselPermissionProvider());
            await base.UninstallAsync();
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            var list = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.List.SearchActive.Active", "Active"),
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.List.SearchActive.Inactive", "Inactive"),
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.Menu.OCarousel", "OCarousel"),
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.Menu.Configuration", "Configuration"),
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.Menu.Carousels", "Carousels"),
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.Configuration", "Carousel settings"),
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.Tab.Info", "Info"),
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.Tab.Properties", "Properties"),
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.Tab.OCarouselItems", "Carousel items"),
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.CarouselList", "Carousels"),
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.EditDetails", "Edit carousel details"),
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.BackToList", "back to carousel list"),
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.AddNew", "Add new carousel"),
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarouselItems.AddNew", "Add new item"),
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarouselItems.SaveBeforeEdit", "You need to save the carousel before you can add items for this carousel page."),
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarouselItems.AddNew", "Add new item"),
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarouselItems.AddNew", "Add new item"),
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.Configuration.Fields.EnableOCarousel", "Enable carousel"),
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.Configuration.Fields.EnableOCarousel.Hint", "Check to enable carousel for your store."),
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.Configuration.Fields.RequireOCarouselPicture", "Require carousel picture"),
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.Configuration.Fields.RequireOCarouselPicture.Hint", "Determines whether main picture is required for carousel (based on theme design)."),

                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.Created", "Carousel has been created successfully."),
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.Updated", "Carousel has been updated successfully."),
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.Deleted", "Carousel has been deleted successfully."),

                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarouselItems.Fields.Product", "Product"),
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarouselItems.Fields.OCarousel", "Carousel"),
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarouselItems.Fields.DisplayOrder", "Display order"),
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarouselItems.Fields.Picture", "Picture"),

                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.Name", "Name"),
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.Name.Hint", "The carousel name."),
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.Title", "Title"),
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.Title.Hint", "The carousel title."),
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.DisplayTitle", "Display title"),
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.DisplayTitle.Hint", "Determines whether title should be displayed on public site (depends on theme design)."),
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.Active", "Active"),
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.Active.Hint", "Determines whether this carousel is active (visible on public store)."),
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.WidgetZone", "Widget zone"),
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.WidgetZone.Hint", "The widget zone where this carousel will be displayed."),
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.DataSourceType", "Data source type"),
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.DataSourceType.Hint", "The data source for this carousel."),
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.Picture", "Picture"),
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.Picture.Hint", "The carousel picture."),
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.CustomUrl", "Custom url"),
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.CustomUrl.Hint", "The carousel custom url."),
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.NumberOfItemsToShow", "Number of items to show"),
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.NumberOfItemsToShow.Hint", "Specify the number of items to show for this carousel."),
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.AutoPlay", "Auto play"),
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.AutoPlay.Hint", "Check to enable auto play."),
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.CustomCssClass", "Custom css class"),
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.CustomCssClass.Hint", "Enter the custom CSS class to be applied."),
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.DisplayOrder", "Display order"),
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.DisplayOrder.Hint", "Display order of the carousel. 1 represents the top of the list."),
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.Loop", "Loop"),
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.Loop.Hint", "heck to enable loop."),
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.StartPosition", "Start position"),
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.StartPosition.Hint", "TStarting position (e.g 0)"),
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.Center", "Center"),
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.Center.Hint", "Check to center item. It works well with even and odd number of items."),
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.Nav", "NAV"),
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.Nav.Hint", "Check to enable next/prev buttons."),
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.LazyLoad", "Lazy load"),
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.LazyLoad.Hint", "Check to enable lazy load."),
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.LazyLoadEager", "Lazy load eager"),
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.LazyLoadEager.Hint", "Specify how many items you want to pre-load images to the right (and left when loop is enabled)."),
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.AutoPlayTimeout", "Auto play timeout"),
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.AutoPlayTimeout.Hint", "It's autoplay interval timeout. (e.g 5000)"),
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.AutoPlayHoverPause", "Auto play hover pause"),
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.AutoPlayHoverPause.Hint", "Check to enable pause on mouse hover."),
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.CreatedOn", "Created on"),
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.CreatedOn.Hint", "The create date of this carousel."),
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.UpdatedOn", "Updated on"),
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.UpdatedOn.Hint", "The last update date of this carousel."),
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.SelectedStoreIds", "Limited to stores"),
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.SelectedStoreIds.Hint", "Option to limit this carousel to a certain store. If you have multiple stores, choose one or several from the list. If you don't use this option just leave this field empty."),
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.ShowBackgroundPicture", "Show Background Picture"),
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.ShowBackgroundPicture.Hint", "Check to enable show Background Picture"),
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.BackgroundPicture", "Background Picture"),
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.BackgroundPicture.Hint", "Background Picture of the carousel"),

                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.Name.Required", "The name field is required."),
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.Title.Required", "The title field is required."),
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.NumberOfItemsToShow.Required", "The number of items to show field is required."),
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.Picture.Required", "The picture field is required."),

                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.List.SearchWidgetZones", "Widget zones"),
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.List.SearchWidgetZones.Hint", "The search widget zones."),
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.List.SearchDataSources", "Data sources"),
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.List.SearchDataSources.Hint", "The search data sources."),
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.List.SearchStore", "Store"),
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.List.SearchStore.Hint", "The search store."),
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.List.SearchActive", "Active"),
                new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.List.SearchActive.Hint", "The search active."),

                new KeyValuePair<string, string>("NopStation.OCarousels.LoadingFailed", "Failed to load carousel content.")
            };

            return list;
        }

        #endregion
    }
}
