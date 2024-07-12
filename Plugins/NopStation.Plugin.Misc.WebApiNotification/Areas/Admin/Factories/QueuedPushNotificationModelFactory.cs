using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.Misc.WebApiNotification.Areas.Admin.Models;
using NopStation.Plugin.Misc.WebApiNotification.Domains;
using NopStation.Plugin.Misc.WebApiNotification.Services;

namespace NopStation.Plugin.Misc.WebApiNotification.Areas.Admin.Factories
{
    public class QueuedPushNotificationModelFactory : IQueuedPushNotificationModelFactory
    {
        #region Fields

        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizationService _localizationService;
        private readonly IQueuedPushNotificationService _queuedPushNotificationService;
        private readonly ICustomerService _customerService;
        private readonly IStoreService _storeService;

        #endregion

        #region Ctor

        public QueuedPushNotificationModelFactory(IDateTimeHelper dateTimeHelper,
            ILocalizationService localizationService,
            IQueuedPushNotificationService queuedPushNotificationService,
            ICustomerService customerService,
            IStoreService storeService)
        {
            _dateTimeHelper = dateTimeHelper;
            _localizationService = localizationService;
            _queuedPushNotificationService = queuedPushNotificationService;
            _customerService = customerService;
            _storeService = storeService;
        }

        #endregion

        #region Methods

        public virtual WebApiQueuedNotificationSearchModel PrepareQueuedPushNotificationSearchModel(WebApiQueuedNotificationSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        public virtual async Task<WebApiQueuedNotificationListModel> PrepareQueuedPushNotificationListModelAsync(WebApiQueuedNotificationSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get queuedPushNotifications
            var queuedPushNotifications = await _queuedPushNotificationService.GetAllQueuedPushNotificationsAsync(pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare list model
            var model = await new WebApiQueuedNotificationListModel().PrepareToGridAsync(searchModel, queuedPushNotifications, () =>
            {
                return queuedPushNotifications.SelectAwait(async queuedPushNotification =>
                {
                    return await PrepareQueuedPushNotificationModelAsync(null, queuedPushNotification, false);
                });
            });

            return model;
        }

        public virtual async Task<WebApiQueuedNotificationModel> PrepareQueuedPushNotificationModelAsync(WebApiQueuedNotificationModel model,
            WebApiQueuedNotification queuedPushNotification, bool excludeProperties = false)
        {
            if (queuedPushNotification != null)
            {
                //fill in model values from the entity
                model = model ?? queuedPushNotification.ToModel<WebApiQueuedNotificationModel>();
                model.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(queuedPushNotification.CreatedOnUtc, DateTimeKind.Utc);
                if (queuedPushNotification.SentOnUtc.HasValue)
                    model.SentOn = await _dateTimeHelper.ConvertToUserTimeAsync(queuedPushNotification.SentOnUtc.Value, DateTimeKind.Utc);
                if (queuedPushNotification.DontSendBeforeDateUtc.HasValue)
                    model.DontSendBeforeDate = await _dateTimeHelper.ConvertToUserTimeAsync(queuedPushNotification.DontSendBeforeDateUtc.Value, DateTimeKind.Utc);
                else
                    model.SendImmediately = true;

                model.ActionTypeStr = await _localizationService.GetLocalizedEnumAsync(queuedPushNotification.ActionType);
                model.DeviceTypeStr = await _localizationService.GetLocalizedEnumAsync(queuedPushNotification.DeviceType);

                if (!string.IsNullOrWhiteSpace(model.Body))
                    model.Body = model.Body.Replace(Environment.NewLine, "<br />");

                var customer = await _customerService.GetCustomerByIdAsync(queuedPushNotification.CustomerId);
                model.CustomerName = customer?.Email ??
                    await _localizationService.GetResourceAsync("Admin.NopStation.WebApiNotification.QueuedPushNotifications.Guest");

                var store = await _storeService.GetStoreByIdAsync(queuedPushNotification.StoreId);
                model.StoreName = store?.Name;
            }

            if (!excludeProperties)
            {

            }
            return model;
        }

        #endregion
    }
}
