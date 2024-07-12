using System;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Customers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Models.Media;
using NopStation.Plugin.Misc.Core.Services;
using NopStation.Plugin.Widgets.Popups.Domains;
using NopStation.Plugin.Widgets.Popups.Models;
using NopStation.Plugin.Widgets.Popups.Services;

namespace NopStation.Plugin.Widgets.Popups.Factories;

public class PopupModelFactory : IPopupModelFactory
{
    private readonly IPictureService _pictureService;
    private readonly IPopupService _popupService;
    private readonly ILocalizationService _localizationService;
    private readonly IWorkContext _workContext;
    private readonly INopStationContext _nopStationContext;
    private readonly IStoreContext _storeContext;
    private readonly IStaticCacheManager _staticCacheManager;
    private readonly PopupSettings _popupSettings;
    private readonly IWebHelper _webHelper;
    private readonly CustomerSettings _customerSettings;

    public PopupModelFactory(IPictureService pictureService,
        IPopupService popupService,
        ILocalizationService localizationService,
        IWorkContext workContext,
        INopStationContext nopStationContext,
        IStoreContext storeContext,
        IStaticCacheManager cacheManager,
        PopupSettings popupSettings,
        IWebHelper webHelper,
        CustomerSettings customerSettings)
    {
        _pictureService = pictureService;
        _popupService = popupService;
        _localizationService = localizationService;
        _workContext = workContext;
        _nopStationContext = nopStationContext;
        _storeContext = storeContext;
        _staticCacheManager = cacheManager;
        _popupSettings = popupSettings;
        _webHelper = webHelper;
        _customerSettings = customerSettings;
    }

    protected async Task<NewsletterPopupModel> PrepareDefaultPopupModelAsync()
    {
        var store = await _storeContext.GetCurrentStoreAsync();

        //prepare picture model
        var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(PopupCacheDefaults.DefaultPopupModelCacheKey,
            await _workContext.GetWorkingLanguageAsync(), _webHelper.IsCurrentConnectionSecured(), store);

        return await _staticCacheManager.GetAsync(cacheKey, async () =>
        {
            var model = new NewsletterPopupModel();

            if (!_popupSettings.EnableNewsletterPopup)
                return model;

            if (_popupSettings.ShowOnHomePageOnly && !(await _nopStationContext.GetRouteNameAsync()).Equals("HomePage", StringComparison.InvariantCultureIgnoreCase))
                return model;

            model.PopupEnabled = true;
            model.AllowCustomerToSelectDoNotShowThisPopupAgain = _popupSettings.AllowCustomerToSelectDoNotShowThisPopupAgain;
            model.PreSelectedDoNotShowThisPopupAgain = _popupSettings.PreSelectedDoNotShowThisPopupAgain;
            model.OpenPopupOnLoadPage = _popupSettings.OpenPopupOnLoadPage;
            model.PopupOpenerSelector = _popupSettings.PopupOpenerSelector;
            model.RedirectUrl = _popupSettings.RedirectUrl;
            model.DelayTime = _popupSettings.DelayTime;
            model.AllowToUnsubscribe = _customerSettings.NewsletterBlockAllowToUnsubscribe;

            model.Title = await _localizationService.GetLocalizedSettingAsync(_popupSettings,
                    x => x.NewsletterPopupTitle, (await _workContext.GetWorkingLanguageAsync()).Id, store.Id);
            model.Desctiption = await _localizationService.GetLocalizedSettingAsync(_popupSettings,
                    x => x.NewsletterPopupDesctiption, (await _workContext.GetWorkingLanguageAsync()).Id, store.Id);

            if (_popupSettings.BackgroundPictureId == 0)
                return model;

            model.HasBackgroundPicture = true;
            var picture = await _pictureService.GetPictureByIdAsync(_popupSettings.BackgroundPictureId);
            var (fullSizeImageUrl, _) = await _pictureService.GetPictureUrlAsync(picture);

            model.BackgroundPicture = new PictureModel
            {
                ImageUrl = fullSizeImageUrl,
                FullSizeImageUrl = fullSizeImageUrl,
                //"title" attribute
                Title = (picture != null && !string.IsNullOrEmpty(picture.TitleAttribute))
                    ? picture.TitleAttribute
                    : await _localizationService.GetResourceAsync("NopStation.Popups.DefaultPopup.ImageLinkTitleFormat"),
                //"alt" attribute
                AlternateText = (picture != null && !string.IsNullOrEmpty(picture.AltAttribute))
                    ? picture.AltAttribute
                    : await _localizationService.GetResourceAsync("NopStation.Popups.DefaultPopup.ImageAlternateTextFormat")
            };

            return model;
        });
    }

