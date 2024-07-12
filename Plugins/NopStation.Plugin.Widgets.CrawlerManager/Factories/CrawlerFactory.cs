using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.Customers;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Customers;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.Widgets.CrawlerManager.Models;
using NopStation.Plugin.Widgets.CrawlerManager.Services;

namespace NopStation.Plugin.Widgets.CrawlerManager.Factories
{
    public class CrawlerFactory : ICrawlerFactory
    {
        #region Fields

        private readonly ICrawlerService _crawlerService;
        private readonly ILocalizationService _localizationService;
        private readonly ICustomerService _customerService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly CustomerSettings _customerSettings;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IGeoLookupService _geoLookupService;

        #endregion

        #region Ctor

        public CrawlerFactory(ICrawlerService crawlerService,
            ILocalizationService localizationService,
            ICustomerService customerService,
            IDateTimeHelper dateTimeHelper,
            CustomerSettings customerSettings,
            IGenericAttributeService genericAttributeService,
            IGeoLookupService geoLookupService)
        {
            _crawlerService = crawlerService;
            _localizationService = localizationService;
            _customerService = customerService;
            _dateTimeHelper = dateTimeHelper;
            _customerSettings = customerSettings;
            _genericAttributeService = genericAttributeService;
            _geoLookupService = geoLookupService;
        }

        #endregion

        #region Methods

        public virtual Task<OnlineCustomerSearchModel> PrepareOnlineGuestCustomerSearchModelAsync(OnlineCustomerSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare page parameters
            searchModel.SetGridPageSize();

            return Task.FromResult(searchModel);
        }

        public virtual async Task<OnlineCustomerListModel> PrepareOnlineGuestCustomerListModelAsync(OnlineCustomerSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get parameters to filter customers
            var lastActivityFrom = DateTime.UtcNow.AddMinutes(-_customerSettings.OnlineCustomerMinutes);

            var guestRole = await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.GuestsRoleName);

            //get online guest customers
            var customers = await _customerService.GetOnlineCustomersAsync(customerRoleIds: new int[] { guestRole.Id },
                 lastActivityFromUtc: lastActivityFrom,
                 pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare list model
            var model = await new OnlineCustomerListModel().PrepareToGridAsync(searchModel, customers, () =>
            {
                return customers.SelectAwait(async customer =>
                {
                    //fill in model values from the entity
                    var customerModel = customer.ToModel<OnlineCustomerModel>();

                    //convert dates to the user time
                    customerModel.LastActivityDate = await _dateTimeHelper.ConvertToUserTimeAsync(customer.LastActivityDateUtc, DateTimeKind.Utc);

                    //fill in additional values (not existing in the entity)
                    customerModel.CustomerInfo = await GetGuestCustomerInfoAsync(customer);
                    customerModel.LastIpAddress = _customerSettings.StoreIpAddresses
                        ? customer.LastIpAddress : await _localizationService.GetResourceAsync("Admin.Customers.OnlineCustomers.Fields.IPAddress.Disabled");
                    customerModel.Location = _geoLookupService.LookupCountryName(customer.LastIpAddress);
                    customerModel.LastVisitedPage = _customerSettings.StoreLastVisitedPage
                        ? await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.LastVisitedPageAttribute)
                        : await _localizationService.GetResourceAsync("Admin.Customers.OnlineCustomers.Fields.LastVisitedPage.Disabled");

                    return customerModel;
                });
            });

            return model;
        }

        public virtual Task<OnlineCustomerSearchModel> PrepareCrawlersSearchModelAsync(OnlineCustomerSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare page parameters
            searchModel.SetGridPageSize();

            return Task.FromResult(searchModel);
        }

        public virtual async Task<CrawlerListModel> PrepareCrawlersListModelAsync(OnlineCustomerSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get online guest crawlers
            var crawlers = await _crawlerService.GetCrawlersAsync(pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare list model
            var model = await new CrawlerListModel().PrepareToGridAsync(searchModel, crawlers, () =>
            {
                return crawlers.SelectAwait(async crawler =>
                {
                    //fill in model values from the entity
                    var crawlerModel = crawler.ToModel<CrawlerModel>();

                    crawlerModel.AddedOnUtc = crawler.AddedOnUtc.ToLocalTime();

                    return crawlerModel;
                });
            });

            return model;
        }

        private async Task<string> GetGuestCustomerInfoAsync(Customer customer)
        {
            if (customer == null)
            {
                return await _localizationService.GetResourceAsync("Admin.Customers.Guest");
            }

            if (customer.IsSystemAccount && !string.IsNullOrEmpty(customer.SystemName))
            {
                return customer.SystemName;
            }

            return $"{await _localizationService.GetResourceAsync("Admin.Customers.Guest")} UA: {customer.AdminComment}";
        }

        #endregion
    }
}
