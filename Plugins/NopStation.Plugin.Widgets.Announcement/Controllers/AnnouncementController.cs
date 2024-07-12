using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Http.Extensions;
using NopStation.Plugin.Misc.Core.Controllers;

namespace NopStation.Plugin.Widgets.Announcement.Controllers;

public class AnnouncementController : NopStationPublicController
{
    [HttpPost]
    public async Task<IActionResult> Close()
    {
        await HttpContext.Session.SetAsync(AnnouncementDefaults.Closed, true);
        return Json(new { result = true });
    }

    [HttpPost]
    public async Task<IActionResult> Minimize(bool minimize)
    {
        await HttpContext.Session.SetAsync(AnnouncementDefaults.Minimized, minimize);
        return Json(new { result = true });
    }
}
