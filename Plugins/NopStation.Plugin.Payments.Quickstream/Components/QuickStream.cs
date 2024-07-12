using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Services.Media;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Payments.Quickstream.Models;
using NopStation.Plugin.Payments.Quickstream.Services;

namespace NopStation.Plugin.Payments.Quickstream.Components
{
    public class QuickStreamViewComponent : NopStationViewComponent
    {
        #region Fields

        private readonly IAcceptedCardService _acceptedCardService;
        private readonly IPictureService _pictureService;

        #endregion

        #region Ctor

        public QuickStreamViewComponent(IAcceptedCardService acceptedCardService,
            IPictureService pictureService)
        {
            _acceptedCardService = acceptedCardService;
            _pictureService = pictureService;
        }

        #endregion

        #region Methods

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            var acceptedCards = await _acceptedCardService.GetAllAcceptedCardsAsync(enabled: true);

            var model = new PaymentInfoViewModel();
            foreach (var acceptedCard in acceptedCards)
                model.AcceptCardUrls.Add(await _pictureService.GetPictureUrlAsync(acceptedCard.PictureId, 100));

            //years
            for (var i = 0; i < 15; i++)
            {
                var year = (DateTime.Now.Year + i).ToString();
                model.ExpireYears.Add(new SelectListItem { Text = year, Value = year, });
            }

            //months
            for (var i = 1; i <= 12; i++)
            {
                model.ExpireMonths.Add(new SelectListItem { Text = i.ToString("D2"), Value = i.ToString(), });
            }

            //set postback values (we cannot access "Form" with "GET" requests)
            if (Request.Method != WebRequestMethods.Http.Get)
            {
                var form = Request.Form;
                model.CardholderName = form["CardholderName"];
                model.CardNumber = form["CardNumber"];
                model.CardCode = form["CardCode"];
                var selectedCcType = model.PaymentTypes.FirstOrDefault(x => x.Value.Equals(form["PaymentType"], StringComparison.InvariantCultureIgnoreCase));
                if (selectedCcType != null)
                    selectedCcType.Selected = true;
                var selectedMonth = model.ExpireMonths.FirstOrDefault(x => x.Value.Equals(form["ExpireMonth"], StringComparison.InvariantCultureIgnoreCase));
                if (selectedMonth != null)
                    selectedMonth.Selected = true;
                var selectedYear = model.ExpireYears.FirstOrDefault(x => x.Value.Equals(form["ExpireYear"], StringComparison.InvariantCultureIgnoreCase));
                if (selectedYear != null)
                    selectedYear.Selected = true;
            }

            return View(model);
        }

        #endregion
    }
}