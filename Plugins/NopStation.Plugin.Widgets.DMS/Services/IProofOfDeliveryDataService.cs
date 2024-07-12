using System.Collections.Generic;
using System.Threading.Tasks;
using NopStation.Plugin.Widgets.DMS.Domain;

namespace NopStation.Plugin.Widgets.DMS.Services
{
    public interface IProofOfDeliveryDataService
    {
        Task DeleteProofOfDeliveryDataAsync(ProofOfDeliveryData proofOfDeliveryData);

        Task InsertroofOfDeliveryDataAsync(ProofOfDeliveryData proofOfDeliveryData);

        Task UpdateProofOfDeliveryDataAsync(ProofOfDeliveryData proofOfDeliveryData);

        Task<ProofOfDeliveryData> GetProofOfDeliveryDataByIdAsync(int proofOfDeliveryDataId);

        Task<IList<ProofOfDeliveryData>> GetProofOfDeliveryDataByIdsAsync(int[] proofOfDeliveryDataIds);

        Task<ProofOfDeliveryData> GetProofOfDeliveryDataByCourierShipmentIdAsync(int courierShipmentId);

        Task<ProofOfDeliveryData> GetProofOfDeliveryDataByNopShipmentIdAsync(int nopShipmentId);
    }
}
