using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using NopStation.Plugin.Misc.QuoteCart.Domain;

namespace NopStation.Plugin.Misc.QuoteCart.Services;

public interface IQuoteCartService
{
    public Task<IList<string>> AddToQuoteCartAsync(Customer customer, Product product, int storeId, string attributesXml = null,
        int quantity = 1, DateTime? rentalStartDate = null, DateTime? rentalEndDate = null);

    public Task<IList<QuoteCartItem>> GetQuoteCartAsync(Customer customer,
        int storeId = 0, int? productId = null, DateTime? createdFromUtc = null, DateTime? createdToUtc = null);

    public Task DeleteFromQuoteCartAsync(int itemId);

    public Task UpdateQuoteCartAsync(QuoteCartItem cartItem);

    Task<IList<int>> GetAllowedCustomerRoleIdsAsync();

    Task<PermissionRecord> GetSendQuotePermissionRecordAsync();

    Task<string> ExportQuoteRequestItemsXmlAsync(IList<QuoteRequestItem> quoteRequestItems);

    Task ClearCartAsync(Customer customer, int storeId);
}
