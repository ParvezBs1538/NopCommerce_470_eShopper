using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Infrastructure;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Services;
using NopStation.Plugin.Misc.WebApi.Areas.Admin.Components;
using NopStation.Plugin.Misc.WebApi.Areas.Admin.Domains;
using NopStation.Plugin.Misc.WebApi.Domains;
using NopStation.Plugin.Misc.WebApi.Extensions;
using NopStation.Plugin.Misc.WebApi.Services;

namespace NopStation.Plugin.Misc.WebApi
{
    public class WebApiPlugin : BasePlugin, IAdminMenuPlugin, IWidgetPlugin, INopStationPlugin
    {
        #region Fields

        private readonly IWebHelper _webHelper;
        private readonly IPermissionService _permissionService;
        private readonly ILocalizationService _localizationService;
        private readonly INopStationCoreService _nopStationCoreService;
        private readonly ISettingService _settingService;
        private readonly IPictureService _pictureService;
        private readonly INopFileProvider _fileProvider;
        private readonly IApiSliderService _sliderService;
        private readonly IApiStringResourceService _apiStringResourceService;
        private readonly ILogger _logger;

        #endregion

        #region Ctor

        public WebApiPlugin(IWebHelper webHelper,
            IPermissionService permissionService,
            ILocalizationService localizationService,
            INopStationCoreService nopStationCoreService,
            ISettingService settingService,
            IPictureService pictureService,
            INopFileProvider fileProvider,
            IApiSliderService sliderService,
            IApiStringResourceService apiStringResourceService,
            ILogger logger)
        {
            _webHelper = webHelper;
            _permissionService = permissionService;
            _localizationService = localizationService;
            _nopStationCoreService = nopStationCoreService;
            _settingService = settingService;
            _pictureService = pictureService;
            _fileProvider = fileProvider;
            _sliderService = sliderService;
            _apiStringResourceService = apiStringResourceService;
            _logger = logger;
        }

        #endregion

        #region Utilities

        private async Task CreateSampleDataAsync()
        {
            try
            {
                var sampleImagesPath = _fileProvider.MapPath("~/Plugins/NopStation.Plugin.Misc.WebApi/Install/");

                var settings = new WebApiSettings()
                {
                    PrimaryThemeColor = WebApiDefaults.PrimaryThemeColor,
                    BottomBarActiveColor = WebApiDefaults.BottomBarActiveColor,
                    BottomBarInactiveColor = WebApiDefaults.BottomBarInactiveColor,
                    BottomBarBackgroundColor = WebApiDefaults.BottomBarBackgroundColor,
                    TopBarBackgroundColor = WebApiDefaults.TopBarBackgroundColor,
                    TopBarTextColor = WebApiDefaults.TopBarTextColor,
                    GradientStartingColor = WebApiDefaults.GradientStartingColor,
                    GradientMiddleColor = WebApiDefaults.GradientMiddleColor,
                    GradientEndingColor = WebApiDefaults.GradientEndingColor,
                    GradientEnabled = false,
                    ShowHomepageSlider = true,
                    ShowBestsellersOnHomepage = true,
                    ShowFeaturedProducts = true,
                    ShowHomepageCategoryProducts = true,
                    ShowManufacturers = true,
                    ShowSubCategoryProducts = true,
                    MaximumNumberOfHomePageSliders = 10,
                    NumberOfHomepageCategoryProducts = 10,
                    NumberOfManufacturers = 10,
                    AndriodForceUpdate = true,
                    AndroidVersion = "1.0.0",
                    IOSForceUpdate = true,
                    IOSVersion = "1.0.0",
                    LogoSize = 180,
                    IOSProductPriceTextSize = 12,
                    IonicProductPriceTextSize = 18,
                    AndroidProductPriceTextSize = 11,
                    SliderAutoPlay = true,
                    SliderAutoPlayTimeout = 5,
                    ProductBarcodeScanKeyId = (int)BarcodeScanKeyType.Sku,
                    LogoId = (await _pictureService.InsertPictureAsync(await _fileProvider.ReadAllBytesAsync(_fileProvider.Combine(sampleImagesPath, "nop-station-logo.png")), MimeTypes.ImagePng, "nop-station")).Id,
                    SecretKey = HelperExtension.RandomString(48)
                };
                await _settingService.SaveSettingAsync(settings);

                var sliders = new List<ApiSlider>
                {
                    new ApiSlider()
                    {
                        CreatedOnUtc = DateTime.UtcNow,
                        DisplayOrder = 1,
                        SliderType = SliderType.None,
                        PictureId = (await _pictureService.InsertPictureAsync(await _fileProvider.ReadAllBytesAsync(_fileProvider.Combine(sampleImagesPath, "slider-1.jpg")), MimeTypes.ImageJpeg, "slider-1")).Id
                    },
                    new ApiSlider()
                    {
                        CreatedOnUtc = DateTime.UtcNow,
                        DisplayOrder = 2,
                        SliderType = SliderType.None,
                        PictureId = (await _pictureService.InsertPictureAsync(await _fileProvider.ReadAllBytesAsync(_fileProvider.Combine(sampleImagesPath, "slider-2.jpg")), MimeTypes.ImageJpeg, "slider-2")).Id
                    },
                    new ApiSlider()
                    {
                        CreatedOnUtc = DateTime.UtcNow,
                        DisplayOrder = 3,
                        SliderType = SliderType.None,
                        PictureId = (await _pictureService.InsertPictureAsync(await _fileProvider.ReadAllBytesAsync(_fileProvider.Combine(sampleImagesPath, "slider-3.jpg")), MimeTypes.ImageJpeg, "slider-3")).Id
                    }
                };

                await _sliderService.InsertApiSliderAsync(sliders);

                var filePath = _fileProvider.Combine(sampleImagesPath, "apiStringResources.json");
                var text = _fileProvider.ReadAllText(filePath, Encoding.UTF8);
                var resources = JsonConvert.DeserializeObject<List<string>>(text);
                var appResources = resources.Select(resource => new ApiStringResource { ResourceName = resource }).ToList();

                await _apiStringResourceService.InsertApiStringResourceAsync(appResources);
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync($"Failed to create web api sample data: {ex.Message}", ex);
            }
        }

