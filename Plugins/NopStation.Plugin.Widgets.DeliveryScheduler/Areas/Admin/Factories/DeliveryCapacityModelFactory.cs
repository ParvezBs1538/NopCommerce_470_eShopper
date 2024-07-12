using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Shipping;
using NopStation.Plugin.Widgets.DeliveryScheduler.Areas.Admin.Models;
using NopStation.Plugin.Widgets.DeliveryScheduler.Domains;
using NopStation.Plugin.Widgets.DeliveryScheduler.Services;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Shipping;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Models.Extensions;

namespace NopStation.Plugin.Widgets.DeliveryScheduler.Areas.Admin.Factories
{
    public class DeliveryCapacityModelFactory : IDeliveryCapacityModelFactory
    {
        private readonly IDeliverySlotService _deliverySlotService;
        private readonly IDeliveryCapacityService _deliveryCapacityService;
        private readonly ILocalizationService _localizationService;
        private readonly IShippingService _shippingService;
        private readonly IWebHelper _webHelper;
        private readonly IStoreContext _storeContext;

        public DeliveryCapacityModelFactory(IDeliverySlotService deliverySlotService,
            IDeliveryCapacityService deliveryCapacityService,
            ILocalizationService localizationService,
            IShippingService shippingService,
            IWebHelper webHelper,
            IStoreContext storeContext)
        {
            _deliverySlotService = deliverySlotService;
            _deliveryCapacityService = deliveryCapacityService;
            _localizationService = localizationService;
            _shippingService = shippingService;
            _webHelper = webHelper;
            _storeContext = storeContext;
        }

        public async Task<DeliveryCapacityConfigurationModel> PrepareConfigurationModelAsync(ShippingMethod shippingMethod)
        {
            if (shippingMethod == null)
                throw new ArgumentNullException(nameof(shippingMethod));
            
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var capacities = await _deliveryCapacityService.SearchDeliveryCapacitiesAsync(shippingMethodId: shippingMethod.Id);

            var model = new DeliveryCapacityConfigurationModel();
            model.ShippingMethodId = shippingMethod.Id;

            var shippingMethods = await _shippingService.GetAllShippingMethodsAsync();
            model.ShippingMethods = shippingMethods.Select(x => new SelectListItem
            {
                Text = _localizationService.GetLocalizedAsync(x, y=> y.Name).Result,
                Value = $"{_webHelper.GetStoreLocation()}Admin/DeliveryCapacity/Configure?shippingMethodId={x.Id}",
                Selected = x.Id == shippingMethod.Id
            }).ToList();

            var slots = await _deliverySlotService.SearchDeliverySlotsAsync(shippingMethodId: shippingMethod.Id, storeId: storeScope);

            foreach (var slot in slots)
            {
                var cm = new DeliveryCapacityModel();
                var capacity = capacities.FirstOrDefault(x => x.DeliverySlotId == slot.Id);
                if (capacity != null)
                    cm = capacity.ToModel<DeliveryCapacityModel>();

                cm.DeliverySlotId = slot.Id;
                cm.DeliverySlot = await _localizationService.GetLocalizedAsync(slot, x => x.TimeSlot);
                model.DeliveryCapacities.Add(slot.Id, cm);
            }

            return model;
        }
    }
}