    protected async Task<PopupModel> PreparePopupModelAsync(Popup popup, bool isMobbile)
    {
        var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(PopupCacheDefaults.PopupModelCacheKey, popup.Id,
            await _workContext.GetWorkingLanguageAsync(), _webHelper.IsCurrentConnectionSecured(),
            await _storeContext.GetCurrentStoreAsync(), isMobbile);

        return await _staticCacheManager.GetAsync(cacheKey, async () =>
        {
            var popupModel = new PopupModel()
            {
                AllowCustomerToSelectDoNotShowThisPopupAgain = popup.AllowCustomerToSelectDoNotShowThisPopupAgain,
                PreSelectedDoNotShowThisPopupAgain = popup.PreSelectedDoNotShowThisPopupAgain,
                ColumnType = popup.ColumnType,
                CssClass = popup.CssClass,
                DelayTime = popup.DelayTime,
                EnableStickyButton = popup.EnableStickyButton,
                Id = popup.Id,
                Name = await _localizationService.GetLocalizedAsync(popup, x => x.Name),
                StickyButtonColor = popup.StickyButtonColor,
                StickyButtonPosition = popup.StickyButtonPosition,
                OpenPopupOnLoadPage = popup.OpenPopupOnLoadPage,
                StickyButtonText = await _localizationService.GetLocalizedAsync(popup, x => x.StickyButtonText)
            };

            var column1 = new PopupModel.ColumnModel
            {
                ContentType = popup.Column1ContentType,
                PopupUrl = await _localizationService.GetLocalizedAsync(popup, x => x.Column1PopupUrl),
                Text = await _localizationService.GetLocalizedAsync(popup, x => x.Column1Text)
            };

            var c1PictureId = isMobbile && popup.Column1MobilePictureId > 0 ? popup.Column1MobilePictureId : popup.Column1DesktopPictureId;
            var c1Picture = await _pictureService.GetPictureByIdAsync(c1PictureId);
            var (c1ImageUrl, _) = await _pictureService.GetPictureUrlAsync(c1Picture);

            column1.Picture = new PictureModel
            {
                ImageUrl = c1ImageUrl,
                FullSizeImageUrl = c1ImageUrl,
                //"title" attribute
                Title = (c1Picture != null && !string.IsNullOrEmpty(c1Picture.TitleAttribute))
                    ? c1Picture.TitleAttribute
                    : string.Format(await _localizationService.GetResourceAsync("NopStation.Popups.Popups.ImageLinkTitleFormat"), popup.Name),
                //"alt" attribute
                AlternateText = (c1Picture != null && !string.IsNullOrEmpty(c1Picture.AltAttribute))
                    ? c1Picture.AltAttribute
                    : string.Format(await _localizationService.GetResourceAsync("NopStation.Popups.Popups.ImageAlternateTextFormat"), popup.Name)
            };

            popupModel.Column1 = column1;

            if (popup.ColumnType == ColumnType.DoubleColumn)
            {
                var column2 = new PopupModel.ColumnModel
                {
                    ContentType = popup.Column2ContentType,
                    PopupUrl = await _localizationService.GetLocalizedAsync(popup, x => x.Column2PopupUrl),
                    Text = await _localizationService.GetLocalizedAsync(popup, x => x.Column2Text)
                };

                var c2PictureId = isMobbile && popup.Column2MobilePictureId > 0 ? popup.Column2MobilePictureId : popup.Column2DesktopPictureId;
                var c2Picture = await _pictureService.GetPictureByIdAsync(c2PictureId);
                var (c2ImageUrl, _) = await _pictureService.GetPictureUrlAsync(c2Picture);

                column2.Picture = new PictureModel
                {
                    ImageUrl = c2ImageUrl,
                    FullSizeImageUrl = c2ImageUrl,
                    //"title" attribute
                    Title = (c2Picture != null && !string.IsNullOrEmpty(c2Picture.TitleAttribute))
                        ? c2Picture.TitleAttribute
                        : string.Format(await _localizationService.GetResourceAsync("NopStation.Popups.Popups.ImageLinkTitleFormat"), popup.Name),
                    //"alt" attribute
                    AlternateText = (c2Picture != null && !string.IsNullOrEmpty(c2Picture.AltAttribute))
                        ? c2Picture.AltAttribute
                        : string.Format(await _localizationService.GetResourceAsync("NopStation.Popups.Popups.ImageAlternateTextFormat"), popup.Name)
                };

                popupModel.Column2 = column2;
            }

            return popupModel;
        });
    }

    public async Task<PopupPublicModel> PreparePopupPublicModelAsync()
    {
        var isMobbile = _nopStationContext.MobileDevice;
        var productId = 0;

        if ((await _nopStationContext.GetRouteNameAsync()).Equals("Product", StringComparison.InvariantCultureIgnoreCase))
            productId = _nopStationContext.GetRouteValue(NopRoutingDefaults.RouteValue.ProductId, 0);

        var popups = await _popupService.GetAllPopupsAsync(
            storeId: _storeContext.GetCurrentStore().Id,
            overrideProduct: true,
            productId: productId,
            validScheduleOnly: true,
            deviceType: isMobbile ? DeviceType.Mobile : DeviceType.Desktop);

        var model = new PopupPublicModel();
        foreach (var popup in popups)
            model.Popups.Add(await PreparePopupModelAsync(popup, isMobbile));

        model.NewsletterPopup = await PrepareDefaultPopupModelAsync();

        return model;
    }
}
