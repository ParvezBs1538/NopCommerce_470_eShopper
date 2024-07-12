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
using NopStation.Plugin.DiscountRules.OrderRange.Models;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;

namespace NopStation.Plugin.DiscountRules.OrderRange.Controllers
{
    public class DiscountRulesOrderRangeController : NopStationAdminController
    {
        #region Fields

        private readonly IDiscountService _discountService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;

        #endregion

        #region Ctor

        public DiscountRulesOrderRangeController(IDiscountService discountService,
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

            var conditionValue = await _settingService.GetSettingByKeyAsync<string>(string.Format(DiscountRequirementDefaults.ConditionValueSettingsKey, discountRequirementId ?? 0));
            var rangeValue = await _settingService.GetSettingByKeyAsync<int>(string.Format(DiscountRequirementDefaults.RangeValueSettingsKey, discountRequirementId ?? 0));

            var model = new RequirementModel
            {
                RequirementId = discountRequirementId ?? 0,
                DiscountId = discountId,
                ConditionValue = conditionValue,
                RangeValue = rangeValue
            };

            EnumForConditionSelection.Conditons[] conditions = (EnumForConditionSelection.Conditons[])Enum.GetValues(typeof(EnumForConditionSelection.Conditons));
            for (int i = 0; i < conditions.Length; i++)
            {
                EnumForConditionSelection.Conditons condition = conditions[i];
                char value = (char)(condition);
                model.AvailableConditions.Add(new SelectListItem
                {
                    Text = condition + "",
                    Value = $"{value}",
                    Selected = conditionValue == $"{value}"
                });
            }
            model.AvailableConditions.Insert(0, new SelectListItem
            {
                Text = "Please selecte a condition",
                Value = "0"
            });
            ViewData.TemplateInfo.HtmlFieldPrefix = string.Format(DiscountRequirementDefaults.HtmlFieldPrefix, discountRequirementId ?? 0);
            return View("~/Plugins/NopStation.Plugin.DiscountRules.OrderRange/Views/Configure.cshtml", model);
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
                await _settingService.SetSettingAsync(string.Format(DiscountRequirementDefaults.ConditionValueSettingsKey, discountRequirement.Id), model.ConditionValue);
                await _settingService.SetSettingAsync(string.Format(DiscountRequirementDefaults.RangeValueSettingsKey, discountRequirement.Id), model.RangeValue);
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