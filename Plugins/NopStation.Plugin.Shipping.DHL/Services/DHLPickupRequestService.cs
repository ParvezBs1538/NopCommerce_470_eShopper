using Nop.Data;
using NopStation.Plugin.Shipping.DHL.Domain;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace NopStation.Plugin.Shipping.DHL.Services
{
    public class DHLPickupRequestService : IDHLPickupRequestService
    {
        private readonly IRepository<DHLPickupRequest> _dHLPickupRequestRepository;

        public DHLPickupRequestService(IRepository<DHLPickupRequest> dHLPickupRequestRepository)
        {
            _dHLPickupRequestRepository = dHLPickupRequestRepository;
        }

        public async Task DeleteDHLPickupRequestAsync(DHLPickupRequest dHLPickupRequest)
        {
            await _dHLPickupRequestRepository.DeleteAsync(dHLPickupRequest);
        }

        public async Task<DHLPickupRequest> GetDHLPickupRequestByIdAsync(int dHLPickupRequestId)
        {
            if (dHLPickupRequestId == 0)
                return null;

            return await _dHLPickupRequestRepository.GetByIdAsync(dHLPickupRequestId, cache => default);
        }

        public DHLPickupRequest GetDHLPickupRequestByOrderId(int orderId)
        {
            if (orderId == 0)
                return null;

            var entity = _dHLPickupRequestRepository.Table.FirstOrDefault(pur => pur.OrderId == orderId);

            return entity;
        }

        public async Task InsertDHLPickupRequestAsync(DHLPickupRequest dHLPickupRequest)
        {
            await _dHLPickupRequestRepository.InsertAsync(dHLPickupRequest);
        }

        public async Task UpdateDHLPickupRequestAsync(DHLPickupRequest dHLPickupRequest)
        {
            await _dHLPickupRequestRepository.UpdateAsync(dHLPickupRequest);
        }
    }
}
