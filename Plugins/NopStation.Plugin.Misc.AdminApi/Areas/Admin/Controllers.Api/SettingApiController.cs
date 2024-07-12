using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core;
using Nop.Core.Configuration;
using Nop.Core.Domain;
using Nop.Core.Domain.Blogs;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Gdpr;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.News;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Seo;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Vendors;
using Nop.Core.Events;
using Nop.Core.Http.Extensions;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Configuration;
using Nop.Services.Authentication.MultiFactor;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Gdpr;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Media.RoxyFileman;
using Nop.Services.Orders;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Settings;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.WebOptimizer;
using NopStation.Plugin.Misc.Core.Models.Api;

namespace NopStation.Plugin.Misc.AdminApi.Areas.Admin.Controllers.Api;

[Route("api/a/setting/[action]")]
public partial class SettingApiController : BaseAdminApiController
{
    #region Fields

    private readonly AppSettings _appSettings;
    private readonly IAddressService _addressService;
    private readonly ICustomerActivityService _customerActivityService;
    private readonly ICustomerService _customerService;
    private readonly INopDataProvider _dataProvider;
    private readonly IEncryptionService _encryptionService;
    private readonly IEventPublisher _eventPublisher;
    private readonly IGenericAttributeService _genericAttributeService;
    private readonly IGdprService _gdprService;
    private readonly ILocalizedEntityService _localizedEntityService;
    private readonly ILocalizationService _localizationService;
    private readonly IMultiFactorAuthenticationPluginManager _multiFactorAuthenticationPluginManager;
    private readonly INopFileProvider _fileProvider;
    private readonly IOrderService _orderService;
    private readonly IPermissionService _permissionService;
    private readonly IPictureService _pictureService;
    private readonly IRoxyFilemanService _roxyFilemanService;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ISettingModelFactory _settingModelFactory;
    private readonly ISettingService _settingService;
    private readonly IStoreContext _storeContext;
    private readonly IStoreService _storeService;
    private readonly IWorkContext _workContext;
    private readonly IUploadService _uploadService;

    #endregion

    #region Ctor

    public SettingApiController(AppSettings appSettings,
        IAddressService addressService,
        ICustomerActivityService customerActivityService,
        ICustomerService customerService,
        INopDataProvider dataProvider,
        IEncryptionService encryptionService,
        IEventPublisher eventPublisher,
        IGenericAttributeService genericAttributeService,
        IGdprService gdprService,
        ILocalizedEntityService localizedEntityService,
        ILocalizationService localizationService,
        IMultiFactorAuthenticationPluginManager multiFactorAuthenticationPluginManager,
        INopFileProvider fileProvider,
        IOrderService orderService,
        IPermissionService permissionService,
        IPictureService pictureService,
        IRoxyFilemanService roxyFilemanService,
        IServiceScopeFactory serviceScopeFactory,
        ISettingModelFactory settingModelFactory,
        ISettingService settingService,
        IStoreContext storeContext,
        IStoreService storeService,
        IWorkContext workContext,
        IUploadService uploadService)
    {
        _appSettings = appSettings;
        _addressService = addressService;
        _customerActivityService = customerActivityService;
        _customerService = customerService;
        _dataProvider = dataProvider;
        _encryptionService = encryptionService;
        _eventPublisher = eventPublisher;
        _genericAttributeService = genericAttributeService;
        _gdprService = gdprService;
        _localizedEntityService = localizedEntityService;
        _localizationService = localizationService;
        _multiFactorAuthenticationPluginManager = multiFactorAuthenticationPluginManager;
        _fileProvider = fileProvider;
        _orderService = orderService;
        _permissionService = permissionService;
        _pictureService = pictureService;
        _roxyFilemanService = roxyFilemanService;
        _serviceScopeFactory = serviceScopeFactory;
        _settingModelFactory = settingModelFactory;
        _settingService = settingService;
        _storeContext = storeContext;
        _storeService = storeService;
        _workContext = workContext;
        _uploadService = uploadService;
    }

    #endregion

    #region Utilities

    protected virtual async Task UpdateGdprConsentLocalesAsync(GdprConsent gdprConsent, GdprConsentModel model)
    {
        foreach (var localized in model.Locales)
        {
            await _localizedEntityService.SaveLocalizedValueAsync(gdprConsent,
                x => x.Message,
                localized.Message,
                localized.LanguageId);

            await _localizedEntityService.SaveLocalizedValueAsync(gdprConsent,
                x => x.RequiredMessage,
                localized.RequiredMessage,
                localized.LanguageId);
        }
    }

    #endregion

    #region Methods

    [HttpGet("{storeId}")]
    public virtual async Task<IActionResult> ChangeStoreScopeConfiguration(int storeId)
    {
        var store = await _storeService.GetStoreByIdAsync(storeId);
        if (store != null || storeId == 0)
        {
            await _genericAttributeService
                .SaveAttributeAsync(await _workContext.GetCurrentCustomerAsync(), NopCustomerDefaults.AdminAreaStoreScopeConfigurationAttribute, storeId);
        }

        return Ok(defaultMessage: true);
    }

    [HttpGet]
    public virtual async Task<IActionResult> GetStoreScopeConfiguration()
    {
        var model = await _settingModelFactory.PrepareStoreScopeConfigurationModelAsync();

        return OkWrap(model);
    }

    public virtual async Task<IActionResult> AppSettings()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _settingModelFactory.PrepareAppSettingsModel();

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> AppSettings([FromBody] BaseQueryModel<AppSettingsModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        var model = queryModel.Data;

        if (ModelState.IsValid)
        {
            var configurations = new List<IConfig>
            {
                model.CacheConfigModel.ToConfig(_appSettings.Get<CacheConfig>()),
                model.HostingConfigModel.ToConfig(_appSettings.Get<HostingConfig>()),
                model.DistributedCacheConfigModel.ToConfig(_appSettings.Get<DistributedCacheConfig>()),
                model.AzureBlobConfigModel.ToConfig(_appSettings.Get<AzureBlobConfig>()),
                model.InstallationConfigModel.ToConfig(_appSettings.Get<InstallationConfig>()),
                model.PluginConfigModel.ToConfig(_appSettings.Get<PluginConfig>()),
                model.CommonConfigModel.ToConfig(_appSettings.Get<CommonConfig>()),
                model.DataConfigModel.ToConfig(_appSettings.Get<DataConfig>()),
                model.WebOptimizerConfigModel.ToConfig(_appSettings.Get<WebOptimizerConfig>())
            };

            await _eventPublisher.PublishAsync(new AppSettingsSavingEvent(configurations));

            AppSettingsHelper.SaveAppSettings(configurations, _fileProvider);

            await _customerActivityService.InsertActivityAsync("EditSettings",
                await _localizationService.GetResourceAsync("ActivityLog.EditSettings"));

            return Ok(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));
        }

