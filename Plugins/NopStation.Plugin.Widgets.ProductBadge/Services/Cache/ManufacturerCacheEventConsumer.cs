using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Services.Caching;

namespace NopStation.Plugin.Widgets.ProductBadge.Services.Cache;

public partial class ManufacturerCacheEventConsumer : CacheEventConsumer<ProductManufacturer>
{
    protected override async Task ClearCacheAsync(ProductManufacturer entity)
    {
        await RemoveByPrefixAsync(CacheDefaults.BadgeManufacturersAllPrefix);
    }
}