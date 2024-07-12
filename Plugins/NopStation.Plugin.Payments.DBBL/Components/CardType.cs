using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;

namespace NopStation.Plugin.Payments.DBBL.Components
{
    [ViewComponent(Name = "CardType")]
    public class CardTypeViewComponent : NopViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View("~/Plugins/Payments.DBBL/Views/Components/CardType.cshtml");
        }
    }
}
