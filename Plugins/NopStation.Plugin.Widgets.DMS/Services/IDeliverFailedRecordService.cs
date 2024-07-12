using System;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Widgets.DMS.Domain;

namespace NopStation.Plugin.Widgets.DMS.Services
{
    public interface IDeliverFailedRecordService
    {
        Task DeleteDeliverFailedRecordAsync(DeliverFailedRecord deliverFailedRecord);

        Task InsertDeliverFailedRecordAsync(DeliverFailedRecord deliverFailedRecord);

        Task UpdateDeliverFailedRecordAsync(DeliverFailedRecord deliverFailedRecord);

        Task<DeliverFailedRecord> GetDeliverFailedRecordByIdAsync(int deliverFailedRecordId);

        Task<IPagedList<DeliverFailedRecord>> GetAllDeliverFailedRecordByShipmentIdAsync(int shipmentId,
             int pageIndex = 0, int pageSize = int.MaxValue);

        Task<IPagedList<DeliverFailedRecord>> SearchDeliverFailedRecord(int? shipmentId = null,
            int? shipperId = null,
            int? courierShipmentId = null,
            bool? deleted = null,
            DateTime? createdFromUtc = null,
            DateTime? createdToUtc = null,
            int pageIndex = 0, int pageSize = int.MaxValue);

    }
}
