using System;
using System.Text;
using System.Threading.Tasks;
using Amazon.Personalize.Model;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using NopStation.Plugin.Misc.AmazonPersonalize.Areas.Admin.Factories;
using NopStation.Plugin.Misc.AmazonPersonalize.Areas.Admin.Models;
using NopStation.Plugin.Misc.AmazonPersonalize.Factories;
using NopStation.Plugin.Misc.AmazonPersonalize.Services;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;

namespace NopStation.Plugin.Misc.AmazonPersonalize.Areas.Admin.Controllers
{
    public class AmazonPersonalizeController : NopStationAdminController
    {
        #region Fields

        private readonly IPermissionService _permissionService;
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly IAmazonPersonalizeModelFactory _amazonPersonalizeModelFactory;
        private readonly IEventReportService _eventReportService;
        private readonly IAmazonPersonalizeHelperFactory _amazonPersonalizeHelperFactory;
        private readonly IRecommenderModelFactory _recommenderModelFactory;
        private readonly ILogger _logger;
        private readonly IPersonalizeExportManager _personalizeExportManager;
        private readonly IDateTimeHelper _dateTimeHelper;

        #endregion Fields

        #region Ctor

        public AmazonPersonalizeController(IPermissionService permissionService,
            IStoreContext storeContext,
            ISettingService settingService,
            INotificationService notificationService,
            ILocalizationService localizationService,
            IAmazonPersonalizeModelFactory amazonPersonalizeModelFactory,
            IEventReportService eventReportService,
            IAmazonPersonalizeHelperFactory amazonPersonalizeHelperFactory,
            IRecommenderModelFactory recommenderModelFactory,
            ILogger logger,
            IPersonalizeExportManager personalizeExportManager,
            IDateTimeHelper dateTimeHelper)
        {
            _permissionService = permissionService;
            _storeContext = storeContext;
            _settingService = settingService;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _amazonPersonalizeModelFactory = amazonPersonalizeModelFactory;
            _eventReportService = eventReportService;
            _amazonPersonalizeHelperFactory = amazonPersonalizeHelperFactory;
            _recommenderModelFactory = recommenderModelFactory;
            _logger = logger;
            _personalizeExportManager = personalizeExportManager;
            _dateTimeHelper = dateTimeHelper;
        }

        #endregion Ctor

        #region Methods

        #region Utilities

        public async Task<IActionResult> Redirect()
        {
            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.NopStation.AmazonPersonalize.Interactions.Info"));
            return RedirectToAction("UploadInteractionsData");
        }

        #endregion Utilities

