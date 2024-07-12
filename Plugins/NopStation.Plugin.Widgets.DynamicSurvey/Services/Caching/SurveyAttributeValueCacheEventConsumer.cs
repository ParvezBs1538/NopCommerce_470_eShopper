using System.Threading.Tasks;
using Nop.Services.Caching;
using NopStation.Plugin.Widgets.DynamicSurvey.Domain;

namespace NopStation.Plugin.Widgets.DynamicSurvey.Services.Caching
{
    public partial class SurveyAttributeValueCacheEventConsumer : CacheEventConsumer<SurveyAttributeValue>
    {
        protected override async Task ClearCacheAsync(SurveyAttributeValue entity)
        {
            await RemoveAsync(DynamicSurveyDefaults.SurveyAttributeValuesByAttributeCacheKey, entity.SurveyAttributeMappingId);
        }
    }
}
