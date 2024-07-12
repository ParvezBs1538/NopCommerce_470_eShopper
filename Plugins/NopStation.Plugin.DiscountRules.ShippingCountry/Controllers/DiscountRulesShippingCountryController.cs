using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Discounts;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Security;
using NopStation.Plugin.DiscountRules.ShippingCountry.Models;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;

namespace NopStation.Plugin.DiscountRules.ShippingCountry.Controllers
{
    public class DiscountRulesShippingCountryController : NopStationAdminController
    {
        #region Fields

        private readonly IDiscountService _discountService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly ICountryService _countryService;

        #endregion

        #region Ctor

        public DiscountRulesShippingCountryController(IDiscountService discountService,
            IPermissionService permissionService,
            ISettingService settingService,
            ICountryService countryService)
        {
            _discountService = discountService;
            _permissionService = permissionService;
            _settingService = settingService;
            _countryService = countryService;
        }

        #endregion

        #region Methods
        public async Task<IActionResult> Configure(int discountId, int? discountRequirementId)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageDiscounts))
                return Content("Access denied");
            var discount = await _discountService.GetDiscountByIdAsync(discountId);
            if (discount == null)
                throw new ArgumentException("Discount could not be loaded");

            if (discountRequirementId.HasValue && await _discountService.GetDiscountRequirementByIdAsync(discountRequirementId.Value) is null)
                return Content("Failed to load requirement.");

            var shippingCountry = await _settingService.GetSettingByKeyAsync<string>(string.Format(DiscountRequirementDefaults.ShippingCountrySettingsKey, discountRequirementId ?? 0));
            var allCountries = await _countryService.GetAllCountriesAsync();
            var model = new RequirementModel
            {
                RequirementId = discountRequirementId ?? 0,
                DiscountId = discountId,
                ShippingCountry = shippingCountry,
                DiscountTypeId = discount.DiscountTypeId,
            };
            foreach (var country in allCountries)
            {
                model.AvailableShippingCountry.Add(new SelectListItem()
                {
                    Text = country.Name,
                    Value = country.Name,
                    Selected = country.Name == shippingCountry
                });
                ;
            }

            ViewData.TemplateInfo.HtmlFieldPrefix = string.Format(DiscountRequirementDefaults.HtmlFieldPrefix, discountRequirementId ?? 0);
            return View("~/Plugins/NopStation.Plugin.DiscountRules.ShippingCountry/Views/Configure.cshtml", model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(RequirementModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageDiscounts))
                return Content("Access denied");

            if (ModelState.IsValid)
            {
                var discount = await _discountService.GetDiscountByIdAsync(model.DiscountId);
                if (discount == null)
                    return NotFound(new { Errors = new[] { "Discount could not be loaded" } });
                var discountRequirement = await _discountService.GetDiscountRequirementByIdAsync(model.RequirementId);
                if (discountRequirement == null)
                {
                    discountRequirement = new DiscountRequirement
                    {
                        DiscountId = discount.Id,
                        DiscountRequirementRuleSystemName = DiscountRequirementDefaults.SystemName
                    };
                    await _discountService.InsertDiscountRequirementAsync(discountRequirement);
                }
                await _settingService.SetSettingAsync(string.Format(DiscountRequirementDefaults.ShippingCountrySettingsKey, discountRequirement.Id), model.ShippingCountry);
                return Ok(new { NewRequirementId = discountRequirement.Id });
            }
            return Ok(new { Errors = GetErrorsFromModelState(ModelState) });
        }

        #endregion

        #region Utilities
        private IEnumerable<string> GetErrorsFromModelState(ModelStateDictionary modelState)
        {
            return ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage));
        }

        #endregion
    }
}