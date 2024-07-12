using System.Threading.Tasks;
using NopStation.Plugin.Payments.Dmoney.Domains;

namespace NopStation.Plugin.Payments.Dmoney.Services
{
    public interface IDmoneyPaymentService
    {
        Task<VerifyTransactionResult> VerifyTransactionAsync(string transactionTrackingNo);
    }
}