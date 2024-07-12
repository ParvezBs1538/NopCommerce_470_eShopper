using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Services.Attributes;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Directory;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Tax;
using NopStation.Plugin.Tax.TaxJar.Domains;
using Taxjar;
using Address = Nop.Core.Domain.Common.Address;
using Customer = Nop.Core.Domain.Customers.Customer;
using Order = Nop.Core.Domain.Orders.Order;

namespace NopStation.Plugin.Tax.TaxJar.Services
{
    public class TaxJarManager
    {
        #region Fields

        private readonly ILogger _logger;
        private readonly ITaxCategoryService _taxCategoryService;
        private readonly ITaxJarCategoryService _taxJarCategoryService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IAddressService _addressService;
        private readonly IProductService _productService;
        private readonly IWorkContext _workContext;
        private readonly TaxJarSettings _taxJarSettings;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IStaticCacheManager _cacheManager;
        private readonly IOrderService _orderService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IPaymentService _paymentService;
        private readonly IAttributeParser<CheckoutAttribute, CheckoutAttributeValue> _checkoutAttributeParser;
        private readonly ShippingSettings _shippingSettings;
        private readonly TaxSettings _taxSettings;
        private readonly TaxjarTransactionLogService _taxjarTransactionLogService;

        #endregion

        #region Ctor

        public TaxJarManager(ILogger logger,
            ITaxCategoryService taxCategoryService,
            ITaxJarCategoryService taxJarCategoryService,
            IShoppingCartService shoppingCartService,
            IOrderTotalCalculationService orderTotalCalculationService,
            IAddressService addressService,
            IProductService productService,
            IWorkContext workContext,
            TaxJarSettings taxJarSettings,
            ICountryService countryService,
            IStateProvinceService stateProvinceService,
            IStaticCacheManager cacheManager,
            IOrderService orderService,
            IGenericAttributeService genericAttributeService,
            IPaymentService paymentService,
            IAttributeParser<CheckoutAttribute, CheckoutAttributeValue> checkoutAttributeParser,
            ShippingSettings shippingSettings,
            TaxSettings taxSettings,
            TaxjarTransactionLogService taxjarTransactionLogService)
        {
            _logger = logger;
            _taxCategoryService = taxCategoryService;
            _taxJarCategoryService = taxJarCategoryService;
            _shoppingCartService = shoppingCartService;
            _orderTotalCalculationService = orderTotalCalculationService;
            _addressService = addressService;
            _productService = productService;
            _workContext = workContext;
            _taxJarSettings = taxJarSettings;
            _countryService = countryService;
            _stateProvinceService = stateProvinceService;
            _cacheManager = cacheManager;
            _orderService = orderService;
            _genericAttributeService = genericAttributeService;
            _paymentService = paymentService;
            _checkoutAttributeParser = checkoutAttributeParser;
            _shippingSettings = shippingSettings;
            _taxSettings = taxSettings;
            _taxjarTransactionLogService = taxjarTransactionLogService;
        }

        #endregion

        #region Utilities

        private async Task<(IEnumerable<LineItem>, decimal, string)> CreateLinesForCheckoutAttributesAsync(string checkoutAttributesXml = null)
        {
            if (string.IsNullOrEmpty(checkoutAttributesXml))
                return (null, decimal.Zero, "");
            string itemCodes = "";
            //get checkout attributes values
            var attributeValues = _checkoutAttributeParser.ParseAttributeValues(checkoutAttributesXml);
            var checkoutAttributesAmount = decimal.Zero;
            var checkoutAttributesListItems = await attributeValues.SelectManyAwait(async attributeWithValues =>
            {
                var attribute = attributeWithValues.attribute;
                return (await attributeWithValues.values.SelectAwait(async value =>
                {
                    if (attribute.IsTaxExempt)
                        return new LineItem();

                    var priceAdjustment = value.PriceAdjustment;
                    checkoutAttributesAmount += priceAdjustment;

                    var itemCode = CommonHelper.EnsureMaximumLength($"{attribute.Name}", 50) + "__" + CommonHelper.EnsureMaximumLength($"({value.Name})", 50);
                    itemCodes += itemCode + "/";
                    //create line
                    var checkoutAttributeItem = new LineItem
                    {
                        id = itemCode,
                        product_identifier = itemCode,
                        //unit_price = value.PriceAdjustment,
                        unit_price = priceAdjustment,

                        //item description
                        description = CommonHelper.EnsureMaximumLength($"{attribute.Name} ({value.Name})", 2096),

                        quantity = 1
                    };

                    //or try to get tax code
                    var attributeTaxCategory = await _taxCategoryService.GetTaxCategoryByIdAsync(attribute.TaxCategoryId);
                    checkoutAttributeItem.product_tax_code = CommonHelper.EnsureMaximumLength(attributeTaxCategory?.Name, 25);

                    return checkoutAttributeItem;
                }).ToListAsync()).ToAsyncEnumerable();
            }).ToListAsync();

            return (checkoutAttributesListItems, checkoutAttributesAmount, itemCodes);
        }

