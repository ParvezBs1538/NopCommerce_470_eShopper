using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Misc.WebApi.Domains;

namespace NopStation.Plugin.Misc.WebApi.Services
{
    public interface IApiDeviceService
    {
        Task DeleteApiDeviceAsync(ApiDevice device);

        Task InsertApiDeviceAsync(ApiDevice device);

        Task UpdateApiDeviceAsync(ApiDevice device);

        Task<ApiDevice> GetApiDeviceByIdAsync(int deviceId);

        Task<ApiDevice> GetApiDeviceByDeviceIdAsync(string deviceToken, int storeId);

        Task<IPagedList<ApiDevice>> SearchApiDevicesAsync(int customerId = 0, IList<int> dtids = null,
            int pageIndex = 0, int pageSize = int.MaxValue, int storeId = 0);

        IList<ApiDevice> GetApiDeviceByIds(int[] deviceByIds);

        Task DeleteApiDevicesAsync(IList<ApiDevice> devices);
    }
}