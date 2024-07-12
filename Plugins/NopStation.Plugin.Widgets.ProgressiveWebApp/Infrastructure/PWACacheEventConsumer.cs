using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Nop.Core.Domain.Customers;
using Nop.Core.Http.Extensions;
using Nop.Services.Events;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Infrastructure.Cache;

namespace NopStation.Plugin.Widgets.ProgressiveWebApp.Infrastructure
{
    public class PWACacheEventConsumer : IConsumer<CustomerLoggedOutEvent>,
        IConsumer<CustomerLoggedinEvent>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PWACacheEventConsumer(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task HandleEventAsync(CustomerLoggedOutEvent eventMessage)
        {
            await _httpContextAccessor.HttpContext.Session.SetAsync(PWAEntityCacheDefaults.CheckSubscription, true);
        }

        public async Task HandleEventAsync(CustomerLoggedinEvent eventMessage)
        {
            await _httpContextAccessor.HttpContext.Session.SetAsync(PWAEntityCacheDefaults.CheckSubscription, true);
        }
    }
}
