using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Services.Cms;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Services;
using NopStation.Plugin.Widgets.VendorShop.Areas.Admin.Components;
using NopStation.Plugin.Widgets.VendorShop.Components;
using NopStation.Plugin.Widgets.VendorShop.Helpers;
using NopStation.Plugin.Widgets.VendorShop.Services;

namespace NopStation.Plugin.Widgets.VendorShop
{
    public class VendorShopPlugin : BasePlugin, IAdminMenuPlugin, INopStationPlugin, IWidgetPlugin
    {
        #region Fields

        private readonly IWebHelper _webHelper;
        private readonly INopStationCoreService _nopStationCoreService;
        private readonly IPermissionService _permissionService;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly IVendorShopFeatureService _vendorShopFeatureService;
        private readonly VendorShopSettings _vendorShopSettings;

        #endregion

        #region Ctor

        public VendorShopPlugin(IWebHelper webHelper,
            INopStationCoreService nopStationCoreService,
            IPermissionService permissionService,
            ILocalizationService localizationService,
            VendorShopSettings vendorShopSettings,
            IWorkContext workContext,
            IVendorShopFeatureService vendorShopFeatureService)
        {
            _webHelper = webHelper;
            _nopStationCoreService = nopStationCoreService;
            _permissionService = permissionService;
            _localizationService = localizationService;
            _vendorShopSettings = vendorShopSettings;
            _workContext = workContext;
            _vendorShopFeatureService = vendorShopFeatureService;
        }

        #endregion

        #region Methods

        public override async Task InstallAsync()
        {
            await this.InstallPluginAsync(new VendorShopPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await this.UninstallPluginAsync(new VendorShopPermissionProvider());
            await base.UninstallAsync();
        }

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/VendorShop/Configure";
        }

