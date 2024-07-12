using System.Threading.Tasks;
using Nop.Services.Caching;
using NopStation.Plugin.Misc.QuoteCart.Domain;

namespace NopStation.Plugin.Misc.QuoteCart.Infrastructure.Cache;

public class QuoteFormCacheEventConsumer : CacheEventConsumer<QuoteForm>
{
    protected override async Task ClearCacheAsync(QuoteForm entity, EntityEventType entityEventType)
    {
        await RemoveAsync(QuoteCartDefaults.FormAttributeValuesByAttributeCacheKey, entity);
        await base.ClearCacheAsync(entity, entityEventType);
    }
}
