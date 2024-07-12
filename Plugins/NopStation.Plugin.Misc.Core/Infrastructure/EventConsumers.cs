using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Nop.Services.Events;
using Nop.Web.Framework.Events;

namespace NopStation.Plugin.Misc.Core.Infrastructure;

public class EventConsumers : IConsumer<PageRenderingEvent>
{
    private readonly IActionContextAccessor _actionContextAccessor;

    public EventConsumers(IActionContextAccessor actionContextAccessor)
    {
        _actionContextAccessor = actionContextAccessor;
    }

    public Task HandleEventAsync(PageRenderingEvent eventMessage)
    {
        var area = _actionContextAccessor.ActionContext.HttpContext.GetRouteValue("area");
        if (area != null && area.ToString().Equals("admin", StringComparison.InvariantCultureIgnoreCase))
        {
            eventMessage.Helper.AppendCssFileParts("~/Plugins/NopStation.Core/contents/css/style.css", "");
        }

        return Task.CompletedTask;
    }
}