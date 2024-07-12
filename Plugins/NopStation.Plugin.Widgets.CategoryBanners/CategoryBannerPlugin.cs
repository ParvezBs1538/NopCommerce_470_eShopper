using Nop.Core;
using Nop.Services.Plugins;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Web.Framework.Menu;
using System.Collections.Generic;
using Nop.Web.Framework.Infrastructure;
using NopStation.Plugin.Misc.Core.Services;
using NopStation.Plugin.Misc.Core;
using Nop.Services.Security;
using NopStation.Plugin.Misc.Core.Helpers;
using System.Threading.Tasks;
using System;
using NopStation.Plugin.Widgets.CategoryBanners.Components;
using NopStation.Plugin.Widgets.CategoryBanners.Areas.Admin.Components;

namespace NopStation.Plugin.Widgets.CategoryBanners
{
    public class CategoryBannerPlugin : BasePlugin, IWidgetPlugin, IAdminMenuPlugin, INopStationPlugin
    {
        #region Fields

        public bool HideInWidgetList => false;

        private readonly IWebHelper _webHelper;
        private readonly INopStationCoreService _nopStationCoreService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;

        #endregion

        #region Ctor

        public CategoryBannerPlugin(IWebHelper webHelper,
            INopStationCoreService nopStationCoreService,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            ISettingService settingService)
        {
            _webHelper = webHelper;
            _nopStationCoreService = nopStationCoreService;
            _localizationService = localizationService;
            _permissionService = permissionService;
            _settingService = settingService;
        }

        #endregion

        #region Methods

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/CategoryBanner/Configure";
        }


        public Type GetWidgetViewComponent(string widgetZone)
        {
            if (widgetZone == AdminWidgetZones.CategoryDetailsBlock)
                return typeof(CategoryBannerAdminViewComponent);

            return typeof(CategoryBannerViewComponent);
        }

