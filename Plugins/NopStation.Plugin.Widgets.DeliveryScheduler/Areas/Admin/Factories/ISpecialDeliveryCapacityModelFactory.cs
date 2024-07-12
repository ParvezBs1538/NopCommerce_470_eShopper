using System.Threading.Tasks;
using NopStation.Plugin.Widgets.DeliveryScheduler.Areas.Admin.Models;
using NopStation.Plugin.Widgets.DeliveryScheduler.Domains;

namespace NopStation.Plugin.Widgets.DeliveryScheduler.Areas.Admin.Factories
{
    public interface ISpecialDeliveryCapacityModelFactory
    {
        Task<SpecialDeliveryCapacitySearchModel> PrepareCapacitySearchModelAsync(SpecialDeliveryCapacitySearchModel specialDeliveryCapacitySearchModel);

        Task<SpecialDeliveryCapacityListModel> PrepareCapacityListModelAsync(SpecialDeliveryCapacitySearchModel searchModel);

        Task<SpecialDeliveryCapacityModel> PrepareCapacityModelAsync(SpecialDeliveryCapacityModel specialDeliveryCapacityModel, SpecialDeliveryCapacity specialDeliveryCapacity, bool excludeProperties = false);
    }
}