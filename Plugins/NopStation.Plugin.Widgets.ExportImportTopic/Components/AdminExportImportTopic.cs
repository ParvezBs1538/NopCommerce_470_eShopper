using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Misc.Core.Components;
using System.Threading.Tasks;
using Nop.Services.Security;

namespace NopStation.Plugin.Widgets.PopupLogin.Components
{
    public class AdminExportImportTopicViewComponent : NopStationViewComponent
    {
        private readonly IPermissionService _permissionService;

        public AdminExportImportTopicViewComponent(
            IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageTopics))
                return Content("");

            return View("~/Plugins/NopStation.Plugin.Widgets.ExportImportTopic/Views/Default.cshtml");
        }
    }
}
