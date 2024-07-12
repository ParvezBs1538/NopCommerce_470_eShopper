using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Services.Affiliates;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using NopStation.Plugin.Misc.Core.Services;

namespace NopStation.Plugin.DiscountRules.DaysOfWeek
{
    public partial class DaysOfWeekDiscountRequirementRule : BasePlugin, IDiscountRequirementRule, INopStationPlugin
    {
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly ICustomerService _customerService;
        private readonly IDiscountService _discountService;
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IWebHelper _webHelper;
        private readonly IAffiliateService _affiliateService;
        private readonly IPaymentPluginManager _paymentPluginManager;
        private readonly IGenericAttributeService _genericAttributeService;

        public DaysOfWeekDiscountRequirementRule(IActionContextAccessor actionContextAccessor,
            ICustomerService customerService,
            IDiscountService discountService,
            ILocalizationService localizationService,
            ISettingService settingService,
            IUrlHelperFactory urlHelperFactory,
            IWebHelper webHelper,
            IAffiliateService affiliateService,
            IPaymentPluginManager paymentPluginManager,
            IGenericAttributeService genericAttributeService)
        {
            _actionContextAccessor = actionContextAccessor;
            _customerService = customerService;
            _discountService = discountService;
            _localizationService = localizationService;
            _settingService = settingService;
            _urlHelperFactory = urlHelperFactory;
            _webHelper = webHelper;
            _affiliateService = affiliateService;
            _paymentPluginManager = paymentPluginManager;
            _genericAttributeService = genericAttributeService;
        }

        public async Task<DiscountRequirementValidationResult> CheckRequirementAsync(DiscountRequirementValidationRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var result = new DiscountRequirementValidationResult();

            var daysOfWeek = await _settingService.GetSettingByKeyAsync<List<int>>(
                string.Format(DiscountRequirementDefaults.SETTINGS_KEY, request.DiscountRequirementId));

            if (daysOfWeek == null || !daysOfWeek.Any())
            {
                result.IsValid = false;
                return result;
            }

            if (request.Customer == null)
            {
                result.IsValid = false;
                return result;
            }

            if (daysOfWeek.Contains((int)DateTime.UtcNow.DayOfWeek + 1))
                result.IsValid = true;
            else
                result.UserError = await _localizationService.GetResourceAsync("NopStation.DiscountRules.DaysOfWeek.InvalidForDaysOfWeek");

            return result;
        }

        public string GetConfigurationUrl(int discountId, int? discountRequirementId)
        {
            var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);

            return urlHelper.Action("Configure", "DiscountRulesDaysOfWeek",
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

            list.Add(new KeyValuePair<string, string>("Admin.NopStation.DiscountRules.DaysOfWeek.Fields.DaysOfWeek", "Days of week"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.DiscountRules.DaysOfWeek.Fields.DaysOfWeek.Hint", "Discount will be applied on specific days of week."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.DiscountRules.DaysOfWeek.Fields.DaysOfWeek.Required", "Select any days of week"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.DiscountRules.DaysOfWeek.Fields.DiscountId.Required", "Discount is required"));

            list.Add(new KeyValuePair<string, string>("NopStation.DiscountRules.DaysOfWeek.InvalidForDaysOfWeek", "Sorry, this offer is not valid for today."));

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