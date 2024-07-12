using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Core;
using Nop.Services.Discounts;
using Nop.Services.Plugins;
using NopStation.Plugin.Misc.Core.Services;

namespace NopStation.Plugin.DiscountRules.CustomerBirthday
{
    public class CustomerBirthdayDiscountRequirementRule : BasePlugin, IDiscountRequirementRule, INopStationPlugin
    {
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IWorkContext _workContext;
        private readonly IWebHelper _webHelper;
        private readonly IDiscountService _discountService;

        public CustomerBirthdayDiscountRequirementRule(IWorkContext workContext,
             IActionContextAccessor actionContextAccessor,
             IUrlHelperFactory urlHelperFactory,
             IWebHelper webHelper,
            IDiscountService discountService)
        {
            _workContext = workContext;
            _actionContextAccessor = actionContextAccessor;
            _urlHelperFactory = urlHelperFactory;
            _webHelper = webHelper;
            _discountService = discountService;
        }

        public async Task<DiscountRequirementValidationResult> CheckRequirementAsync(DiscountRequirementValidationRequest request)
        {
            var result = new DiscountRequirementValidationResult()
            {
                IsValid = false
            };
            var customer = await _workContext.GetCurrentCustomerAsync();
            var dateOfBirth = customer.DateOfBirth;
            if (dateOfBirth == null)
            {
                return result;
            }
            var currentdate = DateTime.Now;
            if (currentdate.Day == dateOfBirth.Value.Day && currentdate.Month == dateOfBirth.Value.Month)
            {
                result.IsValid = true;
            }

            return result;
        }

        public string GetConfigurationUrl(int discountId, int? discountRequirementId)
        {
            var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);

            return urlHelper.Action("Configure", "DiscountRulesCustomerBirthday",
                new { discountId = discountId, discountRequirementId = discountRequirementId }, _webHelper.GetCurrentRequestProtocol());
        }

        public override async Task InstallAsync()
        {
            await this.InstallPluginAsync();
            await base.InstallAsync();
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            var list = new List<KeyValuePair<string, string>>();
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