        private async Task<LineItem> CreateLineForShippingAsync(decimal orderShippingExclTax = decimal.Zero, string shippingMethodName = "")
        {
            if (string.IsNullOrEmpty(shippingMethodName))
                shippingMethodName = "";

            var shippingItem = new LineItem
            {
                id = "shipping_method-" + shippingMethodName,
                product_identifier = "shipping_method-" + shippingMethodName,

                //unit_price = orderShippingExclTax,
                unit_price = orderShippingExclTax,

                //item description
                description = "Shipping rate",

                quantity = 1
            };

            //try to get tax code
            var shippingTaxCategory = await _taxCategoryService.GetTaxCategoryByIdAsync(_taxSettings.ShippingTaxClassId);
            shippingItem.product_tax_code = CommonHelper.EnsureMaximumLength(shippingTaxCategory?.Name, 25);

            return shippingItem;
        }

        private async Task<LineItem> CreateLineForPaymentMethodAsync(string paymentMethodSystemName, decimal paymentMethodAdditionalFeeExclTax = decimal.Zero)
        {
            var paymentItem = new LineItem
            {
                id = "payment_method-" + paymentMethodSystemName,

                product_identifier = "payment_method-" + paymentMethodSystemName,

                //unit_price = paymentMethodAdditionalFeeExclTax,
                unit_price = paymentMethodAdditionalFeeExclTax,

                //item description
                description = "Payment method additional fee",

                quantity = 1
            };

            //try to get tax code
            var paymentTaxCategory = await _taxCategoryService.GetTaxCategoryByIdAsync(_taxSettings.PaymentMethodAdditionalFeeTaxClassId);
            paymentItem.product_tax_code = CommonHelper.EnsureMaximumLength(paymentTaxCategory?.Name, 25);

            return paymentItem;
        }

        protected async Task<TaxjarApi> GetTaxJarClientAsync(string token = null, bool? useSandbox = null)
        {
            if (string.IsNullOrEmpty(token))
                token = _taxJarSettings.Token;

            useSandbox = useSandbox ?? _taxJarSettings.UseSandBox;

            var client = new TaxjarApi(token);
            if (useSandbox.Value)
            {
                client = new TaxjarApi(_taxJarSettings.Token, new
                {
                    apiUrl = TaxJarDefaults.TaxjarSandBoxApiUrl
                });
            }

            client.SetApiConfig("headers", new Dictionary<string, string> {
                { TaxJarDefaults.TaxjarApiVersionHeaderKey, GetApiVersion() }
            });

            return await Task.FromResult(client);
        }

        private async Task<TResult> HandleFunctionAsync<TResult>(Func<Task<TResult>> function)
        {
            try
            {
                return await function();
            }
            catch (Exception ex)
            {
                //log errors
                await _logger.ErrorAsync($"TaxJar error: {ex.Message}", ex);

                return default;
            }
        }

        protected string GetApiVersion()
        {
            return TaxJarDefaults.TaxJarApiVersions.ContainsKey(_taxJarSettings.TaxJarApiVersionId)
                ? TaxJarDefaults.TaxJarApiVersions[_taxJarSettings.TaxJarApiVersionId]
                : TaxJarDefaults.TaxJarApiVersions.OrderByDescending(x => x.Key).FirstOrDefault().Value;
        }

