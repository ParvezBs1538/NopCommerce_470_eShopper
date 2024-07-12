using System.Threading.Tasks;
using Nop.Services.Caching;
using NopStation.Plugin.Misc.QuoteCart.Domain;

namespace NopStation.Plugin.Misc.QuoteCart.Infrastructure.Cache;

public class FormAttributeValueCacheEventConsumer : CacheEventConsumer<FormAttributeValue>
{
    protected override async Task ClearCacheAsync(FormAttributeValue entity)
    {
        await RemoveAsync(QuoteCartDefaults.FormAttributeValuesByAttributeCacheKey, entity.FormAttributeMappingId);
        await base.ClearCacheAsync(entity);
    }
}
