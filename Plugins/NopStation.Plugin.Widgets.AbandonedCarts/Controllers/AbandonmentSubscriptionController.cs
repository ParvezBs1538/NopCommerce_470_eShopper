using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Messages;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Widgets.AbandonedCarts.Domain;
using NopStation.Plugin.Widgets.AbandonedCarts.Services;

namespace NopStation.Plugin.Widgets.AbandonedCarts.Controllers
{
    [AutoValidateAntiforgeryToken]
    public class AbandonmentSubscriptionController : NopStationPublicController
    {
        #region Properties

        private readonly ICustomerAbandonmentInfoService _customerAbandonmentInfoService;
        private readonly INotificationService _notificationService;

        #endregion

        #region Ctors

        public AbandonmentSubscriptionController(ICustomerAbandonmentInfoService customerAbandonmentInfoService,
            INotificationService notificationService)
        {
            _customerAbandonmentInfoService = customerAbandonmentInfoService;
            _notificationService = notificationService;
        }

        #endregion

        #region Methods

        public virtual IActionResult Unsubscribe()
        {
            return View();
        }

        [HttpPost]
        public virtual async Task<IActionResult> Unsubscribe(string returnUrl)
        {
            if (returnUrl == null)
                return InvokeHttp404();

            var customerAbandonment = await _customerAbandonmentInfoService.GetCustomerAbandonmentByTokenAsync(returnUrl);

            if (customerAbandonment != null)
            {
                customerAbandonment.StatusId = (int)CustomerAbandonmentStatus.Unsubscribed;
                customerAbandonment.UnsubscribedOnUtc = DateTime.UtcNow;
                await _customerAbandonmentInfoService.AddOrUpdateCustomerAbandonmentAsync(customerAbandonment);

                _notificationService.SuccessNotification("Unsubscribed from Abandoned Carts email list");

                return RedirectToRoute("Homepage");
            }

            _notificationService.ErrorNotification("User not Found to Unsubscribe");
            return View();
        }

        #endregion
    }
}
