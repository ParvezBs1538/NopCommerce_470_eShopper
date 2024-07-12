using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Nop.Core.Caching;
using Nop.Core.Domain.Vendors;
using Nop.Core.Events;
using Nop.Services.Events;
using Nop.Services.Vendors;
using Nop.Web.Areas.Admin.Models.Vendors;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.VendorShop.Services.Cache
{
    public class VendorShopFeatureEventConsumer : IConsumer<EntityUpdatedEvent<Vendor>>,
         IConsumer<EntityDeletedEvent<Vendor>>,
         IConsumer<ModelReceivedEvent<BaseNopModel>>
    {
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IVendorShopFeatureService _vendorShopFeatureService;
        private readonly IVendorService _vendorService;

        public VendorShopFeatureEventConsumer(IStaticCacheManager staticCacheManager,
            IHttpContextAccessor httpContextAccessor,
            IVendorShopFeatureService vendorShopFeatureService,
            IVendorService vendorService)
        {
            _staticCacheManager = staticCacheManager;
            _httpContextAccessor = httpContextAccessor;
            _vendorShopFeatureService = vendorShopFeatureService;
            _vendorService = vendorService;
        }

        public async Task HandleEventAsync(ModelReceivedEvent<BaseNopModel> eventMessage)
        {
            var vendorModel = eventMessage.Model as VendorModel;
            if (vendorModel is not null)
            {
                var vendor = await _vendorService.GetVendorByIdAsync(vendorModel.Id);
                if (vendor == null)
                    return;
                // Access the HttpContext
                var httpContext = _httpContextAccessor.HttpContext;

                // Get the controller name, action name, area name, and request method
                var controllerName = (httpContext.Request.RouteValues["controller"] as string)?.ToLowerInvariant();
                var actionName = (httpContext.Request.RouteValues["action"] as string)?.ToLowerInvariant();
                var areaName = (httpContext.Request.RouteValues["area"] as string)?.ToLowerInvariant();
                var requestMethod = httpContext.Request.Method.ToLowerInvariant();

                // Check if the conditions are met
                if (areaName == "admin" && controllerName == "vendor" && actionName == "edit" && requestMethod == "post")
                {
                    var form = _httpContextAccessor.HttpContext.Request.Form;
                    if (form.ContainsKey("IsVendorShopEnable") && bool.TryParse(form["IsVendorShopEnable"], out var isEnabled))
                    {
                        var vendorFeature = await _vendorShopFeatureService.GetVendorFeatureMappingByVendorIdAsync(vendorModel.Id);
                        if (vendorFeature == null)
                        {
                            await _vendorShopFeatureService.InsertAsync(new Domains.VendorFeatureMapping
                            {
                                VendorId = vendorModel.Id,
                                Enable = isEnabled,
                            });
                        }
                        else
                        {
                            vendorFeature.Enable = isEnabled;
                            await _vendorShopFeatureService.UpdateAsync(vendorFeature);
                        }
                    }
                }

                return;
            }
        }

        public async Task HandleEventAsync(EntityDeletedEvent<Vendor> eventMessage)
        {
            if (eventMessage.Entity.Id > 0)
            {
                await ClearVendorShopFeatureCacheAsync(eventMessage.Entity.Id);
            }
        }


        public async Task HandleEventAsync(EntityUpdatedEvent<Vendor> eventMessage)
        {
            if (eventMessage.Entity.Id > 0)
            {
                await ClearVendorShopFeatureCacheAsync(eventMessage.Entity.Id);
            }
        }
        private async Task ClearVendorShopFeatureCacheAsync(int vendorId)
        {
            await _staticCacheManager.RemoveByPrefixAsync(VendorShopFeatureCacheDefault.GetVendorShopIsEnabledCachePrefix(vendorId));
        }
    }
}
