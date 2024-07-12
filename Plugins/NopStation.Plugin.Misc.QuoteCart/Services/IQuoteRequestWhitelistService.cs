using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Vendors;
using NopStation.Plugin.Misc.QuoteCart.Domain;

namespace NopStation.Plugin.Misc.QuoteCart.Services;

public interface IQuoteRequestWhitelistService
{
    Task AddPermissionsAsync<T>(IList<int> entityIds) where T : BaseEntity;

    Task<bool> CanQuoteAsync(int productId, Customer customer = null);

    Task<IPagedList<Category>> GetWhitelistedCategoriesAsync(int pageIndex = 0, int pageSize = int.MaxValue);

    Task<IPagedList<QuoteRequestWhitelist>> GetWhitelistedEntityListAsync<T>(int pageIndex = 0, int pageSize = int.MaxValue);

    Task<IPagedList<Manufacturer>> GetWhitelistedManufacturersAsync(int pageIndex = 0, int pageSize = int.MaxValue);

    Task<IPagedList<Product>> GetWhitelistedProductsAsync(int pageIndex = 0, int pageSize = int.MaxValue);

    Task<IPagedList<Vendor>> GetWhitelistedVendorsAsync(int pageIndex = 0, int pageSize = int.MaxValue);

    Task RemovePermissionAsync<T>(int id) where T : BaseEntity;

    Task RemovePermissionsAsync<T>(IList<int> ids) where T : BaseEntity;
}