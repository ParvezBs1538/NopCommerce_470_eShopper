using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Directory;
using NopStation.Plugin.Widgets.AffiliateStation.Areas.Admin.Models;
using NopStation.Plugin.Widgets.AffiliateStation.Domains;
using NopStation.Plugin.Widgets.AffiliateStation.Services;
using Nop.Services;
using Nop.Services.Affiliates;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Models.Extensions;

namespace NopStation.Plugin.Widgets.AffiliateStation.Areas.Admin.Factories
{
    public class AffiliateCustomerModelFactory : IAffiliateCustomerModelFactory
    {
        #region Fields

        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IAffiliateService _affiliateService;
        private readonly ICustomerService _customerService;
        private readonly IAffiliateCustomerService _affiliateCustomerService;
        private readonly ILocalizationService _localizationService;
        private readonly ICurrencyService _currencyService;
        private readonly CurrencySettings _currencySettings;

        #endregion

        #region Ctor

        public AffiliateCustomerModelFactory(IDateTimeHelper dateTimeHelper,
            IAffiliateService affiliateService,
            ICustomerService customerService,
            IAffiliateCustomerService affiliateCustomerService,
            ILocalizationService localizationService,
            ICurrencyService currencyService,
            CurrencySettings currencySettings)
        {
            _dateTimeHelper = dateTimeHelper;
            _affiliateService = affiliateService;
            _customerService = customerService;
            _affiliateCustomerService = affiliateCustomerService;
            _localizationService = localizationService;
            _currencyService = currencyService;
            _currencySettings = currencySettings;
        }

        #endregion

        #region Utilities

        protected async Task PrepareAvailableApplyStatusesAsync(IList<SelectListItem> availableApplyStatuses)
        {
            var availablePositionItems = await ApplyStatus.Applied.ToSelectListAsync(false);
            foreach (var positionItem in availablePositionItems)
            {
                availableApplyStatuses.Add(positionItem);
            }
        }

        #endregion

        #region Methods

        public async Task<AffiliateCustomerSearchModel> PrepareAffiliateCustomerSearchModelAsync()
        {
            var model = new AffiliateCustomerSearchModel();
            model.ApplyStatusIds.Add(0);

            await PrepareAvailableApplyStatusesAsync(model.AvailableApplyStatuses);
            model.AvailableApplyStatuses.Insert(0, new SelectListItem()
            {
                Text = await _localizationService.GetResourceAsync("Admin.NopStation.AffiliateStation.AffiliateCustomers.List.ApplyStatusIds.All"),
                Value = "0"
            });

            model.AvailableActiveStatuses.Add(new SelectListItem()
            {
                Value = "0",
                Text = await _localizationService.GetResourceAsync("Admin.Common.All")
            });
            model.AvailableActiveStatuses.Add(new SelectListItem()
            {
                Value = "1",
                Text = await _localizationService.GetResourceAsync("Admin.NopStation.AffiliateStation.AffiliateCustomers.Active")
            });
            model.AvailableActiveStatuses.Add(new SelectListItem()
            {
                Value = "2",
                Text = await _localizationService.GetResourceAsync("Admin.NopStation.AffiliateStation.AffiliateCustomers.InActive")
            });
            return model;
        }

        public async Task<AffiliateCustomerListModel> PrepareAffiliateCustomerListModelAsync(AffiliateCustomerSearchModel searchModel)
        {
            bool? active = null;
            if (searchModel.ActiveStatusId == 1)
                active = true;
            if (searchModel.ActiveStatusId == 2)
                active = false;
            var createdFromUtc = !searchModel.CreatedFrom.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.CreatedFrom.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var createdToUtc = !searchModel.CreatedTo.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.CreatedTo.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);

            var affiliateCustomers = await _affiliateCustomerService.SearchAffiliateCustomersAsync(
                customerEmail: searchModel.CustomerEmail,
                firstName: searchModel.AffiliateFirstName,
                lastName: searchModel.AffiliateLastName,
                applyStatusIds: searchModel.ApplyStatusIds.Contains(0) ? null : searchModel.ApplyStatusIds,
                pageIndex: searchModel.Page - 1,
                active: active,
                createdFromUtc: createdFromUtc,
                createdToUtc: createdToUtc,
                pageSize: searchModel.PageSize);

            var model = await new AffiliateCustomerListModel().PrepareToGridAsync(searchModel, affiliateCustomers, () =>
            {
                //fill in model values from the entity
                return affiliateCustomers.SelectAwait(async affiliateCustomer =>
                {
                    var affiliateCustomerModel = await PrepareAffiliateCustomerModelAsync(null, affiliateCustomer, true);

                    return affiliateCustomerModel;
                });
            });

            return model;
        }

        public async Task<AffiliateCustomerModel> PrepareAffiliateCustomerModelAsync(AffiliateCustomerModel model,
            AffiliateCustomer affiliateCustomer, bool excludeProperties = false)
        {
            if (affiliateCustomer != null)
            {
                //fill in model values from the entity
                model = model ?? affiliateCustomer.ToModel<AffiliateCustomerModel>();
                var affiliate = await _affiliateService.GetAffiliateByIdAsync(affiliateCustomer.AffiliateId);
                if (affiliate != null && !affiliate.Deleted)
                {
                    model.AffiliateName = await _affiliateService.GetAffiliateFullNameAsync(affiliate);
                    model.Active = affiliate.Active;
                }

                var customer = await _customerService.GetCustomerByIdAsync(affiliateCustomer.CustomerId);
                if (customer != null)
                {
                    model.CustomerFullName = await _customerService.GetCustomerFullNameAsync(customer);
                    model.CustomerEmail = customer.Email;
                }

                //convert dates to the user time
                model.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(affiliateCustomer.CreatedOnUtc, DateTimeKind.Utc);
                model.ApplyStatus = await _localizationService.GetLocalizedEnumAsync(affiliateCustomer.ApplyStatus);
                model.PrimaryStoreCurrencyCode = (await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId)).CurrencyCode;
            }

            //prepare localized models
            if (!excludeProperties)
                await PrepareAvailableApplyStatusesAsync(model.AvailableApplyStatuses);

            return model;
        }

        #endregion
    }
}
