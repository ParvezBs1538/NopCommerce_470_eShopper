using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Services.Caching;

namespace NopStation.Plugin.Widgets.ProductBadge.Services.Cache;

public partial class CategoryCacheEventConsumer : CacheEventConsumer<ProductCategory>
{
    protected override async Task ClearCacheAsync(ProductCategory entity)
    {
        await RemoveByPrefixAsync(CacheDefaults.BadgeCategoriesAllPrefix);
    }
}