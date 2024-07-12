using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Core;
using Nop.Services.Affiliates;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using NopStation.Plugin.Misc.Core.Services;

namespace NopStation.Plugin.DiscountRules.AffiliatedCustomers
{
    public partial class AffiliatedCustomersDiscountRequirementRule : BasePlugin, IDiscountRequirementRule, INopStationPlugin
    {
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly ICustomerService _customerService;
        private readonly IDiscountService _discountService;
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IWebHelper _webHelper;
        private readonly IAffiliateService _affiliateService;

        public AffiliatedCustomersDiscountRequirementRule(IActionContextAccessor actionContextAccessor,
            ICustomerService customerService,
            IDiscountService discountService,
            ILocalizationService localizationService,
            ISettingService settingService,
            IUrlHelperFactory urlHelperFactory,
            IWebHelper webHelper,
            IAffiliateService affiliateService)
        {
            _actionContextAccessor = actionContextAccessor;
            _customerService = customerService;
            _discountService = discountService;
            _localizationService = localizationService;
            _settingService = settingService;
            _urlHelperFactory = urlHelperFactory;
            _webHelper = webHelper;
            _affiliateService = affiliateService;
        }

        public async Task<DiscountRequirementValidationResult> CheckRequirementAsync(DiscountRequirementValidationRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var result = new DiscountRequirementValidationResult();
            var affiliateId = await _settingService.GetSettingByKeyAsync<int>(string.Format(DiscountRequirementDefaults.SETTINGS_KEY, request.DiscountRequirementId));

            if (affiliateId == decimal.Zero)
            {
                result.IsValid = false;
                return result;
            }

            var affiliate = await _affiliateService.GetAffiliateByIdAsync(affiliateId);
            if(affiliate == null || affiliate.Deleted || !affiliate.Active)
            {
                result.IsValid = false;
                return result;
            }

            if (request.Customer == null)
            {
                result.IsValid = false;
                return result;
            }

            if (request.Customer.AffiliateId == affiliateId)
                result.IsValid = true;
            else
                result.UserError = await _localizationService.GetResourceAsync("NopStation.DiscountRules.AffiliatedCustomers.InvalidForAffiliate");

            return result;
        }

        public string GetConfigurationUrl(int discountId, int? discountRequirementId)
        {
            var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);

            return urlHelper.Action("Configure", "DiscountRulesAffiliatedCustomers",
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

            list.Add(new KeyValuePair<string, string>("Admin.NopStation.DiscountRules.AffiliatedCustomers.Fields.Affiliate", "Affiliate"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.DiscountRules.AffiliatedCustomers.Fields.Affiliate.Hint", "Discount will be applied for specific affiliated customers."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.DiscountRules.AffiliatedCustomers.Fields.Affiliate.Required", "Select a valid affiliate"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.DiscountRules.AffiliatedCustomers.Fields.DiscountId.Required", "Discount is required"));

            list.Add(new KeyValuePair<string, string>("NopStation.DiscountRules.AffiliatedCustomers.InvalidForAffiliate", "Sorry, this offer is not valid for your affiliate."));

            return list;
        }

        public override async Task UninstallAsync()
        {
            var discountRequirements = (await _discountService.GetAllDiscountRequirementsAsync())
                .Where(discountRequirement => discountRequirement.DiscountRequirementRuleSystemName == DiscountRequirementDefaults.SYSTEM_NAME);
            foreach (var discountRequirement in discountRequirements)
            {
                await _discountService.DeleteDiscountRequirementAsync(discountRequirement, false);
            }

            await this.UninstallPluginAsync();
            await base.UninstallAsync();
        }
    }
}