        #endregion

        #region Properties

        public bool HideInWidgetList => true;

        #endregion

        #region Methods

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/WebApi/Configure";
        }

        public override async Task InstallAsync()
        {
            await this.InstallPluginAsync(new WebApiPermissionProvider());
            await CreateSampleDataAsync();

            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await this.UninstallPluginAsync(new WebApiPermissionProvider());
            await base.UninstallAsync();
        }

        public override async Task UpdateAsync(string currentVersion, string targetVersion)
        {
            if (currentVersion == "4.60.1.0" && targetVersion == "4.60.1.1")
                await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.NopStation.WebApi.Tab.CategoryBanner", "Category banner");
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menu = new SiteMapNode()
            {
                Visible = true,
                IconClass = "far fa-dot-circle",
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.WebApi.Menu.WebApi")
            };

            if (await _permissionService.AuthorizeAsync(WebApiPermissionProvider.ManageSlider))
            {
                var slider = new SiteMapNode()
                {
                    Visible = true,
                    IconClass = "far fa-circle",
                    Url = $"{_webHelper.GetStoreLocation()}Admin/WebApiSlider/List",
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.WebApi.Menu.Sliders"),
                    SystemName = "WebApiSlider"
                };
                menu.ChildNodes.Add(slider);
            }

            if (await _permissionService.AuthorizeAsync(WebApiPermissionProvider.ManageCategoryIcon))
            {
                var device = new SiteMapNode()
                {
                    Visible = true,
                    IconClass = "far fa-circle",
                    Url = $"{_webHelper.GetStoreLocation()}Admin/WebApiCategoryIcon/List",
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.WebApi.Menu.CategoryIcons"),
                    SystemName = "WebApiCategoryIcon"
                };
                menu.ChildNodes.Add(device);
            }

            if (await _permissionService.AuthorizeAsync(WebApiPermissionProvider.ManageDevice))
            {
                var device = new SiteMapNode()
                {
                    Visible = true,
                    IconClass = "far fa-circle",
                    Url = $"{_webHelper.GetStoreLocation()}Admin/WebApiDevice/List",
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.WebApi.Menu.Devices"),
                    SystemName = "WebApiDevice"
                };
                menu.ChildNodes.Add(device);
            }

            if (await _permissionService.AuthorizeAsync(WebApiPermissionProvider.ManageConfiguration))
            {
                var configure = new SiteMapNode()
                {
                    Visible = true,
                    IconClass = "far fa-circle",
                    Url = $"{_webHelper.GetStoreLocation()}Admin/WebApi/Configure",
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.WebApi.Menu.Configuration"),
                    SystemName = "WebApi.Configuration"
                };
                menu.ChildNodes.Add(configure);
            }

            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/web-api-v2-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=web-api-v2",
                    Visible = true,
                    IconClass = "far fa-circle",
                    OpenUrlInNewTab = true
                };
                menu.ChildNodes.Add(documentation);
            }

            await _nopStationCoreService.ManageSiteMapAsync(rootNode, menu, NopStationMenuType.Plugin);
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            var list = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Enums.NopStation.Plugin.Misc.WebApi.Domains.DeviceType.IPhone", "iPhone"),
                new KeyValuePair<string, string>("Enums.NopStation.Plugin.Misc.WebApi.Domains.DeviceType.Android", "Android"),
                new KeyValuePair<string, string>("Enums.NopStation.Plugin.Misc.WebApi.Domains.DeviceType.WindowsPhone", "Windows phone"),
                new KeyValuePair<string, string>("Enums.NopStation.Plugin.Misc.WebApi.Domains.DeviceType.Others", "Others"),
                new KeyValuePair<string, string>("Enums.NopStation.Plugin.Misc.WebApi.Domains.SliderType.None", "None"),
                new KeyValuePair<string, string>("Enums.NopStation.Plugin.Misc.WebApi.Domains.SliderType.Product", "Product"),
                new KeyValuePair<string, string>("Enums.NopStation.Plugin.Misc.WebApi.Domains.SliderType.Category", "Category"),
                new KeyValuePair<string, string>("Enums.NopStation.Plugin.Misc.WebApi.Domains.SliderType.Manufacturer", "Manufacturer"),
                new KeyValuePair<string, string>("Enums.NopStation.Plugin.Misc.WebApi.Domains.SliderType.Vendor", "Vendor"),
                new KeyValuePair<string, string>("Enums.NopStation.Plugin.Misc.WebApi.Domains.SliderType.Topic", "Topic"),

                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Menu.WebApi", "Mobile web api"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Menu.Sliders", "App sliders"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Menu.CategoryIcons", "Category icon"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Menu.Devices", "Mobile devices"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Menu.Configuration", "Configuration"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Menu.SecurityConfiguration", "Security"),

                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Title", "Web api settings"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.BlockTitle.Common", "Common"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.BlockTitle.Security", "Security"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.BlockTitle.AppSettings", "App settings"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.BlockTitle.Resources", "String resources"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.BlockTitle.ColorSettings", "Color settings"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Updated", "Web api settings updated successfully."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.SecurityConfiguration.Title", "Security settings"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.SecurityConfiguration.Updated", "Security settings updated successfully."),

                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.EnableJwtSecurity", "Enable JWT security"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.EnableJwtSecurity.Hint", "Check to enable JWT security. It will require 'NST' header for every api request."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.ShowHomepageSlider", "Show homepage slider"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.ShowHomepageSlider.Hint", "Check to show slider on app homepage."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.SliderAutoPlay", "Auto play"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.SliderAutoPlay.Hint", "Check to enable auto play. It will be applied for homepage slider."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.SliderAutoPlayTimeout", "Auto play timeout"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.SliderAutoPlayTimeout.Hint", "It's autoplay interval timeout in seconds (e.g 5). It will be applied for homepage slider."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.MaximumNumberOfHomePageSliders", "Maximum sliders"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.MaximumNumberOfHomePageSliders.Hint", "Define the maximum number of home page sliders for app. Keep it '0' if you want to display all active sliders."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.ShowFeaturedProducts", "Show featured products"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.ShowFeaturedProducts.Hint", "Check to display featured products on app landing page."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.ShowBestsellersOnHomepage", "Show best sellers on homepage"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.ShowBestsellersOnHomepage.Hint", "Check to display best selling products on app landing page."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.NumberOfBestsellersOnHomepage", "Number of best sellers on homepage"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.NumberOfBestsellersOnHomepage.Hint", "Set the number of best selling products to be shown on app landing page."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.ShowHomepageCategoryProducts", "Show homepage category products"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.ShowHomepageCategoryProducts.Hint", "Check to display homepage category products on app landing page."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.ShowSubCategoryProducts", "Show sub-category products"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.ShowSubCategoryProducts.Hint", "Check to display sub-category products on app landing page."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.NumberOfHomepageCategoryProducts", "Maximum number of products"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.NumberOfHomepageCategoryProducts.Hint", "Set product number to be shown on app landing page."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.ShowManufacturers", "Show manufacturers"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.ShowManufacturers.Hint", "Check to display manufacturers on app landing page."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.NumberOfManufacturers", "Maximum number of Manufacturers"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.NumberOfManufacturers.Hint", "Set manufacturer number to be shown on app landing page."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.AndroidVersion", "Android version"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.AndroidVersion.Hint", "Current android version published in Google Play Store."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.AndriodForceUpdate", "Andriod force update"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.AndriodForceUpdate.Hint", "By marking it as checked, Android users will be forced to update their app when it will not match with current version published in Play Store."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.PlayStoreUrl", "Play Store url"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.PlayStoreUrl.Hint", "The Play Store url for your app."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.IOSVersion", "iOS Version"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.IOSVersion.Hint", "Current iOS version published in App Store."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.IOSForceUpdate", "iOS force update"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.IOSForceUpdate.Hint", "By marking it as checked, iOS users will be forced to update their app when it will not match with current version published in App Store."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.AppStoreUrl", "App Store url"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.AppStoreUrl.Hint", "The App Store url for your app."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.LogoId", "Logo"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.LogoId.Hint", "The logo which will be displayed in mobile app."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.LogoSize", "Logo Size"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.LogoSize.Hint", "Size of the logo you want to display"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.ShowChangeBaseUrlPanel", "Show change base url panel"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.ShowChangeBaseUrlPanel.Hint", "Determines whether mobile app user can change api base url from their app. Check this option when you are in testing mode."),

                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.PrimaryThemeColor", "Primary theme color"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.PrimaryThemeColor.Hint", "Primary theme color of your app"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.BottomBarBackgroundColor", "Bottom bar background color"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.BottomBarBackgroundColor.Hint", "Bottom bar background color of your app"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.BottomBarActiveColor", "Bottom bar active color"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.BottomBarActiveColor.Hint", "Bottom bar active color of your app"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.BottomBarInactiveColor", "Bottom bar inactive Color"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.BottomBarInactiveColor.Hint", "Bottom bar inactive color of your app"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.TopBarBackgroundColor", "Top bar background color"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.TopBarBackgroundColor.Hint", "Top bar background color of your app"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.TopBarTextColor", "Top bar text color"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.TopBarTextColor.Hint", "Top bar text color of your app"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.GradientStartingColor", "Gradient starting color"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.GradientStartingColor.Hint", "Gradient starting color"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.GradientMiddleColor", "Gradient middle color"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.GradientMiddleColor.Hint", "Gradient middle color"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.GradientEndingColor", "Gradient ending color"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.GradientEndingColor.Hint", "Gradient ending color"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.GradientEnabled", "Gradient enabled"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.GradientEnabled.Hint", "Gradient enabled"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.ResetToDefaultColors", "Reset Colors"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.ResetColors.Confirmation", "Are you sure you want to reset the colors?"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.ResetToDefaultColors", "Reset to default colors"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.ResetToDefaultColors.Hint", "Used for reverting colors back to default. Please Save After Reset."),

                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.SecretKey", "Secret key"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.SecretKey.Hint", "The secret key to sign and verify each JWT token, it can be any string."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.TokenKey", "Token key"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.TokenKey.Hint", "The JSON web token security key (payload: NST_KEY)."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.TokenSecret", "Token secret"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.TokenSecret.Hint", "512 bit JSON web token secret."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.CheckIat", "Check IAT"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.CheckIat.Hint", "Click to check issued at time."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.TokenSecondsValid", "JSON web token. Seconds valid"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.TokenSecondsValid.Hint", "Enter number of seconds for valid JSON web token."),

                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.PanelTitle.ProductPriceTextSize", "Product price text size on product box"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.IOSProductPriceTextSize", "IOS"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.IOSProductPriceTextSize.Hint", "Select IOS product price text size on product box between 8 and 16"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.AndroidProductPriceTextSize", "Android"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.AndroidProductPriceTextSize.Hint", "Android product price text size on product box between 10 and 14"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.IonicProductPriceTextSize", "Ionic"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.IonicProductPriceTextSize.Hint", "Ionic product price text size on product box between 16 and 24"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.IOSProductPriceTextSize.GreaterThanAndLessThanLimit", "IOS product price text size on product box should be in between 8 and 16"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.AndroidProductPriceTextSize.GreaterThanAndLessThanLimit", "Android product price text size on product box is should be in between 10 and 14"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.IonicProductPriceTextSize.GreaterThanAndLessThanLimit", "Ionic product price text size on product box is should be in between 16 and 24"),

                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Sliders.Fields.Picture", "Picture"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Sliders.Fields.Picture.Hint", "The slider picture."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Sliders.Fields.ActiveStartDate", "Active start date"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Sliders.Fields.ActiveStartDate.Hint", "The start date of the slider's availability."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Sliders.Fields.ActiveEndDate", "Active end date"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Sliders.Fields.ActiveEndDate.Hint", "The end date of the slider's availability."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Sliders.Fields.SliderType", "Slider type"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Sliders.Fields.SliderType.Hint", "Choose slider type."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Sliders.Fields.EntityId", "Entity id"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Sliders.Fields.EntityId.Hint", "The 'Entity id'. i.e. Category id, Vendor id."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Sliders.Fields.DisplayOrder", "Display order"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Sliders.Fields.DisplayOrder.Hint", "The slider display order. 1 represents the first item in the list."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Sliders.Fields.CreatedOn", "Created on"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Sliders.Fields.CreatedOn.Hint", "The date/time that the slider was created."),

                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Sliders.Fields.Picture.Required", "The 'Picture' is required."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Sliders.Fields.SliderType.Required", "The 'Slider type' is required."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Sliders.Fields.ActiveEndDate.GreaterThanStartDate", "End date must be greater than start date."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Sliders.Fields.EntityId.Required", "The 'Entity id' is required."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Sliders.Fields.EntityId.InvalidCategory", "Invalid category id."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Sliders.Fields.EntityId.InvalidManufacturer", "Invalid manufacturer id."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Sliders.Fields.EntityId.InvalidProduct", "Invalid product id."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Sliders.Fields.EntityId.InvalidVendor", "Invalid vendor id."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Sliders.Fields.EntityId.InvalidTopic", "Invalid topic id."),

                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Sliders.List.SelectedSliderTypes", "Slider types"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Sliders.List.SelectedSliderTypes.Hint", "Search by slider types."),

                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Sliders.AddNew", "Add new slider"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Sliders.EditDetails", "Edit slider"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Sliders.BackToList", "back to slider list"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Sliders.DeleteSelected", "Delete selected"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Sliders.List", "Sliders"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Sliders.Created", "Slider has been created successfully."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Sliders.Updated", "Slider has been updated successfully."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Sliders.Deleted", "Slider has been deleted successfully."),

                new KeyValuePair<string, string>("Admin.NopStation.WebApi.CategoryIcons.Fields.Category", "Category"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.CategoryIcons.Fields.Category.Hint", "Select a category."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.CategoryIcons.Fields.Picture", "Icon"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.CategoryIcons.Fields.Picture.Hint", "The category icon."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.CategoryIcons.Fields.CategoryBanner", "Category banner"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.CategoryIcons.Fields.CategoryBanner.Hint", "The category banner."),

                new KeyValuePair<string, string>("Admin.NopStation.WebApi.CategoryIcons.List.SearchCategoryName", "Category name"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.CategoryIcons.List.SearchCategoryName.Hint", "Search by category name."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.CategoryIcons.List.SearchStore", "Store"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.CategoryIcons.List.SearchStore.Hint", "Search by store."),

                new KeyValuePair<string, string>("Admin.NopStation.WebApi.CategoryIcons.Fields.Picture.Required", "The 'Picture' is required."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.CategoryIcons.Fields.Category.Required", "The 'Category' is required."),

                new KeyValuePair<string, string>("Admin.NopStation.WebApi.CategoryIcons.List", "Category icons"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.CategoryIcons.DeleteSelected", "Delete selected"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.CategoryIcons.EditDetails", "Edit category icon"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.CategoryIcons.BackToList", "back to icon list"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.CategoryIcons.AddNew", "Add new category icon"),

                new KeyValuePair<string, string>("Admin.NopStation.WebApi.CategoryIcons.Alert.AddIcon", "Please upload icon image."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.CategoryIcons.Alert.IconAddSuccess", "Category icons have been saved successfully."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.CategoryIcons.Alert.IconAddFailed", "Failed to save category icons."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.CategoryIcons.SaveBeforeEdit", "You need to save the category before you can add category icons for this category page."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Tab.CategoryIcons", "Category icons"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Tab.CategoryBanner", "Category banner"),

                new KeyValuePair<string, string>("Admin.NopStation.WebApi.CategoryIcons.Created", "Category icons has been created successfully."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.CategoryIcons.Updated", "Category icons has been updated successfully."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.CategoryIcons.Deleted", "Category icons has been deleted successfully."),

                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Devices.Fields.DeviceToken", "Device token"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Devices.Fields.DeviceToken.Hint", "The token of the device."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Devices.Fields.DeviceType", "Device type"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Devices.Fields.DeviceType.Hint", "The type of the device."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Devices.Fields.Customer", "Customer"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Devices.Fields.Customer.Hint", "The customer of the device."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Devices.Fields.Store", "Store"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Devices.Fields.Store.Hint", "The store device."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Devices.Fields.SubscriptionId", "Subscription id"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Devices.Fields.SubscriptionId.Hint", "The subscription id."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Devices.Fields.IsRegistered", "Is registered"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Devices.Fields.IsRegistered.Hint", "Defines the customer is registered or not."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Devices.Fields.CreatedOn", "Created on"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Devices.Fields.CreatedOn.Hint", "The date, when the customer was created."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Devices.Fields.UpdatedOn", "Updated on"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Devices.Fields.UpdatedOn.Hint", "The date, when the customer was last updated."),

                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Devices.List.SelectedDeviceTypes", "Device type"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Devices.List.SelectedDeviceTypes.Hint", "Search by device types."),

                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Devices.ViewDetails", "View device details"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Devices.BackToList", "back to device list"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Devices.List", "Devices"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Devices.DeleteSelected", "Delete selected"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Common.Unknown", "Unknown"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Devices.Deleted", "Device has been deleted successfully."),

                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Resources.NameAlreadyExists", "A resource already exists with the name: {0}. Please click '{1}' button to add from exsting resources."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Resources.AddFromExistingRecords", "Add from existing records"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.SearchLanguageId", "Language"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.SearchLanguageId.Hint", "Search by language."),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.ProductBarcodeScanKey", "Product barcode scan key"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.ProductBarcodeScanKey.Hint", "Select product barcode scan key."),

                new KeyValuePair<string, string>("NopStation.WebApi.Response.InvalidJwtToken", "Security token is expired or not valid"),
                new KeyValuePair<string, string>("NopStation.WebApi.Response.InvalidToken", "Token has been expired. Please login again"),
                new KeyValuePair<string, string>("NopStation.WebApi.AddToCart.NotMatchingWithCartItem", "This product does not match a passed shopping cart item identifier"),
                new KeyValuePair<string, string>("NopStation.WebApi.AddToCart.SimpleProductOnly", "This product does not match a passed shopping cart item identifier"),
                new KeyValuePair<string, string>("NopStation.WebApi.ShoppingCart.NoFileUploaded", "No file uploaded"),
                new KeyValuePair<string, string>("NopStation.WebApi.Catalog.ProductComparisonDisabled", "Product comparison is disabled"),

                new KeyValuePair<string, string>("NopStation.WebApi.Response.CannotAccessPublicStore", "Cannot access public store"),
                new KeyValuePair<string, string>("NopStation.WebApi.Response.LanguageChanged", "Language changed successfully"),
                new KeyValuePair<string, string>("NopStation.WebApi.Response.CurrencyChanged", "Currency changed successfully"),
                new KeyValuePair<string, string>("NopStation.WebApi.Response.TaxTypeChanged", "Tax type changed successfully"),
                new KeyValuePair<string, string>("NopStation.WebApi.Response.InvalidLicense", "Invalid nop-station product license"),

                new KeyValuePair<string, string>("NopStation.WebApi.Response.Checkout.SaveBillingFailed", "Failed to save billing address"),
                new KeyValuePair<string, string>("NopStation.WebApi.Response.Checkout.SaveShippingFailed", "Failed to save shipping address"),
                new KeyValuePair<string, string>("NopStation.WebApi.Response.Checkout.PaymentMethodNotFound", "No payment method found with the specified name"),
                new KeyValuePair<string, string>("NopStation.WebApi.Response.Checkout.SavePaymentMethodFailed", "Failed to save payment method"),
                new KeyValuePair<string, string>("NopStation.WebApi.Response.Checkout.ConfirmOrderFailed", "Failed to confirm order"),
                new KeyValuePair<string, string>("NopStation.WebApi.Response.Checkout.AnonymousCheckoutNotAllowed", "Anonymous checkout is not allowed"),
                new KeyValuePair<string, string>("NopStation.WebApi.Response.Checkout.AddressCantBeLoaded", "Address can't be loaded"),
                new KeyValuePair<string, string>("NopStation.WebApi.Response.Checkout.ErrorHappened", "Error happened while checkout"),
                new KeyValuePair<string, string>("NopStation.WebApi.Response.Checkout.ShippingNotRequired", "Shipping is not required"),
                new KeyValuePair<string, string>("NopStation.WebApi.Response.Checkout.ShippingCannotBeParsed", "Selected shipping method can't be parsed"),
                new KeyValuePair<string, string>("NopStation.WebApi.Response.Checkout.ShippingCannotBeLoaded", "Selected shipping method can't be loaded"),
                new KeyValuePair<string, string>("NopStation.WebApi.Response.Checkout.PaymentCannotBeParsed", "Selected payment method can't be parsed"),
                new KeyValuePair<string, string>("NopStation.WebApi.Response.Checkout.ProductNotFound", "No product found with the specified id"),
                new KeyValuePair<string, string>("NopStation.WebApi.Response.Product.ProductNotFound", "No product found with the specified id"),
                new KeyValuePair<string, string>("NopStation.WebApi.Response.Product.ProductReviewNotFound", "No product review found with the specified id"),
                new KeyValuePair<string, string>("NopStation.WebApi.Response.Product.ProductByBarCode.ProductNotFound", "No product found with the specified code"),
                new KeyValuePair<string, string>("NopStation.WebApi.Response.Customer.AddressNotFound", "No address found with the specified id"),
                new KeyValuePair<string, string>("NopStation.WebApi.Response.Customer.OrderItemNotFound", "No order item found with the specified id"),
                new KeyValuePair<string, string>("NopStation.WebApi.Response.Customer.CustomerNotFound", "Customer not found"),
                new KeyValuePair<string, string>("NopStation.WebApi.Response.Customer.AddressDeleted", "Address has been deleted successfully"),
                new KeyValuePair<string, string>("NopStation.WebApi.Response.Customer.AddressUpdated", "Address has been updated successfully"),
                new KeyValuePair<string, string>("NopStation.WebApi.Response.Customer.AvatarRemoved", "Avatar has been removed successfully"),
                new KeyValuePair<string, string>("NopStation.WebApi.Response.Topic.FailedToLoad", "Failed to load page details"),
                new KeyValuePair<string, string>("NopStation.WebApi.Response.InvalidDeviceId", "Invalid device id"),
                new KeyValuePair<string, string>("NopStation.WebApi.Response.Checkout.OrderNotFound", "Order not found"),
                new KeyValuePair<string, string>("NopStation.WebApi.Response.PageNotFound", "Page not found"),
                new KeyValuePair<string, string>("NopStation.WebApi.Response.RewardPointsNotAvailable", "Reward points not available"),

                new KeyValuePair<string, string>("NopStation.WebApi.Order.PlacedFromIPhone", "Order placed from iPhone."),
                new KeyValuePair<string, string>("NopStation.WebApi.Order.PlacedFromAndroid", "Order placed from Android."),
                new KeyValuePair<string, string>("NopStation.WebApi.Order.PlacedFromHuawei", "Order placed from Huawei."),
                new KeyValuePair<string, string>("NopStation.WebApi.Order.PlacedFromWeb", "Order placed from web."),

                new KeyValuePair<string, string>("NopStation.WebApi.Common.Account", "Account"),
                new KeyValuePair<string, string>("NopStation.WebApi.Common.Category", "Category"),
                new KeyValuePair<string, string>("NopStation.WebApi.Common.More", "More"),
                new KeyValuePair<string, string>("NopStation.WebApi.Home.SeeAll", "See All"),
                new KeyValuePair<string, string>("NopStation.WebApi.Home.PressAgainToExit", "Press again to exit"),
                new KeyValuePair<string, string>("NopStation.WebApi.Common.Done", "Done"),
                new KeyValuePair<string, string>("NopStation.WebApi.Common.PleaseWait", "Please wait…"),
                new KeyValuePair<string, string>("NopStation.WebApi.Common.Select", "Select"),
                new KeyValuePair<string, string>("NopStation.WebApi.Common.EnterValidEmail", "Please enter valid email"),
                new KeyValuePair<string, string>("NopStation.WebApi.Common.NoData", "No data found"),
                new KeyValuePair<string, string>("NopStation.WebApi.Common.SomethingWrong", "Something went wrong"),
                new KeyValuePair<string, string>("NopStation.WebApi.Filtering.Filter", "Filter"),
                new KeyValuePair<string, string>("NopStation.WebApi.ShoppingCart.BuyNow", "Buy Now"),
                new KeyValuePair<string, string>("NopStation.WebApi.ShoppingCart.Donation.EnterPrice", "Please enter your price:"),
                new KeyValuePair<string, string>("NopStation.WebApi.ShoppingCart.Donation.EnterPrice.Required", "Price required"),
                new KeyValuePair<string, string>("NopStation.WebApi.Account.Info", "Account info"),
                new KeyValuePair<string, string>("NopStation.WebApi.Account.LogoutConfirmation", "Are you sure you want to logout?"),
                new KeyValuePair<string, string>("NopStation.WebApi.AboutUs", "About us"),
                new KeyValuePair<string, string>("NopStation.WebApi.PrivacyPolicy", "Privacy policy"),
                new KeyValuePair<string, string>("NopStation.WebApi.ScanBarcode", "Scan barcode"),
                new KeyValuePair<string, string>("NopStation.WebApi.Settings", "Settings"),
                new KeyValuePair<string, string>("NopStation.WebApi.Settings.NopcommerceUrl", "Your nopCommerce URL"),
                new KeyValuePair<string, string>("NopStation.WebApi.Settings.Language", "Language"),
                new KeyValuePair<string, string>("NopStation.WebApi.Settings.Currency", "Currency"),
                new KeyValuePair<string, string>("NopStation.WebApi.Settings.DarkTheme", "Dark theme"),
                new KeyValuePair<string, string>("NopStation.WebApi.Settings.InvalidUrl", "Invalid URL"),
                new KeyValuePair<string, string>("NopStation.WebApi.Settings.Test", "Test"),
                new KeyValuePair<string, string>("NopStation.WebApi.Settings.Setdefault", "Set default URL"),
                new KeyValuePair<string, string>("NopStation.WebApi.LoginWithFB", "Continue with facebook"),
                new KeyValuePair<string, string>("NopStation.WebApi.Login.Or", "Or"),
                new KeyValuePair<string, string>("NopStation.WebApi.Login.Password.Required", "Password is required"),
                new KeyValuePair<string, string>("NopStation.WebApi.Common.IsRequired", "is Required"),
                new KeyValuePair<string, string>("NopStation.WebApi.Account.Fields.EnterPassword", "Enter password"),
                new KeyValuePair<string, string>("NopStation.WebApi.Address.Fields.AddressSaved", "Address saved successfully"),
                new KeyValuePair<string, string>("NopStation.WebApi.Address.Fields.AddressUpdated", "Address updated successfully"),
                new KeyValuePair<string, string>("NopStation.WebApi.Address.Fields.ConfirmDeleteAddress", "Do you want to delete this address?"),
                new KeyValuePair<string, string>("NopStation.WebApi.Address.Fields.DeleteAddress", "Delete address"),
                new KeyValuePair<string, string>("NopStation.WebApi.Checkout.SelectedAttributes", "Selected attributes"),
                new KeyValuePair<string, string>("NopStation.WebApi.Checkout.OrderCalculation", "Order calculation"),
                new KeyValuePair<string, string>("NopStation.WebApi.RegisterSaveTime", "Register and save time"),
                new KeyValuePair<string, string>("NopStation.WebApi.CreatingAccountLongText", "By creating an account on our website, you will be able to shop faster, be up to date on an orders status, and keep track of the orders you have previously made."),
                new KeyValuePair<string, string>("NopStation.WebApi.ReturningCustomer", "Returning customer?"),
                new KeyValuePair<string, string>("NopStation.WebApi.Wishlist.AddAll", "Add all items to cart"),
                new KeyValuePair<string, string>("NopStation.WebApi.Update.Msg", "The current version of the appilication is no longer supported. Please update the app."),
                new KeyValuePair<string, string>("NopStation.WebApi.Update.Label", "Update new version"),
                new KeyValuePair<string, string>("NopStation.WebApi.NoInternet", "No internet connection"),
                new KeyValuePair<string, string>("NopStation.WebApi.TryAgain", "Try again"),
                new KeyValuePair<string, string>("NopStation.WebApi.ReadBeforeContinue", "Please read this before continue"),
                new KeyValuePair<string, string>("NopStation.WebApi.Accept", "I read & I accept"),
                new KeyValuePair<string, string>("NopStation.WebApi.Updated", "Successfully updated"),
                new KeyValuePair<string, string>("NopStation.WebApi.Checkout.CompletePreviousStep", "Please complete previous step"),
                new KeyValuePair<string, string>("NopStation.WebApi.Barcode.InvalidProduct", "Invalid product"),
                new KeyValuePair<string, string>("NopStation.WebApi.Checkout.OnlinePayment", "Online payment"),
                new KeyValuePair<string, string>("NopStation.WebApi.LoginSuccess", "Successfully logged in."),
                new KeyValuePair<string, string>("NopStation.WebApi.Order.Confirmed", "Order confirmed"),
                new KeyValuePair<string, string>("NopStation.WebApi.Registration.Password", "Password"),
                new KeyValuePair<string, string>("NopStation.WebApi.Registration.PersonalDetails", "Personal details"),
                new KeyValuePair<string, string>("NopStation.WebApi.ShoppingCart.RentNow", "Rent now"),
                new KeyValuePair<string, string>("NopStation.WebApi.Sliders.Fields.Entityid.InvalidProduct", "Product is not valid."),

                new KeyValuePair<string, string>("NopStation.WebApi.Download.NoSampleDownload", "Product doesn't have a sample download."),
                new KeyValuePair<string, string>("NopStation.WebApi.Download.SampleDownloadNotAvailable", "Sample download is not available any more."),
                new KeyValuePair<string, string>("NopStation.WebApi.Download.DownloadDataNotAvailable", "Download data is not available any more."),
                new KeyValuePair<string, string>("NopStation.WebApi.Download.NotAllowed","Downloads are not allowed"),
                new KeyValuePair<string, string>("NopStation.WebApi.Download.NotYourOrder", "This is not your order"),
                new KeyValuePair<string, string>("NopStation.WebApi.Download.DownloadNotAvailable", "Download is not available any more."),

                new KeyValuePair<string, string>("NopStation.WebApi.ReturnRequest.NoFileUploaded", "No file uploaded"),
                new KeyValuePair<string, string>("NopStation.WebApi.Category.CategoryIcons.ImageLinkTitleFormat", "Show banners in category {0}"),
                new KeyValuePair<string, string>("NopStation.WebApi.Category.CategoryIcons.ImageAlternateTextFormat", "Banner for category {0}"),
                new KeyValuePair<string, string>("NopStation.WebApi.Home.OurCategories", "Our categories"),
                new KeyValuePair<string, string>("NopStation.WebApi.Product.Video", "Video"),
                new KeyValuePair<string, string>("NopStation.WebApi.BackInStockSubscriptions.HideBackInStockSubscriptionsTab", "Back in stock subscriptions tab isn't available"),

                new KeyValuePair<string, string>("NopStation.WebApi.Customers.AdminAccountShouldExists.DeleteAdministrator", "Can't delete the last administrator. At least one administrator account should exists"),
                new KeyValuePair<string, string>("NopStation.WebApi.Customers.Deleted", "Customer deleted successfully"),
                new KeyValuePair<string, string>("Nopstation.Webapi.Account.DeleteAccount", "Delete account"),
                new KeyValuePair<string, string>("Nopstation.Webapi.Account.DeleteAccountDialogTitle", "Confirm account deletion"),
                new KeyValuePair<string, string>("Nopstation.Webapi.Account.DeleteAccountDialogBody", "This action can not be revoked.\nAre you sure?"),
                new KeyValuePair<string, string>("Nopstation.Webapi.Account.DeleteAccountPasswordDialogTitle", "Password"),
                new KeyValuePair<string, string>("Nopstation.Webapi.Account.DeleteAccountPasswordDialogBody", "Enter your password"),

                new KeyValuePair<string, string>("NopStation.WebApi.Common.Home", "Home"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.AllowCustomersToDeleteAccount", "Allow customers to delete account"),
                new KeyValuePair<string, string>("Admin.NopStation.WebApi.Configuration.Fields.AllowCustomersToDeleteAccount.Hint", "This settings allow customers to delete account permanently as per European Union's new data privacy law (GDPR)."),
            };

            return list;
        }

        public Task<IList<string>> GetWidgetZonesAsync()
        {
            return Task.FromResult<IList<string>>(new List<string>
            {
                AdminWidgetZones.CategoryDetailsBlock
            });
        }

        public Type GetWidgetViewComponent(string widgetZone)
        {
            return typeof(WebApiCategoryIconAdminViewComponent);
        }

        #endregion
    }
}
