using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Infrastructure;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Services;
using NopStation.Plugin.Widgets.AnywhereSlider.Components;
using NopStation.Plugin.Widgets.AnywhereSlider.Domains;
using NopStation.Plugin.Widgets.AnywhereSlider.Helpers;
using NopStation.Plugin.Widgets.AnywhereSlider.Services;

namespace NopStation.Plugin.Widgets.AnywhereSlider
{
    public class SliderPlugin : BasePlugin, IWidgetPlugin, IAdminMenuPlugin, INopStationPlugin
    {
        #region Fields

        public bool HideInWidgetList => false;

        private readonly IWebHelper _webHelper;
        private readonly INopStationCoreService _nopStationCoreService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly ISliderService _sliderService;
        private readonly IPictureService _pictureService;
        private readonly INopFileProvider _fileProvider;

        #endregion

        #region Ctor

        public SliderPlugin(IWebHelper webHelper,
            INopStationCoreService nopStationCoreService,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            ISettingService settingService,
            ISliderService sliderService,
            IPictureService pictureService,
            INopFileProvider fileProvider)
        {
            _webHelper = webHelper;
            _nopStationCoreService = nopStationCoreService;
            _localizationService = localizationService;
            _permissionService = permissionService;
            _settingService = settingService;
            _sliderService = sliderService;
            _pictureService = pictureService;
            _fileProvider = fileProvider;
        }

        #endregion

        #region Utilities

        protected async Task CreateSampleDataAsync()
        {
            var sliderSetting = new SliderSettings()
            {
                EnableSlider = true
            };
            await _settingService.SaveSettingAsync(sliderSetting);

            var slider = new Slider()
            {
                Active = true,
                AutoPlay = true,
                AutoPlayTimeout = 3000,
                AutoPlayHoverPause = true,
                CreatedOnUtc = DateTime.UtcNow,
                Name = "Home page top",
                Loop = true,
                UpdatedOnUtc = DateTime.UtcNow,
                Nav = true,
                DisplayOrder = 0,
                StartPosition = 0,
                WidgetZoneId = 5,
            };
            await _sliderService.InsertSliderAsync(slider);

            var sampleImagesPath = _fileProvider.MapPath("~/Plugins/NopStation.Plugin.Widgets.AnywhereSlider/Content/sample/");
            var sliderItems = new SliderItem()
            {
                PictureId = (await _pictureService.InsertPictureAsync(await _fileProvider.ReadAllBytesAsync(_fileProvider.Combine(sampleImagesPath, "slider-1.jpg")), MimeTypes.ImageJpeg, "slider-1")).Id,
                MobilePictureId = (await _pictureService.InsertPictureAsync(await _fileProvider.ReadAllBytesAsync(_fileProvider.Combine(sampleImagesPath, "slider-1-mobile.jpg")), MimeTypes.ImageJpeg, "slider-1")).Id,
                Title = "Liquid for Chicken",
                ShortDescription = "The Best General Tso's Chicken",
                SliderId = slider.Id
            };
            await _sliderService.InsertSliderItemAsync(sliderItems);

            var sliderItemsSecond = new SliderItem()
            {
                PictureId = (await _pictureService.InsertPictureAsync(await _fileProvider.ReadAllBytesAsync(_fileProvider.Combine(sampleImagesPath, "slider-2.jpg")), MimeTypes.ImageJpeg, "slider-2")).Id,
                MobilePictureId = (await _pictureService.InsertPictureAsync(await _fileProvider.ReadAllBytesAsync(_fileProvider.Combine(sampleImagesPath, "slider-2-mobile.jpg")), MimeTypes.ImageJpeg, "slider-2")).Id,
                Title = "Pressure Cooker",
                ShortDescription = "Ribollita Into a Weeknight Meal",
                SliderId = slider.Id
            };
            await _sliderService.InsertSliderItemAsync(sliderItemsSecond);

            var sliderItemsThird = new SliderItem()
            {
                PictureId = (await _pictureService.InsertPictureAsync(await _fileProvider.ReadAllBytesAsync(_fileProvider.Combine(sampleImagesPath, "slider-3.jpg")), MimeTypes.ImageJpeg, "slider-3")).Id,
                MobilePictureId = (await _pictureService.InsertPictureAsync(await _fileProvider.ReadAllBytesAsync(_fileProvider.Combine(sampleImagesPath, "slider-3-mobile.jpg")), MimeTypes.ImageJpeg, "slider-3")).Id,
                Title = "Ingredients",
                ShortDescription = "The Best General Tso's Chicken",
                SliderId = slider.Id
            };
            await _sliderService.InsertSliderItemAsync(sliderItemsThird);
        }

        #endregion

        #region Methods

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/AnywhereSlider/Configure";
        }

