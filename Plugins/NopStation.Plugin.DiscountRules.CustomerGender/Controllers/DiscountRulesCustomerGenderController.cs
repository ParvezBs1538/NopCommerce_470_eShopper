using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Discounts;
using Nop.Services.Configuration;
using Nop.Services.Discounts;
using Nop.Services.Security;
using NopStation.Plugin.DiscountRules.CustomerGender.Models;
using NopStation.Plugin.Misc.Core.Controllers;

namespace NopStation.Plugin.DiscountRules.CustomerGender.Controllers
{
    public class DiscountRulesCustomerGenderController : NopStationAdminController
    {
        #region Fields

        private readonly IDiscountService _discountService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;

        #endregion

        #region Ctor

        public DiscountRulesCustomerGenderController(IDiscountService discountService,
            IPermissionService permissionService,
            ISettingService settingService)
        {
            _discountService = discountService;
            _permissionService = permissionService;
            _settingService = settingService;
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
            var gender = await _settingService.GetSettingByKeyAsync<string>(string.Format(DiscountRequirementDefaults.CustomerGenderSettingsKey, discountRequirementId ?? 0));

            var model = new RequirementModel
            {
                RequirementId = discountRequirementId ?? 0,
                DiscountId = discountId,
                Gender = gender
            };
            EnumForGenderSelection.Genders[] genders = (EnumForGenderSelection.Genders[])Enum.GetValues(typeof(EnumForGenderSelection.Genders));
            for (int i = 0; i < genders.Length; i++)
            {
                EnumForGenderSelection.Genders genderEnum = genders[i];
                char value = (char)(genderEnum);
                model.AvailableGender.Add(new SelectListItem
                {
                    Text = genderEnum + "",
                    Value = $"{value}",
                    Selected = gender == $"{value}"
                });
            }
            model.AvailableGender.Insert(0, new SelectListItem
            {
                Text = "Please selecte a Gender",
                Value = "0"
            });
            ViewData.TemplateInfo.HtmlFieldPrefix = string.Format(DiscountRequirementDefaults.HtmlFieldPrefix, discountRequirementId ?? 0);
            return View("~/Plugins/NopStation.Plugin.DiscountRules.CustomerGender/Views/Configure.cshtml", model);
        }

        [HttpPost]
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
                await _settingService.SetSettingAsync(string.Format(DiscountRequirementDefaults.CustomerGenderSettingsKey, discountRequirement.Id), model.Gender);
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