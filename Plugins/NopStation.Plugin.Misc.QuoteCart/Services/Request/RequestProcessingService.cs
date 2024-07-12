using System;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Services.Customers;
using NopStation.Plugin.Misc.QuoteCart.Domain;
using NopStation.Plugin.Misc.QuoteCart.Services.Email;

namespace NopStation.Plugin.Misc.QuoteCart.Services.Request;

public partial class RequestProcessingService : IRequestProcessingService
{
    #region Fields

    private readonly IQuoteRequestService _quoteRequestService;
    private readonly IQuoteCartEmailService _quoteCartEmailService;
    private readonly IWorkContext _workContext;
    private readonly ICustomerService _customerService;

    #endregion

    #region Ctor

    public RequestProcessingService(
        IQuoteRequestService quoteRequestService,
        IQuoteCartEmailService quoteCartEmailService,
        IWorkContext workContext,
        ICustomerService customerService)
    {
        _quoteRequestService = quoteRequestService;
        _quoteCartEmailService = quoteCartEmailService;
        _workContext = workContext;
        _customerService = customerService;
    }

    #endregion

    #region Methods

    public virtual bool CanCancelRequest(QuoteRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.RequestStatus == RequestStatus.Cancelled)
            return false;

        return true;
    }


    public virtual async Task SetRequestStatusAsync(QuoteRequest request, RequestStatus requestStatus, bool notifyCustomer)
    {
        ArgumentNullException.ThrowIfNull(request);

        //set and save new request status
        request.RequestStatusId = (int)requestStatus;
        await _quoteRequestService.UpdateQuoteRequestAsync(request);

        //notifications
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (customer != null)
        {
            var customerFullName = await _customerService.GetCustomerFullNameAsync(customer);

            if (notifyCustomer)
                //notification
                await _quoteCartEmailService
                    .SendEmailAsync(request, QuoteRequestNotificationType.CustomerStatusChanged, customerFullName, customer.Email);
        }
    }

    public virtual async Task CancelRequestAsync(QuoteRequest request, bool notifyCustomer)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!CanCancelRequest(request))
            throw new NopException("Cannot do cancel for request.");

        //cancel Request
        await SetRequestStatusAsync(request, RequestStatus.Cancelled, notifyCustomer);
    }

    #endregion
}
