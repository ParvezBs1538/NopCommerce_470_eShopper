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

namespace NopStation.Plugin.DiscountRules.TimeOfDay
{
    public partial class TimeOfDayDiscountRequirementRule : BasePlugin, IDiscountRequirementRule, INopStationPlugin
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

        public TimeOfDayDiscountRequirementRule(IActionContextAccessor actionContextAccessor,
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

            var timeFrom = await _settingService.GetSettingByKeyAsync<DateTime>(
                string.Format(DiscountRequirementDefaults.SETTINGS_KEY_FROM, request.DiscountRequirementId));

            var timeTo = await _settingService.GetSettingByKeyAsync<DateTime>(
                string.Format(DiscountRequirementDefaults.SETTINGS_KEY_TO, request.DiscountRequirementId));

            if (timeFrom == DateTime.MinValue || timeTo == DateTime.MinValue)
            {
                result.IsValid = false;
                return result;
            }

            if (request.Customer == null)
            {
                result.IsValid = false;
                return result;
            }

            if (DateTime.Today.TimeOfDay >= timeFrom.TimeOfDay && DateTime.Today.TimeOfDay <= timeTo.TimeOfDay)
                result.IsValid = true;
            else
                result.UserError = await _localizationService.GetResourceAsync("NopStation.DiscountRules.TimeOfDay.InvalidForTimeOfDay");

            return result;
        }

        public string GetConfigurationUrl(int discountId, int? discountRequirementId)
        {
            var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);

            return urlHelper.Action("Configure", "DiscountRulesTimeOfDay",
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

            list.Add(new KeyValuePair<string, string>("Admin.NopStation.DiscountRules.TimeOfDay.Fields.TimeOfDayFrom", "From"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.DiscountRules.TimeOfDay.Fields.TimeOfDayFrom.Hint", "Discount will be applied from."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.DiscountRules.TimeOfDay.Fields.TimeOfDayTo", "To"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.DiscountRules.TimeOfDay.Fields.TimeOfDayTo.Hint", "Discount will be applied to."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.DiscountRules.TimeOfDay.Fields.TimeOfDayTo.Invalid", "Invalid time range"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.DiscountRules.TimeOfDay.Fields.DiscountId.Required", "Discount is required"));

            list.Add(new KeyValuePair<string, string>("NopStation.DiscountRules.TimeOfDay.InvalidForTimeOfDay", "Sorry, this offer is not valid for this moment."));

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