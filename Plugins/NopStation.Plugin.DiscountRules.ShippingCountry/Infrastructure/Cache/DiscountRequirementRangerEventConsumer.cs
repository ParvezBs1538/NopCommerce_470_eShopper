using System.Threading.Tasks;
using Nop.Core.Domain.Discounts;
using Nop.Core.Events;
using Nop.Services.Configuration;
using Nop.Services.Events;

namespace NopStation.Plugin.DiscountRules.ShippingCountry.Cache
{
    public class DiscountRequirementRangerEventConsumer : IConsumer<EntityDeletedEvent<DiscountRequirement>>
    {
        #region Fields

        private readonly ISettingService _settingService;

        #endregion

        #region Ctor

        public DiscountRequirementRangerEventConsumer(ISettingService settingService)
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

            //delete saved restricted values identifier if exists
            var setting = await _settingService.GetSettingAsync(string.Format(DiscountRequirementDefaults.ShippingCountrySettingsKey, discountRequirement.Id));
            if (setting != null)
                await _settingService.DeleteSettingAsync(setting);
        }

        #endregion
    }
}