        protected async Task<TaxResponseAttributes> TaxForOrderAsync(Address address, List<LineItem> lineItems, decimal amount, decimal shipping,
            string itemCode, Customer customer)
        {
            try
            {
                var toCountry = await _countryService.GetCountryByIdAsync(address.CountryId ?? 0);
                var fromCountry = await _countryService.GetCountryByIdAsync(_taxJarSettings.CountryId);
                var toStateProvince = await _stateProvinceService.GetStateProvinceByIdAsync(address.StateProvinceId ?? 0);
                var fromStateProvince = await _stateProvinceService.GetStateProvinceByIdAsync(_taxJarSettings.StateId);

                var cacheKey = _cacheManager.PrepareKeyForDefaultCache(TaxJarDefaults.TaxTotalCacheKey,
                    customer,
                    address,
                    itemCode,
                    amount,
                    shipping);
                cacheKey.CacheTime = TaxJarDefaults.TaxTotalCacheTime;

                return await _cacheManager.GetAsync(cacheKey, async () =>
                {
                    return await HandleFunctionAsync(async () =>
                    {
                        var client = await GetTaxJarClientAsync();

                        var order = await client.TaxForOrderAsync(new
                        {
                            transaction_id = Guid.NewGuid(),
                            transaction_date = DateTime.UtcNow,
                            from_country = fromCountry?.TwoLetterIsoCode,
                            from_zip = _taxJarSettings.Zip,
                            from_city = _taxJarSettings.City,
                            from_street = _taxJarSettings.Street,
                            from_state = fromStateProvince?.Abbreviation,
                            to_country = toCountry?.TwoLetterIsoCode,
                            to_zip = address.ZipPostalCode,
                            to_city = address.City,
                            to_street = address.Address1,
                            to_state = toStateProvince?.Abbreviation,
                            amount = amount,
                            shipping = shipping,
                            sales_tax = 0,
                            line_items = lineItems,
                            plugin = TaxJarDefaults.ApplicationName
                        });

                        return order;
                    });
                });
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync(ex.Message, ex);
                return new TaxResponseAttributes();
            }
        }

        private async Task PrepareOrderAddressesAsync(Customer customer, Order order, int storeId)
        {
            order.BillingAddressId = customer.BillingAddressId ?? 0;
            order.ShippingAddressId = customer.ShippingAddressId;
            if (_shippingSettings.AllowPickupInStore)
            {
                var pickupPoint = await _genericAttributeService
                    .GetAttributeAsync<PickupPoint>(customer, NopCustomerDefaults.SelectedPickupPointAttribute, storeId);
                if (pickupPoint != null)
                {
                    var country = await _countryService.GetCountryByTwoLetterIsoCodeAsync(pickupPoint.CountryCode);
                    var state = await _stateProvinceService.GetStateProvinceByAbbreviationAsync(pickupPoint.StateAbbreviation, country?.Id);
                    var pickupAddress = new Address
                    {
                        Address1 = pickupPoint.Address,
                        City = pickupPoint.City,
                        CountryId = country?.Id,
                        StateProvinceId = state?.Id,
                        ZipPostalCode = pickupPoint.ZipPostalCode,
                        CreatedOnUtc = DateTime.UtcNow,
                    };
                    await _addressService.InsertAddressAsync(pickupAddress);
                    order.PickupAddressId = pickupAddress.Id;
                }
            }
        }

        private async Task<Address> GetTaxAddressAsync(Order order)
        {
            Address address = null;

            //tax is based on billing address
            if (_taxSettings.TaxBasedOn == TaxBasedOn.BillingAddress &&
                await _addressService.GetAddressByIdAsync(order.BillingAddressId) is Address billingAddress)
            {
                address = billingAddress;
            }

            //tax is based on shipping address
            if (_taxSettings.TaxBasedOn == TaxBasedOn.ShippingAddress &&
                order.ShippingAddressId.HasValue &&
                await _addressService.GetAddressByIdAsync(order.ShippingAddressId.Value) is Address shippingAddress)
            {
                address = shippingAddress;
            }

            //tax is based on pickup point address
            if (_taxSettings.TaxBasedOnPickupPointAddress &&
                order.PickupAddressId.HasValue &&
                await _addressService.GetAddressByIdAsync(order.PickupAddressId.Value) is Address pickupAddress)
            {
                address = pickupAddress;
            }

            //or use default address for tax calculation
            if (address == null)
                address = await _addressService.GetAddressByIdAsync(_taxSettings.DefaultTaxAddressId);

            return address;
        }

        #endregion

        #region TrasactionAPIs

