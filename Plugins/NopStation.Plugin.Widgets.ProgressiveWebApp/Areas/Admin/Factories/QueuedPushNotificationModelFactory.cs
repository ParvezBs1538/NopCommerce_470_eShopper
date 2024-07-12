using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Areas.Admin.Models;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Domains;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Services;

namespace NopStation.Plugin.Widgets.ProgressiveWebApp.Areas.Admin.Factories
{
    public class QueuedPushNotificationModelFactory : IQueuedPushNotificationModelFactory
    {
        #region Fields

        private readonly CatalogSettings _catalogSettings;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly IQueuedPushNotificationService _queuedPushNotificationService;
        private readonly ICustomerService _customerService;
        private readonly IStoreService _storeService;

        #endregion

        #region Ctor

        public QueuedPushNotificationModelFactory(CatalogSettings catalogSettings,
            IBaseAdminModelFactory baseAdminModelFactory,
            IDateTimeHelper dateTimeHelper,
            ILanguageService languageService,
            ILocalizationService localizationService,
            IQueuedPushNotificationService queuedPushNotificationService,
            ICustomerService customerService,
            IStoreService storeService)
        {
            _catalogSettings = catalogSettings;
            _baseAdminModelFactory = baseAdminModelFactory;
            _dateTimeHelper = dateTimeHelper;
            _languageService = languageService;
            _localizationService = localizationService;
            _queuedPushNotificationService = queuedPushNotificationService;
            _customerService = customerService;
            _storeService = storeService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Prepare queuedPushNotification search model
        /// </summary>
        /// <param name="searchModel">QueuedPushNotification search model</param>
        /// <returns>QueuedPushNotification search model</returns>
        public virtual QueuedPushNotificationSearchModel PrepareQueuedPushNotificationSearchModel(QueuedPushNotificationSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        /// <summary>
        /// Prepare paged queuedPushNotification list model
        /// </summary>
        /// <param name="searchModel">QueuedPushNotification search model</param>
        /// <returns>QueuedPushNotification list model</returns>
        public virtual async Task<QueuedPushNotificationListModel> PrepareQueuedPushNotificationListModelAsync(QueuedPushNotificationSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get queuedPushNotifications
            var queuedPushNotifications = await _queuedPushNotificationService.GetAllQueuedPushNotificationsAsync(null, searchModel.Page - 1, searchModel.PageSize);

            //prepare list model
            var model = await new QueuedPushNotificationListModel().PrepareToGridAsync(searchModel, queuedPushNotifications, () =>
            {
                return queuedPushNotifications.SelectAwait(async queuedPushNotification =>
                {
                    return await PrepareQueuedPushNotificationModelAsync(null, queuedPushNotification, false);
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare queuedPushNotification model
        /// </summary>
        /// <param name="model">QueuedPushNotification model</param>
        /// <param name="queuedPushNotification">QueuedPushNotification</param>
        /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
        /// <returns>QueuedPushNotification model</returns>
        public virtual async Task<QueuedPushNotificationModel> PrepareQueuedPushNotificationModelAsync(QueuedPushNotificationModel model,
            QueuedPushNotification queuedPushNotification, bool excludeProperties = false)
        {
            if (queuedPushNotification != null)
            {
                //fill in model values from the entity
                model = model ?? queuedPushNotification.ToModel<QueuedPushNotificationModel>();
                model.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(queuedPushNotification.CreatedOnUtc, DateTimeKind.Utc);
                if (queuedPushNotification.SentOnUtc.HasValue)
                    model.SentOn = await _dateTimeHelper.ConvertToUserTimeAsync(queuedPushNotification.SentOnUtc.Value, DateTimeKind.Utc);

                if (!string.IsNullOrWhiteSpace(model.Body))
                    model.Body = model.Body.Replace(Environment.NewLine, "<br />");

                if (!queuedPushNotification.CustomerId.HasValue ||
                    queuedPushNotification.CustomerId == 0)
                {
                    model.CustomerName = await _localizationService.GetResourceAsync("Admin.NopStation.PWA.QueuedPushNotifications.All");
                }
                else
                {
                    var customer = await _customerService.GetCustomerByIdAsync(queuedPushNotification.CustomerId.Value);
                    if (customer == null)
                    {
                        model.CustomerId = 0;
                        model.CustomerName = await _localizationService.GetResourceAsync("Admin.NopStation.PWA.QueuedPushNotifications.Unknown");
                    }
                    else if (await _customerService.IsRegisteredAsync(customer))
                        model.CustomerName = customer.Email;
                    else
                        model.CustomerName = "Guest";
                }

                var store = await _storeService.GetStoreByIdAsync(queuedPushNotification.StoreId);
                if (store != null)
                    model.StoreName = store.Name;
            }

            if (!excludeProperties)
            {

            }
            return model;
        }

        #endregion
    }
}
