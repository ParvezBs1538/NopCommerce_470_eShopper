using System.Threading.Tasks;
using Nop.Services.Caching;
using NopStation.Plugin.Misc.WidgetManager.Domain.Conditions;

namespace NopStation.Plugin.Misc.WidgetManager.Services.Caching;

public partial class CustomerConditionMappingCacheEventConsumer : CacheEventConsumer<CustomerConditionMapping>
{
    protected override async Task ClearCacheAsync(CustomerConditionMapping entity)
    {
        await RemoveByPrefixAsync(WidgetManagerDefaults.EntityCustomerConditionMappingPrefiix, entity.EntityId, entity.EntityName);
        await RemoveAsync(WidgetManagerDefaults.EntityCustomerConditionsCacheKey, entity.EntityId, entity.EntityName);
        await RemoveAsync(WidgetManagerDefaults.EntityCustomerConditionMappingsCacheKey, entity.EntityId, entity.EntityName);
        await RemoveAsync(WidgetManagerDefaults.CustomerConditionMappingExistsCacheKey, entity.EntityId, entity.EntityName);
        await RemoveAsync(WidgetManagerDefaults.CustomerConditionMappingExistsCacheKey, 0, entity.EntityName);
    }
}
