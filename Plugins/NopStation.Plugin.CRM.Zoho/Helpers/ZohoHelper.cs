using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Tax;
using NopStation.Plugin.CRM.Zoho.Domain.Zoho;
using NopStation.Plugin.CRM.Zoho.Services;
using NopStation.Plugin.CRM.Zoho.Services.Zoho.Request;
using NopStation.Plugin.Misc.Core.Helpers;

namespace NopStation.Plugin.CRM.Zoho.Helpers
{
    public static class ZohoHelper
    {
        public static Task<AccountRequest> ToZohoAccountAsync(this ZohoStore store)
        {
            var accountRequest = new AccountRequest
            {
                NopEntityId = store.Id.ToString(),
                ZohoId = store.ZohoId,
                Approved = true,
                AccountName = store.Name,
                Phone = store.PhoneNumber,
                Website = IsValidUrl(store.Url) ? store.Url : null
            };

            return Task.FromResult(accountRequest);
        }

        public static async Task<VendorRequest> ToZohoVendorAsync(this ZohoVendor vendor)
        {
            var addressService = NopInstance.Load<IAddressService>();
            var address = await addressService.GetAddressByIdAsync(vendor.AddressId);

            var vendorRequest = new VendorRequest
            {
                NopEntityId = vendor.Id.ToString(),
                City = address?.City,
                Email = vendor.Email,
                VendorName = vendor.Name,
                Phone = address?.PhoneNumber,
                Street = address?.Address1,
                ZipCode = address?.ZipPostalCode,
                ZohoId = vendor.ZohoId,
                Approved = vendor.Active
            };

            if (address != null)
            {
                var countryService = NopInstance.Load<ICountryService>();
                var stateProvinceService = NopInstance.Load<IStateProvinceService>();

                if (address.CountryId.HasValue)
                    vendorRequest.Country = (await countryService.GetCountryByIdAsync(address.CountryId.Value)).Name;
                if (address.StateProvinceId.HasValue)
                    vendorRequest.State = (await stateProvinceService.GetStateProvinceByIdAsync(address.StateProvinceId.Value)).Name;
            }

            return vendorRequest;
        }

        public static async Task<ContactRequest> ToZohoContactAsync(this ZohoCustomer customer)
        {
            var customerService = NopInstance.Load<ICustomerService>();
            var contactRequest = new ContactRequest
            {
                NopEntityId = customer.Id.ToString(),
                FirstName = (await customerService.GetCustomerByIdAsync(customer.Id)).FirstName ?? "Guest",
                LastName = (await customerService.GetCustomerByIdAsync(customer.Id)).LastName ?? $"User {customer.Id}",
                Email = customer.Email ?? customer.Id.ToString("00000000000") + "@yourStore.com",
                Phone = (await customerService.GetCustomerByIdAsync(customer.Id)).Phone ?? "00000000000",
                ZohoId = customer.ZohoId
            };

            var dateOfBirth = (await customerService.GetCustomerByIdAsync(customer.Id)).DateOfBirth;
            if (dateOfBirth.HasValue && dateOfBirth != DateTime.MinValue)
                contactRequest.DateOfBirth = dateOfBirth.Value.ToZohoDate();

            var mappingService = NopInstance.Load<IMappingService>();
            if (await mappingService.GetZohoVendorAsync(customer.VendorId) is ZohoVendor zohoVendor)
                contactRequest.Vendor = await ToZohoVendorAsync(zohoVendor);

            return contactRequest;
        }

        public static async Task<ProductRequest> ToZohoProductAsync(this ZohoProduct product)
        {
            var productRequest = new ProductRequest
            {
                NopEntityId = product.Id.ToString(),
                ZohoId = product.ZohoId,
                ProductActive = product.Published,
                Description = product.Description,
                ProductCode = product.Sku,
                ProductName = product.Name,
                QtyInStock = product.StockQuantity,
                SalesStartDate = product.AvailableStartDate.HasValue ?
                    product.AvailableStartDate.Value.ToZohoDate() : null,
                SalesEndDate = product.AvailableEndDate.HasValue ?
                    product.AvailableEndDate.Value.ToZohoDate() : null,
                UnitPrice = product.Price,
                Taxable = !product.IsTaxExempt
            };

            var mappingService = NopInstance.Load<IMappingService>();
            if (await mappingService.GetZohoVendorAsync(product.VendorId) is ZohoVendor zohoVendor)
                productRequest.Vendor = await ToZohoVendorAsync(zohoVendor);

            var pictureService = NopInstance.Load<IPictureService>();
            var picture = (await pictureService.GetPicturesByProductIdAsync(product.Id, 1)).FirstOrDefault();
            productRequest.RecordImage = (await pictureService.GetPictureUrlAsync(picture, 500)).Url;

            var manufacturerService = NopInstance.Load<IManufacturerService>();
            if ((await manufacturerService.GetProductManufacturersByProductIdAsync(product.Id)).FirstOrDefault() is ProductManufacturer pm)
                productRequest.Manufacturer = (await manufacturerService.GetManufacturerByIdAsync(pm.ManufacturerId)).Name;

            var categoryService = NopInstance.Load<ICategoryService>();
            if ((await categoryService.GetProductCategoriesByProductIdAsync(product.Id)).FirstOrDefault() is ProductCategory pc)
                productRequest.ProductCategory = await categoryService.GetFormattedBreadCrumbAsync(await categoryService.GetCategoryByIdAsync(pc.CategoryId));

            var productTagService = NopInstance.Load<IProductTagService>();
            var productsTags = await productTagService.GetAllProductTagsByProductIdAsync(product.Id);
            productRequest.Tags = productsTags.Select(tag => tag.Name).ToList();

            if (product.TaxCategoryId > 0)
            {
                var taxCategoryService = NopInstance.Load<ITaxCategoryService>();
                var taxCategory = await taxCategoryService.GetTaxCategoryByIdAsync(product.TaxCategoryId);
                productRequest.Taxes = new List<string> { ToNonAlphaNumeric(taxCategory.Name) };
            }

            return productRequest;
        }

