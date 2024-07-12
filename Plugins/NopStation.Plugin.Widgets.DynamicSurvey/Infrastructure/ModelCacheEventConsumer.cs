using System.Threading.Tasks;
using Nop.Core.Caching;
using Nop.Core.Events;
using Nop.Services.Events;
using NopStation.Plugin.Widgets.DynamicSurvey.Domain;

namespace NopStation.Plugin.Widgets.DynamicSurvey.Infrastructure
{
    public partial class ModelCacheEventConsumer :
        IConsumer<EntityUpdatedEvent<SurveyAttributeValue>>
    {
        #region Fields

        private readonly IStaticCacheManager _staticCacheManager;

        #endregion

        #region Ctor

        public ModelCacheEventConsumer(IStaticCacheManager staticCacheManager)
        {
            _staticCacheManager = staticCacheManager;
        }

        #endregion

        #region Methods

        #region Survey attributes

        public async Task HandleEventAsync(EntityUpdatedEvent<SurveyAttributeValue> eventMessage)
        {
            await _staticCacheManager.RemoveByPrefixAsync(DynamicSurveyDefaults.SurveyAttributeImageSquarePicturePrefixCacheKey);
        }

        #endregion

        #endregion
    }
}