        protected async Task<OrderResponseAttributes> CreateTaxTransactionAsync(Address address, Country toCountry, Country fromCountry,
                StateProvince toStateProvince, StateProvince fromStateProvince, List<LineItem> lineItems, Order order,
                decimal amount = decimal.Zero, decimal shipping = decimal.Zero)
        {
            return await HandleFunctionAsync(async () =>
            {
                var client = await GetTaxJarClientAsync();

                var taxjarOrder = await client.CreateOrderAsync(new
                {
                    transaction_id = order.OrderGuid,
                    transaction_date = DateTime.UtcNow,
                    from_country = fromCountry?.TwoLetterIsoCode,
                    from_zip = _taxJarSettings.Zip,
                    from_city = _taxJarSettings.City,
                    from_street = _taxJarSettings.Street,
                    from_state = fromStateProvince?.Abbreviation,
                    to_country = toCountry?.TwoLetterIsoCode,
                    to_zip = address.ZipPostalCode,
                    to_city = address.City,
                    to_street = address.Address1,
                    to_state = toStateProvince?.Abbreviation,
                    amount = shipping,
                    shipping = order.OrderShippingExclTax,
                    sales_tax = order.OrderTax,
                    line_items = lineItems
                });

                return taxjarOrder;
            });
        }

        protected async Task<OrderResponseAttributes> UpdateTaxTransactionAsync(List<LineItem> lineItems, Nop.Core.Domain.Orders.Order order,
            decimal amount = decimal.Zero, decimal shipping = decimal.Zero)
        {
            return await HandleFunctionAsync(async () =>
            {
                var client = await GetTaxJarClientAsync();

                var taxjarOrder = await client.UpdateOrderAsync(new
                {
                    transaction_id = order.OrderGuid,
                    amount = amount,
                    shipping = shipping,
                    sales_tax = order.OrderTax,
                    line_items = lineItems
                });
                return taxjarOrder;
            });
        }

        protected async Task<OrderResponseAttributes> DeleteTaxTransactionAsync(Nop.Core.Domain.Orders.Order order)
        {
            return await HandleFunctionAsync(async () =>
            {
                var client = await GetTaxJarClientAsync();

                var taxjarOrder = await client.DeleteOrderAsync(order.OrderGuid.ToString());
                return taxjarOrder;
            });
        }

        protected async Task<RefundResponseAttributes> CreateRefundTaxtransactionAsync(Address address, Country toCountry,
                StateProvince toStateProvince, List<LineItem> lineItems, Nop.Core.Domain.Orders.Order order, decimal amount, decimal shipping)
        {
            return await HandleFunctionAsync(async () =>
            {
                var client = await GetTaxJarClientAsync();

                var taxjarOrder = await client.CreateRefundAsync(new
                {
                    transaction_id = Guid.NewGuid(),
                    transaction_reference_id = order.OrderGuid,
                    transaction_date = DateTime.UtcNow,
                    to_country = toCountry?.TwoLetterIsoCode,
                    to_zip = address.ZipPostalCode,
                    to_city = address.City,
                    to_street = address.Address1,
                    to_state = toStateProvince?.Abbreviation,
                    amount = amount,
                    shipping = shipping,
                    sales_tax = 0,
                    line_items = lineItems
                });

                return taxjarOrder;
            });
        }

        protected async Task<RefundResponseAttributes> UpdateRefundTaxTransactionAsync(List<LineItem> lineItems, Nop.Core.Domain.Orders.Order order, string transactionReferenceId, decimal amount, decimal shipping)
        {
            return await HandleFunctionAsync(async () =>
            {
                var client = await GetTaxJarClientAsync();

                var taxjarOrder = await client.UpdateRefundAsync(new
                {
                    transaction_reference_id = transactionReferenceId,
                    transaction_id = order.OrderGuid,
                    amount = amount,
                    shipping = shipping,
                    sales_tax = 0,
                    line_items = lineItems
                });
                return taxjarOrder;
            });
        }

        protected async Task<RefundResponseAttributes> DeleteRefundTaxTransactionAsync(Nop.Core.Domain.Orders.Order order)
        {
            return await HandleFunctionAsync(async () =>
            {
                var client = await GetTaxJarClientAsync();

                var taxjarOrder = await client.DeleteRefundAsync(order.OrderGuid.ToString());
                return taxjarOrder;
            });
        }

        #endregion

        public async Task SyncTaxJarCategoriesAsync(string token, bool? useSanbox)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                    throw new ArgumentNullException(nameof(token));

                if (useSanbox == null)
                    throw new ArgumentNullException(nameof(useSanbox));

                var client = await GetTaxJarClientAsync(token, useSanbox);

                var taxjarCategories = client.Categories();
                var oldCategories = await _taxJarCategoryService.GetTaxJarCategoriesAsync();

