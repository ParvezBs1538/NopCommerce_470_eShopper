using System.Threading.Tasks;
using NopStation.Plugin.Payments.MPay24.Areas.Admin.Models;
using NopStation.Plugin.Payments.MPay24.Domains;

namespace NopStation.Plugin.Payments.MPay24.Areas.Admin.Factories
{
    public interface IPaymentOptionModelFactory
    {
        Task<PaymentOptionSearchModel> PreparePaymentOptionSearchModelAsync(PaymentOptionSearchModel searchModel);

        Task<PaymentOptionListModel> PreparePaymentOptionListModelAsync(PaymentOptionSearchModel searchModel);

        Task<PaymentOptionModel> PreparePaymentOptionModelAsync(PaymentOptionModel model, PaymentOption paymentOption,
            bool excludeProperties = false);
    }
}
