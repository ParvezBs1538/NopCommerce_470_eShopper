using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Widgets.DMS.Domain;

namespace NopStation.Plugin.Widgets.DMS.Services
{
    public interface IShipmentNoteService
    {
        Task DeleteShipmentNoteAsync(ShipmentNote shipmentNote);

        Task<IPagedList<ShipmentNote>> GetAllShipmentNotesAsync(
            int? nopShipmentId = null,
            int? courierShipmentId = null,
            bool? displayToCustomer = null,
            bool? displayToShipper = null,
            int pageIndex = 0,
            int pageSize = int.MaxValue);

        Task<ShipmentNote> GetShipmentNoteByIdAsync(int id);

        Task<IList<ShipmentNote>> GetShipmentNoteByNopShipmentIdAsync(int nopShipmentId);

        Task InsertShipmentNoteAsync(ShipmentNote shipmentNote);

        Task UpdateShipmentNoteAsync(ShipmentNote shipmentNote);
    }
}