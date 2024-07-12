using System;
using System.Linq;
using System.Threading.Tasks;
using NopStation.Plugin.SMS.SmsTo.Areas.Admin.Models;
using NopStation.Plugin.SMS.SmsTo.Domains;
using NopStation.Plugin.SMS.SmsTo.Services;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Models.Extensions;

namespace NopStation.Plugin.SMS.SmsTo.Areas.Admin.Factories
{
    public class QueuedSmsModelFactory : IQueuedSmsModelFactory
    {
        #region Fields

        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizationService _localizationService;
        private readonly IQueuedSmsService _queuedSmsService;
        private readonly ICustomerService _customerService;
        private readonly IStoreService _storeService;

        #endregion

        #region Ctor

        public QueuedSmsModelFactory(IDateTimeHelper dateTimeHelper,
            ILocalizationService localizationService,
            IQueuedSmsService queuedSmsService,
            ICustomerService customerService,
            IStoreService storeService)
        {
            _dateTimeHelper = dateTimeHelper;
            _localizationService = localizationService;
            _queuedSmsService = queuedSmsService;
            _customerService = customerService;
            _storeService = storeService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Prepare queuedSms search model
        /// </summary>
        /// <param name="searchModel">QueuedSms search model</param>
        /// <returns>QueuedSms search model</returns>
        public virtual QueuedSmsSearchModel PrepareQueuedSmsSearchModel(QueuedSmsSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        /// <summary>
        /// Prepare paged queuedSms list model
        /// </summary>
        /// <param name="searchModel">QueuedSms search model</param>
        /// <returns>QueuedSms list model</returns>
        public virtual async Task<QueuedSmsListModel> PrepareQueuedSmsListModelAsync(QueuedSmsSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get queuedSmss
            var queuedSmss = await _queuedSmsService.GetAllQueuedSmsAsync(false, 0, searchModel.Page - 1, searchModel.PageSize);

            //prepare list model
            var model = new QueuedSmsListModel().PrepareToGridAsync(searchModel, queuedSmss, () =>
            {
                return queuedSmss.SelectAwait(async queuedSms =>
                {
                    return await PrepareQueuedSmsModelAsync(null, queuedSms, false);
                });
            });

            return await model;
        }

        /// <summary>
        /// Prepare queuedSms model
        /// </summary>
        /// <param name="model">QueuedSms model</param>
        /// <param name="queuedSms">QueuedSms</param>
        /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
        /// <returns>QueuedSms model</returns>
        public virtual async Task<QueuedSmsModel> PrepareQueuedSmsModelAsync(QueuedSmsModel model, 
            QueuedSms queuedSms, bool excludeProperties = false)
        {
            if (queuedSms != null)
            {
                //fill in model values from the entity
                model = model ?? queuedSms.ToModel<QueuedSmsModel>();
                model.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(queuedSms.CreatedOnUtc, DateTimeKind.Utc);
                if(queuedSms.SentOnUtc.HasValue)
                    model.SentOn = await _dateTimeHelper.ConvertToUserTimeAsync(queuedSms.SentOnUtc.Value, DateTimeKind.Utc);

                if (!string.IsNullOrWhiteSpace(model.Body))
                    model.Body = model.Body.Replace(Environment.NewLine, "<br />");

                if (!queuedSms.CustomerId.HasValue ||
                    queuedSms.CustomerId == 0)
                {
                    model.CustomerName = await _localizationService.GetResourceAsync("Admin.NopStation.SmsToSms.QueuedSmss.All");
                }
                else
                {
                    var customer = await _customerService.GetCustomerByIdAsync(queuedSms.CustomerId.Value);
                    if (customer == null)
                    {
                        model.CustomerId = 0;
                        model.CustomerName = await _localizationService.GetResourceAsync("Admin.NopStation.SmsToSms.QueuedSmss.Unknown");
                    }
                    else if (await _customerService.IsRegisteredAsync(customer))
                        model.CustomerName = customer.Email;
                    else
                        model.CustomerName = "Guest";
                }

                var store = await _storeService.GetStoreByIdAsync(queuedSms.StoreId);
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
