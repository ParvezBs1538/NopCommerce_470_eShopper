using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Affiliates;
using NopStation.Plugin.Widgets.AffiliateStation.Domains;
using NopStation.Plugin.Widgets.AffiliateStation.Factories;
using NopStation.Plugin.Widgets.AffiliateStation.Models;
using NopStation.Plugin.Widgets.AffiliateStation.Services;
using NopStation.Plugin.Misc.Core.Controllers;
using Nop.Services.Affiliates;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Web.Extensions;

namespace NopStation.Plugin.Widgets.AffiliateStation.Controllers
{
    public class AffiliateStationController : NopStationPublicController
    {
        #region Fields

        private readonly IWorkContext _workContext;
        private readonly IAffiliateCustomerService _affiliateCustomerService;
        private readonly IAffiliateCustomerModelFactory _affiliateCustomerModelFactory;
        private readonly IOrderCommissionModelFactory _orderCommissionModelFactory;
        private readonly IAffiliateService _affiliateService;
        private readonly ICustomerService _customerService;
        private readonly IAddressService _addressService;

        #endregion

        #region Ctor

        public AffiliateStationController(IWorkContext workContext,
            IAffiliateCustomerService affiliateCustomerService,
            IAffiliateCustomerModelFactory affiliateCustomerModelFactory,
            IOrderCommissionModelFactory orderCommissionModelFactory,
            IAffiliateService affiliateService,
            ICustomerService customerService,
            IAddressService addressService)
        {
            _workContext = workContext;
            _affiliateCustomerService = affiliateCustomerService;
            _affiliateCustomerModelFactory = affiliateCustomerModelFactory;
            _orderCommissionModelFactory = orderCommissionModelFactory;
            _affiliateService = affiliateService;
            _customerService = customerService;
            _addressService = addressService;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Info()
        {
            if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
                return Challenge();

            var model = await _affiliateCustomerModelFactory.PrepareAffiliateInfoModelAsync();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Info(AffiliateInfoModel model)
        {
            if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
                return Challenge();

            if (ModelState.IsValid)
            {
                var address = model.Address.ToEntity();
                address.CreatedOnUtc = DateTime.UtcNow;

                if ((address.CountryId.GetValueOrDefault() == 0) & address.CountryId.HasValue)
                    address.CountryId = null;

                if ((address.StateProvinceId.GetValueOrDefault() == 0) & address.StateProvinceId.HasValue)
                    address.StateProvinceId = null;

                var affiliateCustomer = await _affiliateCustomerService.GetAffiliateCustomerByCustomerIdAsync((await _workContext.GetCurrentCustomerAsync()).Id);
                if (affiliateCustomer == null)
                {
                    await _addressService.InsertAddressAsync(address);

                    var affiliate = new Affiliate();
                    affiliate.FriendlyUrlName = await _affiliateService.ValidateFriendlyUrlNameAsync(affiliate, model.FriendlyUrlName);
                    affiliate.AddressId = address.Id;
                    await _affiliateService.InsertAffiliateAsync(affiliate);

                    var newAffiliateCustomer = new AffiliateCustomer
                    {
                        AffiliateId = affiliate.Id,
                        ApplyStatus = ApplyStatus.Applied,
                        CreatedOnUtc = DateTime.UtcNow,
                        UpdatedOnUtc = DateTime.UtcNow,
                        CustomerId = (await _workContext.GetCurrentCustomerAsync()).Id
                    };

                    await _affiliateCustomerService.InsertAffiliateCustomerAsync(newAffiliateCustomer);
                }
                else
                {
                    var affiliate = await _affiliateService.GetAffiliateByIdAsync(affiliateCustomer.AffiliateId);
                    if (affiliate != null && !affiliate.Deleted)
                    {
                        await _addressService.UpdateAddressAsync(address);

                        affiliate.FriendlyUrlName = await _affiliateService.ValidateFriendlyUrlNameAsync(affiliate, model.FriendlyUrlName);
                        affiliate.AddressId = address.Id;
                        await _affiliateService.UpdateAffiliateAsync(affiliate);

                        affiliateCustomer.UpdatedOnUtc = DateTime.UtcNow;
                        await _affiliateCustomerService.UpdateAffiliateCustomerAsync(affiliateCustomer);
                    }
                    else
                    {
                        var newAffiliate = new Affiliate();
                        newAffiliate.FriendlyUrlName = await _affiliateService.ValidateFriendlyUrlNameAsync(newAffiliate, model.FriendlyUrlName);
                        newAffiliate.AddressId = address.Id;
                        await _affiliateService.InsertAffiliateAsync(newAffiliate);

                        affiliateCustomer.AffiliateId = newAffiliate.Id;
                        affiliateCustomer.UpdatedOnUtc = DateTime.UtcNow;
                        await _affiliateCustomerService.UpdateAffiliateCustomerAsync(affiliateCustomer);
                    }
                }

                return RedirectToAction("Info");
            }

            model = await _affiliateCustomerModelFactory.PrepareAffiliateInfoModelAsync();
            return View(model);
        }

        public async Task<IActionResult> Orders(AffiliatedOrderPagingFilteringModel command)
        {
            if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
                return Challenge();

            var affiliateCustomer = await _affiliateCustomerService.GetAffiliateCustomerByCustomerIdAsync((await _workContext.GetCurrentCustomerAsync()).Id);
            if (affiliateCustomer == null || affiliateCustomer.ApplyStatus != ApplyStatus.Approved)
                return RedirectToRoute("HomePage");
            else
            {
                command.PageNumber = command.PageNumber < 1 ? 1 : command.PageNumber;
                var model = await _orderCommissionModelFactory.PrepareAffiliatedOrderSummaryModelAsync(affiliateCustomer, command);
                return View(model);
            }
        }

        #endregion
    }
}


