using System.Linq;
using Nop.Data;
using NopStation.Plugin.Shipping.DHL.Domain;
using System.Threading.Tasks;

namespace NopStation.Plugin.Shipping.DHL.Services
{
    public class DHLShipmentService : IDHLShipmentService
    {
        #region Fields

        private readonly IRepository<DHLShipment> _dhlShipmentSubmissionRepository;

        #endregion


        #region Ctor

        public DHLShipmentService(IRepository<DHLShipment> dhlShipmentSubmissionRepository)
        {
            _dhlShipmentSubmissionRepository = dhlShipmentSubmissionRepository;
        }

        #endregion


        public async Task DeleteShipmentSubmissionAsync(int id)
        {
            var query = _dhlShipmentSubmissionRepository.Table.Where(d => d.Id == id).FirstOrDefault();

            if (query != null)
                await _dhlShipmentSubmissionRepository.DeleteAsync(query);
        }

        public DHLShipment GetDHLShipmentSubmissionByAirwayBillNumber(string airwayBillNumber)
        {
            if (string.IsNullOrEmpty(airwayBillNumber))
                return null;

            var dhlShipment = from d in _dhlShipmentSubmissionRepository.Table
                              where d.AirwayBillNumber.Equals(airwayBillNumber)
                              select d;

            return dhlShipment.FirstOrDefault();
        }

        public Task<DHLShipment> GetDHLShipmentSubmissionByIdAsync(int id)
        {
            if (id == 0)
                return null;

            return _dhlShipmentSubmissionRepository.GetByIdAsync(id, cache => default);
        }

        public DHLShipment GetDHLShipmentSubmissionByMessageReference(string messageReference)
        {
            var query = _dhlShipmentSubmissionRepository.Table.Where(d => d.MessageReference.Equals(messageReference)).FirstOrDefault();

            if (query != null)
                return query;

            return new DHLShipment();
        }

        public DHLShipment GetDHLShipmentSubmissionByOrderId(int orderId)
        {
            var query = _dhlShipmentSubmissionRepository.Table.Where(d => d.OrderId == orderId).ToList().LastOrDefault();

            if (query != null)
                return query;

            return new DHLShipment();
        }

        public async Task InsertShipmentSubmissionAsync(DHLShipment dhlShipmentSubmission)
        {
            if (dhlShipmentSubmission != null)
            {
                await _dhlShipmentSubmissionRepository.InsertAsync(dhlShipmentSubmission);
            }
        }

        public async Task UpdateShipmentSubmission(DHLShipment dhlShipmentSubmission)
        {
            if (dhlShipmentSubmission != null)
            {
                await _dhlShipmentSubmissionRepository.UpdateAsync(dhlShipmentSubmission);
            }
        }
    }
}
