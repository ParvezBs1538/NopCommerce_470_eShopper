using System.Linq;
using System.Threading.Tasks;
using Nop.Data;
using NopStation.Plugin.Payout.Stripe.Domain;

namespace NopStation.Plugin.Payout.Stripe.Services
{
    public class VendorStripeConfigurationService : IVendorStripeConfigurationService
    {
        private readonly IRepository<VendorStripeConfiguration> _repository;

        public VendorStripeConfigurationService(IRepository<VendorStripeConfiguration> repository)
        {
            _repository = repository;
        }
        public async Task<VendorStripeConfiguration> GetVendorStripeConfigurationByVendorIdAsync(int vendorId)
        {
            return await _repository.Table.Where(x => x.VendorId == vendorId).FirstOrDefaultAsync();
        }

        public async Task InsertVendorStripeConfigutationAsync(VendorStripeConfiguration vendorStripeConfiguration)
        {
            await _repository.InsertAsync(vendorStripeConfiguration);
        }

        public async Task UpdateVendorStripeConfigutationAsync(VendorStripeConfiguration vendorStripeConfiguration)
        {
            await _repository.UpdateAsync(vendorStripeConfiguration);
        }
    }
}
