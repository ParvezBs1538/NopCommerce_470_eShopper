using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Shipping;
using Nop.Data;

namespace NopStation.Plugin.Widgets.DeliveryScheduler.Services
{
    public class CustomShippingService : ICustomShippingService
    {
        #region Fields

        private readonly IRepository<LocalizedProperty> _localizedPropertyRepository;
        private readonly IRepository<ShippingMethod> _shippingMethodRepository;

        #endregion

        #region Ctor

        public CustomShippingService(IRepository<LocalizedProperty> localizedPropertyRepository,
            IRepository<ShippingMethod> shippingMethodRepository)
        {
            _localizedPropertyRepository = localizedPropertyRepository;
            _shippingMethodRepository = shippingMethodRepository;
        }

        #endregion

        #region Methods

        public async Task<ShippingMethod> GetShippingMethodByNameAsync(string shippingMethodName)
        {
            if (string.IsNullOrWhiteSpace(shippingMethodName))
                return null;

            var shippingMethod = new ShippingMethod();
            shippingMethod = await _shippingMethodRepository.Table.FirstOrDefaultAsync(x => x.Name == shippingMethodName);

            if (shippingMethod == null)
            {
                var localeKeyGroup = nameof(ShippingMethod);
                var localShippingMethod = await _localizedPropertyRepository.Table.FirstOrDefaultAsync(x => x.LocaleKeyGroup == localeKeyGroup
                && x.LocaleKey == "Name" && x.LocaleValue == shippingMethodName);

                if (localShippingMethod != null)
                    shippingMethod = await _shippingMethodRepository.GetByIdAsync(localShippingMethod.EntityId);
            }

            return shippingMethod;
        }

        #endregion
    }
}
