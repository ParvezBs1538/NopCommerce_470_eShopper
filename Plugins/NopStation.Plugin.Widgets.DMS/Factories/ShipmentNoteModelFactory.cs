using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using NopStation.Plugin.Widgets.DMS.Domain;
using NopStation.Plugin.Widgets.DMS.Models;
using NopStation.Plugin.Widgets.DMS.Services;

namespace NopStation.Plugin.Widgets.DMS.Factories
{
    public class ShipmentNoteModelFactory : IShipmentNoteModelFactory
    {
        private readonly ICustomerService _customerService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IShipmentNoteService _shipmentNoteService;
        #region Fields



        #endregion

        #region Ctor

        public ShipmentNoteModelFactory(
            ICustomerService customerService,
            IDateTimeHelper dateTimeHelper,
            IShipmentNoteService shipmentNoteService
            )
        {
            _customerService = customerService;
            _dateTimeHelper = dateTimeHelper;
            _shipmentNoteService = shipmentNoteService;
        }

        #endregion

        #region Utilites

        protected async Task<DMSShipmentNoteModel> PrepareCustomerDMSShipmentNoteModelAsync(ShipmentNote shipmentNote)
        {
            if (shipmentNote == null)
                throw new ArgumentNullException(nameof(shipmentNote));

            var updatedByCustomerEmail = "";
            var updatedByCustomer = await _customerService.GetCustomerByIdAsync(shipmentNote.UpdatedByCustomerId);
            updatedByCustomerEmail = updatedByCustomer?.Email ?? "";

            var model = shipmentNote.ToModel<DMSShipmentNoteModel>();
            model.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(shipmentNote.CreatedOnUtc, DateTimeKind.Utc);
            model.UpdatedByCustomerEmail = updatedByCustomerEmail;

            return model;
        }


        #endregion

        #region Methods

        public virtual async Task<DMSShipmentNoteListModel> PrepareDMSShipmentNoteListModelByShipmentIdAsync(int nopShipmentId,
            bool? displayToCustomer = null,
            bool? displayToShipper = null
            )
        {
            if (nopShipmentId < 1)
                return null;

            var model = new DMSShipmentNoteListModel();

            var shipmentNotes = await _shipmentNoteService.GetAllShipmentNotesAsync(
                nopShipmentId: nopShipmentId,
                displayToCustomer: displayToCustomer,
                displayToShipper: displayToShipper
                );

            var data = await shipmentNotes.SelectAwait(async shipmentNote => await PrepareCustomerDMSShipmentNoteModelAsync(shipmentNote)).ToListAsync();
            model.ShipmentNotes = data;

            return model;
        }

        public virtual async Task<DMSShipmentNoteListModel> PrepareDMSShipmentNoteListModelByCourierShipmentIdAsync(int courierShipmentId,
            bool? displayToCustomer = null,
            bool? displayToShipper = null
            )
        {
            if (courierShipmentId < 1)
                return null;

            var model = new DMSShipmentNoteListModel();

            var shipmentNotes = await _shipmentNoteService.GetAllShipmentNotesAsync(
                courierShipmentId: courierShipmentId,
                displayToCustomer: displayToCustomer,
                displayToShipper: displayToShipper
                );

            var data = await shipmentNotes.SelectAwait(async shipmentNote => await PrepareCustomerDMSShipmentNoteModelAsync(shipmentNote)).ToListAsync();
            model.ShipmentNotes = data;

            return model;
        }

        #endregion
    }
}
