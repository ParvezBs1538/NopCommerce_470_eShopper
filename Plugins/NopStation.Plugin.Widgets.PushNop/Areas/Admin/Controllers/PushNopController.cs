using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Security;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Widgets.PushNop.Areas.Admin.Factories;
using NopStation.Plugin.Widgets.PushNop.Areas.Admin.Models;

namespace NopStation.Plugin.Widgets.PushNop.Areas.Admin.Controllers
{
    public class PushNopController : NopStationAdminController
    {
        #region Fields

        private readonly IPushNotificationHomeFactory _pushNotificationHomeModel;
        private readonly IPermissionService _permissionService;

        #endregion

        #region Ctor

        public PushNopController(IPushNotificationHomeFactory pushNotificationHomeModel,
            IPermissionService permissionService)
        {
            _permissionService = permissionService;
            _pushNotificationHomeModel = pushNotificationHomeModel;
        }

        #endregion

        #region Method   

        public async Task<IActionResult> Index()
        {
            if (!await _permissionService.AuthorizeAsync(PushNopPermissionProvider.ManageReports))
                return AccessDeniedView();

            //prepare model
            var model = await _pushNotificationHomeModel.PreparePushNotificationDashboardModelAsync(new PushNopDashbordModel());

            return View(model);
        }

        #endregion
    }
}
