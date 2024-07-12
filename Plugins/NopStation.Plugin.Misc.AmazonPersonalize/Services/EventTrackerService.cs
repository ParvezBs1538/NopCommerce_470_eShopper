using System;
using System.Threading.Tasks;
using Amazon.PersonalizeEvents;
using Amazon.Runtime;
using Nop.Services.Logging;
using NopStation.Plugin.Misc.AmazonPersonalize.Helpers;
using NopStation.Plugin.Misc.AmazonPersonalize.Models;

namespace NopStation.Plugin.Misc.AmazonPersonalize.Services
{
    public class EventTrackerService : IEventTrackerService
    {
        #region Fields

        private readonly ILogger _logger;
        private readonly AmazonPersonalizeSettings _amazonPersonalizeSettings;

        #endregion Fields

        #region Ctor

        public EventTrackerService(ILogger logger,
            AmazonPersonalizeSettings amazonPersonalizeSettings)
        {
            _logger = logger;
            _amazonPersonalizeSettings = amazonPersonalizeSettings;
        }

        #endregion Ctor

        #region Methods

        public async Task AddEventTrackerAsync(EventTrackerModel model)
        {
            try
            {
                var amazonPersonalizeEventsClient = new AmazonPersonalizeEventsClient(new BasicAWSCredentials(_amazonPersonalizeSettings.AccessKey, _amazonPersonalizeSettings.SecretKey), AwsRegionHelper.GetRegionEndPoint(AwsRegionHelper.GetAwsRegion(_amazonPersonalizeSettings.AwsRegionId)));
                var response = await amazonPersonalizeEventsClient.PutEventsAsync(model.PutEventsRequest);
                
                if(_amazonPersonalizeSettings.EnableLogging)
                {
                    if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                        await _logger.InformationAsync($"Adding a {model.EventType} event for userId: {model.UserId} and productId: {model.ProductId}");
                    
                    else
                        await _logger.InformationAsync($"Adding a {model.EventType} event for userId: {model.UserId} and productId: {model.ProductId} failed. Status code: {response.HttpStatusCode}");
                    
                }
                
                
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync(ex.Message, ex);
            }
        }

        #endregion Methods
    }
}