using System.Threading.Tasks;
using Nop.Services.Caching;
using NopStation.Plugin.Misc.WidgetManager.Domain.Conditions;

namespace NopStation.Plugin.Misc.WidgetManager.Services.Caching;

public partial class ProductConditionMappingCacheEventConsumer : CacheEventConsumer<ProductConditionMapping>
{
    protected override async Task ClearCacheAsync(ProductConditionMapping entity)
    {
        await RemoveByPrefixAsync(WidgetManagerDefaults.EntityProductConditionMappingPrefiix, entity.EntityId, entity.EntityName);
        await RemoveAsync(WidgetManagerDefaults.EntityProductConditionsCacheKey, entity.EntityId, entity.EntityName);
        await RemoveAsync(WidgetManagerDefaults.EntityProductConditionMappingsCacheKey, entity.EntityId, entity.EntityName);
        await RemoveAsync(WidgetManagerDefaults.ProductConditionMappingExistsCacheKey, entity.EntityId, entity.EntityName);
        await RemoveAsync(WidgetManagerDefaults.ProductConditionMappingExistsCacheKey, 0, entity.EntityName);
    }
}