        public static async Task<SalesOrderRequest> ToZohoSalesOrderAsync(this ZohoOrder order)
        {
            var mappingService = NopInstance.Load<IMappingService>();
            var addressService = NopInstance.Load<IAddressService>();
            var billingAddress = await addressService.GetAddressByIdAsync(order.BillingAddressId);
            var shippingAddress = await addressService.GetAddressByIdAsync(order.ShippingAddressId ?? 0);

            var salesOrderRequest = new SalesOrderRequest
            {
                NopEntityId = order.Id.ToString(),
                ZohoId = order.ZohoId,
                CurrencySymbol = order.CustomerCurrencyCode,
                Account = await ToZohoAccountAsync(await mappingService.GetZohoStoreAsync(order.StoreId)),
                Contact = await ToZohoContactAsync(await mappingService.GetZohoCustomerAsync(order.CustomerId)),
                CustomerNo = order.CustomerId.ToString(),
                Tax = order.OrderTax,
                Subject = "Order #" + order.Id.ToString("0000000"),
                SubTotal = order.OrderSubtotalExclTax,
                Status = order.OrderStatus.ToString(),
                GrandTotal = order.OrderTotal,
                Carrier = order.ShippingMethod,
                Currency = order.CustomerCurrencyCode,
                Discount = order.OrderDiscount,
                ExchangeRate = order.CurrencyRate,
                TermsAndConditions = "",
                BillingCity = billingAddress.City,
                BillingCode = billingAddress.ZipPostalCode,
                BillingStreet = billingAddress.Address1,
                ShippingCity = shippingAddress?.City,
                ShippingCode = shippingAddress?.ZipPostalCode,
                ShippingStreet = shippingAddress?.Address1
            };

            var countryService = NopInstance.Load<ICountryService>();
            var stateProvinceService = NopInstance.Load<IStateProvinceService>();

            if (billingAddress.CountryId.HasValue)
                salesOrderRequest.BillingCountry = (await countryService.GetCountryByIdAsync(billingAddress.CountryId.Value)).Name;
            if (billingAddress.StateProvinceId.HasValue)
                salesOrderRequest.BillingState = (await stateProvinceService.GetStateProvinceByIdAsync(billingAddress.StateProvinceId.Value)).Name;

            if (shippingAddress != null)
            {
                if (shippingAddress.CountryId.HasValue)
                    salesOrderRequest.ShippingCountry = (await countryService.GetCountryByIdAsync(shippingAddress.CountryId.Value)).Name;
                if (shippingAddress.StateProvinceId.HasValue)
                    salesOrderRequest.ShippingState = (await stateProvinceService.GetStateProvinceByIdAsync(shippingAddress.StateProvinceId.Value)).Name;
            }

            var orderService = NopInstance.Load<IOrderService>();
            var orderItems = await orderService.GetOrderItemsAsync(order.Id);

            foreach (var orderItem in orderItems)
                salesOrderRequest.Products.Add(await ToZohoOrderItemAsync(orderItem, mappingService));

            return salesOrderRequest;
        }

