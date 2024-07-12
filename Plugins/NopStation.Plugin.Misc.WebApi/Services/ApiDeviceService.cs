using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using NopStation.Plugin.Misc.WebApi.Domains;

namespace NopStation.Plugin.Misc.WebApi.Services
{
    public class ApiDeviceService : IApiDeviceService
    {
        #region Fields

        private readonly IRepository<ApiDevice> _deviceRepository;

        #endregion

        #region Ctor

        public ApiDeviceService(IRepository<ApiDevice> deviceRepository)
        {
            _deviceRepository = deviceRepository;
        }

        #endregion

        #region Methods

        public async Task DeleteApiDeviceAsync(ApiDevice device)
        {
            await _deviceRepository.DeleteAsync(device);
        }

        public async Task InsertApiDeviceAsync(ApiDevice device)
        {
            await _deviceRepository.InsertAsync(device);
        }

        public async Task UpdateApiDeviceAsync(ApiDevice device)
        {
            await _deviceRepository.UpdateAsync(device);
        }

        public async Task<ApiDevice> GetApiDeviceByIdAsync(int deviceId)
        {
            if (deviceId == 0)
                return null;

            return await _deviceRepository.GetByIdAsync(deviceId, cache => default);
        }

        public async Task<ApiDevice> GetApiDeviceByDeviceIdAsync(string deviceToken, int storeId)
        {
            return await _deviceRepository.Table.FirstOrDefaultAsync(x => x.DeviceToken == deviceToken && x.StoreId == storeId);
        }

        public async Task<IPagedList<ApiDevice>> SearchApiDevicesAsync(int customerId = 0, IList<int> dtids = null, int pageIndex = 0, int pageSize = int.MaxValue, int storeId = 0)
        {
            var query = _deviceRepository.Table;
            if (dtids != null && dtids.Any())
                query = query.Where(x => dtids.Contains(x.DeviceTypeId));

            if (customerId > 0)
                query = query.Where(x => x.CustomerId == customerId);

            if (storeId > 0)
                query = query.Where(x => x.StoreId == storeId);

            query = query.OrderByDescending(e => e.Id);

            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

        public IList<ApiDevice> GetApiDeviceByIds(int[] deviceIds)
        {
            if (deviceIds == null || deviceIds.Length == 0)
                return new List<ApiDevice>();

            var devices = _deviceRepository.Table.Where(x => deviceIds.Contains(x.Id)).ToList();

            var sortedDevices = new List<ApiDevice>();
            foreach (var id in deviceIds)
            {
                var device = devices.Find(x => x.Id == id);
                if (device != null)
                    sortedDevices.Add(device);
            }
            return sortedDevices;
        }

        public async Task DeleteApiDevicesAsync(IList<ApiDevice> devices)
        {
            await _deviceRepository.DeleteAsync(devices);
        }

        #endregion
    }
}
