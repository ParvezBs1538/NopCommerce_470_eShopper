using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Tax;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Orders;
using Nop.Services.Plugins;
using Nop.Services.Tax;
using NopStation.Plugin.Misc.Core.Services;

namespace NopStation.Plugin.DiscountRules.OrderRange
{
    public class OrderRangeDiscountRequirementRule : BasePlugin, IDiscountRequirementRule, INopStationPlugin
    {
        #region Field

        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly TaxSettings _taxSettings;
        private readonly IWebHelper _webHelper;
        private readonly IDiscountService _discountService;
        private readonly ISettingService _settingService;
        private readonly ICustomerService _customerService;
        private readonly IProductService _productService;
        private readonly ITaxService _taxService;

        #endregion

        #region Ctor

        public OrderRangeDiscountRequirementRule(IShoppingCartService shoppingCartService,
            IStoreContext storeContext,
            IWorkContext workContext,
            TaxSettings taxSettings,
            IActionContextAccessor actionContextAccessor,
            IUrlHelperFactory urlHelperFactory,
            IWebHelper webHelper,
            IDiscountService discountService,
            ISettingService settingService,
            ICustomerService customerService,
            IProductService productService,
            ITaxService taxService)
        {
            _shoppingCartService = shoppingCartService;
            _storeContext = storeContext;
            _workContext = workContext;
            _taxSettings = taxSettings;
            _actionContextAccessor = actionContextAccessor;
            _urlHelperFactory = urlHelperFactory;
            _webHelper = webHelper;
            _discountService = discountService;
            _settingService = settingService;
            _customerService = customerService;
            _productService = productService;
            _taxService = taxService;
        }

        #endregion

        #region Method

        public async Task<DiscountRequirementValidationResult> CheckRequirementAsync(DiscountRequirementValidationRequest request)
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, store.Id);
            var result = new DiscountRequirementValidationResult()
            {
                IsValid = false
            };

            var conditionvalue = await _settingService.GetSettingByKeyAsync<string>(string.Format(DiscountRequirementDefaults.ConditionValueSettingsKey, request.DiscountRequirementId));
            var rangeValue = await _settingService.GetSettingByKeyAsync<int>(string.Format(DiscountRequirementDefaults.RangeValueSettingsKey, request.DiscountRequirementId));
            if (conditionvalue == null || conditionvalue == "0")
                return result;
            if (rangeValue == 0)
                return result;

            if (cart.Any())
            {
                var taxRates = new SortedDictionary<decimal, decimal>();
                var customer = await _customerService.GetShoppingCartCustomerAsync(cart);
                var subTotalExclTaxWithoutDiscount = decimal.Zero;
                var subTotalInclTaxWithoutDiscount = decimal.Zero;
                foreach (var shoppingCartItem in cart)
                {
                    var sciSubTotal = (await _shoppingCartService.GetSubTotalAsync(shoppingCartItem, true)).subTotal;
                    var product = await _productService.GetProductByIdAsync(shoppingCartItem.ProductId);
                    var (sciExclTax, taxRate) = await _taxService.GetProductPriceAsync(product, sciSubTotal, false, customer);
                    var (sciInclTax, _) = await _taxService.GetProductPriceAsync(product, sciSubTotal, true, customer);
                    subTotalExclTaxWithoutDiscount += sciExclTax;
                    subTotalInclTaxWithoutDiscount += sciInclTax;
                    var sciTax = sciInclTax - sciExclTax;
                    if (taxRate <= decimal.Zero || sciTax <= decimal.Zero)
                        continue;
                    if (!taxRates.ContainsKey(taxRate))
                    {
                        taxRates.Add(taxRate, sciTax);
                    }
                    else
                    {
                        taxRates[taxRate] = taxRates[taxRate] + sciTax;
                    }
                }
                var subTotalIncludingTax = await _workContext.GetTaxDisplayTypeAsync() == TaxDisplayType.IncludingTax && !_taxSettings.ForceTaxExclusionFromOrderSubtotal;
                var subtotalBase = subTotalExclTaxWithoutDiscount;
                if (subTotalIncludingTax)
                    subtotalBase = subTotalInclTaxWithoutDiscount;

                if ((conditionvalue == "G" && (subtotalBase > rangeValue)) || (conditionvalue == "L" && (subtotalBase < rangeValue)) || (conditionvalue == "E" && (subtotalBase == rangeValue)))
                {
                    result.IsValid = true;
                }
            }
            return result;
        }

        public string GetConfigurationUrl(int discountId, int? discountRequirementId)
        {
            var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);

            return urlHelper.Action("Configure", "DiscountRulesOrderRange",
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
                new KeyValuePair<string, string>("Admin.NopStation.DiscountRules.OrderRange.Fields.ConditionValue", "Condition"),
                new KeyValuePair<string, string>("Admin.NopStation.DiscountRules.OrderRange.Fields.ConditionValue.Hint", "Selecte a condition."),
                new KeyValuePair<string, string>("Admin.NopStation.DiscountRules.OrderRange.Fields.ConditionValue.Required", "Condition is required."),
                new KeyValuePair<string, string>("Admin.NopStation.DiscountRules.OrderRange.Fields.RangeValue", "Range value($)"),
                new KeyValuePair<string, string>("Admin.NopStation.DiscountRules.OrderRange.Fields.RangeValue.Hint", "Give a Range Value."),
                new KeyValuePair<string, string>("Admin.NopStation.DiscountRules.OrderRange.Fields.RangeValue.Range", "Range Vlaue should be between 1 to 1 Billion."),

                new KeyValuePair<string, string>("NopStation.DiscountRules.OrderRange.InvalidForOrderRange", "Sorry, this offer is not valid for your selected Order Range.")
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

        #endregion
    }
}
