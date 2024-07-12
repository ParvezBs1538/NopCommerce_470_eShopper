using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Nop.Core.Http.Extensions;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Infrastructure.Cache;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Models;

namespace NopStation.Plugin.Widgets.ProgressiveWebApp.Factories
{
    public class ProgressiveWebAppModelFactory : IProgressiveWebAppModelFactory
    {
        private readonly ProgressiveWebAppSettings _progressiveWebAppSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProgressiveWebAppModelFactory(ProgressiveWebAppSettings progressiveWebAppSettings,
            IHttpContextAccessor httpContextAccessor)
        {
            _progressiveWebAppSettings = progressiveWebAppSettings;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<FooterComponentModel> PrepareFooterComponentModelAsync()
        {
            var checkSubscription = await _httpContextAccessor.HttpContext.Session.GetAsync<string>(PWAEntityCacheDefaults.CheckSubscription);

            var model = new FooterComponentModel()
            {
                PublicKey = _progressiveWebAppSettings.VapidPublicKey,
                CheckPushManagerSubscription = bool.TryParse(checkSubscription, out var check) ? check : true
            };

            await _httpContextAccessor.HttpContext.Session.SetAsync(PWAEntityCacheDefaults.CheckSubscription, false);

            return model;
        }
    }
}
