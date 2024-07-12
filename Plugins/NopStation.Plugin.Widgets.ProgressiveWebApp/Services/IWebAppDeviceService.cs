using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Domains;

namespace NopStation.Plugin.Widgets.ProgressiveWebApp.Services
{
    public interface IWebAppDeviceService
    {
        Task DeleteWebAppDeviceAsync(WebAppDevice webAppDevice);

        Task DeleteWebAppDeviceAsync(IList<WebAppDevice> webAppDevices);

        Task InsertWebAppDeviceAsync(WebAppDevice webAppDevice);

        Task UpdateWebAppDeviceAsync(WebAppDevice webAppDevice);

        Task<WebAppDevice> GetWebAppDeviceByIdAsync(int webAppDeviceId);
        Task<IList<WebAppDevice>> GetWebAppDevicesByIdsAsync(int[] ids);

        Task<IPagedList<WebAppDevice>> GetWebAppDevicesAsync(int? customerId = null, int storeId = 0,
            DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int pageIndex = 0, int pageSize = int.MaxValue);

        Task<WebAppDevice> GetWebAppDeviceByEndpointAsync(string endpoint);

        Task<bool> DeviceAlreadySavedAsync(string endpoint);
    }
}