using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Discounts;
using NopStation.Plugin.DiscountRules.AffiliatedCustomers.Models;
using Nop.Services.Configuration;
using Nop.Services.Discounts;
using Nop.Services.Security;
using NopStation.Plugin.Misc.Core.Controllers;
using Nop.Services.Affiliates;
using Nop.Services.Common;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace NopStation.Plugin.DiscountRules.AffiliatedCustomers.Controllers
{
    public class DiscountRulesAffiliatedCustomersController : NopStationAdminController
    {
        private readonly IDiscountService _discountService;
        private readonly IPermissionService _permissionService;
        private readonly IAffiliateService _affiliateService;
        private readonly IAddressService _addressService;
        private readonly ISettingService _settingService;

        public DiscountRulesAffiliatedCustomersController(
            IDiscountService discountService,
            ISettingService settingService,
            IPermissionService permissionService,
            IAffiliateService affiliateService,
            IAddressService addressService)
        {
            _discountService = discountService;
            _permissionService = permissionService;
            _affiliateService = affiliateService;
            _addressService = addressService;
            _settingService = settingService;
        }

        public async Task<IActionResult> Configure(int discountId, int? discountRequirementId)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageDiscounts))
                return Content("Access denied");

            var discount = await _discountService.GetDiscountByIdAsync(discountId);
            if (discount == null)
                throw new ArgumentException("Discount could not be loaded");

            //check whether the discount requirement exists
            if (discountRequirementId.HasValue && await _discountService.GetDiscountRequirementByIdAsync(discountRequirementId.Value) is null)
                return Content("Failed to load requirement.");

            var affiliateId = await _settingService.GetSettingByKeyAsync<int>(string.Format(DiscountRequirementDefaults.SETTINGS_KEY, discountRequirementId ?? 0));
            var affiliates = await _affiliateService.GetAllAffiliatesAsync();

            var model = new RequirementModel
            {
                RequirementId = discountRequirementId ?? 0,
                DiscountId = discountId,
                AffiliateId = affiliateId
            };

            foreach (var affiliate in affiliates)
            {
                var address = await _addressService.GetAddressByIdAsync(affiliate.AddressId);
                var name = affiliate.Id.ToString("0000");
                if (address != null)
                    name = $"{address.FirstName} {address.LastName} - ({address.Email})";

                model.AvailableAffiliates.Add(new SelectListItem
                {
                    Text = name,
                    Value = affiliate.Id.ToString()
                });
            }

            //add a prefix
            ViewData.TemplateInfo.HtmlFieldPrefix = string.Format(DiscountRequirementDefaults.HTML_FIELD_PREFIX, discountRequirementId ?? 0);

            return View("~/Plugins/NopStation.Plugin.DiscountRules.AffiliatedCustomers/Views/Configure.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> Configure(RequirementModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageDiscounts))
                return Content("Access denied");

            if (ModelState.IsValid)
            {
                //load the discount
                var discount = await _discountService.GetDiscountByIdAsync(model.DiscountId);
                if (discount == null)
                    return NotFound(new { Errors = new[] { "Discount could not be loaded" } });

                //get the discount requirement
                var discountRequirement = await _discountService.GetDiscountRequirementByIdAsync(model.RequirementId);

                //the discount requirement does not exist, so create a new one
                if (discountRequirement == null)
                {
                    discountRequirement = new DiscountRequirement
                    {
                        DiscountId = discount.Id,
                        DiscountRequirementRuleSystemName = DiscountRequirementDefaults.SYSTEM_NAME
                    };

                    await _discountService.InsertDiscountRequirementAsync(discountRequirement);
                }

                //save restricted customer role identifier
                await _settingService.SetSettingAsync(string.Format(DiscountRequirementDefaults.SETTINGS_KEY, discountRequirement.Id), model.AffiliateId);

                return Ok(new { NewRequirementId = discountRequirement.Id });
            }

            return BadRequest(new { Errors = GetErrorsFromModelState() });
        }

        #region Utilities

        private IEnumerable<string> GetErrorsFromModelState()
        {
            return ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage));
        }

        #endregion
    }
}