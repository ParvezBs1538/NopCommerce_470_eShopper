using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Services.Caching;

namespace NopStation.Plugin.Widgets.SmartCarousels.Services.Cache;

public partial class ManufacturerCacheEventConsumer : CacheEventConsumer<Manufacturer>
{
    protected override async Task ClearCacheAsync(Manufacturer entity)
    {
        await RemoveByPrefixAsync(CacheDefaults.CarouselManufacturerMappingsPrefix);
    }
}
