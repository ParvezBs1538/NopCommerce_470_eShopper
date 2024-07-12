using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using NopStation.Plugin.Widgets.DMS.Domain;

namespace NopStation.Plugin.Widgets.DMS.Services
{
    public class DeliverFailedRecordService : IDeliverFailedRecordService
    {
        #region Fields

        private readonly IRepository<DeliverFailedRecord> _deliverFailedRecordRepository;

        #endregion

        #region Ctor

        public DeliverFailedRecordService(IRepository<DeliverFailedRecord> deliverFailedRecordRepository)
        {
            _deliverFailedRecordRepository = deliverFailedRecordRepository;
        }

        #endregion

        #region Utilites



        #endregion

        #region Methods

        public virtual async Task DeleteDeliverFailedRecordAsync(DeliverFailedRecord deliverFailedRecord)
        {
            if (deliverFailedRecord == null)
                throw new ArgumentNullException(nameof(deliverFailedRecord));

            await _deliverFailedRecordRepository.DeleteAsync(deliverFailedRecord);
        }

        public virtual async Task InsertDeliverFailedRecordAsync(DeliverFailedRecord deliverFailedRecord)
        {
            if (deliverFailedRecord == null)
                throw new ArgumentNullException(nameof(deliverFailedRecord));

            await _deliverFailedRecordRepository.InsertAsync(deliverFailedRecord);
        }

        public virtual async Task UpdateDeliverFailedRecordAsync(DeliverFailedRecord deliverFailedRecord)
        {
            if (deliverFailedRecord == null)
                throw new ArgumentNullException(nameof(deliverFailedRecord));

            await _deliverFailedRecordRepository.UpdateAsync(deliverFailedRecord);
        }

        public virtual async Task<DeliverFailedRecord> GetDeliverFailedRecordByIdAsync(int deliverFailedRecordId)
        {
            if (deliverFailedRecordId == 0)
                throw new ArgumentNullException(nameof(deliverFailedRecordId));

            return await _deliverFailedRecordRepository.GetByIdAsync(deliverFailedRecordId);
        }


        public virtual async Task<IPagedList<DeliverFailedRecord>> GetAllDeliverFailedRecordByShipmentIdAsync(int shipmentId,
             int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (shipmentId == 0)
                throw new ArgumentNullException(nameof(shipmentId));

            return await _deliverFailedRecordRepository.GetAllPagedAsync(query =>
            {
                query = query.Where(dfr => dfr.ShipmentId == shipmentId);

                return query.OrderByDescending(q => q.Id);
            }, pageIndex, pageSize);
        }

        public virtual async Task<IPagedList<DeliverFailedRecord>> SearchDeliverFailedRecord(int? shipmentId = null,
            int? shipperId = null,
            int? courierShipmentId = null,
            bool? deleted = null,
            DateTime? createdFromUtc = null,
            DateTime? createdToUtc = null,
            int pageIndex = 0, int pageSize = int.MaxValue)
        {
            return await _deliverFailedRecordRepository.GetAllPagedAsync(query =>
            {
                if (deleted.HasValue)
                    query = query.Where(q => q.Deleted == deleted.Value);

                if (shipmentId != null && shipmentId > 0)
                    query = query.Where(q => q.ShipmentId == shipmentId);

                if (shipperId != null && shipperId > 0)
                    query = query.Where(q => q.ShipperId == shipperId);

                if (courierShipmentId != null && courierShipmentId > 0)
                    query = query.Where(q => q.CourierShipmentId == courierShipmentId);

                if (createdFromUtc.HasValue)
                    query = query.Where(q => createdFromUtc.Value <= q.CreatedOnUtc);

                if (createdToUtc.HasValue)
                    query = query.Where(q => createdToUtc.Value >= q.CreatedOnUtc);

                return query.OrderByDescending(q => q.Id);
            }, pageIndex, pageSize);
        }



        #endregion
    }
}
