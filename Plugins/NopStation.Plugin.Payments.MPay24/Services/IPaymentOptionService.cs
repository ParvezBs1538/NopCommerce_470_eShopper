using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Payments.MPay24.Domains;

namespace NopStation.Plugin.Payments.MPay24.Services
{
    public interface IPaymentOptionService
    {
        Task InsertPaymentOptionAsync(PaymentOption paymentOption);

        Task InsertPaymentOptionAsync(IList<PaymentOption> paymentOptions);

        Task UpdatePaymentOptionAsync(PaymentOption paymentOption);

        Task DeletePaymentOptionAsync(PaymentOption paymentOption);

        Task<IPagedList<PaymentOption>> GetAllMPay24PaymentOptionsAsync(string name = "", string brand = "",
            bool? active = null, int storeId = 0, int pageIndex = 0, int pageSize = int.MaxValue);

        Task<PaymentOption> GetPaymentOptionByIdAsync(int id);

        Task<PaymentOption> GetPaymentOptionByShortNameAsync(string shortName);
    }
}
