using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Widgets.AffiliateStation.Areas.Admin.Factories;
using NopStation.Plugin.Widgets.AffiliateStation.Areas.Admin.Models;
using NopStation.Plugin.Widgets.AffiliateStation.Domains;
using NopStation.Plugin.Widgets.AffiliateStation.Extensions;
using NopStation.Plugin.Widgets.AffiliateStation.Services;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace NopStation.Plugin.Widgets.AffiliateStation.Areas.Admin.Controllers
{
    public class OrderCommissionController : NopStationAdminController
    {
        #region Fileds

        private readonly IOrderCommissionService _orderCommissionService;
        private readonly IOrderCommissionModelFactory _orderCommissionModelFactory;
        private readonly IPermissionService _permissionService;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IDateTimeHelper _dateTimeHelper;

        #endregion

        #region Ctor

        public OrderCommissionController(IOrderCommissionService orderCommissionService,
            IOrderCommissionModelFactory orderCommissionModelFactory,
            IPermissionService permissionService,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IDateTimeHelper dateTimeHelper)
        {
            _orderCommissionService = orderCommissionService;
            _orderCommissionModelFactory = orderCommissionModelFactory;
            _permissionService = permissionService;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _dateTimeHelper = dateTimeHelper;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Index()
        {
            if (!await _permissionService.AuthorizeAsync(AffiliateStationPermissionProvider.ManageOrderCommission))
                return AccessDeniedView();

            return RedirectToAction("List");
        }

        public async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(AffiliateStationPermissionProvider.ManageOrderCommission))
                return AccessDeniedView();

            var model = await _orderCommissionModelFactory.PrepareOrderCommissionSearchModelAsync(new OrderCommissionSearchModel());

            return View(model);
        }

        public async Task<IActionResult> GetList(OrderCommissionSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(AffiliateStationPermissionProvider.ManageOrderCommission))
                return AccessDeniedView();

            var model = await _orderCommissionModelFactory.PrepareOrderCommissionListModelAsync(searchModel);
            return Json(model);
        }

        public async Task<IActionResult> ReportAggregates(OrderCommissionSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(AffiliateStationPermissionProvider.ManageOrderCommission))
                return AccessDeniedView();

            var model = await _orderCommissionModelFactory.PrepareCommissionAggregatorModelAsync(searchModel);
            return Json(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(AffiliateStationPermissionProvider.ManageOrderCommission))
                return AccessDeniedView();

            //try to get a affiliate customer with the specified id
            var orderCommission = await _orderCommissionService.GetOrderCommissionByIdAsync(id);
            if (orderCommission == null)
                return RedirectToAction("List");

            var model = await _orderCommissionModelFactory.PrepareOrderCommissionModelAsync(null, orderCommission);
            return View(model);
        }

        [EditAccess, HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired(FormValueRequirement.StartsWith, "save")]
        public async Task<IActionResult> Edit(OrderCommissionModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(AffiliateStationPermissionProvider.ManageOrderCommission))
                return AccessDeniedView();

            //try to get a affiliate customer with the specified id
            var orderCommission = await _orderCommissionService.GetOrderCommissionByIdAsync(model.Id);
            if (orderCommission == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                orderCommission = model.ToEntity(orderCommission);

                if (model.CommissionStatusId != (int)CommissionStatus.Pending)
                    orderCommission.CommissionPaidOn = _dateTimeHelper.ConvertToUtcTime(model.CommissionPaidOn.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());

                await _orderCommissionService.UpdateOrderCommissionAsync(orderCommission);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.AffiliateStation.OrderCommissions.Updated"));

                if (continueEditing)
                    return RedirectToAction("Edit", new { id = model.Id });

                return RedirectToAction("List");
            }

            model = await _orderCommissionModelFactory.PrepareOrderCommissionModelAsync(model, orderCommission);
            return View(model);
        }

        [EditAccess, HttpPost, ActionName("Edit")]
        [FormValueRequired("btnSaveTotalCommissionAmount")]
        public virtual async Task<IActionResult> EditTotalCommissionAmount(int id, OrderCommissionModel model)
        {
            if (!await _permissionService.AuthorizeAsync(AffiliateStationPermissionProvider.ManageOrderCommission))
                return AccessDeniedView();

            //try to get a affiliate customer with the specified id
            var orderCommission = await _orderCommissionService.GetOrderCommissionByIdAsync(id);
            if (orderCommission == null)
                return RedirectToAction("List");

            orderCommission.TotalCommissionAmount = model.TotalCommissionAmount;

            await _orderCommissionService.UpdateOrderCommissionAsync(orderCommission);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.AffiliateStation.OrderCommissions.Updated"));

            return RedirectToAction("Edit", new { id = model.Id });
        }

        [EditAccess, HttpPost]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(AffiliateStationPermissionProvider.ManageOrderCommission))
                return AccessDeniedView();

            //try to get a affiliate customer with the specified id
            var orderCommission = await _orderCommissionService.GetOrderCommissionByIdAsync(id);
            if (orderCommission == null)
                return RedirectToAction("List");

            await _orderCommissionService.DeleteOrderCommissionAsync(orderCommission);

            return RedirectToAction("List");
        }

        #endregion
    }
}
