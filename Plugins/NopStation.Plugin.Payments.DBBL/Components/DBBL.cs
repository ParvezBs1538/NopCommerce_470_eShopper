using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Payments.DBBL.Models;
using Nop.Web.Framework.Components;

namespace NopStation.Plugin.Payments.DBBL.Components
{
    [ViewComponent(Name = "DBBL")]
    public class DBBLViewComponent : NopViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var model = new CardListModel();
            model.AvailableCards.Add(new CardListModel.CardModel()
            {
                DisplayName = "DBBL Nexus",
                LogoUrl = "/Plugins/Payments.DBBL/Content/Images/dbbl-nexus.png",
                Value = "1"
            });
            model.AvailableCards.Add(new CardListModel.CardModel()
            {
                DisplayName = "DBBL Master Debit",
                LogoUrl = "/Plugins/Payments.DBBL/Content/Images/dbbl-master.png",
                Value = "2"
            });
            model.AvailableCards.Add(new CardListModel.CardModel()
            {
                DisplayName = "DBBL VISA Debit",
                LogoUrl = "/Plugins/Payments.DBBL/Content/Images/dbbl-visa.png",
                Value = "3"
            });
            model.AvailableCards.Add(new CardListModel.CardModel()
            {
                DisplayName = "VISA",
                LogoUrl = "/Plugins/Payments.DBBL/Content/Images/visa.png",
                Value = "4"
            });
            model.AvailableCards.Add(new CardListModel.CardModel()
            {
                DisplayName = "Master",
                LogoUrl = "/Plugins/Payments.DBBL/Content/Images/Master.png",
                Value = "5"
            });
            model.AvailableCards.Add(new CardListModel.CardModel()
            {
                DisplayName = "Rocket",
                LogoUrl = "/Plugins/Payments.DBBL/Content/Images/dbbl-mb.png",
                Value = "6"
            });

            return View("~/Plugins/Payments.DBBL/Views/Components/DBBL.cshtml", model);
        }
    }
}
