using System.Threading.Tasks;
using Nop.Core.Domain.Discounts;
using Nop.Core.Events;
using Nop.Services.Configuration;
using Nop.Services.Events;

namespace NopStation.Plugin.DiscountRules.TimeOfDay.Infrastructure.Cache
{
    public partial class DiscountRequirementEventConsumer : 
        IConsumer<EntityDeletedEvent<DiscountRequirement>>
    {
        #region Fields
        
        private readonly ISettingService _settingService;

        #endregion

        #region Ctor

        public DiscountRequirementEventConsumer(ISettingService settingService)
        {
            _settingService = settingService;
        }

        #endregion

        #region Methods

        public async Task HandleEventAsync(EntityDeletedEvent<DiscountRequirement> eventMessage)
        {
            var discountRequirement = eventMessage?.Entity;
            if (discountRequirement == null)
                return;

            var setting1 = await _settingService.GetSettingAsync(string.Format(DiscountRequirementDefaults.SETTINGS_KEY_FROM, discountRequirement.Id));
            if (setting1 != null)
                await _settingService.DeleteSettingAsync(setting1);

            var setting2 = await _settingService.GetSettingAsync(string.Format(DiscountRequirementDefaults.SETTINGS_KEY_TO, discountRequirement.Id));
            if (setting2 != null)
                await _settingService.DeleteSettingAsync(setting2);
        }

        #endregion
    }
}