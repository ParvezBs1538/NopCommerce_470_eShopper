using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Services.Customers;
using Nop.Services.Messages;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.Widgets.VendorShop.Areas.Admin.Models.Subscriber;
using NopStation.Plugin.Widgets.VendorShop.Domains;
using NopStation.Plugin.Widgets.VendorShop.Services;

namespace NopStation.Plugin.Widgets.VendorShop.Areas.Admin.Factories
{
    public class VendorSubscriberModelFactory : IVendorSubscriberModelFactory
    {
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;
        private readonly IStoreContext _storeContext;
        private readonly IMessageTokenProvider _messageTokenProvider;
        private readonly IVendorSubscriberService _vendorSubscriberService;

        public VendorSubscriberModelFactory(IWorkContext workContext,
            ICustomerService customerService,
            IStoreContext storeContext,
            IMessageTokenProvider messageTokenProvider,
            IVendorSubscriberService vendorSubscriberService)
        {
            _workContext = workContext;
            _customerService = customerService;
            _storeContext = storeContext;
            _messageTokenProvider = messageTokenProvider;
            _vendorSubscriberService = vendorSubscriberService;
        }

        public async Task<VendorSubscriberListModel> PrepareVendorSubscriberListModelAsync(VendorSubscriberSearchModel searchModel)
        {
            if (searchModel == null)
            {
                throw new ArgumentNullException(nameof(searchModel));
            }
            var currentCustomer = await _workContext.GetCurrentCustomerAsync();
            var isVendor = await _customerService.IsVendorAsync(currentCustomer);

            if (!isVendor)
            {
                throw new ArgumentNullException(nameof(VendorSubscriber));
            }
            var vendorId = currentCustomer.VendorId;
            var subscribers = await _vendorSubscriberService.GetVendorSubscribersAsync(searchModel, vendorId, searchModel.ActiveStoreScopeConfiguration);

            var model = await new VendorSubscriberListModel().PrepareToGridAsync(searchModel, subscribers, () =>
            {
                return subscribers.SelectAwait(async subscriber =>
                {
                    return new VendorSubscriberModel
                    {
                        Id = subscriber.Id,
                        SubscribedOn = subscriber.SubscribedOn,
                        SubscriberEmail = (await _customerService.GetCustomerByIdAsync(subscriber.CustomerId)).Email,
                    };
                });
            });

            return model;
        }

        public async Task<VendorSubscriberSearchModel> PrepareVendorSubscriberSearchModelAsync(VendorSubscriberSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            searchModel.AllowedTokens = string.Join(", ", (await _messageTokenProvider.GetListOfAllowedTokensAsync(new[] { TokenGroupNames.StoreTokens, TokenGroupNames.CustomerTokens, TokenGroupNames.VendorTokens })).ToList());

            searchModel.SetGridPageSize();

            return await Task.FromResult(searchModel);
        }
    }
}