        public static async Task<Dictionary<string, object>> ToZohoShipmentAsync(this ZohoShipment shipment,
            ZohoCRMShipmentSettings shipmentSettings)
        {
            var mappingService = NopInstance.Load<IMappingService>();
            var maps = new Dictionary<string, object>();
            maps.Add("Name", "Shipment #" + shipment.Id.ToString("0000000"));
            maps.Add("NopEntityId", shipment.Id.ToString());
            maps.Add("id", shipment.ZohoId);

            if (!string.IsNullOrWhiteSpace(shipmentSettings.OrderField) &&
                !maps.ContainsKey(shipmentSettings.OrderField))
                maps.Add(shipmentSettings.OrderField,
                    await ToZohoSalesOrderAsync(await mappingService.GetZohoOrderAsync(shipment.OrderId)));

            if (!string.IsNullOrWhiteSpace(shipmentSettings.TrackingNumberField) && !
                maps.ContainsKey(shipmentSettings.TrackingNumberField))
                maps.Add(shipmentSettings.TrackingNumberField, shipment.TrackingNumber);

            if (!string.IsNullOrWhiteSpace(shipmentSettings.WeightField) &&
                !maps.ContainsKey(shipmentSettings.WeightField))
                maps.Add(shipmentSettings.WeightField, shipment.TotalWeight);

            if (!string.IsNullOrWhiteSpace(shipmentSettings.DeliveryDateUtcField) &&
                !maps.ContainsKey(shipmentSettings.DeliveryDateUtcField))
                maps.Add(shipmentSettings.DeliveryDateUtcField,
                    !shipment.DeliveryDateUtc.HasValue ? null : shipment.DeliveryDateUtc.Value.ToZohoDateTime());

            if (!string.IsNullOrWhiteSpace(shipmentSettings.ShippedDateUtcField) &&
                !maps.ContainsKey(shipmentSettings.ShippedDateUtcField))
                maps.Add(shipmentSettings.ShippedDateUtcField,
                    !shipment.ShippedDateUtc.HasValue ? null : shipment.ShippedDateUtc.Value.ToZohoDateTime());

            return maps;
        }

        public static async Task<Dictionary<string, object>> ToZohoShipmentItemAsync(this ZohoShipmentItem shipmentItem,
            ZohoCRMShipmentSettings shipmentSettings)
        {
            var mappingService = NopInstance.Load<IMappingService>();
            var maps = new Dictionary<string, object>();
            maps.Add("Name", "Shipment Item #" + shipmentItem.Id.ToString("0000000"));
            maps.Add("NopEntityId", shipmentItem.Id.ToString());
            maps.Add("id", shipmentItem.ZohoId);

            if (!string.IsNullOrWhiteSpace(shipmentSettings.ItemProductField) && !maps.ContainsKey(shipmentSettings.ItemProductField))
                maps.Add(shipmentSettings.ItemProductField,
                    await ToZohoProductAsync(await mappingService.GetZohoProductAsync(shipmentItem.ProductId)));

            if (!string.IsNullOrWhiteSpace(shipmentSettings.ItemShipmentField) && !maps.ContainsKey(shipmentSettings.ItemShipmentField))
                maps.Add(shipmentSettings.ItemShipmentField,
                    await ToZohoShipmentAsync(await mappingService.GetZohoShipmentAsync(shipmentItem.ShipmentId), shipmentSettings));

            if (!string.IsNullOrWhiteSpace(shipmentSettings.ItemQuantityField) && !maps.ContainsKey(shipmentSettings.ItemQuantityField))
                maps.Add(shipmentSettings.ItemQuantityField, shipmentItem.Quantity);

            return maps;
        }

        public static string ToZohoDate(this DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd");
        }

        public static string ToZohoDateTime(this DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-ddThh:mm:ss");
        }

        private static async Task<OrderProductRequest> ToZohoOrderItemAsync(OrderItem orderItem, IMappingService mappingService)
        {
            var product = await mappingService.GetZohoProductAsync(orderItem.ProductId);

            var orderProductRequest = new OrderProductRequest
            {
                Discount = orderItem.DiscountAmountExclTax,
                TotalAfterDiscount = orderItem.PriceExclTax,
                UnitPrice = orderItem.UnitPriceExclTax,
                Quantity = orderItem.Quantity,
                Product = await ToZohoProductAsync(product),
                Tax = orderItem.PriceInclTax - orderItem.PriceExclTax,
                Total = orderItem.PriceInclTax,
                NetTotal = orderItem.PriceInclTax,
                ListPrice = product.Price,
                ProductDescription = product.Description,
                QuantityInStock = product.StockQuantity
            };

            if (product.TaxCategoryId > 0)
            {
                var taxCategoryService = NopInstance.Load<ITaxCategoryService>();
                var taxCategory = await taxCategoryService.GetTaxCategoryByIdAsync(product.TaxCategoryId);

                orderProductRequest.Taxes = new List<OrderTaxRequest>{
                    new OrderTaxRequest()
                    {
                        Value = orderProductRequest.Tax,
                        Percentage = orderProductRequest.Tax / orderItem.PriceExclTax * 100,
                        Name = taxCategory.Name
                    }
                };
            }

            return orderProductRequest;
        }

        private static string ToNonAlphaNumeric(string strValue)
        {
            var rgx = new Regex("[^a-zA-Z0-9 -]");
            return rgx.Replace(strValue, "");
        }

        private static bool IsValidUrl(string url)
        {
            return Regex.IsMatch(url, @"^http:\/\/\w+(\.\w+)*(:[0-9]+)?\/?(\/[.\w]*)*$", RegexOptions.IgnoreCase);
        }
    }
}
