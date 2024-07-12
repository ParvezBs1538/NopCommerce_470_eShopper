using System.Threading.Tasks;
using NopStation.Plugin.Widgets.DMS.Areas.Admin.Models.ShipperDevice;
using NopStation.Plugin.Widgets.DMS.Domain;

namespace NopStation.Plugin.Widgets.DMS.Areas.Admin.Factories
{

    public interface IDeviceModelFactory
    {
        Task<DeviceSearchModel> PrepareDeviceSearchModelAsync(DeviceSearchModel searchModel);

        Task<DeviceListModel> PrepareDeviceListModelAsync(DeviceSearchModel searchModel);

        Task<DeviceModel> PrepareDeviceModelAsync(DeviceModel model, ShipperDevice device);
    }
}