        #region Config

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(AmazonPersonalizePermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var model = await _amazonPersonalizeModelFactory.PrepareConfigurationModelAsync();

            return View(model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(AmazonPersonalizePermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var amazonPersonalizeSettings = await _settingService.LoadSettingAsync<AmazonPersonalizeSettings>(storeScope);

            model.FrequentlyBoughtTogetherWidgetZoneId = AmazonPersonalizeDefaults.ProductDetailsBottomWidgetZoneId;
            model.CustomersWhoViewedXAlsoViewedWidgetZoneId = AmazonPersonalizeDefaults.ProductDetailsBottomWidgetZoneId;

            amazonPersonalizeSettings = model.ToSettings(amazonPersonalizeSettings);

            #region Common

            await _settingService.SaveSettingOverridablePerStoreAsync(amazonPersonalizeSettings, x => x.EnableAmazonPersonalize, model.EnableAmazonPersonalize_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(amazonPersonalizeSettings, x => x.AccessKey, model.AccessKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(amazonPersonalizeSettings, x => x.SecretKey, model.SecretKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(amazonPersonalizeSettings, x => x.AwsRegionId, model.AwsRegionId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(amazonPersonalizeSettings, x => x.EventTrackerId, model.EventTrackerId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(amazonPersonalizeSettings, x => x.DataSetGroupArn, model.DataSetGroupArn_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(amazonPersonalizeSettings, x => x.EnableLogging, model.EnableLogging_OverrideForStore, storeScope, false);

            #endregion Common

            #region RecommendedForYou

            await _settingService.SaveSettingOverridablePerStoreAsync(amazonPersonalizeSettings, x => x.EnableRecommendedForYou, model.EnableRecommendedForYou_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(amazonPersonalizeSettings, x => x.RecommendedForYouARN, model.RecommendedForYouARN_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(amazonPersonalizeSettings, x => x.RecommendedForYouWidgetZoneId, model.RecommendedForYouWidgetZoneId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(amazonPersonalizeSettings, x => x.RecommendedForYouNumberOfItems, model.RecommendedForYouNumberOfItems_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(amazonPersonalizeSettings, x => x.RecommendedForYouAllowForGuestCustomer, model.RecommendedForYouAllowForGuestCustomer_OverrideForStore, storeScope, false);

            #endregion RecommendedForYou

            #region MostViewed

            await _settingService.SaveSettingOverridablePerStoreAsync(amazonPersonalizeSettings, x => x.EnableMostViewed, model.EnableMostViewed_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(amazonPersonalizeSettings, x => x.MostViewedARN, model.MostViewedARN_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(amazonPersonalizeSettings, x => x.MostViewedWidgetZoneId, model.MostViewedWidgetZoneId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(amazonPersonalizeSettings, x => x.MostViewedNumberOfItems, model.MostViewedNumberOfItems_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(amazonPersonalizeSettings, x => x.MostViewedAllowForGuestCustomer, model.MostViewedAllowForGuestCustomer_OverrideForStore, storeScope, false);

            #endregion MostViewed

            #region CustomersWhoViewedXAlsoViewed

            await _settingService.SaveSettingOverridablePerStoreAsync(amazonPersonalizeSettings, x => x.EnableCustomersWhoViewedXAlsoViewed, model.EnableCustomersWhoViewedXAlsoViewed_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(amazonPersonalizeSettings, x => x.CustomersWhoViewedXAlsoViewedARN, model.CustomersWhoViewedXAlsoViewedARN_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(amazonPersonalizeSettings, x => x.CustomersWhoViewedXAlsoViewedWidgetZoneId, model.CustomersWhoViewedXAlsoViewedWidgetZoneId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(amazonPersonalizeSettings, x => x.CustomersWhoViewedXAlsoViewedNumberOfItems, model.CustomersWhoViewedXAlsoViewedNumberOfItems_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(amazonPersonalizeSettings, x => x.CustomersWhoViewedXAlsoViewedAllowForGuestCustomer, model.CustomersWhoViewedXAlsoViewedAllowForGuestCustomer_OverrideForStore, storeScope, false);

            #endregion CustomersWhoViewedXAlsoViewed

            #region BestSellers

            await _settingService.SaveSettingOverridablePerStoreAsync(amazonPersonalizeSettings, x => x.EnableBestSellers, model.EnableBestSellers_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(amazonPersonalizeSettings, x => x.BestSellersARN, model.BestSellersARN_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(amazonPersonalizeSettings, x => x.BestSellersWidgetZoneId, model.BestSellersWidgetZoneId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(amazonPersonalizeSettings, x => x.BestSellersNumberOfItems, model.BestSellersNumberOfItems_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(amazonPersonalizeSettings, x => x.BestSellersAllowForGuestCustomer, model.BestSellersAllowForGuestCustomer_OverrideForStore, storeScope, false);

            #endregion BestSellers

            #region FrequentlyBoughtTogether

            await _settingService.SaveSettingOverridablePerStoreAsync(amazonPersonalizeSettings, x => x.EnableFrequentlyBoughtTogether, model.EnableFrequentlyBoughtTogether_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(amazonPersonalizeSettings, x => x.FrequentlyBoughtTogetherARN, model.FrequentlyBoughtTogetherARN_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(amazonPersonalizeSettings, x => x.FrequentlyBoughtTogetherWidgetZoneId, model.FrequentlyBoughtTogetherWidgetZoneId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(amazonPersonalizeSettings, x => x.FrequentlyBoughtTogetherNumberOfItems, model.FrequentlyBoughtTogetherNumberOfItems_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(amazonPersonalizeSettings, x => x.FrequentlyBoughtTogetherAllowForGuestCustomer, model.FrequentlyBoughtTogetherAllowForGuestCustomer_OverrideForStore, storeScope, false);

            #endregion FrequentlyBoughtTogether

            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

            return RedirectToAction("Configure");
        }

        #endregion Config

        #region Recommender

        public async Task<IActionResult> RecommenderList()
        {
            if (!await _permissionService.AuthorizeAsync(AmazonPersonalizePermissionProvider.ManageRecommenders))
                return AccessDeniedView();

            var model = new RecommenderSearchModel();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> RecommenderList(RecommenderSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(AmazonPersonalizePermissionProvider.ManageRecommenders))
                return await AccessDeniedDataTablesJson();

            searchModel.SetGridPageSize();
            var model = await _recommenderModelFactory.PrepareRecommenderListModelAsync(searchModel);
            return Json(model);
        }

        [HttpPost]
        public async Task<IActionResult> StopRecommender(string recommenderarn)
        {
            if (!await _permissionService.AuthorizeAsync(AmazonPersonalizePermissionProvider.ManageRecommenders))
                return Json(new { result = false });

            try
            {
                var stopRecommendationRequest = new StopRecommenderRequest();
                stopRecommendationRequest.RecommenderArn = recommenderarn;
                var personalizeClient = await _amazonPersonalizeHelperFactory.GetAmazonPersonalizeClient().StopRecommenderAsync(stopRecommendationRequest);

                if (personalizeClient.RecommenderArn != null && personalizeClient.HttpStatusCode == System.Net.HttpStatusCode.OK)
                    return Json(new { result = true });

                return Json(new { result = false });
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync(ex.Message, ex, null);
                return Json(new { result = false });
            }
        }

        [HttpPost]
        public async Task<IActionResult> StartRecommender(string recommenderarn)
        {
            if (!await _permissionService.AuthorizeAsync(AmazonPersonalizePermissionProvider.ManageRecommenders))
                return Json(new { result = false });

            try
            {
                var startRecommendationRequest = new StartRecommenderRequest();
                startRecommendationRequest.RecommenderArn = recommenderarn;
                var personalizeClient = await _amazonPersonalizeHelperFactory.GetAmazonPersonalizeClient().StartRecommenderAsync(startRecommendationRequest);

                if (personalizeClient.RecommenderArn != null && personalizeClient.HttpStatusCode == System.Net.HttpStatusCode.OK)
                    return Json(new { result = true });

                return Json(new { result = false });
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync(ex.Message, ex, null);
                return Json(new { result = false });
            }
        }

        #endregion Recommender

        #region Interactions data

        [HttpPost]
        public async Task<IActionResult> CheckInteractionsDataEligibility(UploadInteractionsDataModel model)
        {
            if (!await _permissionService.AuthorizeAsync(AmazonPersonalizePermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var fileName = $"{AmazonPersonalizeDefaults.InteractionsFileName}.csv";

            var startDateValue = !model.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !model.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);

            var minRequirementsData = await _eventReportService.GetCustomerMinInteractions();
            if (minRequirementsData.Count < 25)
                return await Redirect();

            var interactionsReportLines = await _eventReportService.GetEventReportLineAsync((await _storeContext.GetCurrentStoreAsync()).Id, startDateValue, endDateValue);

            if (interactionsReportLines.Count < 2000)
            {
                return await Redirect();
            }
            else
            {
                var result = await _personalizeExportManager.ExportInteractionsTxtAsync(interactionsReportLines);
                return File(Encoding.UTF8.GetBytes(result), MimeTypes.TextCsv, fileName);
            }
        }

        public async Task<IActionResult> UploadInteractionsData()
        {
            if (!await _permissionService.AuthorizeAsync(AmazonPersonalizePermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            return View(new UploadInteractionsDataModel());
        }

        [EditAccessAjax, HttpPost]
        public async Task<IActionResult> UploadInteractionsData(UploadInteractionsDataModel model)
        {
            if (!await _permissionService.AuthorizeAsync(AmazonPersonalizePermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var minRequirementsData = await _eventReportService.GetCustomerMinInteractions();
            if (minRequirementsData.Count < 25)
                return await Redirect();

            await _amazonPersonalizeHelperFactory.UploadInteractionsAsync(model);
            return Json(new { Message = await _localizationService.GetResourceAsync("Admin.NopStation.AlgoliaSearch.UploadProduct.UploadCompleted") });
        }

        #endregion Interactions data

        #endregion Methods
    }
}