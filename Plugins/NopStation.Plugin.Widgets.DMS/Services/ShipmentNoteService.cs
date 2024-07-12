using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using NopStation.Plugin.Widgets.DMS.Domain;

namespace NopStation.Plugin.Widgets.DMS.Services
{
    public class ShipmentNoteService : IShipmentNoteService
    {
        #region Fields

        private readonly IRepository<ShipmentNote> _shipmentNoteRepository;

        #endregion

        #region Ctor

        public ShipmentNoteService(IRepository<ShipmentNote> shipmentNoteRepository)
        {
            _shipmentNoteRepository = shipmentNoteRepository;
        }

        #endregion

        #region Methods

        public async Task InsertShipmentNoteAsync(ShipmentNote shipmentNote)
        {
            await _shipmentNoteRepository.InsertAsync(shipmentNote);
        }

        public async Task UpdateShipmentNoteAsync(ShipmentNote shipmentNote)
        {
            await _shipmentNoteRepository.UpdateAsync(shipmentNote);
        }

        public async Task DeleteShipmentNoteAsync(ShipmentNote shipmentNote)
        {
            await _shipmentNoteRepository.DeleteAsync(shipmentNote);
        }

        public async Task<IList<ShipmentNote>> GetShipmentNoteByNopShipmentIdAsync(int nopShipmentId)
        {
            if (nopShipmentId < 1)
                return null;

            return await _shipmentNoteRepository.Table.Where(x => x.NopShipmentId == nopShipmentId).OrderByDescending(sn => sn.CreatedOnUtc).ToListAsync();
        }

        public async Task<ShipmentNote> GetShipmentNoteByIdAsync(int id)
        {
            if (id == 0)
                return null;

            return await _shipmentNoteRepository.GetByIdAsync(id, cache => default);
        }

        public async Task<IPagedList<ShipmentNote>> GetAllShipmentNotesAsync(
            int? nopShipmentId = null,
            int? courierShipmentId = null,
            bool? displayToCustomer = null,
            bool? displayToShipper = null,
            int pageIndex = 0,
            int pageSize = int.MaxValue)
        {
            if (pageSize == int.MaxValue)
                --pageSize;

            var query = _shipmentNoteRepository.Table;

            if (nopShipmentId.HasValue)
                query = query.Where(x => x.NopShipmentId == nopShipmentId.Value);

            if (courierShipmentId.HasValue)
                query = query.Where(x => x.CourierShipmentId == courierShipmentId.Value);

            if (displayToCustomer.HasValue)
                query = query.Where(x => x.DisplayToCustomer == displayToCustomer.Value);

            if (displayToShipper.HasValue)
                query = query.Where(x => x.DisplayToShipper == displayToShipper.Value);

            return await query.OrderByDescending(x => x.CreatedOnUtc).ToPagedListAsync(pageIndex, pageSize);
        }

        #endregion
    }

}
