using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Events;
using Nop.Services.Events;
using NopStation.Plugin.Misc.QuoteCart.Domain;
using NopStation.Plugin.Misc.QuoteCart.Services.Request;

namespace NopStation.Plugin.Misc.QuoteCart.Infrastructure.Cache;

public class QuoteCartEventConsumer :
    IConsumer<EntityInsertedEvent<QuoteCartItem>>,
    IConsumer<EntityUpdatedEvent<QuoteCartItem>>,
    IConsumer<EntityDeletedEvent<QuoteCartItem>>,
    IConsumer<EntityInsertedEvent<QuoteForm>>,
    IConsumer<EntityUpdatedEvent<QuoteForm>>,
    IConsumer<EntityDeletedEvent<QuoteForm>>,
    IConsumer<EntityInsertedEvent<QuoteRequest>>,
    IConsumer<EntityUpdatedEvent<QuoteRequest>>,
    IConsumer<EntityDeletedEvent<QuoteRequest>>,
    IConsumer<EntityInsertedEvent<QuoteRequestItem>>,
    IConsumer<EntityUpdatedEvent<QuoteRequestItem>>,
    IConsumer<EntityDeletedEvent<QuoteRequestItem>>
{
    #region Fields

    private readonly IQuoteRequestItemService _quoteRequestItemService;
    private readonly IStaticCacheManager _staticCacheManager;
    private readonly IWorkContext _workContext;

    #endregion

    #region Ctor

    public QuoteCartEventConsumer(
        IQuoteRequestItemService quoteRequestItemService,
        IStaticCacheManager staticCacheManager,
        IWorkContext workContext)
    {
        _quoteRequestItemService = quoteRequestItemService;
        _staticCacheManager = staticCacheManager;
        _workContext = workContext;
    }

    #endregion

    #region Methods

    public async Task HandleEventAsync(EntityUpdatedEvent<QuoteCartItem> eventMessage)
    {
        var prefix = string.Format(QuoteCartDefaults.QuoteCartItemsByCustomerPrefix, (await _workContext.GetCurrentCustomerAsync()).Id);
        await _staticCacheManager.RemoveByPrefixAsync(prefix);
    }

    public async Task HandleEventAsync(EntityDeletedEvent<QuoteCartItem> eventMessage)
    {
        var prefix = string.Format(QuoteCartDefaults.QuoteCartItemsByCustomerPrefix, (await _workContext.GetCurrentCustomerAsync()).Id);
        await _staticCacheManager.RemoveByPrefixAsync(prefix);
    }

    public async Task HandleEventAsync(EntityInsertedEvent<QuoteCartItem> eventMessage)
    {
        var prefix = string.Format(QuoteCartDefaults.QuoteCartItemsByCustomerPrefix, (await _workContext.GetCurrentCustomerAsync()).Id);
        await _staticCacheManager.RemoveByPrefixAsync(prefix);
    }


    public async Task HandleEventAsync(EntityUpdatedEvent<QuoteForm> eventMessage)
    {
        var prefix = string.Format(QuoteCartDefaults.QuoteCartItemsByCustomerPrefix, (await _workContext.GetCurrentCustomerAsync()).Id);
        await _staticCacheManager.RemoveByPrefixAsync(prefix);
    }

    public async Task HandleEventAsync(EntityDeletedEvent<QuoteForm> eventMessage)
    {
        var prefix = string.Format(QuoteCartDefaults.QuoteCartItemsByCustomerPrefix, (await _workContext.GetCurrentCustomerAsync()).Id);
        await _staticCacheManager.RemoveByPrefixAsync(prefix);
    }

    public async Task HandleEventAsync(EntityInsertedEvent<QuoteForm> eventMessage)
    {
        var prefix = string.Format(QuoteCartDefaults.QuoteCartItemsByCustomerPrefix, (await _workContext.GetCurrentCustomerAsync()).Id);
        await _staticCacheManager.RemoveByPrefixAsync(prefix);
    }


    public async Task HandleEventAsync(EntityUpdatedEvent<QuoteRequest> eventMessage)
    {
        var prefix = string.Format(QuoteCartDefaults.QuoteCartItemsByCustomerPrefix, (await _workContext.GetCurrentCustomerAsync()).Id);
        await _staticCacheManager.RemoveByPrefixAsync(prefix);
    }

    public async Task HandleEventAsync(EntityDeletedEvent<QuoteRequest> eventMessage)
    {
        var prefix = string.Format(QuoteCartDefaults.QuoteCartItemsByCustomerPrefix, (await _workContext.GetCurrentCustomerAsync()).Id);
        await _staticCacheManager.RemoveByPrefixAsync(prefix);

        // delete all request items of the request
        var requestItems = await _quoteRequestItemService.GetAllQuoteRequestItemsAsync(requestId: eventMessage.Entity.Id);
        if (!requestItems.Any())
            return;

        foreach (var requestItem in requestItems)
        {
            await _quoteRequestItemService.DeleteQuoteRequestItemAsync(requestItem);
        }
    }

    public async Task HandleEventAsync(EntityInsertedEvent<QuoteRequest> eventMessage)
    {
        var prefix = string.Format(QuoteCartDefaults.QuoteCartItemsByCustomerPrefix, (await _workContext.GetCurrentCustomerAsync()).Id);
        await _staticCacheManager.RemoveByPrefixAsync(prefix);
    }

    public async Task HandleEventAsync(EntityUpdatedEvent<QuoteRequestItem> eventMessage)
    {
        var prefix = string.Format(QuoteCartDefaults.QuoteCartItemsByCustomerPrefix, (await _workContext.GetCurrentCustomerAsync()).Id);
        await _staticCacheManager.RemoveByPrefixAsync(prefix);
    }

    public async Task HandleEventAsync(EntityDeletedEvent<QuoteRequestItem> eventMessage)
    {
        var prefix = string.Format(QuoteCartDefaults.QuoteCartItemsByCustomerPrefix, (await _workContext.GetCurrentCustomerAsync()).Id);
        await _staticCacheManager.RemoveByPrefixAsync(prefix);
    }

    public async Task HandleEventAsync(EntityInsertedEvent<QuoteRequestItem> eventMessage)
    {
        var prefix = string.Format(QuoteCartDefaults.QuoteCartItemsByCustomerPrefix, (await _workContext.GetCurrentCustomerAsync()).Id);
        await _staticCacheManager.RemoveByPrefixAsync(prefix);
    }

    #endregion
}