                for (var i = 0; i < oldCategories.Count; i++)
                {
                    var category = oldCategories[i];
                    if (!taxjarCategories.Any(x => x.ProductTaxCode == category.TaxCode))
                    {
                        var taxCategory = await _taxCategoryService.GetTaxCategoryByIdAsync(category.TaxCategoryId);
                        await _taxJarCategoryService.DeleteTaxJarCategoryAsync(category);
                        await _taxCategoryService.DeleteTaxCategoryAsync(taxCategory);
                    }
                }

                foreach (var taxjarCategory in taxjarCategories)
                {
                    var category = await _taxJarCategoryService.GetTaxJarCategoryByValueAsync(taxjarCategory.ProductTaxCode);
                    if (category == null)
                    {
                        var taxCategory = new TaxCategory()
                        {
                            Name = taxjarCategory.Name
                        };
                        await _taxCategoryService.InsertTaxCategoryAsync(taxCategory);
                        await _taxJarCategoryService.InsertTaxJarCategoryAsync(new TaxJarCategory
                        {
                            Description = taxjarCategory.Description,
                            TaxCategoryId = taxCategory.Id,
                            TaxCode = taxjarCategory.ProductTaxCode
                        });
                    }
                    else
                    {
                        category.Description = taxjarCategory.Description;
                        await _taxJarCategoryService.UpdateTaxJarCategoryAsync(category);

                        var taxCategory = await _taxCategoryService.GetTaxCategoryByIdAsync(category.TaxCategoryId);
                        taxCategory.Name = taxjarCategory.Name;
                        await _taxCategoryService.UpdateTaxCategoryAsync(taxCategory);
                    }
                }

            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync(ex.Message, ex);
            }
        }

        public async Task<TaxResponseAttributes> GetTaxRateAsync(Address address,
            List<LineItem> lineItems, decimal amount, decimal shipping, string itemCode, Customer customer)
        {
            var transaction = await TaxForOrderAsync(address, lineItems, amount, shipping, itemCode, customer);

            //we return the tax total, since we used the amount of 100 when requesting, so the total is the same as the rate
            return transaction;
        }

        public async Task<TaxTotalResult> CreateTaxTotalTransactionAsync(TaxTotalRequest taxTotalRequest)
        {
            var taxTotalResult = new TaxTotalResult();

            //create dummy order to create tax transaction
            var customer = taxTotalRequest.Customer ?? await _workContext.GetCurrentCustomerAsync();
            var order = new Order { CustomerId = customer.Id };

            //addresses
            await PrepareOrderAddressesAsync(customer, order, taxTotalRequest.StoreId);

            var address = await GetTaxAddressAsync(order);

            var defaultTaxCategory = await _taxJarCategoryService.GetTaxJarCategoryByTaxCategoryIdAsync(_taxJarSettings.DefaultTaxCategoryId);

            var lineItems = new List<LineItem>();
            decimal amount = 0;
            decimal shipping = 0;
            string itemCode = "";
            foreach (var sci in taxTotalRequest.ShoppingCart)
            {
                var product = await _productService.GetProductByIdAsync(sci.ProductId);
                var taxJarCategory = await _taxJarCategoryService.GetTaxJarCategoryByTaxCategoryIdAsync(product.TaxCategoryId) ?? defaultTaxCategory;

                var (subTotal, shoppingCartItemDiscountBase, _, maximumDiscountQty) = await _shoppingCartService.GetSubTotalAsync(sci, true);
                var shoppingCartItemDiscount = shoppingCartItemDiscountBase;
                amount += subTotal;

                (var unitPrice, _, _) = await _shoppingCartService.GetUnitPriceAsync(sci, true);
                var shoppingCartUnitPriceWithDiscount = unitPrice;
                itemCode += product.Sku + "/";
                lineItems.Add(new LineItem
                {
                    id = product.Id.ToString(),
                    discount = shoppingCartItemDiscount,
                    quantity = sci.Quantity,
                    product_identifier = product.Id.ToString(),
                    product_tax_code = taxJarCategory?.TaxCode,
                    unit_price = shoppingCartUnitPriceWithDiscount,
                    description = product.Name,
                    sales_tax = 0
                });
            }

            var shoppingCartShippingBase = await _orderTotalCalculationService.GetShoppingCartShippingTotalAsync(taxTotalRequest.ShoppingCart);
            if (shoppingCartShippingBase.HasValue)
                shipping = shoppingCartShippingBase.Value;

            //checkout attributes
            var checkoutAttributesXml = await _genericAttributeService
                .GetAttributeAsync<string>(customer, NopCustomerDefaults.CheckoutAttributes, taxTotalRequest.StoreId);
            (var checkoutAttributesLineItems, var checkoutAttributesAmount, var checkoutAttributesItemCodes) = await CreateLinesForCheckoutAttributesAsync(checkoutAttributesXml);

            if (checkoutAttributesLineItems != null && checkoutAttributesLineItems.Any())
            {
                lineItems.AddRange(checkoutAttributesLineItems);
                amount += checkoutAttributesAmount;
                itemCode += checkoutAttributesItemCodes + "/";
            }

            //payment method
            if (taxTotalRequest.UsePaymentMethodAdditionalFee && _taxSettings.PaymentMethodAdditionalFeeIsTaxable)
            {
                var paymentMethodSystemName = await _genericAttributeService
                    .GetAttributeAsync<string>(customer, NopCustomerDefaults.SelectedPaymentMethodAttribute, taxTotalRequest.StoreId);
                if (!string.IsNullOrEmpty(paymentMethodSystemName))
                {
                    var paymentMethodAdditionalFeeExclTax = await _paymentService.GetAdditionalHandlingFeeAsync(taxTotalRequest.ShoppingCart, paymentMethodSystemName);
                    var paymentMethodLineItem = await CreateLineForPaymentMethodAsync(paymentMethodSystemName, paymentMethodAdditionalFeeExclTax);
                    if (paymentMethodLineItem != null)
                    {
                        lineItems.Add(paymentMethodLineItem);
                        amount += paymentMethodLineItem.unit_price;
                        itemCode += paymentMethodSystemName + "/";
                    }
                }
            }

            if (!_taxSettings.ShippingIsTaxable)
                shipping = decimal.Zero;

            var rates = await TaxForOrderAsync(address, lineItems, amount, shipping, itemCode, customer);
            if (rates == null || rates.TaxableAmount == 0)
                return new TaxTotalResult { Errors = new List<string> { "No response from the service" } };

            taxTotalResult.TaxTotal = rates.AmountToCollect;
            foreach (var item in rates.Breakdown.LineItems)
            {
                if (taxTotalResult.TaxRates.ContainsKey(item.StateSalesTaxRate))
                    taxTotalResult.TaxRates[item.StateSalesTaxRate] += item.TaxableAmount;
                else
                    taxTotalResult.TaxRates.Add(item.StateSalesTaxRate, item.TaxableAmount);
            }

            return taxTotalResult;
        }

        #region Transactions

        public async Task CreateUpdateOrderTaxTransactionAsync(Order order, bool update = false)
        {
            var address = await GetTaxAddressAsync(order);
            var toCountry = await _countryService.GetCountryByIdAsync(address.CountryId ?? 0);
            var fromCountry = await _countryService.GetCountryByIdAsync(_taxJarSettings.CountryId);
            var toStateProvince = await _stateProvinceService.GetStateProvinceByIdAsync(address.StateProvinceId ?? 0);
            var fromStateProvince = await _stateProvinceService.GetStateProvinceByIdAsync(_taxJarSettings.StateId);
            var shipping = order.OrderShippingExclTax;

            var itemList = await _orderService.GetOrderItemsAsync(order.Id);
            var lineItems = new List<LineItem>();
            var amount = decimal.Zero;

            var defaultTaxCategory = await _taxJarCategoryService.GetTaxJarCategoryByTaxCategoryIdAsync(_taxJarSettings.DefaultTaxCategoryId);
            foreach (var item in itemList)
            {
                var product = await _productService.GetProductByIdAsync(item.ProductId);
                var taxJarCategory = await _taxJarCategoryService.GetTaxJarCategoryByTaxCategoryIdAsync(product.TaxCategoryId) ?? defaultTaxCategory;
                amount += item.PriceExclTax;
                lineItems.Add(new LineItem()
                {
                    id = item.Id.ToString(),
                    discount = item.DiscountAmountExclTax,
                    quantity = item.Quantity,
                    product_identifier = item.Id.ToString(),
                    product_tax_code = taxJarCategory?.TaxCode,
                    unit_price = item.UnitPriceExclTax,
                    description = product.Name,
                    sales_tax = item.UnitPriceExclTax - item.UnitPriceExclTax
                });
            }

            //checkout attributes
            var checkoutAttributesXml = order.CheckoutAttributesXml;
            (var checkoutAttributesLineItems, var checkoutAttributesAmount, _) = await CreateLinesForCheckoutAttributesAsync(checkoutAttributesXml);
            if (checkoutAttributesLineItems != null && checkoutAttributesLineItems.Any())
            {
                lineItems.AddRange(checkoutAttributesLineItems);
                amount += checkoutAttributesAmount;
            }

            //payment method
            if (_taxSettings.PaymentMethodAdditionalFeeIsTaxable)
            {
                var paymentMethodLineItem = await CreateLineForPaymentMethodAsync(order.PaymentMethodSystemName, order.PaymentMethodAdditionalFeeExclTax);
                if (paymentMethodLineItem != null)
                {
                    lineItems.Add(paymentMethodLineItem);
                    amount += paymentMethodLineItem.unit_price;
                }
            }

            if (!_taxSettings.ShippingIsTaxable)
                shipping = decimal.Zero;

            if (update)
            {
                var taxjarTransactionLog = await _taxjarTransactionLogService.GetTaxjarTransactionLogByTransactionIdAsync(order.OrderGuid.ToString());

                if (taxjarTransactionLog == null)
                    return;

                var res = await UpdateTaxTransactionAsync(lineItems, order, amount: amount, shipping);
                if (res == null)
                    return;
                taxjarTransactionLog.Amount = res.Amount;
                taxjarTransactionLog.UpdatedDateUtc = DateTime.UtcNow;
                taxjarTransactionLog.OrderTaxAmount = res.SalesTax;
                await _taxjarTransactionLogService.UpdateTaxjarTransactionLogAsync(taxjarTransactionLog);
            }
            else
            {
                var res = await CreateTaxTransactionAsync(address, toCountry, fromCountry, toStateProvince, fromStateProvince, lineItems, order, amount, shipping);

                if (res == null)
                    return;

                var taxjarTransactionLog = new TaxjarTransactionLog()
                {
                    CustomerId = order.CustomerId,
                    CreatedDateUtc = order.CreatedOnUtc,
                    UpdatedDateUtc = DateTime.UtcNow,
                    Amount = res.Amount,
                    OrderTaxAmount = res.SalesTax,
                    TransactionDate = res.TransactionDate,
                    TransactionId = res.TransactionId,
                    TransactionType = "order",
                    UserId = res.UserId
                };

                await _taxjarTransactionLogService.InsertTaxjarTransactionLogAsync(taxjarTransactionLog);
            }
        }

        public async Task DeleteOrderTaxTransactionAsync(Nop.Core.Domain.Orders.Order order)
        {
            var res = await DeleteTaxTransactionAsync(order);

            if (res == null)
                return;
            var taxjarTransactionLog = await _taxjarTransactionLogService.GetTaxjarTransactionLogByTransactionIdAsync(order.OrderGuid.ToString());

            if (taxjarTransactionLog == null)
                return;

            taxjarTransactionLog.Amount = 0;
            taxjarTransactionLog.UpdatedDateUtc = DateTime.UtcNow;
            await _taxjarTransactionLogService.UpdateTaxjarTransactionLogAsync(taxjarTransactionLog);
        }

        public async Task CreateUpdateRefundTaxTransactionAsync(Order order, decimal amount, bool update = false)
        {
            var address = await GetTaxAddressAsync(order);
            var toCountry = await _countryService.GetCountryByIdAsync(address.CountryId ?? 0);
            var fromCountry = await _countryService.GetCountryByIdAsync(_taxJarSettings.CountryId);
            var toStateProvince = await _stateProvinceService.GetStateProvinceByIdAsync(address.StateProvinceId ?? 0);
            var fromStateProvince = await _stateProvinceService.GetStateProvinceByIdAsync(_taxJarSettings.StateId);
            var shipping = order.OrderShippingExclTax;

            var itemList = await _orderService.GetOrderItemsAsync(order.Id);
            var lineItems = new List<LineItem>();

            if (order.OrderTotal != amount)
            {
                lineItems.Add(new LineItem()
                {
                    id = "1",
                    discount = 0,
                    quantity = 1,
                    product_identifier = "refund",
                    product_tax_code = "",
                    unit_price = amount,
                    description = "Partial Refund"
                });
                shipping = decimal.Zero;
            }
            else
            {
                amount = order.OrderSubtotalExclTax + order.OrderShippingExclTax + order.PaymentMethodAdditionalFeeExclTax;
                var defaultTaxCategory = await _taxJarCategoryService.GetTaxJarCategoryByTaxCategoryIdAsync(_taxJarSettings.DefaultTaxCategoryId);
                foreach (var item in itemList)
                {
                    var product = await _productService.GetProductByIdAsync(item.ProductId);
                    var taxJarCategory = await _taxJarCategoryService.GetTaxJarCategoryByTaxCategoryIdAsync(product.TaxCategoryId) ?? defaultTaxCategory;
                    lineItems.Add(new LineItem()
                    {
                        id = item.Id.ToString(),
                        discount = item.DiscountAmountExclTax,
                        quantity = item.Quantity,
                        product_identifier = item.Id.ToString(),
                        product_tax_code = taxJarCategory?.TaxCode,
                        unit_price = item.UnitPriceExclTax,
                        description = product.Name,
                        sales_tax = item.UnitPriceInclTax - item.UnitPriceExclTax
                    });
                }

                #region Line Item for CheckoutAttributesXml

                //checkout attributes
                var checkoutAttributesXml = order.CheckoutAttributesXml;
                (var checkoutAttributesLineItems, var checkoutAttributesAmount, _) = await CreateLinesForCheckoutAttributesAsync(checkoutAttributesXml);
                if (checkoutAttributesLineItems != null && checkoutAttributesLineItems.Any())
                {
                    lineItems.AddRange(checkoutAttributesLineItems);
                }

                #endregion

                #region Line Item for Payment Method

                //payment method
                if (_taxSettings.PaymentMethodAdditionalFeeIsTaxable)
                {
                    var paymentMethodLineItem = await CreateLineForPaymentMethodAsync(order.PaymentMethodSystemName, order.PaymentMethodAdditionalFeeExclTax);
                    if (paymentMethodLineItem != null)
                    {
                        lineItems.Add(paymentMethodLineItem);
                    }
                }

                #endregion

                #region Line Item for Shipping Method

                if (!_taxSettings.ShippingIsTaxable)
                    shipping = decimal.Zero;

                #endregion
            }

            if (update)
            {
                var taxjarTransactionLog = await _taxjarTransactionLogService.GetTaxjarTransactionLogByTransactionIdAsync(order.OrderGuid.ToString());
                if (taxjarTransactionLog == null)
                    return;

                var res = await UpdateRefundTaxTransactionAsync(lineItems, order, taxjarTransactionLog.TransactionReferanceId, amount, shipping);
                if (res == null)
                    return;

                taxjarTransactionLog.Amount = amount;
                taxjarTransactionLog.UpdatedDateUtc = DateTime.UtcNow;
                taxjarTransactionLog.OrderTaxAmount = res.SalesTax;
                await _taxjarTransactionLogService.UpdateTaxjarTransactionLogAsync(taxjarTransactionLog);
            }
            else
            {
                var res = await CreateRefundTaxtransactionAsync(address, toCountry, toStateProvince, lineItems, order, amount, shipping);
                if (res == null)
                    return;

                var taxjarTransactionLog = new TaxjarTransactionLog()
                {
                    CustomerId = order.CustomerId,
                    CreatedDateUtc = order.CreatedOnUtc,
                    UpdatedDateUtc = DateTime.UtcNow,
                    Amount = amount + shipping,
                    OrderTaxAmount = res.SalesTax,
                    TransactionDate = res.TransactionDate,
                    TransactionId = res.TransactionId,
                    TransactionReferanceId = res.TransactionReferenceId,
                    TransactionType = "refund",
                    UserId = res.UserId,
                };

                await _taxjarTransactionLogService.InsertTaxjarTransactionLogAsync(taxjarTransactionLog);
            }
        }

        public async Task DeleteRefundTransactionAsync(Order order)
        {
            var res = await DeleteRefundTaxTransactionAsync(order);
            var taxjarTransactionLog = await _taxjarTransactionLogService.GetTaxjarTransactionLogByTransactionIdAsync(res.TransactionId);
            if (taxjarTransactionLog == null)
                return;
            taxjarTransactionLog.Amount = 0;
            taxjarTransactionLog.UpdatedDateUtc = DateTime.UtcNow;
            await _taxjarTransactionLogService.UpdateTaxjarTransactionLogAsync(taxjarTransactionLog);
        }

        #endregion
    }
}
