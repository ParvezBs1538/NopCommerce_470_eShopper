using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using NopStation.Plugin.Widgets.DeliveryScheduler.Areas.Admin.Models;
using NopStation.Plugin.Widgets.DeliveryScheduler.Domains;
using NopStation.Plugin.Widgets.DeliveryScheduler.Services;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Shipping;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Models.Extensions;

namespace NopStation.Plugin.Widgets.DeliveryScheduler.Areas.Admin.Factories
{
    public class DeliverySlotModelFactory : IDeliverySlotModelFactory
    {
        private readonly IDeliverySlotService _deliverySlotService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedModelFactory _localizedModelFactory;
        private readonly IShippingService _shippingService;
        private readonly IStoreMappingSupportedModelFactory _storeMappingSupportedModelFactory;

        public DeliverySlotModelFactory(IDeliverySlotService deliverySlotService,
            IDateTimeHelper dateTimeHelper,
            ILocalizationService localizationService,
            ILocalizedModelFactory localizedModelFactory,
            IShippingService shippingService,
            IStoreMappingSupportedModelFactory storeMappingSupportedModelFactory)
        {
            _deliverySlotService = deliverySlotService;
            _dateTimeHelper = dateTimeHelper;
            _localizationService = localizationService;
            _localizedModelFactory = localizedModelFactory;
            _shippingService = shippingService;
            _storeMappingSupportedModelFactory = storeMappingSupportedModelFactory;
        }

        public async Task<DeliverySlotListModel> PrepareDeliverySlotListModelAsync(DeliverySlotSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var slots = await _deliverySlotService.SearchDeliverySlotsAsync(pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare list model
            var model = await new DeliverySlotListModel().PrepareToGridAsync(searchModel, slots, () =>
            {
                return slots.SelectAwait(async slot =>
                {
                    return await PrepareDeliverySlotModelAsync(null, slot, true);
                });
            });

            return model;
        }

        public async Task<DeliverySlotModel> PrepareDeliverySlotModelAsync(DeliverySlotModel model, DeliverySlot deliverySlot,  
            bool excludeProperties = false)
        {
            Func<DeliverySlotLocalizedModel, int, Task> localizedModelConfiguration = null;

            if (deliverySlot != null)
            {
                if (model == null)
                {
                    model = deliverySlot.ToModel<DeliverySlotModel>();
                    model.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(deliverySlot.CreatedOnUtc, DateTimeKind.Utc);
                    model.SelectedShippingMethodIds = _deliverySlotService.GetShippingMethodIdsWithAccess(deliverySlot);

                    if (!excludeProperties)
                    {
                        localizedModelConfiguration = async (locale, languageId) =>
                        {
                            locale.TimeSlot = await _localizationService.GetLocalizedAsync(deliverySlot, entity => entity.TimeSlot, languageId, false, false);
                        };
                    }
                }
            }

            if (!excludeProperties)
            {
                model.AvailableShippingMethods = (await _shippingService.GetAllShippingMethodsAsync())
                    .Select(s => new SelectListItem()
                    {
                        Text = s.Name,
                        Value = s.Id.ToString(),
                        Selected = model.SelectedShippingMethodIds.Contains(s.Id)
                    }).ToList();
                model.Locales = await _localizedModelFactory.PrepareLocalizedModelsAsync(localizedModelConfiguration);

                //prepare available stores
                await _storeMappingSupportedModelFactory.PrepareModelStoresAsync(model, deliverySlot, excludeProperties);
            }

            return model;
        }

        public Task<DeliverySlotSearchModel> PrepareDeliverySlotSearchModelAsync(DeliverySlotSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare page parameters
            searchModel.SetGridPageSize();

            return Task.FromResult(searchModel);
        }
    }
}
