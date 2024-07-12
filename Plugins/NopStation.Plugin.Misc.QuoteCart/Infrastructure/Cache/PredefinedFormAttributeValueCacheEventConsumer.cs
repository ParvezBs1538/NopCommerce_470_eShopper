using System.Threading.Tasks;
using Nop.Services.Caching;
using NopStation.Plugin.Misc.QuoteCart.Domain;

namespace NopStation.Plugin.Misc.QuoteCart.Infrastructure.Cache;

public class PredefinedFormAttributeValueCacheEventConsumer : CacheEventConsumer<PredefinedFormAttributeValue>
{
    protected override async Task ClearCacheAsync(PredefinedFormAttributeValue entity)
    {
        await RemoveAsync(QuoteCartDefaults.PredefinedFormAttributeValuesByAttributeCacheKey, entity.FormAttributeId);
        await base.ClearCacheAsync(entity);
    }
}
