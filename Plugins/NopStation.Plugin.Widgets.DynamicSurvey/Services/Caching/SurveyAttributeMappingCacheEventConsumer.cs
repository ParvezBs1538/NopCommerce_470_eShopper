using System.Threading.Tasks;
using Nop.Services.Caching;
using NopStation.Plugin.Widgets.DynamicSurvey.Domain;

namespace NopStation.Plugin.Widgets.DynamicSurvey.Services.Caching
{
    public partial class SurveyAttributeMappingCacheEventConsumer : CacheEventConsumer<SurveyAttributeMapping>
    {
        protected override async Task ClearCacheAsync(SurveyAttributeMapping entity)
        {
            await RemoveAsync(DynamicSurveyDefaults.SurveyAttributeMappingsBySurveyCacheKey, entity.SurveyId);
            await RemoveAsync(DynamicSurveyDefaults.SurveyAttributeValuesByAttributeCacheKey, entity);
        }
    }
}
