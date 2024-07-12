using System;
using System.Threading.Tasks;
using Amazon.Personalize;
using Nop.Core;
using Nop.Services.Helpers;
using Nop.Services.Logging;
using NopStation.Plugin.Misc.AmazonPersonalize.Areas.Admin.Models;
using NopStation.Plugin.Misc.AmazonPersonalize.Helpers;
using NopStation.Plugin.Misc.AmazonPersonalize.Services;

namespace NopStation.Plugin.Misc.AmazonPersonalize.Factories
{
    public class AmazonPersonalizeHelperFactory : IAmazonPersonalizeHelperFactory
    {
        private readonly InteractionsUploadHub _interactionsUploadHub;
        private readonly IEventReportService _eventReportService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IStoreContext _storeContext;
        private readonly IEventTrackerModelFactory _eventTrackerModelFactory;
        private readonly IEventTrackerService _eventTrackerService;
        private readonly ILogger _logger;
        private readonly AmazonPersonalizeSettings _amazonPersonalizeSettings;

        public AmazonPersonalizeHelperFactory(InteractionsUploadHub uploadHub,
            IEventReportService eventReportService,
            IDateTimeHelper dateTimeHelper,
            IStoreContext storeContext,
            IEventTrackerModelFactory eventTrackerModelFactory,
            IEventTrackerService eventTrackerService,
            ILogger logger,
            AmazonPersonalizeSettings amazonPersonalizeSettings)
        {
            _interactionsUploadHub = uploadHub;
            _eventReportService = eventReportService;
            _dateTimeHelper = dateTimeHelper;
            _storeContext = storeContext;
            _eventTrackerModelFactory = eventTrackerModelFactory;
            _eventTrackerService = eventTrackerService;
            _logger = logger;
            _amazonPersonalizeSettings = amazonPersonalizeSettings;
        }

        public AmazonPersonalizeClient GetAmazonPersonalizeClient()
        {
            return new AmazonPersonalizeClient(_amazonPersonalizeSettings.AccessKey, _amazonPersonalizeSettings.SecretKey, AwsRegionHelper.GetRegionEndPoint(AwsRegionHelper.GetAwsRegion(_amazonPersonalizeSettings.AwsRegionId)));
        }

        public async Task UploadInteractionsAsync(UploadInteractionsDataModel model)
        {
            var pageIndex = 0;
            var currentPageProducts = 0;
            var totalProducts = 0;
            var totalPages = 0;
            var uploaded = 0;
            var failed = 0;

            var startDateValue = !model.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !model.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);
            try
            {
                while (true)
                {
                    await _interactionsUploadHub.UploadInteractionsAsync(pageIndex, totalPages, currentPageProducts, totalProducts, 0, failed, uploaded, -10, "Interactions data fetching from database...");

                    var interactions = await _eventReportService.GetEventReportLineAsync((await _storeContext.GetCurrentStoreAsync()).Id, startDateValue, endDateValue, pageIndex: pageIndex, pageSize: 100);

                    if (interactions == null || interactions.Count == 0)
                        break;

                    currentPageProducts = interactions.Count;
                    totalProducts = interactions.TotalCount;
                    totalPages = interactions.TotalPages;

                    var binding = 0;

                    foreach (var interaction in interactions)
                    {
                        try
                        {
                            await _interactionsUploadHub.UploadInteractionsAsync(pageIndex, totalPages, currentPageProducts, totalProducts, binding + 1, failed, uploaded, 110);
                           
                            await _eventTrackerService.AddEventTrackerAsync(await _eventTrackerModelFactory.PrepareMultiplePutEventsRequest(interaction));

                            binding++;

                            uploaded++;
                            await _interactionsUploadHub.UploadInteractionsAsync(pageIndex, totalPages, currentPageProducts, totalProducts, binding, failed, uploaded, 10);
                        }
                        catch (Exception ex)
                        {
                            await _logger.ErrorAsync("Amazon personalize: " + ex.Message + ", Product Id = " + interaction.ItemId + ", Customer Id = " + interaction.UserId, ex);
                            failed++;

                            await _interactionsUploadHub.UploadInteractionsAsync(pageIndex, totalPages, currentPageProducts, totalProducts, binding, failed, uploaded, -1, ex.Message);
                            continue;
                        }
                    }
                    pageIndex++;
                }
                await _interactionsUploadHub.UploadInteractionsAsync(pageIndex, totalPages, currentPageProducts, totalProducts, 0, failed, uploaded, 100);
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync("Amazon personalize: " + ex.Message, ex);
                await _interactionsUploadHub.UploadInteractionsAsync(pageIndex, totalPages, currentPageProducts, totalProducts, 0, failed, uploaded, -1, ex.Message);
            }
        }
    }
}