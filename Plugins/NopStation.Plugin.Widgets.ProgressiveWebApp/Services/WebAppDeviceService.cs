using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Data;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Domains;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Infrastructure.Cache;

namespace NopStation.Plugin.Widgets.ProgressiveWebApp.Services
{
    public class WebAppDeviceService : IWebAppDeviceService
    {
        #region Fields

        private readonly IRepository<WebAppDevice> _webAppDeviceRepository;
        private readonly IStaticCacheManager _cacheManager;

        #endregion

        #region Ctor

        public WebAppDeviceService(
            IRepository<WebAppDevice> webAppDeviceRepository,
            IStaticCacheManager cacheManager)
        {
            _webAppDeviceRepository = webAppDeviceRepository;
            _cacheManager = cacheManager;
        }

        #endregion

        #region Utilities

        protected virtual async Task<IDictionary<string, IList<WebAppDevice>>> GetAllDevicesCachedAsync()
        {
            var key = new CacheKey(PWAEntityCacheDefaults.WebAppDevicesAllCacheKey, PWAEntityCacheDefaults.WebAppDevicesPrefixCacheKey);

            //cache
            return await _cacheManager.GetAsync(key, () =>
            {
                //we use no tracking here for performance optimization
                //anyway records are loaded only for read-only operations
                var query = from s in _webAppDeviceRepository.Table
                            orderby s.Id
                            select s;
                var devices = query.ToList();
                var dictionary = new Dictionary<string, IList<WebAppDevice>>();
                foreach (var d in devices)
                {
                    var resourceName = d.PushEndpoint.ToLowerInvariant();
                    var deviceForCaching = new WebAppDevice
                    {
                        Id = d.Id,
                        PushEndpoint = d.PushEndpoint,
                        CreatedOnUtc = d.CreatedOnUtc,
                        CustomerId = d.CustomerId,
                        PushAuth = d.PushAuth,
                        PushP256DH = d.PushP256DH,
                        VapidPrivateKey = d.VapidPrivateKey,
                        VapidPublicKey = d.VapidPublicKey
                    };
                    if (!dictionary.ContainsKey(resourceName))
                    {
                        dictionary.Add(resourceName, new List<WebAppDevice>
                        {
                            deviceForCaching
                        });
                    }
                    else
                    {
                        dictionary[resourceName].Add(deviceForCaching);
                    }
                }

                return dictionary;
            });
        }

        #endregion

        #region Methods

        public async Task DeleteWebAppDeviceAsync(WebAppDevice webAppDevice)
        {
            await _cacheManager.RemoveByPrefixAsync(PWAEntityCacheDefaults.WebAppDevicesPrefixCacheKey);

            await _webAppDeviceRepository.DeleteAsync(webAppDevice);
        }

        public async Task DeleteWebAppDeviceAsync(IList<WebAppDevice> webAppDevices)
        {
            await _cacheManager.RemoveByPrefixAsync(PWAEntityCacheDefaults.WebAppDevicesPrefixCacheKey);

            await _webAppDeviceRepository.DeleteAsync(webAppDevices);
        }

        public async Task InsertWebAppDeviceAsync(WebAppDevice webAppDevice)
        {
            await _cacheManager.RemoveByPrefixAsync(PWAEntityCacheDefaults.WebAppDevicesPrefixCacheKey);

            await _webAppDeviceRepository.InsertAsync(webAppDevice);
        }

        public async Task UpdateWebAppDeviceAsync(WebAppDevice webAppDevice)
        {
            await _cacheManager.RemoveByPrefixAsync(PWAEntityCacheDefaults.WebAppDevicesPrefixCacheKey);

            await _webAppDeviceRepository.UpdateAsync(webAppDevice);
        }

        public async Task<WebAppDevice> GetWebAppDeviceByIdAsync(int webAppDeviceId)
        {
            if (webAppDeviceId == 0)
                return null;

            return await _webAppDeviceRepository.GetByIdAsync(webAppDeviceId);
        }
        public virtual async Task<IList<WebAppDevice>> GetWebAppDevicesByIdsAsync(int[] ids)
        {
            return await _webAppDeviceRepository.GetByIdsAsync(ids, cache => default, false);
        }

        public async Task<IPagedList<WebAppDevice>> GetWebAppDevicesAsync(int? customerId = null, int storeId = 0,
            DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var cache = new CacheKey(PWAEntityCacheDefaults.WebAppDevicesCustomerCacheKey, PWAEntityCacheDefaults.WebAppDevicesPrefixCacheKey);
            var cacheKey = _cacheManager.PrepareKeyForDefaultCache(cache, customerId, storeId, createdFromUtc, createdToUtc, pageIndex, pageSize);

            return await _cacheManager.GetAsync(cacheKey, () =>
            {
                var query = from s in _webAppDeviceRepository.Table
                            select s;

                if (createdFromUtc.HasValue)
                    query = query.Where(x => x.CreatedOnUtc >= createdFromUtc.Value);

                if (createdToUtc.HasValue)
                    query = query.Where(x => x.CreatedOnUtc <= createdToUtc.Value);

                if (customerId.HasValue && customerId.Value > 0)
                    query = query.Where(x => x.CustomerId == customerId);

                if (storeId > 0)
                    query = query.Where(x => x.StoreId == storeId);

                return query.OrderByDescending(x => x.CreatedOnUtc).ToPagedListAsync(pageIndex, pageSize);
            });
        }

        public async Task<WebAppDevice> GetWebAppDeviceByEndpointAsync(string endpoint)
        {
            if (string.IsNullOrWhiteSpace(endpoint))
                return null;

            var devices = await GetAllDevicesCachedAsync();
            endpoint = endpoint.Trim().ToLowerInvariant();
            if (!devices.ContainsKey(endpoint))
                return null;

            var device = devices[endpoint].FirstOrDefault();

            return device != null ? await GetWebAppDeviceByIdAsync(device.Id) : null;
        }

        public async Task<bool> DeviceAlreadySavedAsync(string endpoint)
        {
            if (string.IsNullOrWhiteSpace(endpoint))
                return false;

            return await GetWebAppDeviceByEndpointAsync(endpoint) != null;
        }

        #endregion
    }
}
