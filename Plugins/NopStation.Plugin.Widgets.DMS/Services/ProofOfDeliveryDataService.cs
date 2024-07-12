using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Data;
using NopStation.Plugin.Widgets.DMS.Domain;

namespace NopStation.Plugin.Widgets.DMS.Services
{
    public class ProofOfDeliveryDataService : IProofOfDeliveryDataService
    {
        #region Fields

        private readonly IRepository<ProofOfDeliveryData> _proofOfDeliveryDataRepository;

        #endregion

        #region Ctor

        public ProofOfDeliveryDataService(IRepository<ProofOfDeliveryData> proofOfDeliveryDataRepository)
        {
            _proofOfDeliveryDataRepository = proofOfDeliveryDataRepository;
        }

        #endregion

        #region Utilites



        #endregion

        #region Method

        public virtual async Task DeleteProofOfDeliveryDataAsync(ProofOfDeliveryData proofOfDeliveryData)
        {
            if (proofOfDeliveryData == null)
                throw new ArgumentNullException(nameof(proofOfDeliveryData));

            await _proofOfDeliveryDataRepository.DeleteAsync(proofOfDeliveryData);
        }


        public virtual async Task InsertroofOfDeliveryDataAsync(ProofOfDeliveryData proofOfDeliveryData)
        {
            if (proofOfDeliveryData == null)
                throw new ArgumentNullException(nameof(proofOfDeliveryData));

            await _proofOfDeliveryDataRepository.InsertAsync(proofOfDeliveryData);
        }


        public virtual async Task UpdateProofOfDeliveryDataAsync(ProofOfDeliveryData proofOfDeliveryData)
        {
            if (proofOfDeliveryData == null)
                throw new ArgumentNullException(nameof(proofOfDeliveryData));

            await _proofOfDeliveryDataRepository.UpdateAsync(proofOfDeliveryData);
        }

        public virtual async Task<ProofOfDeliveryData> GetProofOfDeliveryDataByIdAsync(int proofOfDeliveryDataId)
        {
            if (proofOfDeliveryDataId < 1)
                throw new ArgumentNullException(nameof(proofOfDeliveryDataId));

            return await _proofOfDeliveryDataRepository.GetByIdAsync(proofOfDeliveryDataId);
        }

        public virtual async Task<ProofOfDeliveryData> GetProofOfDeliveryDataByNopShipmentIdAsync(int nopShipmentId)
        {
            if (nopShipmentId < 1)
                throw new ArgumentNullException(nameof(nopShipmentId));

            var query = _proofOfDeliveryDataRepository.Table.OrderByDescending(pod => pod.Id);

            await Task.CompletedTask;

            return query.FirstOrDefault(pod => pod.NopShipmentId == nopShipmentId);
        }

        public virtual async Task<ProofOfDeliveryData> GetProofOfDeliveryDataByCourierShipmentIdAsync(int courierShipmentId)
        {
            if (courierShipmentId < 1)
                throw new ArgumentNullException(nameof(courierShipmentId));

            var query = _proofOfDeliveryDataRepository.Table.OrderByDescending(pod => pod.Id);

            await Task.CompletedTask;

            return query.FirstOrDefault(pod => pod.CourierShipmentId == courierShipmentId);
        }


        public virtual async Task<IList<ProofOfDeliveryData>> GetProofOfDeliveryDataByIdsAsync(int[] proofOfDeliveryDataIds)
        {
            if (!proofOfDeliveryDataIds.Any())
                throw new ArgumentNullException(nameof(proofOfDeliveryDataIds));

            return await _proofOfDeliveryDataRepository.GetByIdsAsync(proofOfDeliveryDataIds);
        }

        #endregion
    }
}
