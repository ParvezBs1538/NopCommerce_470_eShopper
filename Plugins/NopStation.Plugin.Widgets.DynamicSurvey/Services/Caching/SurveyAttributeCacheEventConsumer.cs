using System.Threading.Tasks;
using Nop.Services.Caching;
using NopStation.Plugin.Widgets.DynamicSurvey.Domain;

namespace NopStation.Plugin.Widgets.DynamicSurvey.Services.Caching
{
    public partial class SurveyAttributeCacheEventConsumer : CacheEventConsumer<SurveyAttribute>
    {
        protected override async Task ClearCacheAsync(SurveyAttribute entity, EntityEventType entityEventType)
        {
            if (entityEventType == EntityEventType.Insert)
                await RemoveAsync(DynamicSurveyDefaults.SurveyAttributeValuesByAttributeCacheKey, entity);

            await base.ClearCacheAsync(entity, entityEventType);
        }
    }
}
