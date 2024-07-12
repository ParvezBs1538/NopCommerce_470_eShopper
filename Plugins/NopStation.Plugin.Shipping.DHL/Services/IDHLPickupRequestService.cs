using NopStation.Plugin.Shipping.DHL.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NopStation.Plugin.Shipping.DHL.Services
{
    public interface IDHLPickupRequestService
    {
        Task DeleteDHLPickupRequestAsync(DHLPickupRequest dHLPickupRequest);

        Task<DHLPickupRequest> GetDHLPickupRequestByIdAsync(int dHLPickupRequestId);

        DHLPickupRequest GetDHLPickupRequestByOrderId(int orderId);

        Task InsertDHLPickupRequestAsync(DHLPickupRequest dHLPickupRequest);

        Task UpdateDHLPickupRequestAsync(DHLPickupRequest dHLPickupRequest);
    }
}
