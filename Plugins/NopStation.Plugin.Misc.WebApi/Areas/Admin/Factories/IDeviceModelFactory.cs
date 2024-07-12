using System.Threading.Tasks;
using NopStation.Plugin.Misc.WebApi.Areas.Admin.Models;
using NopStation.Plugin.Misc.WebApi.Domains;

namespace NopStation.Plugin.Misc.WebApi.Areas.Admin.Factories
{
    public interface IDeviceModelFactory
    {
        Task<DeviceSearchModel> PrepareDeviceSearchModelAsync(DeviceSearchModel searchModel);

        Task<DeviceListModel> PrepareDeviceListModelAsync(DeviceSearchModel searchModel);

        Task<DeviceModel> PrepareDeviceModelAsync(DeviceModel model, ApiDevice device);
    }
}