using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Web.Models.Order;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Widgets.DMS.Factories;

namespace NopStation.Plugin.Widgets.DMS.Components
{
    [ViewComponent(Name = DMSDefaults.CUSTOMER_SHIPMENT_NOTE_TABLE_VIEW_COMPONENT_NAME)]
    public class CustomerShipmentNoteTableViewComponent : NopStationViewComponent
    {
        private readonly IShipmentNoteModelFactory _shipmentNoteModelFactory;
        #region Fields



        #endregion

        #region Ctor

        public CustomerShipmentNoteTableViewComponent(
            IShipmentNoteModelFactory shipmentNoteModelFactory
            )
        {
            _shipmentNoteModelFactory = shipmentNoteModelFactory;
        }

        #endregion

        #region Utilites



        #endregion

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {

            if (additionalData.GetType() != typeof(ShipmentDetailsModel))
                return Content("");

            var data = additionalData as ShipmentDetailsModel;
            var shipmentId = data.Id;
            var model = await _shipmentNoteModelFactory.PrepareDMSShipmentNoteListModelByShipmentIdAsync(shipmentId, displayToCustomer: true);
            if (model == null || model.ShipmentNotes.Count < 1)
                return Content("");

            return View(model);
        }
    }
}
