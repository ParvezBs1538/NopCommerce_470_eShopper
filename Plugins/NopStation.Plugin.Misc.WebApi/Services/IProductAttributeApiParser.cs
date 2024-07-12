using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;

namespace NopStation.Plugin.Misc.WebApi.Services
{
    public interface IProductAttributeApiParser
    {
        Task<decimal> ParseCustomerEnteredPriceAsync(Product product, NameValueCollection form);

        int ParseEnteredQuantity(Product product, NameValueCollection form);

        Task<string> ParseProductAttributesAsync(Product product, NameValueCollection form, List<string> errors);

        void ParseRentalDates(Product product, NameValueCollection form, out DateTime? startDate, out DateTime? endDate);
    }
}