        public Task<IList<string>> GetWidgetZonesAsync()
        {
            var widgetZones = WidgetZonelHelper.GetCustomWidgetZones();
            widgetZones.Add(PublicWidgetZones.Footer);
            widgetZones.Add(PublicWidgetZones.VendorDetailsTop);
            widgetZones.Add(AdminWidgetZones.VendorDetailsBlock);
            return Task.FromResult<IList<string>>(widgetZones);
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            if (await _permissionService.AuthorizeAsync(VendorShopPermissionProvider.ManageConfiguration))
            {
                var menuItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.VendorShop.OCarousels.Menu.VendorShop"),
                    Visible = true,
                    IconClass = "far fa-dot-circle",
                };
                var configItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.VendorShop.OCarousels.Menu.Configuration"),
                    Url = "~/Admin/VendorShop/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "VendorShop.Configuration"
                };
                menuItem.ChildNodes.Add(configItem);
                await _nopStationCoreService.ManageSiteMapAsync(rootNode, menuItem, NopStationMenuType.Plugin);
            }
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor == null || !await _vendorShopFeatureService.IsEnableVendorShopAsync(currentVendor.Id))
            {
                return;
            }

            if (_vendorShopSettings.EnableOCarousel && await _permissionService.AuthorizeAsync(VendorShopPermissionProvider.ManageOCarousels))
            {
                var menuItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.VendorShop.OCarousels.Menu.OCarousel"),
                    Visible = true,
                    IconClass = "far fa-dot-circle",
                };

                var listItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.VendorShop.OCarousels.Menu.Carousels"),
                    Url = "~/Admin/OCarouselVendorShop/List",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "VendorShop.OCarousels"
                };
                menuItem.ChildNodes.Add(listItem);

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
            if (_vendorShopSettings.EnableSlider && await _permissionService.AuthorizeAsync(VendorShopPermissionProvider.ManageSliders))
            {
                var menuItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.VendorShop.AnywhereSlider.Menu.AnywhereSlider"),
                    Visible = true,
                    IconClass = "far fa-dot-circle",
                };
                var listItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.VendorShop.AnywhereSlider.Menu.Sliders"),
                    Url = "~/Admin/AnywhereSliderVendorShop/List",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "VendorShop.AnywhereSlider"
                };
                menuItem.ChildNodes.Add(listItem);

                if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
                {
                    var documentation = new SiteMapNode()
                    {
                        Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                        Url = "https://www.nop-station.com/anywhere-slider-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=anywhere-slider",
                        Visible = true,
                        IconClass = "far fa-circle",
                        OpenUrlInNewTab = true
                    };
                    menuItem.ChildNodes.Add(documentation);
                }
                await _nopStationCoreService.ManageSiteMapAsync(rootNode, menuItem, NopStationMenuType.Plugin);
            }

            if (await _permissionService.AuthorizeAsync(VendorShopPermissionProvider.ManageVendorProfile))
            {
                var profileNode = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.VendorShop.VendorProfile"),
                    Visible = true,
                    Url = "~/Admin/VendorShop/Profile",
                    SystemName = "VendorShop.VendorProfile",
                    IconClass = "far fa-dot-circle",

                };

                await _nopStationCoreService.ManageSiteMapAsync(rootNode, profileNode, NopStationMenuType.Plugin);
            }


            if (_vendorShopSettings.EnableProductTabs && await _permissionService.AuthorizeAsync(VendorShopPermissionProvider.ManageProductTab))
            {
                var menuItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.VendorShop.ProductTabs.Menu.VendorShop"),
                    Visible = true,
                    IconClass = "far fa-dot-circle",
                };

                var listItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.VendorShop.ProductTabs.Menu.List"),
                    Url = $"{_webHelper.GetStoreLocation()}Admin/ProductTabVendorShop/List",
                    Visible = true,
                    IconClass = "far fa-dot-circle",
                    SystemName = "VendorShop.ProductTabs"
                };
                menuItem.ChildNodes.Add(listItem);

                await _nopStationCoreService.ManageSiteMapAsync(rootNode, menuItem, NopStationMenuType.Plugin);
            }
            if (await _permissionService.AuthorizeAsync(VendorShopPermissionProvider.ManageSubscriber) && _vendorShopSettings.EnableVendorShopCampaign)
            {
                var menuItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.VendorShop.Subscriber.Menu.Name"),
                    Visible = true,
                    IconClass = "far fa-dot-circle",
                };

                var listItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.VendorShop.Subscriber.Menu.List"),
                    Url = $"{_webHelper.GetStoreLocation()}Admin/VendorSubscribers/List",
                    Visible = true,
                    IconClass = "far fa-dot-circle",
                    SystemName = "VendorShop.Subscriber"
                };
                menuItem.ChildNodes.Add(listItem);

                await _nopStationCoreService.ManageSiteMapAsync(rootNode, menuItem, NopStationMenuType.Plugin);
            }


        }

        public Type GetWidgetViewComponent(string widgetZone)
        {
            if (widgetZone == AdminWidgetZones.VendorDetailsBlock)
                return typeof(VendorShopFeatureViewComponent);
            if (widgetZone == PublicWidgetZones.Footer)
                return typeof(VendorShopFooterViewComponent);
            if (widgetZone == PublicWidgetZones.VendorDetailsTop)
                return typeof(VendorShopCustomCssViewComponent);
            return typeof(VendorShopViewComponent);
        }


        public List<KeyValuePair<string, string>> PluginResouces()
        {
            var list = new List<KeyValuePair<string, string>>
            {
                 new KeyValuePair<string, string>("Admin.NopStation.VendorShop.VendorFeature.Title", "Vendor Shop"),
                 new KeyValuePair<string, string>("Admin.NopStation.VendorShop.VendorFeature.Fields.EnableFeature", "Enable vendor shop"),
                 new KeyValuePair<string, string>("Admin.NopStation.VendorShop.VendorFeature.Fields.EnableFeature.Hint", "Check to enable vendor shop features for this vendor."),
                // vendor shop
               
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.Configuration.Fields.EnableProductTabs", "Enable product tab"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.Configuration.Fields.EnableVendorCustomCss", "Enable custom CSS"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.Configuration.Fields.EnableVendorCustomCss.Hint", "Check to allow vendors to write custom css for his/her shop."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.Configuration.Fields.EnableProductTabs.Hint", "Check to allow vendors to manage product tab on his/her shop."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.Configuration.PageTitle", "Vendorshop configuration"),
                //Carousel

                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.Menu.VendorShop", "Vendor shop"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.OCarousels.List.SearchActive.Active", "Active"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.OCarousels.List.SearchActive.Inactive", "Inactive"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.Menu.OCarousel", "OCarousel"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.Menu.Configuration", "Configuration"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.Menu.Carousels", "Carousels"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.Configuration", "Carousel settings"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.Tab.Info", "Info"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.Tab.Properties", "Properties"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.Tab.OCarouselItems", "Carousel items"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.CarouselList", "Carousels"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.EditDetails", "Edit carousel details"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.BackToList", "back to carousel list"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.AddNew", "Add new carousel"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.OCarouselItems.AddNew", "Add new item"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.OCarouselItems.SaveBeforeEdit", "You need to save the carousel before you can add items for his/her carousel page."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.Configuration.Fields.EnableOCarousel", "Enable carousel"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.Configuration.Fields.EnableOCarousel.Hint", "Check to allow vendors to manage carousel for his/her shop."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.Configuration.Fields.EnableSlider", "Enable slider"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.Configuration.Fields.EnableSlider.Hint", "Check to allow vendors to manage slider for his/her shop."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.Configuration.Fields.RequireOCarouselPicture", "Require carousel picture"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.Configuration.Fields.RequireOCarouselPicture.Hint", "Determines whether main picture is required for carousel (based on theme design)."),

                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.Created", "Carousel has been created successfully."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.Updated", "Carousel has been updated successfully."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.Deleted", "Carousel has been deleted successfully."),

                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.OCarouselItems.Fields.Product", "Product"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.OCarouselItems.Fields.OCarousel", "Carousel"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.OCarouselItems.Fields.DisplayOrder", "Display order"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.OCarouselItems.Fields.Picture", "Picture"),

                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.OCarousels.Fields.Name", "Name"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.OCarousels.Fields.Name.Hint", "The carousel name."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.OCarousels.Fields.Title", "Title"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.OCarousels.Fields.Title.Hint", "The carousel title."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.OCarousels.Fields.DisplayTitle", "Display title"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.OCarousels.Fields.DisplayTitle.Hint", "Determines whether title should be displayed on public site (depends on theme design)."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.OCarousels.Fields.Active", "Active"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.OCarousels.Fields.Active.Hint", "Determines whether this carousel is active (visible on public store)."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.OCarousels.Fields.WidgetZone", "Widget zone"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.OCarousels.Fields.WidgetZone.Hint", "The widget zone where this carousel will be displayed."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.OCarousels.Fields.DataSourceType", "Data source type"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.OCarousels.Fields.DataSourceType.Hint", "The data source for this carousel."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.OCarousels.Fields.Picture", "Picture"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.OCarousels.Fields.Picture.Hint", "The carousel picture."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.OCarousels.Fields.CustomUrl", "Custom url"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.OCarousels.Fields.CustomUrl.Hint", "The carousel custom url."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.OCarousels.Fields.NumberOfItemsToShow", "Number of items to show"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.OCarousels.Fields.NumberOfItemsToShow.Hint", "Specify the number of items to show for this carousel."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.OCarousels.Fields.AutoPlay", "Auto play"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.OCarousels.Fields.AutoPlay.Hint", "Check to enable auto play."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.OCarousels.Fields.CustomCssClass", "Custom css class"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.OCarousels.Fields.CustomCssClass.Hint", "Enter the custom CSS class to be applied."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.OCarousels.Fields.DisplayOrder", "Display order"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.OCarousels.Fields.DisplayOrder.Hint", "Display order of the carousel. 1 represents the top of the list."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.OCarousels.Fields.Loop", "Loop"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.OCarousels.Fields.Loop.Hint", "heck to enable loop."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.OCarousels.Fields.StartPosition", "Start position"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.OCarousels.Fields.StartPosition.Hint", "TStarting position (e.g 0)"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.OCarousels.Fields.Center", "Center"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.OCarousels.Fields.Center.Hint", "Check to center item. It works well with even and odd number of items."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.OCarousels.Fields.Nav", "NAV"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.OCarousels.Fields.Nav.Hint", "Check to enable next/prev buttons."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.OCarousels.Fields.LazyLoad", "Lazy load"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.OCarousels.Fields.LazyLoad.Hint", "Check to enable lazy load."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.OCarousels.Fields.LazyLoadEager", "Lazy load eager"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.OCarousels.Fields.LazyLoadEager.Hint", "Specify how many items you want to pre-load images to the right (and left when loop is enabled)."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.OCarousels.Fields.AutoPlayTimeout", "Auto play timeout"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.OCarousels.Fields.AutoPlayTimeout.Hint", "It's autoplay interval timeout. (e.g 5000)"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.OCarousels.Fields.AutoPlayHoverPause", "Auto play hover pause"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.OCarousels.Fields.AutoPlayHoverPause.Hint", "Check to enable pause on mouse hover."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.OCarousels.Fields.CreatedOn", "Created on"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.OCarousels.Fields.CreatedOn.Hint", "The create date of this carousel."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.OCarousels.Fields.UpdatedOn", "Updated on"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.OCarousels.Fields.UpdatedOn.Hint", "The last update date of this carousel."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.OCarousels.Fields.SelectedStoreIds", "Limited to stores"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.OCarousels.Fields.SelectedStoreIds.Hint", "Option to limit this carousel to a certain store. If you have multiple stores, choose one or several from the list. If you don't use this option just leave this field empty."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.OCarousels.Fields.ShowBackgroundPicture", "Show Background Picture"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.OCarousels.Fields.ShowBackgroundPicture.Hint", "Check to enable show Background Picture"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.OCarousels.Fields.BackgroundPicture", "Background Picture"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.OCarousels.Fields.BackgroundPicture.Hint", "Background Picture of the carousel"),

                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.OCarousels.Fields.Name.Required", "The name field is required."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.OCarousels.Fields.Title.Required", "The title field is required."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.OCarousels.Fields.VendorId.Required", "Not a vendor account, this page is for vendor only."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.OCarousels.Fields.NumberOfItemsToShow.Required", "The number of items to show field is required."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.OCarousels.Fields.Picture.Required", "The picture field is required."),

                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.OCarousels.List.SearchWidgetZones", "Widget zones"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.OCarousels.List.SearchWidgetZones.Hint", "The search widget zones."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.OCarousels.List.SearchDataSources", "Data sources"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.OCarousels.List.SearchDataSources.Hint", "The search data sources."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.OCarousels.List.SearchStore", "Store"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.OCarousels.List.SearchStore.Hint", "The search store."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.OCarousels.List.SearchActive", "Active"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.OCarousels.OCarousels.List.SearchActive.Hint", "The search active."),

                new KeyValuePair<string, string>("NopStation.VendorShop.OCarousels.LoadingFailed", "Failed to load carousel content."),


                //Slider

                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.List.SearchActive.Active", "Active"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.List.SearchActive.Inactive", "Inactive"),

                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.Menu.AnywhereSlider", "Anywhere slider"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.Menu.Configuration", "Configuration"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.Menu.Sliders", "Sliders"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.Configuration", "Slider settings"),

                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.Tab.Info", "Info"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.Tab.Properties", "Properties"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.Tab.SliderItems", "Slider items"),

                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.SliderList", "Sliders"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.EditDetails", "Edit slider details"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.BackToList", "back to slider list"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.AddNew", "Add new slider"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.SliderItems.SaveBeforeEdit", "You need to save the slider before you can add items for this slider page."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.SliderItems.AddNew", "Add new item"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.SliderItems.Pictures.Alert.PictureAdd", "Failed to add product picture."),

                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.Configuration.Fields.EnableSlider", "Enable slider"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.Configuration.Fields.EnableSlider.Hint", "Check to enable slider for your store."),

                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.Configuration.Updated", "Slider configuration updated successfully."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.Created", "Slider has been created successfully."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.Updated", "Slider has been updated successfully."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.Deleted", "Slider has been deleted successfully."),

                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.SliderItems.Fields.DisplayOrder", "Display order"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.SliderItems.Fields.Picture", "Picture"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.SliderItems.Fields.MobilePicture", "Mobile picture"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.SliderItems.Fields.ImageAltText", "Alt"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.SliderItems.Fields.Title", "Title"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.SliderItems.Fields.ShortDescription", "Short description"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.SliderItems.Fields.Link", "Link"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.SliderItems.Fields.DisplayOrder.Hint", "The display order for this slider item. 1 represents the top of the list."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.SliderItems.Fields.Picture.Hint", "Picture of this slider item."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.SliderItems.Fields.MobilePicture.Hint", "Mobile view picture of this slider item."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.SliderItems.Fields.ImageAltText.Hint", "Override \"alt\" attribute for \"img\" HTML element."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.SliderItems.Fields.Title.Hint", "Override \"title\" attribute for \"img\" HTML element."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.SliderItems.Fields.Link.Hint", "Custom link for slider item picture."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.SliderItems.Fields.ShortDescription.Hint", "Short description for this slider item."),

                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.Fields.Name", "Name"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.Fields.Name.Hint", "The slider name."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.Fields.Title", "Title"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.Fields.Title.Hint", "The slider title."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.Fields.DisplayTitle", "Display title"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.Fields.DisplayTitle.Hint", "Determines whether title should be displayed on public site (depends on theme design)."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.Fields.Active", "Active"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.Fields.Active.Hint", "Determines whether this slider is active (visible on public store)."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.Fields.WidgetZone", "Widget zone"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.Fields.WidgetZone.Hint", "The widget zone where this slider will be displayed."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.Fields.Picture", "Picture"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.Fields.Picture.Hint", "The slider picture."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.Fields.CustomUrl", "Custom url"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.Fields.CustomUrl.Hint", "The slider custom url."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.Fields.AutoPlay", "Auto play"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.Fields.AutoPlay.Hint", "Check to enable auto play."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.Fields.CustomCssClass", "Custom css class"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.Fields.CustomCssClass.Hint", "Enter the custom CSS class to be applied."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.Fields.DisplayOrder", "Display order"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.Fields.DisplayOrder.Hint", "Display order of the slider. 1 represents the top of the list."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.Fields.Loop", "Loop"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.Fields.Loop.Hint", "heck to enable loop."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.Fields.Margin", "Margin"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.Fields.Margin.Hint", "It's margin-right (px) on item."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.Fields.StartPosition", "Start position"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.Fields.StartPosition.Hint", "Starting position."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.Fields.Center", "Center"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.Fields.Center.Hint", "Check to center item. It works well with even and odd number of items."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.Fields.Nav", "NAV"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.Fields.Nav.Hint", "Check to enable next/prev buttons."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.Fields.LazyLoad", "Lazy load"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.Fields.LazyLoad.Hint", "Check to enable lazy load."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.Fields.LazyLoadEager", "Lazy load eager"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.Fields.LazyLoadEager.Hint", "Specify how many items you want to pre-load images to the right (and left when loop is enabled)."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.Fields.AutoPlayTimeout", "Auto play timeout"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.Fields.AutoPlayTimeout.Hint", "It's autoplay interval timeout. (e.g 5000)"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.Fields.AutoPlayHoverPause", "Auto play hover pause"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.Fields.AutoPlayHoverPause.Hint", "Check to enable pause on mouse hover."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.Fields.AnimateOut", "Animate out"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.Fields.AnimateOut.Hint", "Animate out."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.Fields.AnimateIn", "Animate in"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.Fields.AnimateIn.Hint", "Animate in."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.Fields.CreatedOn", "Created on"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.Fields.CreatedOn.Hint", "The create date of this slider."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.Fields.UpdatedOn", "Updated on"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.Fields.UpdatedOn.Hint", "The last update date of this slider."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.Fields.SelectedStoreIds", "Limited to stores"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.Fields.SelectedStoreIds.Hint", "Option to limit this slider to a certain store. If you have multiple stores, choose one or several from the list. If you don't use this option just leave this field empty."),

                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.Fields.Name.Required", "The name field is required."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.Fields.VendorId.Required", "Not a vendor account, this page is for vendor only."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.Fields.BackGroundPicture.Required", "The background picture is required."),

                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.List.SearchWidgetZones", "Widget zones"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.List.SearchWidgetZones.Hint", "The search widget zones."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.List.SearchStore", "Store"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.List.SearchStore.Hint", "The search store."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.List.SearchActive", "Active"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.List.SearchActive.Hint", "The search active."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.SliderItems.EditDetails", "Edit details"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.SliderItems.Fields.Title.Required", "Title is required."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.SliderItems.Fields.Picture.Required", "Picture is required."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.SliderItems.Fields.MobilePicture.Required", "Mobile picture is required."),

                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.SliderItems.Pictures.Alert.AddNew", "Upload picture first."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.Fields.ShowBackgroundPicture", "Show background picture"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.Fields.ShowBackgroundPicture.Hint", "Determines whether to show background picture or not."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.Fields.BackgroundPicture", "Background picture"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.Sliders.Fields.BackgroundPicture.Hint", "Background picture for this slider."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.SliderItems.Fields.ShopNowLink", "ShopNow Link"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.AnywhereSlider.SliderItems.Fields.ShopNowLink.Hint", "Your ShopNow Link"),
                new KeyValuePair<string, string>("NopStation.VendorShop.AnywhereSlider.ShopNow", "Shop Now"),
                new KeyValuePair<string, string>("NopStation.VendorShop.AnywhereSlider.LoadingFailed", "Failed to load slider content."),

                new KeyValuePair<string, string>("NopStation.VendorShop.VendorHomePage", "Home page"),
                new KeyValuePair<string, string>("NopStation.VendorShop.VendorCatalogPage", "Products"),
                new KeyValuePair<string, string>("NopStation.VendorShop.VendorProfilePage","Profile"),

                // vendor profile
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.VendorProfile", "Vendor profile"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.VendorProfile.Updated", "Profile updated"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.VendorProfile.Edit", "Edit profile"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.VendorProfile.Fields.Description", "Description"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.VendorProfile.Fields.Description.Hint", "Your shop description"),

                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.VendorProfile.Fields.ProfilePictureId", "Profile picture"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.VendorProfile.Fields.ProfilePictureId.Hint", "Your logo or profile picture"),

                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.VendorProfile.Fields.BannerPictureId", "Banner picture"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.VendorProfile.Fields.BannerPictureId.Hint", "Banner picture"),

                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.VendorProfile.Fields.MobileBannerPictureId", "Banner picture(mobile)"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.VendorProfile.Fields.MobileBannerPictureId.Hint", "Banner picture for mobile"),

                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.VendorProfile.Fields.CustomCss", "Custom CSS"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.VendorProfile.Fields.CustomCss.Hint", "Write custom css for your shop."),

                // product tab
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.Menu.ProductTab", "Product tab"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.Menu.List", "List"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.Menu.Configuration", "Configuration"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.List.SearchActive.Active", "Active"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.List.SearchActive.Inactive", "Inactive"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.Fields.Name.Required", "The product tab name is required."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.Fields.Title.Required", "The product tab title is required."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.Fields.Picture.Required", "The product tab picture is required."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.ProductTabItems.Fields.Name.Required", "The product tab item name is required."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.Fields.Name", "Name"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.Fields.Name.Hint", "The name of the product tab."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.Fields.Title", "Title"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.Fields.Title.Hint", "The title of the product tab."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.Fields.DisplayTitle", "Display title"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.Fields.DisplayTitle.Hint", "Check to display title."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.Fields.Picture", "Picture"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.Fields.Picture.Hint", "Select product tab picture."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.Fields.DisplayOrder", "Display Order"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.Fields.DisplayOrder.Hint", "Display order of the product tab. 1 represents the top of the list."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.Fields.Active", "Active"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.Fields.Active.Hint", "Determines whether product tab is active or not."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.Fields.CustomUrl", "Custom URL"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.Fields.CustomUrl.Hint", "The custom url."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.Fields.WidgetZone", "Widget zone"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.Fields.WidgetZone.Hint", "The widget-zone of the product tab."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.Fields.AutoPlay", "Auto play"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.Fields.AutoPlay.Hint", "Check to enable auto-play."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.Fields.CustomCssClass", "Custom CSS Class"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.Fields.CustomCssClass.Hint", "Enter the custom CSS class to be applied."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.Fields.Loop", "Loop"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.Fields.Loop.Hint", "Check to enable 'infinity loop' which duplicates last and first items to get loop illusion. (e.g false)"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.Fields.Margin", "Margin"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.Fields.Margin.Hint", "It's margin-right(px) on item. (Default 0)"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.Fields.StartPosition", "Starting position"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.Fields.StartPosition.Hint", "Starting position (e.g 0)"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.Fields.Center", "Center"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.Fields.Center.Hint", "Check to center item. It works well with even an odd number of items. (e.g false)"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.Fields.Nav", "NAV"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.Fields.Nav.Hint", "Check to enable next/prev buttons. (e.g false)"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.Fields.LazyLoad", "Lazy load"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.Fields.LazyLoad.Hint", "Check to enable lazy-load images (e.g false)"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.Fields.LazyLoadEager", "Lazy load eager"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.Fields.LazyLoadEager.Hint", "Check to eagerly pre-load images to the right (and left when loop is enabled) based on how many items you want to preload."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.Fields.AutoPlayTimeout", "Auto play timeout"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.Fields.AutoPlayTimeout.Hint", "It's autoplay interval timeout. (e.g 5000)"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.Fields.AutoPlayHoverPause", "Auto play hover pause"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.Fields.AutoPlayHoverPause.Hint", "Check to enable pause on mouse hover. (e.g false)"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.Fields.SelectedStoreIds", "Limited to stores"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.Fields.SelectedStoreIds.Hint", "Option to limit this product tab to a certain store. If you have multiple stores, choose one or several from the list. If you don't use this option just leave this field empty."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.Deleted", "Product tab deleted successfully."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.Updated", "Product tab updated successfully."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.Created", "Product tab created successfully."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.ProductTabItems.Fields.Name", "Name"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.ProductTabItems.Fields.Name.Hint", "Product tab item name."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.ProductTabItems.Fields.DisplayOrder", "Display order"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.ProductTabItems.Fields.DisplayOrder.Hint", "Display order of the product tab item. 1 represents the top of the list."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.ProductTabItems.Updated", "Product tab item updated successfully"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.ProductTabItems.Created", "Product tab item created successfully"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.Tab.Info", "Info"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.Tab.Properties", "Properties"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.Tab.ProductTabItems", "Product tab items"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.ProductTabItems.BtnAddNew", "Add new tab item"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.ProductTabItems.SaveBeforeEdit", "You need to save the product tab before you can add item for this product tab page."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.ProductTabItems.Tab.Info", "Info"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.ProductTabItems.Tab.Products", "Products"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.ProductTabItems.ProductTabItemProducts.BtnAddNew", "Add new product"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.ProductTabItems.ProductTabItemProducts.SaveBeforeEdit", "You need to save the product tab item before you can add product for this product tab item page."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.Configuration", "Product tab settings"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.AddNew", "Add new product tab"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.EditDetails", "Edit product tab"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.BackToList", "back to tab list"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.ProductTabItems.AddNew", "Add new tab item"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.ProductTabItems.EditDetails", "Edit tab item"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.ProductTabItems.BackToProductTab", "back to product tab"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.ProductTabList", "Product tabs"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.ProductTabItems.Products.AddNew", "Add new products"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.Configuration.Fields.EnableProductTab", "Enable product tab"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.Configuration.Fields.EnableProductTab.Hint", "Check to enable product tab."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.ProductTabItemProducts.Fields.ProductTabItem", "Tab item"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.ProductTabItemProducts.Fields.Product", "Product"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.ProductTabItemProducts.Fields.DisplayOrder", "Display order"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.ProductTabItemProducts.Fields.CreatedOn", "CreatedOn Utc"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.ProductTabItemProducts.Fields.CreatedOn.Hint", "Created Date"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.ProductTabItemProducts.Fields.UpdatedOn", "UpdatedOn Utc"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.ProductTabItemProducts.Fields.UpdatedOn.Hint", "Updated Date"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.Configuration.Updated", "Configuration Has been Updated"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.Menu.VendorShop", "Product tabs"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.Fields.CreatedOn", "Created On"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProductTabs.ProductTabs.Fields.UpdatedOn", "Updated On"),


                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.Configuration.Fields.EnableVendorShopCampaign", "Enable vendor campaign"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.Configuration.Fields.EnableVendorShopCampaign.Hint", "Allows vendor to send campaign email to shop subscriber."),

                //profile 
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProfileTabs.EnumNone","None"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProfileTabs.EnumRecent", "Recent"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProfileTabs.EnumRatingHighToLow","Rating high to low"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProfileTabs.EnumRatingLowToHigh","Rating low to high"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProfileTabs.EnumAll","All"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProfileTabs.EnumOneStar","1"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProfileTabs.EnumTwoStar","2"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProfileTabs.EnumThreeStar","3"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProfileTabs.EnumFourStar","4"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProfileTabs.EnumFiveStar","5"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProfileTabs.Reviews.OrderBy","Sort by"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProfileTabs.Reviews.OrderBy.Label","Sort by"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProfileTabs.Reviews.FilterByStar"," Rating"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProfileTabs.Reviews.FilterByStar.Label"," Rating"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProfileTabs.Reviews.NoResult","No reviews found"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProfileTabs.Title","Products review"),

                //subscribe and unsubscribe
                new KeyValuePair<string, string>("NopStation.VendorShop.Subscribe.Text","Subscribe"),
                new KeyValuePair<string, string>("NopStation.VendorShop.Unsubscribe.Text","Unsubscribe"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.Subscriber.Menu.Name","Vendor subscribers"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.Subscriber.Menu.List","Subscribers"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.Subscriber.Search.SearchSubscriberEmail","Customer Email"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.Subscriber.Search.SearchSubscriberEmail.Hint","Search customer by email"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.Subscriber.Title","Vendor subscribers"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.Subscriber.EmailSelected","Email selected"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.Subscriber.EmailAll","Email to all"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.Subscriber.Email","Email"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.Subscriber.Promotions.Campaigns.Fields.CampaignName","Campaign name"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.Subscriber.Promotions.Campaigns.Fields.CampaignName.Hint","The name of this campaign."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.Subscriber.Promotions.Campaigns.Fields.Subject","Email subject"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.Subscriber.Promotions.Campaigns.Fields.Subject.Hint","The subject of your campaign email."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.Subscriber.Promotions.Campaigns.Fields.Body","Email body"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.Subscriber.Promotions.Campaigns.Fields.Body.Hint","The body of your campaign email."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.Subscriber.Promotions.Campaigns.Fields.SendingDate","Sending Date"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.Subscriber.Promotions.Campaigns.Fields.SendingDate.Hint","Schedule email send."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.Subscriber.Promotions.Campaigns.Fields.AllowedTokens","Allowed Tokens"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.Subscriber.Promotions.Campaigns.Fields.AllowedTokens.Hint","This is a list of the message tokens you can use in your campaign emails."),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.Subscriber.SubscribedOn","Subscribed on"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.Campaign.Title","Campain message"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.Alert.NothingSelected","No id is selected"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.Alert.EmptyField","Some field can't be empty"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.Email.SendDate","send date can't be less than current date"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.Email.Success","Email sent successfully"),
                new KeyValuePair<string, string>("NopStation.VendorShop.Subscribe.NotSubscriber","You are not a subscriber"),
                new KeyValuePair<string, string>("NopStation.VendorShop.Subscribe.Unsubscribe","Unsubscribed successfully"),
                new KeyValuePair<string, string>("NopStation.VendorShop.Subscribe.SubscribeSuccess","Subscribed successfully"),
                new KeyValuePair<string, string>("NopStation.VendorShop.Subscribe.AlreadySubscriber","You are already a subscriber"),

                new KeyValuePair<string, string>("NopStation.VendorShop.OverallPositive","<em>{0}</em>% positive review"),
                new KeyValuePair<string, string>("NopStation.VendorShop.BasedOnTotalReview","Based on {0} customers review"),
                new KeyValuePair<string, string>("NopStation.VendorShop.ButtonText.ContactUs","Contact us."),

                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.ProfileTabs.OverallRetings","Shop Rating"),
                new KeyValuePair<string, string>("Admin.NopStation.VendorShop.Alert.NoSubscriberToSend","No subsribers")

        };
            return list;
        }

        #endregion

        #region Properties

        public bool HideInWidgetList => false;

        #endregion
    }
}

