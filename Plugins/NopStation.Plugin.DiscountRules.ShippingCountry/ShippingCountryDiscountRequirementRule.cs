using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Core;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Plugins;
using NopStation.Plugin.Misc.Core.Services;

namespace NopStation.Plugin.DiscountRules.ShippingCountry
{
    public class ShippingCountryDiscountRequirementRule : BasePlugin, IDiscountRequirementRule, INopStationPlugin
    {
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IWorkContext _workContext;
        private readonly IWebHelper _webHelper;
        private readonly IDiscountService _discountService;
        private readonly ISettingService _settingService;
        private readonly IAddressService _addressService;
        private readonly ICountryService _countryService;

        public ShippingCountryDiscountRequirementRule(IWorkContext workContext,
             IActionContextAccessor actionContextAccessor,
             IUrlHelperFactory urlHelperFactory,
             IWebHelper webHelper,
            IDiscountService discountService,
            ISettingService settingService,
            IAddressService addressService,
            ICountryService countryService
            )
        {
            _workContext = workContext;
            _actionContextAccessor = actionContextAccessor;
            _urlHelperFactory = urlHelperFactory;
            _webHelper = webHelper;
            _discountService = discountService;
            _settingService = settingService;
            _addressService = addressService;
            _countryService = countryService;
        }
        public async Task<DiscountRequirementValidationResult> CheckRequirementAsync(DiscountRequirementValidationRequest request)
        {
            var result = new DiscountRequirementValidationResult()
            {
                IsValid = false
            };
            var requirmentShippingCountry = await _settingService.GetSettingByKeyAsync<string>(string.Format(DiscountRequirementDefaults.ShippingCountrySettingsKey,
                request.DiscountRequirementId));

            var custormer = await _workContext.GetCurrentCustomerAsync();
            var shippingAddress = await _addressService.GetAddressByIdAsync(custormer.ShippingAddressId.Value);
            var shippingCountry = await _countryService.GetCountryByAddressAsync(shippingAddress);
            if (shippingCountry != null)
            {
                if (shippingCountry.Name == requirmentShippingCountry)
                { result.IsValid = true; }
            }
            return result;
        }

        public string GetConfigurationUrl(int discountId, int? discountRequirementId)
        {
            var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);

            return urlHelper.Action("Configure", "DiscountRulesShippingCountry",
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
                new KeyValuePair<string, string>("Admin.NopStation.DiscountRules.ShippingCountry.Fields.ShippingCountry", "Shipping Country"),
                new KeyValuePair<string, string>("Admin.NopStation.DiscountRules.ShippingCountry.Fields.ShippingCountry.Hint", "Selecte a shipping country."),
                new KeyValuePair<string, string>("Admin.NopStation.DiscountRules.ShippingCountry.Fields.ShippingCountry.Required", "Shipping country is required."),
                new KeyValuePair<string, string>("Admin.NopStation.DiscountRules.ShippingCountry.Fields.ShippingCountry.Select", "Select country."),
                new KeyValuePair<string, string>("Admin.NopStation.DiscountRules.ShippingCountry.Fields.DiscountTypeId.valid", "this discount can only assingned to shipping."),
                new KeyValuePair<string, string>("NopStation.DiscountRules.ShippingCountry.InvalidForShippingCountry", "Sorry, this offer is not valid for your selected shipping country.")
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
