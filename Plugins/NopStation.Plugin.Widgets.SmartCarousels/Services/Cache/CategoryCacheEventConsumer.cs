using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Services.Caching;

namespace NopStation.Plugin.Widgets.SmartCarousels.Services.Cache;

public partial class CategoryCacheEventConsumer : CacheEventConsumer<Category>
{
    protected override async Task ClearCacheAsync(Category entity)
    {
        await RemoveByPrefixAsync(CacheDefaults.CarouselCategoryMappingsPrefix);
    }
}
