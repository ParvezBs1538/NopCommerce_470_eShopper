using System.Threading.Tasks;
using NopStation.Plugin.Widgets.DMS.Models;

namespace NopStation.Plugin.Widgets.DMS.Factories
{
    public interface IShipmentNoteModelFactory
    {
        Task<DMSShipmentNoteListModel> PrepareDMSShipmentNoteListModelByShipmentIdAsync(int nopShipmentId,
            bool? displayToCustomer = null,
            bool? displayToShipper = null
            );

        Task<DMSShipmentNoteListModel> PrepareDMSShipmentNoteListModelByCourierShipmentIdAsync(int courierShipmentId,
            bool? displayToCustomer = null,
            bool? displayToShipper = null
            );
    }
}
