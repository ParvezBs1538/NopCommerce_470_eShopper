using System.Threading.Tasks;
using Nop.Services.Caching;
using NopStation.Plugin.Widgets.DynamicSurvey.Domain;

namespace NopStation.Plugin.Widgets.DynamicSurvey.Services.Caching
{
    public partial class SurveyCacheEventConsumer : CacheEventConsumer<Survey>
    {
        protected override async Task ClearCacheAsync(Survey entity, EntityEventType entityEventType)
        {
            await RemoveByPrefixAsync(DynamicSurveyDefaults.SurveyBySystemNamePrefix, entity.SystemName);

            await base.ClearCacheAsync(entity, entityEventType);
        }
    }
}
