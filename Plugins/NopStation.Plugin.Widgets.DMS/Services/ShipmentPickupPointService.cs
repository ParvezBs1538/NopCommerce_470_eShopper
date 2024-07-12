using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using NopStation.Plugin.Widgets.DMS.Domain;

namespace NopStation.Plugin.Widgets.DMS.Services
{
    public class ShipmentPickupPointService : IShipmentPickupPointService
    {
        #region Fields

        private readonly IRepository<ShipmentPickupPoint> _shipmentPickupPointRepository;

        #endregion

        #region Ctor

        public ShipmentPickupPointService(IRepository<ShipmentPickupPoint> shipmentPickupPointRepository)
        {
            _shipmentPickupPointRepository = shipmentPickupPointRepository;
        }

        #endregion

        #region Methods

        public async Task InsertShipmentPickupPointAsync(ShipmentPickupPoint shipmentPickupPoint)
        {
            await _shipmentPickupPointRepository.InsertAsync(shipmentPickupPoint);
        }

        public async Task UpdateShipmentPickupPointAsync(ShipmentPickupPoint shipmentPickupPoint)
        {
            await _shipmentPickupPointRepository.UpdateAsync(shipmentPickupPoint);
        }

        public async Task DeleteShipmentPickupPointAsync(ShipmentPickupPoint shipmentPickupPoint)
        {
            await _shipmentPickupPointRepository.DeleteAsync(shipmentPickupPoint);
        }

        public async Task<ShipmentPickupPoint> GetShipmentPickupPointByIdAsync(int id)
        {
            if (id == 0)
                return null;

            return await _shipmentPickupPointRepository.GetByIdAsync(id, cache => default);
        }

        public async Task<IPagedList<ShipmentPickupPoint>> GetAllShipmentPickupPointsAsync(int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _shipmentPickupPointRepository.Table;

            return await query.OrderBy(x => x.Id)
                .ToPagedListAsync(pageIndex, pageSize);
        }

        #endregion
    }

}