        public Task<IList<string>> GetWidgetZonesAsync()
        {
            return Task.FromResult<IList<string>>(new List<string>
            {
                AdminWidgetZones.CategoryDetailsBlock,
                PublicWidgetZones.CategoryDetailsTop
            });
        }
        
        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.CategoryBanners.Menu.CategoryBanner"),
                Visible = true,
                IconClass = "far fa-dot-circle"
            };

            if (await _permissionService.AuthorizeAsync(CategoryBannerPermissionProvider.ManageCategoryBanner))
            {
                var categoryIconItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.CategoryBanners.Menu.Configuration"),
                    Url = "~/Admin/CategoryBanner/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "CategoryBanner.Configure"
                };
                menuItem.ChildNodes.Add(categoryIconItem);
            }
            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/category-banner-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=category-banner",
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
            var settings = new CategoryBannerSettings()
            {
                HideInPublicStore = false,
                AutoPlay = true,
                AutoPlayHoverPause = true,
                AutoPlayTimeout = 3000,
                BannerPictureSize = 800,
                Loop = true,
                Nav = true
            };
            await _settingService.SaveSettingAsync(settings);

            await this.InstallPluginAsync(new CategoryBannerPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await this.UninstallPluginAsync(new CategoryBannerPermissionProvider());
            await base.UninstallAsync();
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            var list = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Admin.NopStation.CategoryBanners.Menu.CategoryBanner", "Category banner"),
                new KeyValuePair<string, string>("Admin.NopStation.CategoryBanners.Menu.Configuration", "Configuration"),

                new KeyValuePair<string, string>("Admin.NopStation.CategoryBanners.Configuration", "Category banner settings"),
                new KeyValuePair<string, string>("Admin.NopStation.CategoryBanners.Tab.Banners", "Banners"),
                new KeyValuePair<string, string>("Admin.NopStation.CategoryBanners.AddNew", "Add a new banner"),
                new KeyValuePair<string, string>("Admin.NopStation.CategoryBanners.AddButton", "Add category banner"),
                new KeyValuePair<string, string>("Admin.NopStation.CategoryBanners.Alert.AddNew", "Upload picture first."),
                new KeyValuePair<string, string>("Admin.NopStation.CategoryBanners.Alert.BannerAdd", "Failed to add product banner."),
                new KeyValuePair<string, string>("Admin.NopStation.CategoryBanners.SaveBeforeEdit", "You need to save the category before you can upload banner for this category page."),

                new KeyValuePair<string, string>("Admin.NopStation.CategoryBanners.CategoryBanners.Fields.DisplayOrder", "Display order"),
                new KeyValuePair<string, string>("Admin.NopStation.CategoryBanners.CategoryBanners.Fields.DisplayOrder.Hint", "Display order of the banner. 1 represents the top of the list."),
                new KeyValuePair<string, string>("Admin.NopStation.CategoryBanners.CategoryBanners.Fields.ForMobile", "For mobile"),
                new KeyValuePair<string, string>("Admin.NopStation.CategoryBanners.CategoryBanners.Fields.ForMobile.Hint", "Check to display banner for mobile device. To display on computer, keep it uncheked."),
                new KeyValuePair<string, string>("Admin.NopStation.CategoryBanners.CategoryBanners.Fields.OverrideAltAttribute", "Alt"),
                new KeyValuePair<string, string>("Admin.NopStation.CategoryBanners.CategoryBanners.Fields.OverrideAltAttribute.Hint", "Override \"alt\" attribute for \"img\" HTML element. If empty, then a default rule will be used (e.g. category name)."),
                new KeyValuePair<string, string>("Admin.NopStation.CategoryBanners.CategoryBanners.Fields.OverrideTitleAttribute", "Title"),
                new KeyValuePair<string, string>("Admin.NopStation.CategoryBanners.CategoryBanners.Fields.OverrideTitleAttribute.Hint", "Override \"title\" attribute for \"img\" HTML element. If empty, then a default rule will be used (e.g. category name)."),
                new KeyValuePair<string, string>("Admin.NopStation.CategoryBanners.CategoryBanners.Fields.Picture", "Picture"),
                new KeyValuePair<string, string>("Admin.NopStation.CategoryBanners.CategoryBanners.Fields.Picture.Hint", "Choose a picture to upload. If the picture size exceeds your stores max image size setting, it will be automatically resized."),

                new KeyValuePair<string, string>("Admin.NopStation.CategoryBanners.Configuration.Fields.HideInPublicStore", "Hide in public store"),
                new KeyValuePair<string, string>("Admin.NopStation.CategoryBanners.Configuration.Fields.HideInPublicStore.Hint", "Check to hide category banner in public store."),
                new KeyValuePair<string, string>("Admin.NopStation.CategoryBanners.Configuration.Fields.Loop", "Loop"),
                new KeyValuePair<string, string>("Admin.NopStation.CategoryBanners.Configuration.Fields.Loop.Hint", "Check to enable loop. It will be applied for banner slider, when multiple banners uploaded for same category."),
                new KeyValuePair<string, string>("Admin.NopStation.CategoryBanners.Configuration.Fields.AutoPlay", "Auto play"),
                new KeyValuePair<string, string>("Admin.NopStation.CategoryBanners.Configuration.Fields.AutoPlay.Hint", "Check to enable auto play. It will be applied for banner slider, when multiple banners uploaded for same category."),
                new KeyValuePair<string, string>("Admin.NopStation.CategoryBanners.Configuration.Fields.Nav", "NAV"),
                new KeyValuePair<string, string>("Admin.NopStation.CategoryBanners.Configuration.Fields.Nav.Hint", "Check to enable next/prev buttons. It will be applied for banner slider, when multiple banners uploaded for same category."),
                new KeyValuePair<string, string>("Admin.NopStation.CategoryBanners.Configuration.Fields.BannerPictureSize", "Banner picture size"),
                new KeyValuePair<string, string>("Admin.NopStation.CategoryBanners.Configuration.Fields.BannerPictureSize.Hint", "Set banner picture size in pixel."),
                new KeyValuePair<string, string>("Admin.NopStation.CategoryBanners.Configuration.Fields.AutoPlayTimeout", "Auto play timeout"),
                new KeyValuePair<string, string>("Admin.NopStation.CategoryBanners.Configuration.Fields.AutoPlayTimeout.Hint", "It's autoplay interval timeout in miliseconds (e.g 5000). It will be applied for banner slider, when multiple banners uploaded for same category."),
                new KeyValuePair<string, string>("Admin.NopStation.CategoryBanners.Configuration.Fields.AutoPlayHoverPause", "Auto play hover pause"),
                new KeyValuePair<string, string>("Admin.NopStation.CategoryBanners.Configuration.Fields.AutoPlayHoverPause.Hint", "Check to enable pause on mouse hover. It will be applied for banner slider, when multiple banners uploaded for same category.")
            };

            return list;
        }


        #endregion
    }
}
