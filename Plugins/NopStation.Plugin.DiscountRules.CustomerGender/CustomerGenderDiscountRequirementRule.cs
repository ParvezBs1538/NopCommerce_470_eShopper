using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Discounts;
using Nop.Services.Plugins;
using NopStation.Plugin.Misc.Core.Services;

namespace NopStation.Plugin.DiscountRules.CustomerGender
{
    public class CustomerGenderDiscountRequirementRule : BasePlugin, IDiscountRequirementRule, INopStationPlugin
    {
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IWebHelper _webHelper;
        private readonly IDiscountService _discountService;
        private readonly ISettingService _settingService;
        private readonly IWorkContext _workContext;

        public CustomerGenderDiscountRequirementRule(IWorkContext workContext,
             IActionContextAccessor actionContextAccessor,
             IUrlHelperFactory urlHelperFactory,
             IWebHelper webHelper,
            IDiscountService discountService,
            ISettingService settingService)
        {
            _workContext = workContext;
            _actionContextAccessor = actionContextAccessor;
            _urlHelperFactory = urlHelperFactory;
            _webHelper = webHelper;
            _discountService = discountService;
            _settingService = settingService;
        }

        public async Task<DiscountRequirementValidationResult> CheckRequirementAsync(DiscountRequirementValidationRequest request)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var gender = customer.Gender;

            var result = new DiscountRequirementValidationResult()
            {
                IsValid = false
            };
            var decisionValue = await _settingService.GetSettingByKeyAsync<string>(string.Format(DiscountRequirementDefaults.CustomerGenderSettingsKey, request.DiscountRequirementId));
            if (decisionValue == gender)
            {
                result.IsValid = true;
            }
            return result;
        }

        public string GetConfigurationUrl(int discountId, int? discountRequirementId)
        {
            var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);

            return urlHelper.Action("Configure", "DiscountRulesCustomerGender",
                new { discountId = discountId, discountRequirementId = discountRequirementId }, _webHelper.GetCurrentRequestProtocol());
        }

        public override async Task InstallAsync()
        {
            await this.InstallPluginAsync();
            await base.InstallAsync();
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            var list = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Admin.NopStation.DiscountRules.CustomerGender.Fields.Gender", "Gender"),
                new KeyValuePair<string, string>("Admin.NopStation.DiscountRules.CustomerGender.Fields.DaysOfWeek.Hint", "Discount will be applied on specific gender."),
                new KeyValuePair<string, string>("Admin.NopStation.DiscountRules.CustomerGender.Fields.Gender.Required", "Select Gender"),
                new KeyValuePair<string, string>("NopStation.DiscountRules.DaysOfWeek.InvalidForDaysOfWeek", "Sorry, this offer is not valid for today.")
            };
            return list;
        }

        public override async Task UninstallAsync()
        {
            var discountRequirements = (await _discountService.GetAllDiscountRequirementsAsync())
                .Where(discountRequirement => discountRequirement.DiscountRequirementRuleSystemName == DiscountRequirementDefaults.SystemName);
            foreach (var discountRequirement in discountRequirements)
            {
                await _discountService.DeleteDiscountRequirementAsync(discountRequirement, false);
            }
            await this.UninstallPluginAsync();
            await base.UninstallAsync();
        }
    }
}
