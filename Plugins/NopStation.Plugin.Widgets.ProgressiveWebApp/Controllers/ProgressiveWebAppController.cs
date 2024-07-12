using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Domains;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Infrastructure.Cache;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Models;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Services;

namespace NopStation.Plugin.Widgets.ProgressiveWebApp.Controllers
{
    public class ProgressiveWebAppController : NopStationPublicController
    {
        #region Fields

        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IWebAppDeviceService _webAppDeviceService;
        private readonly IStaticCacheManager _cacheManager;
        private readonly IWebPushService _webPushService;
        private readonly ProgressiveWebAppSettings _progressiveWebAppSettings;

        #endregion

        public ProgressiveWebAppController(IWorkContext workContext,
            IStoreContext storeContext,
            IWebAppDeviceService webAppDeviceService,
            IStaticCacheManager cacheManager,
            IWebPushService webPushService,
            ProgressiveWebAppSettings progressiveWebAppSettings)
        {
            _workContext = workContext;
            _storeContext = storeContext;
            _webAppDeviceService = webAppDeviceService;
            _cacheManager = cacheManager;
            _webPushService = webPushService;
            _progressiveWebAppSettings = progressiveWebAppSettings;
        }

        public async Task<IActionResult> SaveDevice(DeviceModel model)
        {
            if (ModelState.IsValid && _webPushService.ValidDevice(model.Endpoint, model.P256dh, model.Auth))
            {
                var cache = new CacheKey(PWAModelCacheDefaults.CurrentUserDevice);
                var cacheKey = _cacheManager.PrepareKeyForDefaultCache(cache, model.Endpoint);

                var cachedWebAppDevice = await _cacheManager.GetAsync(cacheKey, async () =>
                {
                    var webAppDevice = await _webAppDeviceService.GetWebAppDeviceByEndpointAsync(model.Endpoint);
                    if (webAppDevice == null)
                    {
                        webAppDevice = new WebAppDevice()
                        {
                            PushAuth = model.Auth,
                            CreatedOnUtc = DateTime.UtcNow,
                            PushEndpoint = model.Endpoint,
                            PushP256DH = model.P256dh,
                            CustomerId = (await _workContext.GetCurrentCustomerAsync()).Id,
                            VapidPrivateKey = _progressiveWebAppSettings.VapidPrivateKey,
                            VapidPublicKey = _progressiveWebAppSettings.VapidPublicKey,
                            StoreId = (await _storeContext.GetCurrentStoreAsync()).Id
                        };
                        await _webAppDeviceService.InsertWebAppDeviceAsync(webAppDevice);
                    }
                    return webAppDevice;
                });

                if (cachedWebAppDevice.CustomerId != (await _workContext.GetCurrentCustomerAsync()).Id)
                {
                    var webAppDevice = await _webAppDeviceService.GetWebAppDeviceByIdAsync(cachedWebAppDevice.Id);
                    webAppDevice.CustomerId = (await _workContext.GetCurrentCustomerAsync()).Id;
                    await _webAppDeviceService.UpdateWebAppDeviceAsync(webAppDevice);
                    await _cacheManager.SetAsync(cacheKey, webAppDevice);
                }
            }

            return Json(null);
        }

        public IActionResult CheckDevice(DeviceModel model)
        {
            return Json(null);
        }
    }
}