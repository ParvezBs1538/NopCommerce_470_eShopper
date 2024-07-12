using NopStation.Plugin.Shipping.DHL.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NopStation.Plugin.Shipping.DHL.Services
{
    public interface IDHLShipmentService
    {
        Task DeleteShipmentSubmissionAsync(int id);

        DHLShipment GetDHLShipmentSubmissionByAirwayBillNumber(string airwayBillNumber);

        Task<DHLShipment> GetDHLShipmentSubmissionByIdAsync(int id);

        DHLShipment GetDHLShipmentSubmissionByMessageReference(string messageReference);

        DHLShipment GetDHLShipmentSubmissionByOrderId(int orderId);

        Task InsertShipmentSubmissionAsync(DHLShipment dhlShipmentSubmission);

        Task UpdateShipmentSubmission(DHLShipment dhlShipmentSubmission);
    }
}
