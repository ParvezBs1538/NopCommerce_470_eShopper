using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Authentication.External;
using NopStation.Plugin.Misc.Core.Components;

namespace NopStation.Plugin.ExternalAuth.Google.Components
{
    public class GoogleAuthenticationViewComponent : NopStationViewComponent
    {
        private readonly IAuthenticationPluginManager _authenticationPluginManager;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;

        public GoogleAuthenticationViewComponent(IAuthenticationPluginManager authenticationPluginManager,
            IStoreContext storeContext,
            IWorkContext workContext)
        {
            _authenticationPluginManager = authenticationPluginManager;
            _storeContext = storeContext;
            _workContext = workContext;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var methodIsAvailable = await _authenticationPluginManager.IsPluginActiveAsync(GoogleAuthenticationDefaults.SystemName, await _workContext.GetCurrentCustomerAsync(),
                (await _storeContext.GetCurrentStoreAsync()).Id);

            if (methodIsAvailable)
                return Content(string.Empty);

            return View();
        }
    }
}