        public Type GetWidgetViewComponent(string widgetZone)
        {
            if (widgetZone == PublicWidgetZones.Footer)
                return typeof(AnywhereSliderFooterViewComponent);

            return typeof(AnywhereSliderViewComponent);
        }

        public Task<IList<string>> GetWidgetZonesAsync()
        {
            var widgetZones = SliderHelper.GetCustomWidgetZones();
            widgetZones.Add(PublicWidgetZones.Footer);

            return Task.FromResult<IList<string>>(widgetZones);
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.AnywhereSlider.Menu.AnywhereSlider"),
                Visible = true,
                IconClass = "far fa-dot-circle",
            };

            if (await _permissionService.AuthorizeAsync(SliderPermissionProvider.ManageSliders))
            {
                var listItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.AnywhereSlider.Menu.Sliders"),
                    Url = "~/Admin/AnywhereSlider/List",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "AnywhereSlider"
                };
                menuItem.ChildNodes.Add(listItem);

                var configItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.AnywhereSlider.Menu.Configuration"),
                    Url = "~/Admin/AnywhereSlider/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "AnywhereSlider.Configuration"
                };
                menuItem.ChildNodes.Add(configItem);
            }

            var documentation = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                Url = "https://www.nop-station.com/anywhere-slider-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=anywhere-slider",
                Visible = true,
                IconClass = "far fa-circle",
                OpenUrlInNewTab = true
            };
            menuItem.ChildNodes.Add(documentation);

            await _nopStationCoreService.ManageSiteMapAsync(rootNode, menuItem, NopStationMenuType.Plugin);
        }

        public override async Task InstallAsync()
        {
            await CreateSampleDataAsync();
            await this.InstallPluginAsync(new SliderPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await this.UninstallPluginAsync(new SliderPermissionProvider());
            await base.UninstallAsync();
        }

        #endregion

        #region Nop-station

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            var list = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.Sliders.List.SearchActive.Active", "Active"),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.Sliders.List.SearchActive.Inactive", "Inactive"),

                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.Menu.AnywhereSlider", "Anywhere slider"),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.Menu.Configuration", "Configuration"),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.Menu.Sliders", "Sliders"),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.Configuration", "Slider settings"),

                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.Tab.Info", "Info"),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.Tab.Properties", "Properties"),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.Tab.SliderItems", "Slider items"),

                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.SliderList", "Sliders"),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.EditDetails", "Edit slider details"),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.BackToList", "back to slider list"),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.AddNew", "Add new slider"),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.SliderItems.SaveBeforeEdit", "You need to save the slider before you can add items for this slider page."),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.SliderItems.AddNew", "Add new item"),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.SliderItems.Pictures.Alert.PictureAdd", "Failed to add product picture."),

                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.Configuration.Fields.EnableSlider", "Enable slider"),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.Configuration.Fields.EnableSlider.Hint", "Check to enable slider for your store."),

                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.Configuration.Updated", "Slider configuration updated successfully."),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.Sliders.Created", "Slider has been created successfully."),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.Sliders.Updated", "Slider has been updated successfully."),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.Sliders.Deleted", "Slider has been deleted successfully."),

                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.SliderItems.Fields.DisplayOrder", "Display order"),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.SliderItems.Fields.Picture", "Picture"),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.SliderItems.Fields.MobilePicture", "Mobile picture"),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.SliderItems.Fields.ImageAltText", "Alt"),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.SliderItems.Fields.Title", "Title"),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.SliderItems.Fields.ShortDescription", "Short description"),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.SliderItems.Fields.Link", "Link"),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.SliderItems.Fields.DisplayOrder.Hint", "The display order for this slider item. 1 represents the top of the list."),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.SliderItems.Fields.Picture.Hint", "Picture of this slider item."),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.SliderItems.Fields.MobilePicture.Hint", "Mobile view picture of this slider item."),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.SliderItems.Fields.ImageAltText.Hint", "Override \"alt\" attribute for \"img\" HTML element."),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.SliderItems.Fields.Title.Hint", "Override \"title\" attribute for \"img\" HTML element."),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.SliderItems.Fields.Link.Hint", "Custom link for slider item picture."),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.SliderItems.Fields.ShortDescription.Hint", "Short description for this slider item."),

                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.Sliders.Fields.Name", "Name"),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.Sliders.Fields.Name.Hint", "The slider name."),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.Sliders.Fields.Title", "Title"),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.Sliders.Fields.Title.Hint", "The slider title."),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.Sliders.Fields.DisplayTitle", "Display title"),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.Sliders.Fields.DisplayTitle.Hint", "Determines whether title should be displayed on public site (depends on theme design)."),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.Sliders.Fields.Active", "Active"),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.Sliders.Fields.Active.Hint", "Determines whether this slider is active (visible on public store)."),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.Sliders.Fields.WidgetZone", "Widget zone"),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.Sliders.Fields.WidgetZone.Hint", "The widget zone where this slider will be displayed."),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.Sliders.Fields.Picture", "Picture"),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.Sliders.Fields.Picture.Hint", "The slider picture."),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.Sliders.Fields.CustomUrl", "Custom url"),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.Sliders.Fields.CustomUrl.Hint", "The slider custom url."),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.Sliders.Fields.AutoPlay", "Auto play"),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.Sliders.Fields.AutoPlay.Hint", "Check to enable auto play."),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.Sliders.Fields.CustomCssClass", "Custom css class"),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.Sliders.Fields.CustomCssClass.Hint", "Enter the custom CSS class to be applied."),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.Sliders.Fields.DisplayOrder", "Display order"),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.Sliders.Fields.DisplayOrder.Hint", "Display order of the slider. 1 represents the top of the list."),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.Sliders.Fields.Loop", "Loop"),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.Sliders.Fields.Loop.Hint", "heck to enable loop."),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.Sliders.Fields.Margin", "Margin"),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.Sliders.Fields.Margin.Hint", "It's margin-right (px) on item."),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.Sliders.Fields.StartPosition", "Start position"),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.Sliders.Fields.StartPosition.Hint", "Starting position."),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.Sliders.Fields.Center", "Center"),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.Sliders.Fields.Center.Hint", "Check to center item. It works well with even and odd number of items."),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.Sliders.Fields.Nav", "NAV"),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.Sliders.Fields.Nav.Hint", "Check to enable next/prev buttons."),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.Sliders.Fields.LazyLoad", "Lazy load"),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.Sliders.Fields.LazyLoad.Hint", "Check to enable lazy load."),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.Sliders.Fields.LazyLoadEager", "Lazy load eager"),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.Sliders.Fields.LazyLoadEager.Hint", "Specify how many items you want to pre-load images to the right (and left when loop is enabled)."),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.Sliders.Fields.AutoPlayTimeout", "Auto play timeout"),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.Sliders.Fields.AutoPlayTimeout.Hint", "It's autoplay interval timeout. (e.g 5000)"),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.Sliders.Fields.AutoPlayHoverPause", "Auto play hover pause"),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.Sliders.Fields.AutoPlayHoverPause.Hint", "Check to enable pause on mouse hover."),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.Sliders.Fields.AnimateOut", "Animate out"),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.Sliders.Fields.AnimateOut.Hint", "Animate out."),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.Sliders.Fields.AnimateIn", "Animate in"),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.Sliders.Fields.AnimateIn.Hint", "Animate in."),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.Sliders.Fields.CreatedOn", "Created on"),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.Sliders.Fields.CreatedOn.Hint", "The create date of this slider."),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.Sliders.Fields.UpdatedOn", "Updated on"),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.Sliders.Fields.UpdatedOn.Hint", "The last update date of this slider."),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.Sliders.Fields.SelectedStoreIds", "Limited to stores"),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.Sliders.Fields.SelectedStoreIds.Hint", "Option to limit this slider to a certain store. If you have multiple stores, choose one or several from the list. If you don't use this option just leave this field empty."),

                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.Sliders.Fields.Name.Required", "The name field is required."),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.Sliders.Fields.BackGroundPicture.Required", "The background picture is required."),

                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.Sliders.List.SearchWidgetZones", "Widget zones"),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.Sliders.List.SearchWidgetZones.Hint", "The search widget zones."),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.Sliders.List.SearchStore", "Store"),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.Sliders.List.SearchStore.Hint", "The search store."),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.Sliders.List.SearchActive", "Active"),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.Sliders.List.SearchActive.Hint", "The search active."),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.SliderItems.EditDetails", "Edit details"),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.SliderItems.Fields.Title.Required", "Title is required."),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.SliderItems.Fields.Picture.Required", "Picture is required."),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.SliderItems.Fields.MobilePicture.Required", "Title is required."),

                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.SliderItems.Pictures.Alert.AddNew", "Upload picture first."),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.Sliders.Fields.ShowBackgroundPicture", "Show background picture"),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.Sliders.Fields.ShowBackgroundPicture.Hint", "Determines whether to show background picture or not."),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.Sliders.Fields.BackgroundPicture", "Background picture"),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.Sliders.Fields.BackgroundPicture.Hint", "Background picture for this slider."),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.Sliders.Fields.Name.Required", "Slider Name Is Required"),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.SliderItems.Fields.ShopNowLink", "ShopNow Link"),
                new KeyValuePair<string, string>("Admin.NopStation.AnywhereSlider.SliderItems.Fields.ShopNowLink.Hint", "Your ShopNow Link"),
                new KeyValuePair<string, string>("NopStation.AnywhereSlider.ShopNow", "Shop Now"),
                new KeyValuePair<string, string>("NopStation.AnywhereSlider.LoadingFailed", "Failed to load slider content.")
            };

            return list;
        }

        #endregion
    }
}
