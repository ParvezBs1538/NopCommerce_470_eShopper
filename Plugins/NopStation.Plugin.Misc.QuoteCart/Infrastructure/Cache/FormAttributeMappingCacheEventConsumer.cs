using System.Threading.Tasks;
using Nop.Services.Caching;
using NopStation.Plugin.Misc.QuoteCart.Domain;

namespace NopStation.Plugin.Misc.QuoteCart.Infrastructure.Cache;

public class FormAttributeMappingCacheEventConsumer : CacheEventConsumer<FormAttributeMapping>
{
    protected override async Task ClearCacheAsync(FormAttributeMapping entity)
    {
        await RemoveAsync(QuoteCartDefaults.FormAttributeMappingsByQuoteFormCacheKey, entity.QuoteFormId);
        await RemoveAsync(QuoteCartDefaults.FormAttributeMappingsByAttributeCacheKey, entity);

        await base.ClearCacheAsync(entity);
    }
}
