using System.Threading.Tasks;
using NopStation.Plugin.Misc.QuoteCart.Domain;

namespace NopStation.Plugin.Misc.QuoteCart.Services.Request;

public interface IRequestProcessingService
{
    bool CanCancelRequest(QuoteRequest request);

    Task CancelRequestAsync(QuoteRequest request, bool notifyCustomer);

    Task SetRequestStatusAsync(QuoteRequest request, RequestStatus requestStatus, bool notifyCustomer);
}