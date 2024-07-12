using System.Linq;
using System.Threading.Tasks;
using Nop.Data;
using NopStation.Plugin.Payout.PayPal.Domain;

namespace NopStation.Plugin.Payout.PayPal.Services
{
    public class VendorPayPalConfigurationService : IVendorPayPalConfigurationService
    {
        private readonly IRepository<VendorPayPalConfiguration> _repository;

        public VendorPayPalConfigurationService(IRepository<VendorPayPalConfiguration> repository)
        {
            _repository = repository;
        }
        public async Task<VendorPayPalConfiguration> GetVendorPayPalConfigurationByVendorIdAsync(int vendorId)
        {
            if (vendorId <= 0)
            {
                return null;
            }
            return await _repository.Table.Where(x => x.VendorId == vendorId).FirstOrDefaultAsync();
        }

        public async Task InsertVendorPaypalConfigutationAsync(VendorPayPalConfiguration vendorPayPalConfiguration)
        {
            await _repository.InsertAsync(vendorPayPalConfiguration);
        }
        public async Task UpdateVendorPaypalConfigutationAsync(VendorPayPalConfiguration vendorPayPalConfiguration)
        {
            await _repository.UpdateAsync(vendorPayPalConfiguration);
        }
    }
}