        //prepare model
        model = await _settingModelFactory.PrepareAppSettingsModel(model);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    public virtual async Task<IActionResult> Blog()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _settingModelFactory.PrepareBlogSettingsModelAsync();

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Blog([FromBody] BaseQueryModel<BlogSettingsModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        if (ModelState.IsValid)
        {
            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var blogSettings = await _settingService.LoadSettingAsync<BlogSettings>(storeScope);
            blogSettings = model.ToSettings(blogSettings);

            //we do not clear cache after each setting update.
            //this behavior can increase performance because cached settings will not be cleared 
            //and loaded from database after each update
            await _settingService.SaveSettingOverridablePerStoreAsync(blogSettings, x => x.Enabled, model.Enabled_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(blogSettings, x => x.PostsPageSize, model.PostsPageSize_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(blogSettings, x => x.AllowNotRegisteredUsersToLeaveComments, model.AllowNotRegisteredUsersToLeaveComments_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(blogSettings, x => x.NotifyAboutNewBlogComments, model.NotifyAboutNewBlogComments_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(blogSettings, x => x.NumberOfTags, model.NumberOfTags_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(blogSettings, x => x.ShowHeaderRssUrl, model.ShowHeaderRssUrl_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(blogSettings, x => x.BlogCommentsMustBeApproved, model.BlogCommentsMustBeApproved_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingAsync(blogSettings, x => x.ShowBlogCommentsPerStore, clearCache: false);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            //activity log
            await _customerActivityService.InsertActivityAsync("EditSettings", await _localizationService.GetResourceAsync("ActivityLog.EditSettings"));

            return Ok(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));
        }

        //prepare model
        model = await _settingModelFactory.PrepareBlogSettingsModelAsync(model);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    public virtual async Task<IActionResult> Vendor()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _settingModelFactory.PrepareVendorSettingsModelAsync();

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Vendor([FromBody] BaseQueryModel<VendorSettingsModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        if (ModelState.IsValid)
        {
            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var vendorSettings = await _settingService.LoadSettingAsync<VendorSettings>(storeScope);
            vendorSettings = model.ToSettings(vendorSettings);

            //we do not clear cache after each setting update.
            //this behavior can increase performance because cached settings will not be cleared 
            //and loaded from database after each update
            await _settingService.SaveSettingOverridablePerStoreAsync(vendorSettings, x => x.VendorsBlockItemsToDisplay, model.VendorsBlockItemsToDisplay_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(vendorSettings, x => x.ShowVendorOnProductDetailsPage, model.ShowVendorOnProductDetailsPage_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(vendorSettings, x => x.ShowVendorOnOrderDetailsPage, model.ShowVendorOnOrderDetailsPage_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(vendorSettings, x => x.AllowCustomersToContactVendors, model.AllowCustomersToContactVendors_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(vendorSettings, x => x.AllowCustomersToApplyForVendorAccount, model.AllowCustomersToApplyForVendorAccount_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(vendorSettings, x => x.TermsOfServiceEnabled, model.TermsOfServiceEnabled_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(vendorSettings, x => x.AllowSearchByVendor, model.AllowSearchByVendor_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(vendorSettings, x => x.AllowVendorsToEditInfo, model.AllowVendorsToEditInfo_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(vendorSettings, x => x.NotifyStoreOwnerAboutVendorInformationChange, model.NotifyStoreOwnerAboutVendorInformationChange_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(vendorSettings, x => x.MaximumProductNumber, model.MaximumProductNumber_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(vendorSettings, x => x.AllowVendorsToImportProducts, model.AllowVendorsToImportProducts_OverrideForStore, storeScope, false);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            //activity log
            await _customerActivityService.InsertActivityAsync("EditSettings", await _localizationService.GetResourceAsync("ActivityLog.EditSettings"));

            return Ok(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));
        }

        //prepare model
        model = await _settingModelFactory.PrepareVendorSettingsModelAsync(model);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    public virtual async Task<IActionResult> Forum()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _settingModelFactory.PrepareForumSettingsModelAsync();

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Forum([FromBody] BaseQueryModel<ForumSettingsModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        if (ModelState.IsValid)
        {
            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var forumSettings = await _settingService.LoadSettingAsync<ForumSettings>(storeScope);
            forumSettings = model.ToSettings(forumSettings);

            //we do not clear cache after each setting update.
            //this behavior can increase performance because cached settings will not be cleared 
            //and loaded from database after each update
            await _settingService.SaveSettingOverridablePerStoreAsync(forumSettings, x => x.ForumsEnabled, model.ForumsEnabled_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(forumSettings, x => x.RelativeDateTimeFormattingEnabled, model.RelativeDateTimeFormattingEnabled_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(forumSettings, x => x.ShowCustomersPostCount, model.ShowCustomersPostCount_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(forumSettings, x => x.AllowGuestsToCreatePosts, model.AllowGuestsToCreatePosts_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(forumSettings, x => x.AllowGuestsToCreateTopics, model.AllowGuestsToCreateTopics_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(forumSettings, x => x.AllowCustomersToEditPosts, model.AllowCustomersToEditPosts_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(forumSettings, x => x.AllowCustomersToDeletePosts, model.AllowCustomersToDeletePosts_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(forumSettings, x => x.AllowPostVoting, model.AllowPostVoting_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(forumSettings, x => x.MaxVotesPerDay, model.MaxVotesPerDay_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(forumSettings, x => x.AllowCustomersToManageSubscriptions, model.AllowCustomersToManageSubscriptions_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(forumSettings, x => x.TopicsPageSize, model.TopicsPageSize_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(forumSettings, x => x.PostsPageSize, model.PostsPageSize_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(forumSettings, x => x.ForumEditor, model.ForumEditor_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(forumSettings, x => x.SignaturesEnabled, model.SignaturesEnabled_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(forumSettings, x => x.AllowPrivateMessages, model.AllowPrivateMessages_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(forumSettings, x => x.ShowAlertForPM, model.ShowAlertForPM_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(forumSettings, x => x.NotifyAboutPrivateMessages, model.NotifyAboutPrivateMessages_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(forumSettings, x => x.ActiveDiscussionsFeedEnabled, model.ActiveDiscussionsFeedEnabled_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(forumSettings, x => x.ActiveDiscussionsFeedCount, model.ActiveDiscussionsFeedCount_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(forumSettings, x => x.ForumFeedsEnabled, model.ForumFeedsEnabled_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(forumSettings, x => x.ForumFeedCount, model.ForumFeedCount_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(forumSettings, x => x.SearchResultsPageSize, model.SearchResultsPageSize_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(forumSettings, x => x.ActiveDiscussionsPageSize, model.ActiveDiscussionsPageSize_OverrideForStore, storeScope, false);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            //activity log
            await _customerActivityService.InsertActivityAsync("EditSettings", await _localizationService.GetResourceAsync("ActivityLog.EditSettings"));

            return Ok(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));
        }

        //prepare model
        model = await _settingModelFactory.PrepareForumSettingsModelAsync(model);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    public virtual async Task<IActionResult> News()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _settingModelFactory.PrepareNewsSettingsModelAsync();

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> News([FromBody] BaseQueryModel<NewsSettingsModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        if (ModelState.IsValid)
        {
            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var newsSettings = await _settingService.LoadSettingAsync<NewsSettings>(storeScope);
            newsSettings = model.ToSettings(newsSettings);

            //we do not clear cache after each setting update.
            //this behavior can increase performance because cached settings will not be cleared 
            //and loaded from database after each update
            await _settingService.SaveSettingOverridablePerStoreAsync(newsSettings, x => x.Enabled, model.Enabled_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(newsSettings, x => x.AllowNotRegisteredUsersToLeaveComments, model.AllowNotRegisteredUsersToLeaveComments_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(newsSettings, x => x.NotifyAboutNewNewsComments, model.NotifyAboutNewNewsComments_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(newsSettings, x => x.ShowNewsOnMainPage, model.ShowNewsOnMainPage_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(newsSettings, x => x.MainPageNewsCount, model.MainPageNewsCount_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(newsSettings, x => x.NewsArchivePageSize, model.NewsArchivePageSize_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(newsSettings, x => x.ShowHeaderRssUrl, model.ShowHeaderRssUrl_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(newsSettings, x => x.NewsCommentsMustBeApproved, model.NewsCommentsMustBeApproved_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingAsync(newsSettings, x => x.ShowNewsCommentsPerStore, clearCache: false);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            //activity log
            await _customerActivityService.InsertActivityAsync("EditSettings", await _localizationService.GetResourceAsync("ActivityLog.EditSettings"));

            return Ok(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));
        }

        //prepare model
        model = await _settingModelFactory.PrepareNewsSettingsModelAsync(model);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    public virtual async Task<IActionResult> Shipping()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _settingModelFactory.PrepareShippingSettingsModelAsync();

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Shipping([FromBody] BaseQueryModel<ShippingSettingsModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        if (ModelState.IsValid)
        {
            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var shippingSettings = await _settingService.LoadSettingAsync<ShippingSettings>(storeScope);
            shippingSettings = model.ToSettings(shippingSettings);

            //we do not clear cache after each setting update.
            //this behavior can increase performance because cached settings will not be cleared 
            //and loaded from database after each update
            await _settingService.SaveSettingOverridablePerStoreAsync(shippingSettings, x => x.ShipToSameAddress, model.ShipToSameAddress_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(shippingSettings, x => x.AllowPickupInStore, model.AllowPickupInStore_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(shippingSettings, x => x.DisplayPickupPointsOnMap, model.DisplayPickupPointsOnMap_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(shippingSettings, x => x.IgnoreAdditionalShippingChargeForPickupInStore, model.IgnoreAdditionalShippingChargeForPickupInStore_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(shippingSettings, x => x.GoogleMapsApiKey, model.GoogleMapsApiKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(shippingSettings, x => x.UseWarehouseLocation, model.UseWarehouseLocation_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(shippingSettings, x => x.NotifyCustomerAboutShippingFromMultipleLocations, model.NotifyCustomerAboutShippingFromMultipleLocations_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(shippingSettings, x => x.FreeShippingOverXEnabled, model.FreeShippingOverXEnabled_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(shippingSettings, x => x.FreeShippingOverXValue, model.FreeShippingOverXValue_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(shippingSettings, x => x.FreeShippingOverXIncludingTax, model.FreeShippingOverXIncludingTax_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(shippingSettings, x => x.EstimateShippingCartPageEnabled, model.EstimateShippingCartPageEnabled_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(shippingSettings, x => x.EstimateShippingProductPageEnabled, model.EstimateShippingProductPageEnabled_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(shippingSettings, x => x.EstimateShippingCityNameEnabled, model.EstimateShippingCityNameEnabled_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(shippingSettings, x => x.DisplayShipmentEventsToCustomers, model.DisplayShipmentEventsToCustomers_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(shippingSettings, x => x.DisplayShipmentEventsToStoreOwner, model.DisplayShipmentEventsToStoreOwner_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(shippingSettings, x => x.HideShippingTotal, model.HideShippingTotal_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(shippingSettings, x => x.BypassShippingMethodSelectionIfOnlyOne, model.BypassShippingMethodSelectionIfOnlyOne_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(shippingSettings, x => x.ConsiderAssociatedProductsDimensions, model.ConsiderAssociatedProductsDimensions_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(shippingSettings, x => x.ShippingSorting, model.ShippingSorting_OverrideForStore, storeScope, false);

            if (model.ShippingOriginAddress_OverrideForStore || storeScope == 0)
            {
                //update address
                var addressId = await _settingService.SettingExistsAsync(shippingSettings, x => x.ShippingOriginAddressId, storeScope) ?
                    shippingSettings.ShippingOriginAddressId : 0;
                var originAddress = await _addressService.GetAddressByIdAsync(addressId) ??
                    new Address
                    {
                        CreatedOnUtc = DateTime.UtcNow
                    };
                //update ID manually (in case we're in multi-store configuration mode it'll be set to the shared one)
                model.ShippingOriginAddress.Id = addressId;
                originAddress = model.ShippingOriginAddress.ToEntity(originAddress);
                if (originAddress.Id > 0)
                    await _addressService.UpdateAddressAsync(originAddress);
                else
                    await _addressService.InsertAddressAsync(originAddress);
                shippingSettings.ShippingOriginAddressId = originAddress.Id;

                await _settingService.SaveSettingAsync(shippingSettings, x => x.ShippingOriginAddressId, storeScope, false);
            }
            else if (storeScope > 0)
                await _settingService.DeleteSettingAsync(shippingSettings, x => x.ShippingOriginAddressId, storeScope);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            //activity log
            await _customerActivityService.InsertActivityAsync("EditSettings", await _localizationService.GetResourceAsync("ActivityLog.EditSettings"));

            return Ok(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));
        }

        //prepare model
        model = await _settingModelFactory.PrepareShippingSettingsModelAsync(model);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    public virtual async Task<IActionResult> Tax()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _settingModelFactory.PrepareTaxSettingsModelAsync();

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Tax([FromBody] BaseQueryModel<TaxSettingsModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        if (ModelState.IsValid)
        {
            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var taxSettings = await _settingService.LoadSettingAsync<TaxSettings>(storeScope);
            taxSettings = model.ToSettings(taxSettings);

            //we do not clear cache after each setting update.
            //this behavior can increase performance because cached settings will not be cleared 
            //and loaded from database after each update
            await _settingService.SaveSettingOverridablePerStoreAsync(taxSettings, x => x.PricesIncludeTax, model.PricesIncludeTax_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(taxSettings, x => x.AllowCustomersToSelectTaxDisplayType, model.AllowCustomersToSelectTaxDisplayType_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(taxSettings, x => x.TaxDisplayType, model.TaxDisplayType_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(taxSettings, x => x.DisplayTaxSuffix, model.DisplayTaxSuffix_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(taxSettings, x => x.DisplayTaxRates, model.DisplayTaxRates_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(taxSettings, x => x.HideZeroTax, model.HideZeroTax_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(taxSettings, x => x.HideTaxInOrderSummary, model.HideTaxInOrderSummary_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(taxSettings, x => x.ForceTaxExclusionFromOrderSubtotal, model.ForceTaxExclusionFromOrderSubtotal_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(taxSettings, x => x.DefaultTaxCategoryId, model.DefaultTaxCategoryId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(taxSettings, x => x.TaxBasedOn, model.TaxBasedOn_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(taxSettings, x => x.TaxBasedOnPickupPointAddress, model.TaxBasedOnPickupPointAddress_OverrideForStore, storeScope, false);

            if (model.DefaultTaxAddress_OverrideForStore || storeScope == 0)
            {
                //update address
                var addressId = await _settingService.SettingExistsAsync(taxSettings, x => x.DefaultTaxAddressId, storeScope) ?
                    taxSettings.DefaultTaxAddressId : 0;
                var originAddress = await _addressService.GetAddressByIdAsync(addressId) ??
                    new Address
                    {
                        CreatedOnUtc = DateTime.UtcNow
                    };
                //update ID manually (in case we're in multi-store configuration mode it'll be set to the shared one)
                model.DefaultTaxAddress.Id = addressId;
                originAddress = model.DefaultTaxAddress.ToEntity(originAddress);
                if (originAddress.Id > 0)
                    await _addressService.UpdateAddressAsync(originAddress);
                else
                    await _addressService.InsertAddressAsync(originAddress);
                taxSettings.DefaultTaxAddressId = originAddress.Id;

                await _settingService.SaveSettingAsync(taxSettings, x => x.DefaultTaxAddressId, storeScope, false);
            }
            else if (storeScope > 0)
                await _settingService.DeleteSettingAsync(taxSettings, x => x.DefaultTaxAddressId, storeScope);

            await _settingService.SaveSettingOverridablePerStoreAsync(taxSettings, x => x.ShippingIsTaxable, model.ShippingIsTaxable_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(taxSettings, x => x.ShippingPriceIncludesTax, model.ShippingPriceIncludesTax_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(taxSettings, x => x.ShippingTaxClassId, model.ShippingTaxClassId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(taxSettings, x => x.PaymentMethodAdditionalFeeIsTaxable, model.PaymentMethodAdditionalFeeIsTaxable_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(taxSettings, x => x.PaymentMethodAdditionalFeeIncludesTax, model.PaymentMethodAdditionalFeeIncludesTax_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(taxSettings, x => x.PaymentMethodAdditionalFeeTaxClassId, model.PaymentMethodAdditionalFeeTaxClassId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(taxSettings, x => x.EuVatEnabled, model.EuVatEnabled_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(taxSettings, x => x.EuVatEnabledForGuests, model.EuVatEnabledForGuests_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(taxSettings, x => x.EuVatShopCountryId, model.EuVatShopCountryId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(taxSettings, x => x.EuVatAllowVatExemption, model.EuVatAllowVatExemption_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(taxSettings, x => x.EuVatUseWebService, model.EuVatUseWebService_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(taxSettings, x => x.EuVatAssumeValid, model.EuVatAssumeValid_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(taxSettings, x => x.EuVatEmailAdminWhenNewVatSubmitted, model.EuVatEmailAdminWhenNewVatSubmitted_OverrideForStore, storeScope, false);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            //activity log
            await _customerActivityService.InsertActivityAsync("EditSettings", await _localizationService.GetResourceAsync("ActivityLog.EditSettings"));

            return Ok(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));
        }

        //prepare model
        model = await _settingModelFactory.PrepareTaxSettingsModelAsync(model);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    public virtual async Task<IActionResult> Catalog()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _settingModelFactory.PrepareCatalogSettingsModelAsync();

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Catalog([FromBody] BaseQueryModel<CatalogSettingsModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        if (ModelState.IsValid)
        {
            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var catalogSettings = await _settingService.LoadSettingAsync<CatalogSettings>(storeScope);
            catalogSettings = model.ToSettings(catalogSettings);

            //we do not clear cache after each setting update.
            //this behavior can increase performance because cached settings will not be cleared 
            //and loaded from database after each update
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.AllowViewUnpublishedProductPage, model.AllowViewUnpublishedProductPage_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.DisplayDiscontinuedMessageForUnpublishedProducts, model.DisplayDiscontinuedMessageForUnpublishedProducts_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.ShowSkuOnProductDetailsPage, model.ShowSkuOnProductDetailsPage_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.ShowSkuOnCatalogPages, model.ShowSkuOnCatalogPages_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.ShowManufacturerPartNumber, model.ShowManufacturerPartNumber_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.ShowGtin, model.ShowGtin_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.ShowFreeShippingNotification, model.ShowFreeShippingNotification_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.ShowShortDescriptionOnCatalogPages, model.ShowShortDescriptionOnCatalogPages_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.AllowProductSorting, model.AllowProductSorting_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.AllowProductViewModeChanging, model.AllowProductViewModeChanging_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.DefaultViewMode, model.DefaultViewMode_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.ShowProductsFromSubcategories, model.ShowProductsFromSubcategories_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.ShowCategoryProductNumber, model.ShowCategoryProductNumber_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.ShowCategoryProductNumberIncludingSubcategories, model.ShowCategoryProductNumberIncludingSubcategories_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.CategoryBreadcrumbEnabled, model.CategoryBreadcrumbEnabled_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.ShowShareButton, model.ShowShareButton_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.PageShareCode, model.PageShareCode_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.ProductReviewsMustBeApproved, model.ProductReviewsMustBeApproved_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.OneReviewPerProductFromCustomer, model.OneReviewPerProductFromCustomer, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.AllowAnonymousUsersToReviewProduct, model.AllowAnonymousUsersToReviewProduct_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.ProductReviewPossibleOnlyAfterPurchasing, model.ProductReviewPossibleOnlyAfterPurchasing_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.NotifyStoreOwnerAboutNewProductReviews, model.NotifyStoreOwnerAboutNewProductReviews_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.NotifyCustomerAboutProductReviewReply, model.NotifyCustomerAboutProductReviewReply_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.EmailAFriendEnabled, model.EmailAFriendEnabled_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.AllowAnonymousUsersToEmailAFriend, model.AllowAnonymousUsersToEmailAFriend_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.RecentlyViewedProductsNumber, model.RecentlyViewedProductsNumber_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.RecentlyViewedProductsEnabled, model.RecentlyViewedProductsEnabled_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.NewProductsEnabled, model.NewProductsEnabled_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.NewProductsPageSize, model.NewProductsPageSize_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.NewProductsAllowCustomersToSelectPageSize, model.NewProductsAllowCustomersToSelectPageSize_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.NewProductsPageSizeOptions, model.NewProductsPageSizeOptions_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.CompareProductsEnabled, model.CompareProductsEnabled_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.ShowBestsellersOnHomepage, model.ShowBestsellersOnHomepage_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.NumberOfBestsellersOnHomepage, model.NumberOfBestsellersOnHomepage_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.SearchPageProductsPerPage, model.SearchPageProductsPerPage_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.SearchPageAllowCustomersToSelectPageSize, model.SearchPageAllowCustomersToSelectPageSize_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.SearchPagePageSizeOptions, model.SearchPagePageSizeOptions_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.SearchPagePriceRangeFiltering, model.SearchPagePriceRangeFiltering_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.SearchPagePriceFrom, model.SearchPagePriceFrom_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.SearchPagePriceTo, model.SearchPagePriceTo_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.SearchPageManuallyPriceRange, model.SearchPageManuallyPriceRange_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.ProductSearchAutoCompleteEnabled, model.ProductSearchAutoCompleteEnabled_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.ProductSearchEnabled, model.ProductSearchEnabled_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.ProductSearchAutoCompleteNumberOfProducts, model.ProductSearchAutoCompleteNumberOfProducts_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.ShowProductImagesInSearchAutoComplete, model.ShowProductImagesInSearchAutoComplete_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.ShowLinkToAllResultInSearchAutoComplete, model.ShowLinkToAllResultInSearchAutoComplete_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.ProductSearchTermMinimumLength, model.ProductSearchTermMinimumLength_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.ProductsAlsoPurchasedEnabled, model.ProductsAlsoPurchasedEnabled_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.ProductsAlsoPurchasedNumber, model.ProductsAlsoPurchasedNumber_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.NumberOfProductTags, model.NumberOfProductTags_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.ProductsByTagPageSize, model.ProductsByTagPageSize_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.ProductsByTagAllowCustomersToSelectPageSize, model.ProductsByTagAllowCustomersToSelectPageSize_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.ProductsByTagPageSizeOptions, model.ProductsByTagPageSizeOptions_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.ProductsByTagPriceRangeFiltering, model.ProductsByTagPriceRangeFiltering_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.ProductsByTagPriceFrom, model.ProductsByTagPriceFrom_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.ProductsByTagPriceTo, model.ProductsByTagPriceTo_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.ProductsByTagManuallyPriceRange, model.ProductsByTagManuallyPriceRange_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.IncludeShortDescriptionInCompareProducts, model.IncludeShortDescriptionInCompareProducts_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.IncludeFullDescriptionInCompareProducts, model.IncludeFullDescriptionInCompareProducts_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.ManufacturersBlockItemsToDisplay, model.ManufacturersBlockItemsToDisplay_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.DisplayTaxShippingInfoFooter, model.DisplayTaxShippingInfoFooter_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.DisplayTaxShippingInfoProductDetailsPage, model.DisplayTaxShippingInfoProductDetailsPage_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.DisplayTaxShippingInfoProductBoxes, model.DisplayTaxShippingInfoProductBoxes_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.DisplayTaxShippingInfoShoppingCart, model.DisplayTaxShippingInfoShoppingCart_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.DisplayTaxShippingInfoWishlist, model.DisplayTaxShippingInfoWishlist_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.DisplayTaxShippingInfoOrderDetailsPage, model.DisplayTaxShippingInfoOrderDetailsPage_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.ShowProductReviewsPerStore, model.ShowProductReviewsPerStore_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.ShowProductReviewsTabOnAccountPage, model.ShowProductReviewsOnAccountPage_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.ProductReviewsPageSizeOnAccountPage, model.ProductReviewsPageSizeOnAccountPage_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.ProductReviewsSortByCreatedDateAscending, model.ProductReviewsSortByCreatedDateAscending_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.ExportImportProductAttributes, model.ExportImportProductAttributes_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.ExportImportProductSpecificationAttributes, model.ExportImportProductSpecificationAttributes_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.ExportImportProductCategoryBreadcrumb, model.ExportImportProductCategoryBreadcrumb_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.ExportImportCategoriesUsingCategoryName, model.ExportImportCategoriesUsingCategoryName_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.ExportImportAllowDownloadImages, model.ExportImportAllowDownloadImages_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.ExportImportSplitProductsFile, model.ExportImportSplitProductsFile_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.RemoveRequiredProducts, model.RemoveRequiredProducts_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.ExportImportRelatedEntitiesByName, model.ExportImportRelatedEntitiesByName_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.ExportImportProductUseLimitedToStores, model.ExportImportProductUseLimitedToStores_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.DisplayDatePreOrderAvailability, model.DisplayDatePreOrderAvailability_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.UseAjaxCatalogProductsLoading, model.UseAjaxCatalogProductsLoading_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.EnableManufacturerFiltering, model.EnableManufacturerFiltering_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.EnablePriceRangeFiltering, model.EnablePriceRangeFiltering_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.EnableSpecificationAttributeFiltering, model.EnableSpecificationAttributeFiltering_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.DisplayFromPrices, model.DisplayFromPrices_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.AttributeValueOutOfStockDisplayType, model.AttributeValueOutOfStockDisplayType_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.AllowCustomersToSearchWithManufacturerName, model.AllowCustomersToSearchWithManufacturerName_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.AllowCustomersToSearchWithCategoryName, model.AllowCustomersToSearchWithCategoryName_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.DisplayAllPicturesOnCatalogPages, model.DisplayAllPicturesOnCatalogPages_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.ProductUrlStructureTypeId, model.ProductUrlStructureTypeId_OverrideForStore, storeScope, false);

            //now settings not overridable per store
            await _settingService.SaveSettingAsync(catalogSettings, x => x.IgnoreDiscounts, 0, false);
            await _settingService.SaveSettingAsync(catalogSettings, x => x.IgnoreFeaturedProducts, 0, false);
            await _settingService.SaveSettingAsync(catalogSettings, x => x.IgnoreAcl, 0, false);
            await _settingService.SaveSettingAsync(catalogSettings, x => x.IgnoreStoreLimitations, 0, false);
            await _settingService.SaveSettingAsync(catalogSettings, x => x.CacheProductPrices, 0, false);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            //activity log
            await _customerActivityService.InsertActivityAsync("EditSettings", await _localizationService.GetResourceAsync("ActivityLog.EditSettings"));

            return Ok(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));
        }

        //prepare model
        model = await _settingModelFactory.PrepareCatalogSettingsModelAsync(model);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    [HttpPost]
    public virtual async Task<IActionResult> SortOptionsList([FromBody] BaseQueryModel<SortOptionSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _settingModelFactory.PrepareSortOptionListModelAsync(searchModel);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> SortOptionUpdate([FromBody] BaseQueryModel<SortOptionModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        var model = queryModel.Data;

        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var catalogSettings = await _settingService.LoadSettingAsync<CatalogSettings>(storeScope);

        catalogSettings.ProductSortingEnumDisplayOrder[model.Id] = model.DisplayOrder;
        if (model.IsActive && catalogSettings.ProductSortingEnumDisabled.Contains(model.Id))
            catalogSettings.ProductSortingEnumDisabled.Remove(model.Id);
        if (!model.IsActive && !catalogSettings.ProductSortingEnumDisabled.Contains(model.Id))
            catalogSettings.ProductSortingEnumDisabled.Add(model.Id);

        await _settingService.SaveSettingAsync(catalogSettings, x => x.ProductSortingEnumDisplayOrder, storeScope, false);
        await _settingService.SaveSettingAsync(catalogSettings, x => x.ProductSortingEnumDisabled, storeScope, false);

        //now clear settings cache
        await _settingService.ClearCacheAsync();

        return Ok(defaultMessage: true);
    }

    public virtual async Task<IActionResult> RewardPoints()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _settingModelFactory.PrepareRewardPointsSettingsModelAsync();

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> RewardPoints([FromBody] BaseQueryModel<RewardPointsSettingsModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        if (ModelState.IsValid)
        {
            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var rewardPointsSettings = await _settingService.LoadSettingAsync<RewardPointsSettings>(storeScope);
            rewardPointsSettings = model.ToSettings(rewardPointsSettings);

            if (model.ActivatePointsImmediately)
                rewardPointsSettings.ActivationDelay = 0;

            //we do not clear cache after each setting update.
            //this behavior can increase performance because cached settings will not be cleared 
            //and loaded from database after each update
            await _settingService.SaveSettingOverridablePerStoreAsync(rewardPointsSettings, x => x.Enabled, model.Enabled_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(rewardPointsSettings, x => x.ExchangeRate, model.ExchangeRate_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(rewardPointsSettings, x => x.MinimumRewardPointsToUse, model.MinimumRewardPointsToUse_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(rewardPointsSettings, x => x.MaximumRewardPointsToUsePerOrder, model.MaximumRewardPointsToUsePerOrder_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(rewardPointsSettings, x => x.MaximumRedeemedRate, model.MaximumRedeemedRate_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(rewardPointsSettings, x => x.PointsForRegistration, model.PointsForRegistration_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(rewardPointsSettings, x => x.RegistrationPointsValidity, model.RegistrationPointsValidity_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(rewardPointsSettings, x => x.PointsForPurchases_Amount, model.PointsForPurchases_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(rewardPointsSettings, x => x.PointsForPurchases_Points, model.PointsForPurchases_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(rewardPointsSettings, x => x.MinOrderTotalToAwardPoints, model.MinOrderTotalToAwardPoints_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(rewardPointsSettings, x => x.PurchasesPointsValidity, model.PurchasesPointsValidity_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(rewardPointsSettings, x => x.ActivationDelay, model.ActivationDelay_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(rewardPointsSettings, x => x.ActivationDelayPeriodId, model.ActivationDelay_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(rewardPointsSettings, x => x.DisplayHowMuchWillBeEarned, model.DisplayHowMuchWillBeEarned_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(rewardPointsSettings, x => x.PageSize, model.PageSize_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingAsync(rewardPointsSettings, x => x.PointsAccumulatedForAllStores, 0, false);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            //activity log
            await _customerActivityService.InsertActivityAsync("EditSettings", await _localizationService.GetResourceAsync("ActivityLog.EditSettings"));

            return Ok(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));
        }

        //prepare model
        model = await _settingModelFactory.PrepareRewardPointsSettingsModelAsync(model);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    public virtual async Task<IActionResult> Order()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _settingModelFactory.PrepareOrderSettingsModelAsync();

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Order([FromBody] BaseQueryModel<OrderSettingsModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        if (ModelState.IsValid)
        {
            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var orderSettings = await _settingService.LoadSettingAsync<OrderSettings>(storeScope);
            orderSettings = model.ToSettings(orderSettings);

            //we do not clear cache after each setting update.
            //this behavior can increase performance because cached settings will not be cleared 
            //and loaded from database after each update
            await _settingService.SaveSettingOverridablePerStoreAsync(orderSettings, x => x.IsReOrderAllowed, model.IsReOrderAllowed_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(orderSettings, x => x.MinOrderSubtotalAmount, model.MinOrderSubtotalAmount_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(orderSettings, x => x.MinOrderSubtotalAmountIncludingTax, model.MinOrderSubtotalAmountIncludingTax_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(orderSettings, x => x.MinOrderTotalAmount, model.MinOrderTotalAmount_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(orderSettings, x => x.AutoUpdateOrderTotalsOnEditingOrder, model.AutoUpdateOrderTotalsOnEditingOrder_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(orderSettings, x => x.AnonymousCheckoutAllowed, model.AnonymousCheckoutAllowed_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(orderSettings, x => x.CheckoutDisabled, model.CheckoutDisabled_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(orderSettings, x => x.TermsOfServiceOnShoppingCartPage, model.TermsOfServiceOnShoppingCartPage_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(orderSettings, x => x.TermsOfServiceOnOrderConfirmPage, model.TermsOfServiceOnOrderConfirmPage_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(orderSettings, x => x.OnePageCheckoutEnabled, model.OnePageCheckoutEnabled_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(orderSettings, x => x.OnePageCheckoutDisplayOrderTotalsOnPaymentInfoTab, model.OnePageCheckoutDisplayOrderTotalsOnPaymentInfoTab_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(orderSettings, x => x.DisableBillingAddressCheckoutStep, model.DisableBillingAddressCheckoutStep_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(orderSettings, x => x.DisableOrderCompletedPage, model.DisableOrderCompletedPage_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(orderSettings, x => x.DisplayPickupInStoreOnShippingMethodPage, model.DisplayPickupInStoreOnShippingMethodPage_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(orderSettings, x => x.AttachPdfInvoiceToOrderPlacedEmail, model.AttachPdfInvoiceToOrderPlacedEmail_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(orderSettings, x => x.AttachPdfInvoiceToOrderPaidEmail, model.AttachPdfInvoiceToOrderPaidEmail_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(orderSettings, x => x.AttachPdfInvoiceToOrderProcessingEmail, model.AttachPdfInvoiceToOrderProcessingEmail_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(orderSettings, x => x.AttachPdfInvoiceToOrderCompletedEmail, model.AttachPdfInvoiceToOrderCompletedEmail_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(orderSettings, x => x.ReturnRequestsEnabled, model.ReturnRequestsEnabled_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(orderSettings, x => x.ReturnRequestsAllowFiles, model.ReturnRequestsAllowFiles_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(orderSettings, x => x.ReturnRequestNumberMask, model.ReturnRequestNumberMask_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(orderSettings, x => x.NumberOfDaysReturnRequestAvailable, model.NumberOfDaysReturnRequestAvailable_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(orderSettings, x => x.CustomOrderNumberMask, model.CustomOrderNumberMask_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(orderSettings, x => x.ExportWithProducts, model.ExportWithProducts_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(orderSettings, x => x.AllowAdminsToBuyCallForPriceProducts, model.AllowAdminsToBuyCallForPriceProducts_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(orderSettings, x => x.ShowProductThumbnailInOrderDetailsPage, model.ShowProductThumbnailInOrderDetailsPage_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(orderSettings, x => x.DeleteGiftCardUsageHistory, model.DeleteGiftCardUsageHistory_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingAsync(orderSettings, x => x.ActivateGiftCardsAfterCompletingOrder, 0, false);
            await _settingService.SaveSettingAsync(orderSettings, x => x.DeactivateGiftCardsAfterCancellingOrder, 0, false);
            await _settingService.SaveSettingAsync(orderSettings, x => x.DeactivateGiftCardsAfterDeletingOrder, 0, false);
            await _settingService.SaveSettingAsync(orderSettings, x => x.CompleteOrderWhenDelivered, 0, false);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            //order ident
            if (model.OrderIdent.HasValue)
            {
                try
                {
                    await _dataProvider.SetTableIdentAsync<Order>(model.OrderIdent.Value);
                }
                catch (Exception exc)
                {
                    return BadRequest(exc.Message);
                }
            }

            //activity log
            await _customerActivityService.InsertActivityAsync("EditSettings", await _localizationService.GetResourceAsync("ActivityLog.EditSettings"));

            return Ok(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));
        }

        //prepare model
        model = await _settingModelFactory.PrepareOrderSettingsModelAsync(model);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    public virtual async Task<IActionResult> ShoppingCart()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _settingModelFactory.PrepareShoppingCartSettingsModelAsync();

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ShoppingCart([FromBody] BaseQueryModel<ShoppingCartSettingsModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        if (ModelState.IsValid)
        {
            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var shoppingCartSettings = await _settingService.LoadSettingAsync<ShoppingCartSettings>(storeScope);
            shoppingCartSettings = model.ToSettings(shoppingCartSettings);

            //we do not clear cache after each setting update.
            //this behavior can increase performance because cached settings will not be cleared 
            //and loaded from database after each update
            await _settingService.SaveSettingOverridablePerStoreAsync(shoppingCartSettings, x => x.DisplayCartAfterAddingProduct, model.DisplayCartAfterAddingProduct_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(shoppingCartSettings, x => x.DisplayWishlistAfterAddingProduct, model.DisplayWishlistAfterAddingProduct_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(shoppingCartSettings, x => x.MaximumShoppingCartItems, model.MaximumShoppingCartItems_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(shoppingCartSettings, x => x.MaximumWishlistItems, model.MaximumWishlistItems_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(shoppingCartSettings, x => x.AllowOutOfStockItemsToBeAddedToWishlist, model.AllowOutOfStockItemsToBeAddedToWishlist_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(shoppingCartSettings, x => x.MoveItemsFromWishlistToCart, model.MoveItemsFromWishlistToCart_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(shoppingCartSettings, x => x.CartsSharedBetweenStores, model.CartsSharedBetweenStores_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(shoppingCartSettings, x => x.ShowProductImagesOnShoppingCart, model.ShowProductImagesOnShoppingCart_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(shoppingCartSettings, x => x.ShowProductImagesOnWishList, model.ShowProductImagesOnWishList_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(shoppingCartSettings, x => x.ShowDiscountBox, model.ShowDiscountBox_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(shoppingCartSettings, x => x.ShowGiftCardBox, model.ShowGiftCardBox_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(shoppingCartSettings, x => x.CrossSellsNumber, model.CrossSellsNumber_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(shoppingCartSettings, x => x.EmailWishlistEnabled, model.EmailWishlistEnabled_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(shoppingCartSettings, x => x.AllowAnonymousUsersToEmailWishlist, model.AllowAnonymousUsersToEmailWishlist_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(shoppingCartSettings, x => x.MiniShoppingCartEnabled, model.MiniShoppingCartEnabled_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(shoppingCartSettings, x => x.ShowProductImagesInMiniShoppingCart, model.ShowProductImagesInMiniShoppingCart_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(shoppingCartSettings, x => x.MiniShoppingCartProductNumber, model.MiniShoppingCartProductNumber_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(shoppingCartSettings, x => x.AllowCartItemEditing, model.AllowCartItemEditing_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(shoppingCartSettings, x => x.GroupTierPricesForDistinctShoppingCartItems, model.GroupTierPricesForDistinctShoppingCartItems_OverrideForStore, storeScope, false);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            //activity log
            await _customerActivityService.InsertActivityAsync("EditSettings", await _localizationService.GetResourceAsync("ActivityLog.EditSettings"));

            return Ok(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));
        }

        //prepare model
        model = await _settingModelFactory.PrepareShoppingCartSettingsModelAsync(model);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    public virtual async Task<IActionResult> Media()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _settingModelFactory.PrepareMediaSettingsModelAsync();

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Media([FromBody] BaseQueryModel<MediaSettingsModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        if (ModelState.IsValid)
        {
            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var mediaSettings = await _settingService.LoadSettingAsync<MediaSettings>(storeScope);
            mediaSettings = model.ToSettings(mediaSettings);

            //we do not clear cache after each setting update.
            //this behavior can increase performance because cached settings will not be cleared 
            //and loaded from database after each update
            await _settingService.SaveSettingOverridablePerStoreAsync(mediaSettings, x => x.AvatarPictureSize, model.AvatarPictureSize_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(mediaSettings, x => x.ProductThumbPictureSize, model.ProductThumbPictureSize_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(mediaSettings, x => x.ProductDetailsPictureSize, model.ProductDetailsPictureSize_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(mediaSettings, x => x.ProductThumbPictureSizeOnProductDetailsPage, model.ProductThumbPictureSizeOnProductDetailsPage_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(mediaSettings, x => x.AssociatedProductPictureSize, model.AssociatedProductPictureSize_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(mediaSettings, x => x.CategoryThumbPictureSize, model.CategoryThumbPictureSize_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(mediaSettings, x => x.ManufacturerThumbPictureSize, model.ManufacturerThumbPictureSize_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(mediaSettings, x => x.VendorThumbPictureSize, model.VendorThumbPictureSize_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(mediaSettings, x => x.CartThumbPictureSize, model.CartThumbPictureSize_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(mediaSettings, x => x.OrderThumbPictureSize, model.OrderThumbPictureSize_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(mediaSettings, x => x.MiniCartThumbPictureSize, model.MiniCartThumbPictureSize_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(mediaSettings, x => x.MaximumImageSize, model.MaximumImageSize_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(mediaSettings, x => x.MultipleThumbDirectories, model.MultipleThumbDirectories_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(mediaSettings, x => x.DefaultImageQuality, model.DefaultImageQuality_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(mediaSettings, x => x.ImportProductImagesUsingHash, model.ImportProductImagesUsingHash_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(mediaSettings, x => x.DefaultPictureZoomEnabled, model.DefaultPictureZoomEnabled_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(mediaSettings, x => x.AllowSVGUploads, model.AllowSVGUploads_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(mediaSettings, x => x.ProductDefaultImageId, model.ProductDefaultImageId_OverrideForStore, storeScope, false);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            //activity log
            await _customerActivityService.InsertActivityAsync("EditSettings", await _localizationService.GetResourceAsync("ActivityLog.EditSettings"));

            return Ok(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));
        }

        //prepare model
        model = await _settingModelFactory.PrepareMediaSettingsModelAsync(model);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ChangePictureStorage()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        await _pictureService.SetIsStoreInDbAsync(!await _pictureService.IsStoreInDbAsync());

        //activity log
        await _customerActivityService.InsertActivityAsync("EditSettings", await _localizationService.GetResourceAsync("ActivityLog.EditSettings"));

        return Ok(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));
    }

    public virtual async Task<IActionResult> CustomerUser()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _settingModelFactory.PrepareCustomerUserSettingsModelAsync();

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> CustomerUser([FromBody] BaseQueryModel<CustomerUserSettingsModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        if (ModelState.IsValid)
        {
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var customerSettings = await _settingService.LoadSettingAsync<CustomerSettings>(storeScope);

            var lastUsernameValidationRule = customerSettings.UsernameValidationRule;
            var lastUsernameValidationEnabledValue = customerSettings.UsernameValidationEnabled;
            var lastUsernameValidationUseRegexValue = customerSettings.UsernameValidationUseRegex;

            //Phone number validation settings
            var lastPhoneNumberValidationRule = customerSettings.PhoneNumberValidationRule;
            var lastPhoneNumberValidationEnabledValue = customerSettings.PhoneNumberValidationEnabled;
            var lastPhoneNumberValidationUseRegexValue = customerSettings.PhoneNumberValidationUseRegex;

            var addressSettings = await _settingService.LoadSettingAsync<AddressSettings>(storeScope);
            var dateTimeSettings = await _settingService.LoadSettingAsync<DateTimeSettings>(storeScope);
            var externalAuthenticationSettings = await _settingService.LoadSettingAsync<ExternalAuthenticationSettings>(storeScope);
            var multiFactorAuthenticationSettings = await _settingService.LoadSettingAsync<MultiFactorAuthenticationSettings>(storeScope);

            customerSettings = model.CustomerSettings.ToSettings(customerSettings);

            if (customerSettings.UsernameValidationEnabled && customerSettings.UsernameValidationUseRegex)
            {
                try
                {
                    //validate regex rule
                    var unused = Regex.IsMatch("test_user_name", customerSettings.UsernameValidationRule);
                }
                catch (ArgumentException)
                {
                    //restoring previous settings
                    customerSettings.UsernameValidationRule = lastUsernameValidationRule;
                    customerSettings.UsernameValidationEnabled = lastUsernameValidationEnabledValue;
                    customerSettings.UsernameValidationUseRegex = lastUsernameValidationUseRegexValue;

                    return BadRequest(await _localizationService.GetResourceAsync("Admin.Configuration.Settings.CustomerSettings.RegexValidationRule.Error"));
                }
            }

            if (customerSettings.PhoneNumberValidationEnabled && customerSettings.PhoneNumberValidationUseRegex)
            {
                try
                {
                    //validate regex rule
                    var unused = Regex.IsMatch("123456789", customerSettings.PhoneNumberValidationRule);
                }
                catch (ArgumentException)
                {
                    //restoring previous settings
                    customerSettings.PhoneNumberValidationRule = lastPhoneNumberValidationRule;
                    customerSettings.PhoneNumberValidationEnabled = lastPhoneNumberValidationEnabledValue;
                    customerSettings.PhoneNumberValidationUseRegex = lastPhoneNumberValidationUseRegexValue;

                    return BadRequest(await _localizationService.GetResourceAsync("Admin.Configuration.Settings.CustomerSettings.PhoneNumberRegexValidationRule.Error"));
                }
            }

            await _settingService.SaveSettingAsync(customerSettings);

            addressSettings = model.AddressSettings.ToSettings(addressSettings);
            await _settingService.SaveSettingAsync(addressSettings);

            dateTimeSettings.DefaultStoreTimeZoneId = model.DateTimeSettings.DefaultStoreTimeZoneId;
            dateTimeSettings.AllowCustomersToSetTimeZone = model.DateTimeSettings.AllowCustomersToSetTimeZone;
            await _settingService.SaveSettingAsync(dateTimeSettings);

            externalAuthenticationSettings.AllowCustomersToRemoveAssociations = model.ExternalAuthenticationSettings.AllowCustomersToRemoveAssociations;
            await _settingService.SaveSettingAsync(externalAuthenticationSettings);

            multiFactorAuthenticationSettings = model.MultiFactorAuthenticationSettings.ToSettings(multiFactorAuthenticationSettings);
            await _settingService.SaveSettingAsync(multiFactorAuthenticationSettings);

            //activity log
            await _customerActivityService.InsertActivityAsync("EditSettings", await _localizationService.GetResourceAsync("ActivityLog.EditSettings"));

            return Ok(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));
        }

        //prepare model
        model = await _settingModelFactory.PrepareCustomerUserSettingsModelAsync(model);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    #region GDPR

    public virtual async Task<IActionResult> Gdpr()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _settingModelFactory.PrepareGdprSettingsModelAsync();

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Gdpr([FromBody] BaseQueryModel<GdprSettingsModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        if (ModelState.IsValid)
        {
            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var gdprSettings = await _settingService.LoadSettingAsync<GdprSettings>(storeScope);
            gdprSettings = model.ToSettings(gdprSettings);

            //we do not clear cache after each setting update.
            //this behavior can increase performance because cached settings will not be cleared 
            //and loaded from database after each update
            await _settingService.SaveSettingOverridablePerStoreAsync(gdprSettings, x => x.GdprEnabled, model.GdprEnabled_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(gdprSettings, x => x.LogPrivacyPolicyConsent, model.LogPrivacyPolicyConsent_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(gdprSettings, x => x.LogNewsletterConsent, model.LogNewsletterConsent_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(gdprSettings, x => x.LogUserProfileChanges, model.LogUserProfileChanges_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(gdprSettings, x => x.DeleteInactiveCustomersAfterMonths, model.DeleteInactiveCustomersAfterMonths_OverrideForStore, storeScope, false);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            //activity log
            await _customerActivityService.InsertActivityAsync("EditSettings", await _localizationService.GetResourceAsync("ActivityLog.EditSettings"));

            return Ok(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));
        }

        //prepare model
        model = await _settingModelFactory.PrepareGdprSettingsModelAsync(model);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    [HttpPost]
    public virtual async Task<IActionResult> GdprConsentList([FromBody] BaseQueryModel<GdprConsentSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _settingModelFactory.PrepareGdprConsentListModelAsync(searchModel);

        return OkWrap(model);
    }

    public virtual async Task<IActionResult> CreateGdprConsent()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _settingModelFactory.PrepareGdprConsentModelAsync(new GdprConsentModel(), null);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> CreateGdprConsent([FromBody] BaseQueryModel<GdprConsentModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        if (ModelState.IsValid)
        {
            var gdprConsent = model.ToEntity<GdprConsent>();
            await _gdprService.InsertConsentAsync(gdprConsent);

            //locales                
            await UpdateGdprConsentLocalesAsync(gdprConsent, model);

            return Ok(await _localizationService.GetResourceAsync("Admin.Configuration.Settings.Gdpr.Consent.Added"));
        }

        //prepare model
        model = await _settingModelFactory.PrepareGdprConsentModelAsync(model, null, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    [HttpGet("{id}")]
    public virtual async Task<IActionResult> EditGdprConsent(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        //try to get a consent with the specified id
        var gdprConsent = await _gdprService.GetConsentByIdAsync(id);
        if (gdprConsent == null)
            return NotFound("No gdpr consent found with the specified id");

        //prepare model
        var model = await _settingModelFactory.PrepareGdprConsentModelAsync(null, gdprConsent);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> EditGdprConsent([FromBody] BaseQueryModel<GdprConsentModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get a GDPR consent with the specified id
        var gdprConsent = await _gdprService.GetConsentByIdAsync(model.Id);
        if (gdprConsent == null)
            return NotFound("No gdpr consent found with the specified id");

        if (ModelState.IsValid)
        {
            gdprConsent = model.ToEntity(gdprConsent);
            await _gdprService.UpdateConsentAsync(gdprConsent);

            //locales                
            await UpdateGdprConsentLocalesAsync(gdprConsent, model);

            return Ok(await _localizationService.GetResourceAsync("Admin.Configuration.Settings.Gdpr.Consent.Updated"));
        }

        //prepare model
        model = await _settingModelFactory.PrepareGdprConsentModelAsync(model, gdprConsent, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> DeleteGdprConsent(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        //try to get a GDPR consent with the specified id
        var gdprConsent = await _gdprService.GetConsentByIdAsync(id);
        if (gdprConsent == null)
            return NotFound("No gdpr consent found with the specified id");

        await _gdprService.DeleteConsentAsync(gdprConsent);

        return Ok(await _localizationService.GetResourceAsync("Admin.Configuration.Settings.Gdpr.Consent.Deleted"));
    }

    #endregion

    public virtual async Task<IActionResult> GeneralCommon(bool showtour = false)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _settingModelFactory.PrepareGeneralCommonSettingsModelAsync();

        //show configuration tour
        if (showtour)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var hideCard = await _genericAttributeService.GetAttributeAsync<bool>(customer, NopCustomerDefaults.HideConfigurationStepsAttribute);
            var closeCard = await _genericAttributeService.GetAttributeAsync<bool>(customer, NopCustomerDefaults.CloseConfigurationStepsAttribute);

            if (!hideCard && !closeCard)
                ViewBag.ShowTour = true;
        }
        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> GeneralCommon([FromBody] BaseQueryModel<GeneralCommonSettingsModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        if (ModelState.IsValid)
        {
            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();

            //store information settings
            var storeInformationSettings = await _settingService.LoadSettingAsync<StoreInformationSettings>(storeScope);
            var commonSettings = await _settingService.LoadSettingAsync<CommonSettings>(storeScope);
            var sitemapSettings = await _settingService.LoadSettingAsync<SitemapSettings>(storeScope);

            storeInformationSettings.StoreClosed = model.StoreInformationSettings.StoreClosed;
            storeInformationSettings.DefaultStoreTheme = model.StoreInformationSettings.DefaultStoreTheme;
            storeInformationSettings.AllowCustomerToSelectTheme = model.StoreInformationSettings.AllowCustomerToSelectTheme;
            storeInformationSettings.LogoPictureId = model.StoreInformationSettings.LogoPictureId;
            //EU Cookie law
            storeInformationSettings.DisplayEuCookieLawWarning = model.StoreInformationSettings.DisplayEuCookieLawWarning;
            //social pages
            storeInformationSettings.FacebookLink = model.StoreInformationSettings.FacebookLink;
            storeInformationSettings.TwitterLink = model.StoreInformationSettings.TwitterLink;
            storeInformationSettings.YoutubeLink = model.StoreInformationSettings.YoutubeLink;
            storeInformationSettings.InstagramLink = model.StoreInformationSettings.InstagramLink;
            //contact us
            commonSettings.SubjectFieldOnContactUsForm = model.StoreInformationSettings.SubjectFieldOnContactUsForm;
            commonSettings.UseSystemEmailForContactUsForm = model.StoreInformationSettings.UseSystemEmailForContactUsForm;
            //terms of service
            commonSettings.PopupForTermsOfServiceLinks = model.StoreInformationSettings.PopupForTermsOfServiceLinks;
            //sitemap
            sitemapSettings.SitemapEnabled = model.SitemapSettings.SitemapEnabled;
            sitemapSettings.SitemapPageSize = model.SitemapSettings.SitemapPageSize;
            sitemapSettings.SitemapIncludeCategories = model.SitemapSettings.SitemapIncludeCategories;
            sitemapSettings.SitemapIncludeManufacturers = model.SitemapSettings.SitemapIncludeManufacturers;
            sitemapSettings.SitemapIncludeProducts = model.SitemapSettings.SitemapIncludeProducts;
            sitemapSettings.SitemapIncludeProductTags = model.SitemapSettings.SitemapIncludeProductTags;
            sitemapSettings.SitemapIncludeBlogPosts = model.SitemapSettings.SitemapIncludeBlogPosts;
            sitemapSettings.SitemapIncludeNews = model.SitemapSettings.SitemapIncludeNews;
            sitemapSettings.SitemapIncludeTopics = model.SitemapSettings.SitemapIncludeTopics;

            //minification
            commonSettings.EnableHtmlMinification = model.MinificationSettings.EnableHtmlMinification;
            //use response compression
            commonSettings.UseResponseCompression = model.MinificationSettings.UseResponseCompression;
            //custom header and footer HTML
            commonSettings.HeaderCustomHtml = model.CustomHtmlSettings.HeaderCustomHtml;
            commonSettings.FooterCustomHtml = model.CustomHtmlSettings.FooterCustomHtml;

            //we do not clear cache after each setting update.
            //this behavior can increase performance because cached settings will not be cleared 
            //and loaded from database after each update
            await _settingService.SaveSettingOverridablePerStoreAsync(storeInformationSettings, x => x.StoreClosed, model.StoreInformationSettings.StoreClosed_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(storeInformationSettings, x => x.DefaultStoreTheme, model.StoreInformationSettings.DefaultStoreTheme_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(storeInformationSettings, x => x.AllowCustomerToSelectTheme, model.StoreInformationSettings.AllowCustomerToSelectTheme_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(storeInformationSettings, x => x.LogoPictureId, model.StoreInformationSettings.LogoPictureId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(storeInformationSettings, x => x.DisplayEuCookieLawWarning, model.StoreInformationSettings.DisplayEuCookieLawWarning_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(storeInformationSettings, x => x.FacebookLink, model.StoreInformationSettings.FacebookLink_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(storeInformationSettings, x => x.TwitterLink, model.StoreInformationSettings.TwitterLink_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(storeInformationSettings, x => x.YoutubeLink, model.StoreInformationSettings.YoutubeLink_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(storeInformationSettings, x => x.InstagramLink, model.StoreInformationSettings.InstagramLink_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(commonSettings, x => x.SubjectFieldOnContactUsForm, model.StoreInformationSettings.SubjectFieldOnContactUsForm_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(commonSettings, x => x.UseSystemEmailForContactUsForm, model.StoreInformationSettings.UseSystemEmailForContactUsForm_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(commonSettings, x => x.PopupForTermsOfServiceLinks, model.StoreInformationSettings.PopupForTermsOfServiceLinks_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(sitemapSettings, x => x.SitemapEnabled, model.SitemapSettings.SitemapEnabled_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(sitemapSettings, x => x.SitemapPageSize, model.SitemapSettings.SitemapPageSize_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(sitemapSettings, x => x.SitemapIncludeCategories, model.SitemapSettings.SitemapIncludeCategories_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(sitemapSettings, x => x.SitemapIncludeManufacturers, model.SitemapSettings.SitemapIncludeManufacturers_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(sitemapSettings, x => x.SitemapIncludeProducts, model.SitemapSettings.SitemapIncludeProducts_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(sitemapSettings, x => x.SitemapIncludeProductTags, model.SitemapSettings.SitemapIncludeProductTags_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(sitemapSettings, x => x.SitemapIncludeBlogPosts, model.SitemapSettings.SitemapIncludeBlogPosts_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(sitemapSettings, x => x.SitemapIncludeNews, model.SitemapSettings.SitemapIncludeNews_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(sitemapSettings, x => x.SitemapIncludeTopics, model.SitemapSettings.SitemapIncludeTopics_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(commonSettings, x => x.EnableHtmlMinification, model.MinificationSettings.EnableHtmlMinification_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(commonSettings, x => x.UseResponseCompression, model.MinificationSettings.UseResponseCompression_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(commonSettings, x => x.HeaderCustomHtml, model.CustomHtmlSettings.HeaderCustomHtml_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(commonSettings, x => x.FooterCustomHtml, model.CustomHtmlSettings.FooterCustomHtml_OverrideForStore, storeScope, false);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            //seo settings
            var seoSettings = await _settingService.LoadSettingAsync<SeoSettings>(storeScope);
            seoSettings.PageTitleSeparator = model.SeoSettings.PageTitleSeparator;
            seoSettings.PageTitleSeoAdjustment = (PageTitleSeoAdjustment)model.SeoSettings.PageTitleSeoAdjustment;
            seoSettings.GenerateProductMetaDescription = model.SeoSettings.GenerateProductMetaDescription;
            seoSettings.ConvertNonWesternChars = model.SeoSettings.ConvertNonWesternChars;
            seoSettings.CanonicalUrlsEnabled = model.SeoSettings.CanonicalUrlsEnabled;
            seoSettings.WwwRequirement = (WwwRequirement)model.SeoSettings.WwwRequirement;
            seoSettings.TwitterMetaTags = model.SeoSettings.TwitterMetaTags;
            seoSettings.OpenGraphMetaTags = model.SeoSettings.OpenGraphMetaTags;
            seoSettings.MicrodataEnabled = model.SeoSettings.MicrodataEnabled;
            seoSettings.CustomHeadTags = model.SeoSettings.CustomHeadTags;

            //we do not clear cache after each setting update.
            //this behavior can increase performance because cached settings will not be cleared 
            //and loaded from database after each update
            await _settingService.SaveSettingOverridablePerStoreAsync(seoSettings, x => x.PageTitleSeparator, model.SeoSettings.PageTitleSeparator_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(seoSettings, x => x.PageTitleSeoAdjustment, model.SeoSettings.PageTitleSeoAdjustment_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(seoSettings, x => x.GenerateProductMetaDescription, model.SeoSettings.GenerateProductMetaDescription_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(seoSettings, x => x.ConvertNonWesternChars, model.SeoSettings.ConvertNonWesternChars_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(seoSettings, x => x.CanonicalUrlsEnabled, model.SeoSettings.CanonicalUrlsEnabled_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(seoSettings, x => x.WwwRequirement, model.SeoSettings.WwwRequirement_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(seoSettings, x => x.TwitterMetaTags, model.SeoSettings.TwitterMetaTags_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(seoSettings, x => x.OpenGraphMetaTags, model.SeoSettings.OpenGraphMetaTags_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(seoSettings, x => x.CustomHeadTags, model.SeoSettings.CustomHeadTags_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(seoSettings, x => x.MicrodataEnabled, model.SeoSettings.MicrodataEnabled_OverrideForStore, storeScope, false);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            //security settings
            var securitySettings = await _settingService.LoadSettingAsync<SecuritySettings>(storeScope);
            if (securitySettings.AdminAreaAllowedIpAddresses == null)
                securitySettings.AdminAreaAllowedIpAddresses = new List<string>();
            securitySettings.AdminAreaAllowedIpAddresses.Clear();
            if (!string.IsNullOrEmpty(model.SecuritySettings.AdminAreaAllowedIpAddresses))
                foreach (var s in model.SecuritySettings.AdminAreaAllowedIpAddresses.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                    if (!string.IsNullOrWhiteSpace(s))
                        securitySettings.AdminAreaAllowedIpAddresses.Add(s.Trim());
            securitySettings.HoneypotEnabled = model.SecuritySettings.HoneypotEnabled;
            await _settingService.SaveSettingAsync(securitySettings);

            //robots.txt settings
            var robotsTxtSettings = await _settingService.LoadSettingAsync<RobotsTxtSettings>(storeScope);
            robotsTxtSettings.AllowSitemapXml = model.RobotsTxtSettings.AllowSitemapXml;
            robotsTxtSettings.AdditionsRules = model.RobotsTxtSettings.AdditionsRules?.Split(Environment.NewLine).ToList();
            robotsTxtSettings.DisallowLanguages = model.RobotsTxtSettings.DisallowLanguages?.ToList() ?? new List<int>();
            robotsTxtSettings.DisallowPaths = model.RobotsTxtSettings.DisallowPaths?.Split(Environment.NewLine).ToList();
            robotsTxtSettings.LocalizableDisallowPaths = model.RobotsTxtSettings.LocalizableDisallowPaths?.Split(Environment.NewLine).ToList();

            //we do not clear cache after each setting update.
            //this behavior can increase performance because cached settings will not be cleared 
            //and loaded from database after each update
            await _settingService.SaveSettingOverridablePerStoreAsync(robotsTxtSettings, x => x.AllowSitemapXml, model.RobotsTxtSettings.AllowSitemapXml_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(robotsTxtSettings, x => x.AdditionsRules, model.RobotsTxtSettings.AdditionsRules_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(robotsTxtSettings, x => x.DisallowLanguages, model.RobotsTxtSettings.DisallowLanguages_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(robotsTxtSettings, x => x.DisallowPaths, model.RobotsTxtSettings.DisallowPaths_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(robotsTxtSettings, x => x.LocalizableDisallowPaths, model.RobotsTxtSettings.LocalizableDisallowPaths_OverrideForStore, storeScope, false);

            // now clear settings cache
            await _settingService.ClearCacheAsync();

            //captcha settings
            var captchaSettings = await _settingService.LoadSettingAsync<CaptchaSettings>(storeScope);
            captchaSettings.Enabled = model.CaptchaSettings.Enabled;
            captchaSettings.ShowOnLoginPage = model.CaptchaSettings.ShowOnLoginPage;
            captchaSettings.ShowOnRegistrationPage = model.CaptchaSettings.ShowOnRegistrationPage;
            captchaSettings.ShowOnContactUsPage = model.CaptchaSettings.ShowOnContactUsPage;
            captchaSettings.ShowOnEmailWishlistToFriendPage = model.CaptchaSettings.ShowOnEmailWishlistToFriendPage;
            captchaSettings.ShowOnEmailProductToFriendPage = model.CaptchaSettings.ShowOnEmailProductToFriendPage;
            captchaSettings.ShowOnBlogCommentPage = model.CaptchaSettings.ShowOnBlogCommentPage;
            captchaSettings.ShowOnNewsCommentPage = model.CaptchaSettings.ShowOnNewsCommentPage;
            captchaSettings.ShowOnProductReviewPage = model.CaptchaSettings.ShowOnProductReviewPage;
            captchaSettings.ShowOnForgotPasswordPage = model.CaptchaSettings.ShowOnForgotPasswordPage;
            captchaSettings.ShowOnApplyVendorPage = model.CaptchaSettings.ShowOnApplyVendorPage;
            captchaSettings.ShowOnForum = model.CaptchaSettings.ShowOnForum;
            captchaSettings.ShowOnCheckoutPageForGuests = model.CaptchaSettings.ShowOnCheckoutPageForGuests;
            captchaSettings.ReCaptchaPublicKey = model.CaptchaSettings.ReCaptchaPublicKey;
            captchaSettings.ReCaptchaPrivateKey = model.CaptchaSettings.ReCaptchaPrivateKey;
            captchaSettings.CaptchaType = (CaptchaType)model.CaptchaSettings.CaptchaType;
            captchaSettings.ReCaptchaV3ScoreThreshold = model.CaptchaSettings.ReCaptchaV3ScoreThreshold;

            //we do not clear cache after each setting update.
            //this behavior can increase performance because cached settings will not be cleared 
            //and loaded from database after each update
            await _settingService.SaveSettingOverridablePerStoreAsync(captchaSettings, x => x.Enabled, model.CaptchaSettings.Enabled_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(captchaSettings, x => x.ShowOnLoginPage, model.CaptchaSettings.ShowOnLoginPage_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(captchaSettings, x => x.ShowOnRegistrationPage, model.CaptchaSettings.ShowOnRegistrationPage_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(captchaSettings, x => x.ShowOnContactUsPage, model.CaptchaSettings.ShowOnContactUsPage_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(captchaSettings, x => x.ShowOnEmailWishlistToFriendPage, model.CaptchaSettings.ShowOnEmailWishlistToFriendPage_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(captchaSettings, x => x.ShowOnEmailProductToFriendPage, model.CaptchaSettings.ShowOnEmailProductToFriendPage_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(captchaSettings, x => x.ShowOnBlogCommentPage, model.CaptchaSettings.ShowOnBlogCommentPage_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(captchaSettings, x => x.ShowOnNewsCommentPage, model.CaptchaSettings.ShowOnNewsCommentPage_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(captchaSettings, x => x.ShowOnProductReviewPage, model.CaptchaSettings.ShowOnProductReviewPage_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(captchaSettings, x => x.ShowOnApplyVendorPage, model.CaptchaSettings.ShowOnApplyVendorPage_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(captchaSettings, x => x.ShowOnForgotPasswordPage, model.CaptchaSettings.ShowOnForgotPasswordPage_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(captchaSettings, x => x.ShowOnForum, model.CaptchaSettings.ShowOnForum_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(captchaSettings, x => x.ShowOnCheckoutPageForGuests, model.CaptchaSettings.ShowOnCheckoutPageForGuests_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(captchaSettings, x => x.ReCaptchaPublicKey, model.CaptchaSettings.ReCaptchaPublicKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(captchaSettings, x => x.ReCaptchaPrivateKey, model.CaptchaSettings.ReCaptchaPrivateKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(captchaSettings, x => x.ReCaptchaV3ScoreThreshold, model.CaptchaSettings.ReCaptchaV3ScoreThreshold_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(captchaSettings, x => x.CaptchaType, model.CaptchaSettings.CaptchaType_OverrideForStore, storeScope, false);

            // now clear settings cache
            await _settingService.ClearCacheAsync();

            if (captchaSettings.Enabled &&
                (string.IsNullOrWhiteSpace(captchaSettings.ReCaptchaPublicKey) || string.IsNullOrWhiteSpace(captchaSettings.ReCaptchaPrivateKey)))
            {
                //captcha is enabled but the keys are not entered
                return BadRequest(await _localizationService.GetResourceAsync("Admin.Configuration.Settings.GeneralCommon.CaptchaAppropriateKeysNotEnteredError"));
            }

            //PDF settings
            var pdfSettings = await _settingService.LoadSettingAsync<PdfSettings>(storeScope);
            pdfSettings.LetterPageSizeEnabled = model.PdfSettings.LetterPageSizeEnabled;
            pdfSettings.LogoPictureId = model.PdfSettings.LogoPictureId;
            pdfSettings.DisablePdfInvoicesForPendingOrders = model.PdfSettings.DisablePdfInvoicesForPendingOrders;
            pdfSettings.InvoiceFooterTextColumn1 = model.PdfSettings.InvoiceFooterTextColumn1;
            pdfSettings.InvoiceFooterTextColumn2 = model.PdfSettings.InvoiceFooterTextColumn2;

            //we do not clear cache after each setting update.
            //this behavior can increase performance because cached settings will not be cleared 
            //and loaded from database after each update
            await _settingService.SaveSettingOverridablePerStoreAsync(pdfSettings, x => x.LetterPageSizeEnabled, model.PdfSettings.LetterPageSizeEnabled_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(pdfSettings, x => x.LogoPictureId, model.PdfSettings.LogoPictureId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(pdfSettings, x => x.DisablePdfInvoicesForPendingOrders, model.PdfSettings.DisablePdfInvoicesForPendingOrders_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(pdfSettings, x => x.InvoiceFooterTextColumn1, model.PdfSettings.InvoiceFooterTextColumn1_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(pdfSettings, x => x.InvoiceFooterTextColumn2, model.PdfSettings.InvoiceFooterTextColumn2_OverrideForStore, storeScope, false);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            //localization settings
            var localizationSettings = await _settingService.LoadSettingAsync<LocalizationSettings>(storeScope);
            localizationSettings.UseImagesForLanguageSelection = model.LocalizationSettings.UseImagesForLanguageSelection;
            if (localizationSettings.SeoFriendlyUrlsForLanguagesEnabled != model.LocalizationSettings.SeoFriendlyUrlsForLanguagesEnabled)
            {
                localizationSettings.SeoFriendlyUrlsForLanguagesEnabled = model.LocalizationSettings.SeoFriendlyUrlsForLanguagesEnabled;
            }

            localizationSettings.AutomaticallyDetectLanguage = model.LocalizationSettings.AutomaticallyDetectLanguage;
            localizationSettings.LoadAllLocaleRecordsOnStartup = model.LocalizationSettings.LoadAllLocaleRecordsOnStartup;
            localizationSettings.LoadAllLocalizedPropertiesOnStartup = model.LocalizationSettings.LoadAllLocalizedPropertiesOnStartup;
            localizationSettings.LoadAllUrlRecordsOnStartup = model.LocalizationSettings.LoadAllUrlRecordsOnStartup;
            await _settingService.SaveSettingAsync(localizationSettings);

            //display default menu item
            var displayDefaultMenuItemSettings = await _settingService.LoadSettingAsync<DisplayDefaultMenuItemSettings>(storeScope);

            //we do not clear cache after each setting update.
            //this behavior can increase performance because cached settings will not be cleared 
            //and loaded from database after each update
            displayDefaultMenuItemSettings.DisplayHomepageMenuItem = model.DisplayDefaultMenuItemSettings.DisplayHomepageMenuItem;
            displayDefaultMenuItemSettings.DisplayNewProductsMenuItem = model.DisplayDefaultMenuItemSettings.DisplayNewProductsMenuItem;
            displayDefaultMenuItemSettings.DisplayProductSearchMenuItem = model.DisplayDefaultMenuItemSettings.DisplayProductSearchMenuItem;
            displayDefaultMenuItemSettings.DisplayCustomerInfoMenuItem = model.DisplayDefaultMenuItemSettings.DisplayCustomerInfoMenuItem;
            displayDefaultMenuItemSettings.DisplayBlogMenuItem = model.DisplayDefaultMenuItemSettings.DisplayBlogMenuItem;
            displayDefaultMenuItemSettings.DisplayForumsMenuItem = model.DisplayDefaultMenuItemSettings.DisplayForumsMenuItem;
            displayDefaultMenuItemSettings.DisplayContactUsMenuItem = model.DisplayDefaultMenuItemSettings.DisplayContactUsMenuItem;

            await _settingService.SaveSettingOverridablePerStoreAsync(displayDefaultMenuItemSettings, x => x.DisplayHomepageMenuItem, model.DisplayDefaultMenuItemSettings.DisplayHomepageMenuItem_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(displayDefaultMenuItemSettings, x => x.DisplayNewProductsMenuItem, model.DisplayDefaultMenuItemSettings.DisplayNewProductsMenuItem_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(displayDefaultMenuItemSettings, x => x.DisplayProductSearchMenuItem, model.DisplayDefaultMenuItemSettings.DisplayProductSearchMenuItem_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(displayDefaultMenuItemSettings, x => x.DisplayCustomerInfoMenuItem, model.DisplayDefaultMenuItemSettings.DisplayCustomerInfoMenuItem_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(displayDefaultMenuItemSettings, x => x.DisplayBlogMenuItem, model.DisplayDefaultMenuItemSettings.DisplayBlogMenuItem_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(displayDefaultMenuItemSettings, x => x.DisplayForumsMenuItem, model.DisplayDefaultMenuItemSettings.DisplayForumsMenuItem_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(displayDefaultMenuItemSettings, x => x.DisplayContactUsMenuItem, model.DisplayDefaultMenuItemSettings.DisplayContactUsMenuItem_OverrideForStore, storeScope, false);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            //display default footer item
            var displayDefaultFooterItemSettings = await _settingService.LoadSettingAsync<DisplayDefaultFooterItemSettings>(storeScope);

            //we do not clear cache after each setting update.
            //this behavior can increase performance because cached settings will not be cleared 
            //and loaded from database after each update
            displayDefaultFooterItemSettings.DisplaySitemapFooterItem = model.DisplayDefaultFooterItemSettings.DisplaySitemapFooterItem;
            displayDefaultFooterItemSettings.DisplayContactUsFooterItem = model.DisplayDefaultFooterItemSettings.DisplayContactUsFooterItem;
            displayDefaultFooterItemSettings.DisplayProductSearchFooterItem = model.DisplayDefaultFooterItemSettings.DisplayProductSearchFooterItem;
            displayDefaultFooterItemSettings.DisplayNewsFooterItem = model.DisplayDefaultFooterItemSettings.DisplayNewsFooterItem;
            displayDefaultFooterItemSettings.DisplayBlogFooterItem = model.DisplayDefaultFooterItemSettings.DisplayBlogFooterItem;
            displayDefaultFooterItemSettings.DisplayForumsFooterItem = model.DisplayDefaultFooterItemSettings.DisplayForumsFooterItem;
            displayDefaultFooterItemSettings.DisplayRecentlyViewedProductsFooterItem = model.DisplayDefaultFooterItemSettings.DisplayRecentlyViewedProductsFooterItem;
            displayDefaultFooterItemSettings.DisplayCompareProductsFooterItem = model.DisplayDefaultFooterItemSettings.DisplayCompareProductsFooterItem;
            displayDefaultFooterItemSettings.DisplayNewProductsFooterItem = model.DisplayDefaultFooterItemSettings.DisplayNewProductsFooterItem;
            displayDefaultFooterItemSettings.DisplayCustomerInfoFooterItem = model.DisplayDefaultFooterItemSettings.DisplayCustomerInfoFooterItem;
            displayDefaultFooterItemSettings.DisplayCustomerOrdersFooterItem = model.DisplayDefaultFooterItemSettings.DisplayCustomerOrdersFooterItem;
            displayDefaultFooterItemSettings.DisplayCustomerAddressesFooterItem = model.DisplayDefaultFooterItemSettings.DisplayCustomerAddressesFooterItem;
            displayDefaultFooterItemSettings.DisplayShoppingCartFooterItem = model.DisplayDefaultFooterItemSettings.DisplayShoppingCartFooterItem;
            displayDefaultFooterItemSettings.DisplayWishlistFooterItem = model.DisplayDefaultFooterItemSettings.DisplayWishlistFooterItem;
            displayDefaultFooterItemSettings.DisplayApplyVendorAccountFooterItem = model.DisplayDefaultFooterItemSettings.DisplayApplyVendorAccountFooterItem;

            await _settingService.SaveSettingOverridablePerStoreAsync(displayDefaultFooterItemSettings, x => x.DisplaySitemapFooterItem, model.DisplayDefaultFooterItemSettings.DisplaySitemapFooterItem_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(displayDefaultFooterItemSettings, x => x.DisplayContactUsFooterItem, model.DisplayDefaultFooterItemSettings.DisplayContactUsFooterItem_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(displayDefaultFooterItemSettings, x => x.DisplayProductSearchFooterItem, model.DisplayDefaultFooterItemSettings.DisplayProductSearchFooterItem_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(displayDefaultFooterItemSettings, x => x.DisplayNewsFooterItem, model.DisplayDefaultFooterItemSettings.DisplayNewsFooterItem_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(displayDefaultFooterItemSettings, x => x.DisplayBlogFooterItem, model.DisplayDefaultFooterItemSettings.DisplayBlogFooterItem_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(displayDefaultFooterItemSettings, x => x.DisplayForumsFooterItem, model.DisplayDefaultFooterItemSettings.DisplayForumsFooterItem_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(displayDefaultFooterItemSettings, x => x.DisplayRecentlyViewedProductsFooterItem, model.DisplayDefaultFooterItemSettings.DisplayRecentlyViewedProductsFooterItem_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(displayDefaultFooterItemSettings, x => x.DisplayCompareProductsFooterItem, model.DisplayDefaultFooterItemSettings.DisplayCompareProductsFooterItem_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(displayDefaultFooterItemSettings, x => x.DisplayNewProductsFooterItem, model.DisplayDefaultFooterItemSettings.DisplayNewProductsFooterItem_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(displayDefaultFooterItemSettings, x => x.DisplayCustomerInfoFooterItem, model.DisplayDefaultFooterItemSettings.DisplayCustomerInfoFooterItem_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(displayDefaultFooterItemSettings, x => x.DisplayCustomerOrdersFooterItem, model.DisplayDefaultFooterItemSettings.DisplayCustomerOrdersFooterItem_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(displayDefaultFooterItemSettings, x => x.DisplayCustomerAddressesFooterItem, model.DisplayDefaultFooterItemSettings.DisplayCustomerAddressesFooterItem_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(displayDefaultFooterItemSettings, x => x.DisplayShoppingCartFooterItem, model.DisplayDefaultFooterItemSettings.DisplayShoppingCartFooterItem_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(displayDefaultFooterItemSettings, x => x.DisplayWishlistFooterItem, model.DisplayDefaultFooterItemSettings.DisplayWishlistFooterItem_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(displayDefaultFooterItemSettings, x => x.DisplayApplyVendorAccountFooterItem, model.DisplayDefaultFooterItemSettings.DisplayApplyVendorAccountFooterItem_OverrideForStore, storeScope, false);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            //admin area
            var adminAreaSettings = await _settingService.LoadSettingAsync<AdminAreaSettings>(storeScope);

            //we do not clear cache after each setting update.
            //this behavior can increase performance because cached settings will not be cleared 
            //and loaded from database after each update
            adminAreaSettings.UseRichEditorInMessageTemplates = model.AdminAreaSettings.UseRichEditorInMessageTemplates;

            await _settingService.SaveSettingOverridablePerStoreAsync(adminAreaSettings, x => x.UseRichEditorInMessageTemplates, model.AdminAreaSettings.UseRichEditorInMessageTemplates_OverrideForStore, storeScope, false);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            //activity log
            await _customerActivityService.InsertActivityAsync("EditSettings", await _localizationService.GetResourceAsync("ActivityLog.EditSettings"));

            return Ok(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));
        }

        //prepare model
        model = await _settingModelFactory.PrepareGeneralCommonSettingsModelAsync(model);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ChangeEncryptionKey([FromBody] BaseQueryModel<GeneralCommonSettingsModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var securitySettings = await _settingService.LoadSettingAsync<SecuritySettings>(storeScope);

        var model = queryModel.Data;
        try
        {
            if (model.SecuritySettings.EncryptionKey == null)
                model.SecuritySettings.EncryptionKey = string.Empty;

            model.SecuritySettings.EncryptionKey = model.SecuritySettings.EncryptionKey.Trim();

            var newEncryptionPrivateKey = model.SecuritySettings.EncryptionKey;
            if (string.IsNullOrEmpty(newEncryptionPrivateKey) || newEncryptionPrivateKey.Length != 16)
                return BadRequest(await _localizationService.GetResourceAsync("Admin.Configuration.Settings.GeneralCommon.EncryptionKey.TooShort"));

            var oldEncryptionPrivateKey = securitySettings.EncryptionKey;
            if (oldEncryptionPrivateKey == newEncryptionPrivateKey)
                return BadRequest(await _localizationService.GetResourceAsync("Admin.Configuration.Settings.GeneralCommon.EncryptionKey.TheSame"));

            //update encrypted order info
            var orders = await _orderService.SearchOrdersAsync();
            foreach (var order in orders)
            {
                var decryptedCardType = _encryptionService.DecryptText(order.CardType, oldEncryptionPrivateKey);
                var decryptedCardName = _encryptionService.DecryptText(order.CardName, oldEncryptionPrivateKey);
                var decryptedCardNumber = _encryptionService.DecryptText(order.CardNumber, oldEncryptionPrivateKey);
                var decryptedMaskedCreditCardNumber = _encryptionService.DecryptText(order.MaskedCreditCardNumber, oldEncryptionPrivateKey);
                var decryptedCardCvv2 = _encryptionService.DecryptText(order.CardCvv2, oldEncryptionPrivateKey);
                var decryptedCardExpirationMonth = _encryptionService.DecryptText(order.CardExpirationMonth, oldEncryptionPrivateKey);
                var decryptedCardExpirationYear = _encryptionService.DecryptText(order.CardExpirationYear, oldEncryptionPrivateKey);

                var encryptedCardType = _encryptionService.EncryptText(decryptedCardType, newEncryptionPrivateKey);
                var encryptedCardName = _encryptionService.EncryptText(decryptedCardName, newEncryptionPrivateKey);
                var encryptedCardNumber = _encryptionService.EncryptText(decryptedCardNumber, newEncryptionPrivateKey);
                var encryptedMaskedCreditCardNumber = _encryptionService.EncryptText(decryptedMaskedCreditCardNumber, newEncryptionPrivateKey);
                var encryptedCardCvv2 = _encryptionService.EncryptText(decryptedCardCvv2, newEncryptionPrivateKey);
                var encryptedCardExpirationMonth = _encryptionService.EncryptText(decryptedCardExpirationMonth, newEncryptionPrivateKey);
                var encryptedCardExpirationYear = _encryptionService.EncryptText(decryptedCardExpirationYear, newEncryptionPrivateKey);

                order.CardType = encryptedCardType;
                order.CardName = encryptedCardName;
                order.CardNumber = encryptedCardNumber;
                order.MaskedCreditCardNumber = encryptedMaskedCreditCardNumber;
                order.CardCvv2 = encryptedCardCvv2;
                order.CardExpirationMonth = encryptedCardExpirationMonth;
                order.CardExpirationYear = encryptedCardExpirationYear;
                await _orderService.UpdateOrderAsync(order);
            }

            //update password information
            //optimization - load only passwords with PasswordFormat.Encrypted
            var customerPasswords = await _customerService.GetCustomerPasswordsAsync(passwordFormat: PasswordFormat.Encrypted);
            foreach (var customerPassword in customerPasswords)
            {
                var decryptedPassword = _encryptionService.DecryptText(customerPassword.Password, oldEncryptionPrivateKey);
                var encryptedPassword = _encryptionService.EncryptText(decryptedPassword, newEncryptionPrivateKey);

                customerPassword.Password = encryptedPassword;
                await _customerService.UpdateCustomerPasswordAsync(customerPassword);
            }

            securitySettings.EncryptionKey = newEncryptionPrivateKey;
            await _settingService.SaveSettingAsync(securitySettings);

            return Ok(await _localizationService.GetResourceAsync("Admin.Configuration.Settings.GeneralCommon.EncryptionKey.Changed"));
        }
        catch (Exception exc)
        {
            return BadRequest(exc.Message);
        }
    }

    [HttpPost]
    public virtual async Task<IActionResult> UploadLocalePattern()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        try
        {
            await _uploadService.UploadLocalePatternAsync();
            return Ok(await _localizationService.GetResourceAsync("Admin.Configuration.Settings.GeneralCommon.LocalePattern.SuccessUpload"));
        }
        catch (Exception exc)
        {
            return BadRequest(exc.Message);
        }
    }

    [HttpPost]
    public virtual async Task<IActionResult> UploadIcons()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        var iconsFile = await Request.GetFirstOrDefaultFileAsync();

        try
        {
            if (iconsFile == null || iconsFile.Length == 0)
            {
                return BadRequest(await _localizationService.GetResourceAsync("Admin.Common.UploadFile"));
            }

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var commonSettings = await _settingService.LoadSettingAsync<CommonSettings>(storeScope);

            switch (_fileProvider.GetFileExtension(iconsFile.FileName))
            {
                case ".ico":
                    await _uploadService.UploadFaviconAsync(iconsFile);
                    commonSettings.FaviconAndAppIconsHeadCode = string.Format(NopCommonDefaults.SingleFaviconHeadLink, storeScope, iconsFile.FileName);

                    break;

                case ".zip":
                    await _uploadService.UploadIconsArchiveAsync(iconsFile);

                    var headCodePath = _fileProvider.GetAbsolutePath(string.Format(NopCommonDefaults.FaviconAndAppIconsPath, storeScope), NopCommonDefaults.HeadCodeFileName);
                    if (!_fileProvider.FileExists(headCodePath))
                        return BadRequest(string.Format(await _localizationService.GetResourceAsync("Admin.Configuration.Settings.GeneralCommon.FaviconAndAppIcons.MissingFile"), NopCommonDefaults.HeadCodeFileName));

                    using (var sr = new StreamReader(headCodePath))
                        commonSettings.FaviconAndAppIconsHeadCode = await sr.ReadToEndAsync();

                    break;

                default:
                    return BadRequest("File is not supported.");
            }

            await _settingService.SaveSettingOverridablePerStoreAsync(commonSettings, x => x.FaviconAndAppIconsHeadCode, true, storeScope);

            //delete old favicon icon if exist
            var oldFaviconIconPath = _fileProvider.GetAbsolutePath(string.Format(NopCommonDefaults.OldFaviconIconName, storeScope));
            if (_fileProvider.FileExists(oldFaviconIconPath))
            {
                _fileProvider.DeleteFile(oldFaviconIconPath);
            }

            //activity log
            await _customerActivityService.InsertActivityAsync("UploadIcons", string.Format(await _localizationService.GetResourceAsync("ActivityLog.UploadNewIcons"), storeScope));
            return Ok(await _localizationService.GetResourceAsync("Admin.Configuration.FaviconAndAppIcons.Uploaded"));
        }
        catch (Exception exc)
        {
            return BadRequest(exc.Message);
        }
    }

    [HttpGet("{settingName}")]
    public virtual async Task<IActionResult> AllSettings(string settingName)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _settingModelFactory.PrepareSettingSearchModelAsync(new SettingSearchModel { SearchSettingName = WebUtility.HtmlEncode(settingName) });

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> AllSettings([FromBody] BaseQueryModel<SettingSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _settingModelFactory.PrepareSettingListModelAsync(searchModel);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> SettingUpdate([FromBody] BaseQueryModel<SettingModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        if (model.Name != null)
            model.Name = model.Name.Trim();

        if (model.Value != null)
            model.Value = model.Value.Trim();

        if (!ModelState.IsValid)
            return BadRequest(ModelState.SerializeErrors());

        //try to get a setting with the specified id
        var setting = await _settingService.GetSettingByIdAsync(model.Id);
        if (setting == null)
            return NotFound("No setting found with the specified id");

        if (!setting.Name.Equals(model.Name, StringComparison.InvariantCultureIgnoreCase))
        {
            //setting name has been changed
            await _settingService.DeleteSettingAsync(setting);
        }

        await _settingService.SetSettingAsync(model.Name, model.Value, setting.StoreId);

        //activity log
        await _customerActivityService.InsertActivityAsync("EditSettings", await _localizationService.GetResourceAsync("ActivityLog.EditSettings"), setting);

        return Ok(defaultMessage: true);
    }

    [HttpPost]
    public virtual async Task<IActionResult> SettingAdd([FromBody] BaseQueryModel<SettingModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        if (model.Name != null)
            model.Name = model.Name.Trim();

        if (model.Value != null)
            model.Value = model.Value.Trim();

        if (!ModelState.IsValid)
            return BadRequest(ModelState.SerializeErrors());

        var storeId = model.StoreId;
        await _settingService.SetSettingAsync(model.Name, model.Value, storeId);

        //activity log
        await _customerActivityService.InsertActivityAsync("AddNewSetting",
            string.Format(await _localizationService.GetResourceAsync("ActivityLog.AddNewSetting"), model.Name),
            await _settingService.GetSettingAsync(model.Name, storeId));

        return Ok(defaultMessage: true);
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> SettingDelete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
            return AdminApiAccessDenied();

        //try to get a setting with the specified id
        var setting = await _settingService.GetSettingByIdAsync(id);
        if (setting == null)
            return NotFound("No setting found with the specified id");

        await _settingService.DeleteSettingAsync(setting);

        //activity log
        await _customerActivityService.InsertActivityAsync("DeleteSetting",
            string.Format(await _localizationService.GetResourceAsync("ActivityLog.DeleteSetting"), setting.Name), setting);

        return Ok(defaultMessage: true);
    }

    [HttpGet("{loadAllLocaleRecordsOnStartup}")]
    //action displaying notification (warning) to a store owner about a lot of traffic 
    //between the distributed cache server and the application when LoadAllLocaleRecordsOnStartup setting is set
    public async Task<IActionResult> DistributedCacheHighTrafficWarning(bool loadAllLocaleRecordsOnStartup)
    {
        //LoadAllLocaleRecordsOnStartup is set and distributed cache is used, so display warning
        if (_appSettings.Get<DistributedCacheConfig>().Enabled && _appSettings.Get<DistributedCacheConfig>().DistributedCacheType != DistributedCacheType.Memory && loadAllLocaleRecordsOnStartup)
        {
            return Ok(string.Format(await _localizationService.GetResourceAsync("Admin.Configuration.Settings.GeneralCommon.LoadAllLocaleRecordsOnStartup.Warning")));
        }

        return Ok(defaultMessage: true);
    }

    [HttpGet("{forceMultifactorAuthentication}")]
    //Action that displays a notification (warning) to the store owner about the absence of active authentication providers
    public async Task<IActionResult> ForceMultifactorAuthenticationWarning(bool forceMultifactorAuthentication)
    {
        //ForceMultifactorAuthentication is set and the store haven't active Authentication provider , so display warning
        if (forceMultifactorAuthentication && !await _multiFactorAuthenticationPluginManager.HasActivePluginsAsync())
        {
            return Ok(string.Format(await _localizationService.GetResourceAsync("Admin.Configuration.Settings.CustomerUser.ForceMultifactorAuthentication.Warning")));
        }

        return Ok(defaultMessage: true);
    }

    //Action that displays a notification (warning) to the store owner about the need to restart the application after changing the setting
    public async Task<IActionResult> SeoFriendlyUrlsForLanguagesEnabledWarning(bool seoFriendlyUrlsForLanguagesEnabled)
    {
        //load settings for a chosen store scope
        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var localizationSettings = await _settingService.LoadSettingAsync<LocalizationSettings>(storeScope);

        if (seoFriendlyUrlsForLanguagesEnabled != localizationSettings.SeoFriendlyUrlsForLanguagesEnabled)
        {
            return Ok(string.Format(await _localizationService
                    .GetResourceAsync("Admin.Configuration.Settings.GeneralCommon.SeoFriendlyUrlsForLanguagesEnabled.Warning")));
        }

        return Ok(defaultMessage: true);
    }

    #endregion
}