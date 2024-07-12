using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Services.Caching;

namespace NopStation.Plugin.Widgets.ProductBadge.Services.Cache;

public partial class ProductCacheEventConsumer : CacheEventConsumer<Product>
{
    protected override async Task ClearCacheAsync(Product entity)
    {
        await RemoveByPrefixAsync(CacheDefaults.BadgeProductsAllPrefix);
        await RemoveByPrefixAsync(CacheDefaults.BadgeVendorsAllPrefix);
    }
}