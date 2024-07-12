using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using Nop.Services.Customers;
using NopStation.Plugin.Misc.QuoteCart.Domain;
using NopStation.Plugin.Misc.QuoteCart.Services.Email;

namespace NopStation.Plugin.Misc.QuoteCart.Services.RequestMessage;

public class QuoteRequestMessageService : IQuoteRequestMessageService
{
    #region Fields

    private readonly ICustomerService _customerService;
    private readonly IQuoteCartEmailService _quoteCartEmailService;
    private readonly IQuoteFormService _quoteFormService;
    private readonly IRepository<QuoteRequestMessage> _quoteRequestMessageRepository;
    private readonly IStoreContext _storeContext;
    private readonly IWorkContext _workContext;

    #endregion

    #region Ctor

    public QuoteRequestMessageService(
        ICustomerService customerService,
        IQuoteCartEmailService quoteCartEmailService,
        IQuoteFormService quoteFormService,
        IRepository<QuoteRequestMessage> quoteRequestMessageRepository,
        IStoreContext storeContext,
        IWorkContext workContext)
    {
        _customerService = customerService;
        _quoteCartEmailService = quoteCartEmailService;
        _quoteFormService = quoteFormService;
        _quoteRequestMessageRepository = quoteRequestMessageRepository;
        _storeContext = storeContext;
        _workContext = workContext;
    }

    #endregion

    #region Methods

    public async Task DeleteQuoteRequestMessageAsync(QuoteRequestMessage requestMessage)
    {
        await _quoteRequestMessageRepository.DeleteAsync(requestMessage);
    }

    public async Task<IPagedList<QuoteRequestMessage>> GetAllQuoteRequestMessagesAsync(int requestId = 0, int storeId = 0, int pageIndex = 0, int pageSize = int.MaxValue)
    {
        var query = from s in _quoteRequestMessageRepository.Table
                    select s;

        if (storeId > 0)
            query = query.Where(s => s.StoreId == storeId);

        if (requestId > 0)
            query = query.Where(s => s.QuoteRequestId == requestId);

        return await query.ToPagedListAsync(pageIndex, pageSize);
    }

    public async Task<QuoteRequestMessage> GetQuoteRequestMessageByIdAsync(int messageId)
    {
        if (messageId == 0)
            return null;

        return await _quoteRequestMessageRepository.GetByIdAsync(messageId, includeDeleted: false);
    }


    public async Task SendMessageAsync(QuoteRequest quoteRequest, string message)
    {
        ArgumentNullException.ThrowIfNull(quoteRequest);

        if (string.IsNullOrWhiteSpace(message))
            return;

        var currentUser = await _workContext.GetCurrentCustomerAsync();
        var store = await _storeContext.GetCurrentStoreAsync();

        var quoteRequestMessage = new QuoteRequestMessage
        {
            QuoteRequestId = quoteRequest.Id,
            Content = message,
            StoreId = store.Id,
            CustomerId = currentUser.Id,
            CreatedOnUtc = DateTime.UtcNow
        };
        await InsertQuoteRequestMessageAsync(quoteRequestMessage);

        var form = await _quoteFormService.GetFormByIdAsync(quoteRequest.FormId);

        if (form == null)
            return;

        var isAuthor = quoteRequest.CustomerId == currentUser.Id;
        var customer = await _customerService.GetCustomerByIdAsync(quoteRequest.CustomerId);

        var customerFullName = await _customerService.GetCustomerFullNameAsync(customer);

        if (isAuthor && form.SendEmailToCustomer)
        {
            await _quoteCartEmailService.SendEmailAsync(quoteRequest, QuoteRequestNotificationType.CustomerReplySent, customerFullName, customer.Email, message);
        }
        else if (form.SendEmailToStoreOwner)
        {
            await _quoteCartEmailService.SendEmailToStoreOwner(quoteRequest, QuoteRequestNotificationType.StoreReplySent, message);
        }
    }

    public async Task InsertQuoteRequestMessageAsync(QuoteRequestMessage requestMessage)
    {
        await _quoteRequestMessageRepository.InsertAsync(requestMessage);
    }

    public async Task UpdateQuoteRequestMessageAsync(QuoteRequestMessage requestMessage)
    {
        await _quoteRequestMessageRepository.UpdateAsync(requestMessage);
    }

    #endregion
}
