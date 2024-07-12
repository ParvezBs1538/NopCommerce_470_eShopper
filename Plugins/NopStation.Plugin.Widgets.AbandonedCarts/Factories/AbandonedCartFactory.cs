using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.Customers;
using Nop.Services.Seo;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.Widgets.AbandonedCarts.Domain;
using NopStation.Plugin.Widgets.AbandonedCarts.Models;
using NopStation.Plugin.Widgets.AbandonedCarts.Services;
using NUglify.Helpers;

namespace NopStation.Plugin.Widgets.AbandonedCarts.Factories
{
    public class AbandonedCartFactory : IAbandonedCartFactory
    {
        #region Fields

        private readonly IAbandonedCartService _abandonedCartService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly ICustomerAbandonmentInfoService _customerAbandonmentInfoService;

        #endregion

        #region Ctor

        public AbandonedCartFactory(IAbandonedCartService abandonedCartService,
            IUrlRecordService urlRecordService,
            ICustomerAbandonmentInfoService customerAbandonmentInfoService)
        {
            _abandonedCartService = abandonedCartService;
            _urlRecordService = urlRecordService;
            _customerAbandonmentInfoService = customerAbandonmentInfoService;
        }

        #endregion

        #region Methods

        public virtual AbandonedCartSearchModel PrepareAbandonedCartSearchModel(AbandonedCartSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        public virtual async Task<AbandonedCartsListModel> PrepareAbandonedCartsListModelAsync(AbandonedCartSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var abandonedCarts = await _abandonedCartService.GetAllAbandonmentsAsync(
                firstName: searchModel.SearchFirstName,
                lastName: searchModel.SearchLastName,
                email: searchModel.SearchEmail,
                productId: searchModel.ProductId,
                createdFromUtc: searchModel.StartDate,
                createdToUtc: searchModel.EndDate,
                customerId: searchModel.CustomerId,
                statusId: searchModel.StatusId,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare grid model
            var model = await new AbandonedCartsListModel().PrepareToGridAsync(searchModel, abandonedCarts, () =>
            {
                return abandonedCarts
                .OrderByDescending(cart => cart.Id)
                .SelectAwait(async abdCart =>
                {
                    abdCart.StatusChangedOn = abdCart.StatusChangedOn.ToLocalTime();
                    abdCart.FirstNotificationSentOn = abdCart.FirstNotificationSentOn == DateTime.MinValue ? null : ((DateTime)abdCart.FirstNotificationSentOn).ToLocalTime();
                    abdCart.SecondNotificationSentOn = abdCart.SecondNotificationSentOn == DateTime.MinValue ? null : ((DateTime)abdCart.SecondNotificationSentOn).ToLocalTime();

                    abdCart.StatusName = Enum.GetName(typeof(AbandonedStatus), abdCart.StatusId);
                    return abdCart;
                });
            });
            return model;
        }

        public virtual async Task<CustomerAbandonmentListModel> PrepareCustomerAbandonmentListModelAsync(AbandonedCartSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var abandonedCarts = await _customerAbandonmentInfoService.GetAllCustomerAbandonmentsAsync(
                firstName: searchModel.SearchFirstName,
                lastName: searchModel.SearchLastName,
                email: searchModel.SearchEmail,
                productId: searchModel.ProductId,
                createdFromUtc: searchModel.StartDate,
                createdToUtc: searchModel.EndDate,
                customerId: searchModel.CustomerId,
                statusId: searchModel.StatusId,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare grid model
            var model = await new CustomerAbandonmentListModel().PrepareToGridAsync(searchModel, abandonedCarts, () =>
            {
                return abandonedCarts.SelectAwait(async abdCart =>
                {
                    var customer = new Customer()
                    {
                        Id = abdCart.CustomerId
                    };
                    abdCart.StatusName = Enum.GetName(typeof(CustomerAbandonmentStatus), abdCart.StatusId);

                    abdCart.TotalItems = await _abandonedCartService.GetCustomerAbandonedCartsCountAsync(firstName: searchModel.SearchFirstName,
                        lastName: searchModel.SearchLastName,
                        email: searchModel.SearchEmail,
                        productId: searchModel.ProductId,
                        createdFromUtc: searchModel.StartDate,
                        createdToUtc: searchModel.EndDate,
                        customerId: abdCart.CustomerId,
                        statusId: searchModel.StatusId);
                    abdCart.LastNotificationSentOn = abdCart.LastNotificationSentOn == null ? null : ((DateTime)abdCart.LastNotificationSentOn).ToLocalTime();
                    abdCart.UnsubscribedOnUtc = abdCart.UnsubscribedOnUtc == null ? null : ((DateTime)abdCart.UnsubscribedOnUtc).ToLocalTime();

                    return abdCart;
                });
            });
            return model;
        }

        public async Task<IList<ProductInfoModel>> PrepareProductInfoModelsByCustomerAsync(int customerId)
        {
            var products = await _abandonedCartService.GetProductsByCustomerAsync(customerId);

            products.ForEach(async c => c.SlugValue = await _urlRecordService.GetSeNameAsync(c.ProductId, "Product"));
            return products;
        }

        public async Task<AbandonmentDetailsViewModel> PrepareAbandonedCartDetailViewModel(int id)
        {
            if (id <= 0)
                return null;

            var abandonmentDetails = await _abandonedCartService.GetAbandonedCartDetailByIdAsync(id);
            if (abandonmentDetails == null || abandonmentDetails?.AbandonmentInfo?.CustomerId <= 0)
                return abandonmentDetails;
            abandonmentDetails.AbandonmentInfo.FirstNotificationSentOn = abandonmentDetails.AbandonmentInfo.FirstNotificationSentOn == DateTime.MinValue ? null : ((DateTime)abandonmentDetails.AbandonmentInfo.FirstNotificationSentOn).ToLocalTime();
            abandonmentDetails.AbandonmentInfo.SecondNotificationSentOn = abandonmentDetails.AbandonmentInfo.SecondNotificationSentOn == DateTime.MinValue ? null : ((DateTime)abandonmentDetails.AbandonmentInfo.SecondNotificationSentOn).ToLocalTime();
            abandonmentDetails.AbandonmentInfo.StatusChangedOn = abandonmentDetails.AbandonmentInfo.StatusChangedOn.ToLocalTime();

            var customerAbandonment = await _customerAbandonmentInfoService.GetCustomerAbandonmentByCustomerIdAsync(abandonmentDetails.AbandonmentInfo.CustomerId);
            if (customerAbandonment == null)
                return abandonmentDetails;
            abandonmentDetails.CustomerAbandonmentInfo = new CustomerAbandonmentInfoModel()
            {
                Id = customerAbandonment.Id,
                NotificationSentFrequency = customerAbandonment.NotificationSentFrequency,
                StatusId = customerAbandonment.StatusId,
                StatusName = Enum.GetName(typeof(CustomerAbandonmentStatus), customerAbandonment.StatusId),
                LastNotificationSentOn = customerAbandonment.LastNotificationSentOn == DateTime.MinValue ? null : ((DateTime)customerAbandonment.LastNotificationSentOn).ToLocalTime(),
                UnsubscribedOnUtc = customerAbandonment.UnsubscribedOnUtc == DateTime.MinValue ? null : ((DateTime)customerAbandonment.UnsubscribedOnUtc).ToLocalTime()
            };
            return abandonmentDetails;
        }

        #endregion
    }
}
