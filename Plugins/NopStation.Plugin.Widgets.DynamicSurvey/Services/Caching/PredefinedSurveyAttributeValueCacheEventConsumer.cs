using System.Threading.Tasks;
using Nop.Services.Caching;
using NopStation.Plugin.Widgets.DynamicSurvey.Domain;

namespace NopStation.Plugin.Widgets.DynamicSurvey.Services.Caching
{
    public partial class PredefinedSurveyAttributeValueCacheEventConsumer : CacheEventConsumer<PredefinedSurveyAttributeValue>
    {
        protected override async Task ClearCacheAsync(PredefinedSurveyAttributeValue entity)
        {
            await RemoveAsync(DynamicSurveyDefaults.PredefinedSurveyAttributeValuesByAttributeCacheKey, entity.SurveyAttributeId);
        }
    }
}
