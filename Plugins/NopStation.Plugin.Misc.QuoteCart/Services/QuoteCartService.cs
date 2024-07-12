using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.ExportImport;
using Nop.Services.Localization;
using NopStation.Plugin.Misc.QuoteCart.Domain;

namespace NopStation.Plugin.Misc.QuoteCart.Services;

public class QuoteCartService : IQuoteCartService
{
    #region Fields

    private readonly ILocalizationService _localizationService;
    private readonly IRepository<QuoteCartItem> _qciRepository;
    private readonly IRepository<PermissionRecord> _permissionRecordRepository;
    private readonly IRepository<PermissionRecordCustomerRoleMapping> _permissionRecordCustomerRoleMappingRepository;
    private readonly IStaticCacheManager _staticCacheManager;
    private readonly IProductAttributeParser _productAttributeParser;
    private readonly QuoteCartSettings _quoteCartSettings;

    #endregion

    #region Ctor

    public QuoteCartService(
        ILocalizationService localizationService,
        IRepository<QuoteCartItem> qciRepository,
        IRepository<PermissionRecord> permissionRecordRepository,
        IRepository<PermissionRecordCustomerRoleMapping> permissionRecordCustomerRoleMappingRepository,
        IStaticCacheManager staticCacheManager,
        IProductAttributeParser productAttributeParser,
        QuoteCartSettings quoteCartSettings)
    {
        _qciRepository = qciRepository;
        _permissionRecordRepository = permissionRecordRepository;
        _permissionRecordCustomerRoleMappingRepository = permissionRecordCustomerRoleMappingRepository;
        _staticCacheManager = staticCacheManager;
        _productAttributeParser = productAttributeParser;
        _quoteCartSettings = quoteCartSettings;
        _localizationService = localizationService;
    }

    #endregion

    #region Utilities 

    protected virtual async Task<bool> ShoppingCartItemIsEqualAsync(QuoteCartItem quoteCartItem,
        Product product,
        string attributesXml)
    {
        if (quoteCartItem.ProductId != product.Id)
            return false;

        //attributes
        var attributesEqual = await _productAttributeParser.AreProductAttributesEqualAsync(quoteCartItem.AttributesXml, attributesXml, false, false);

        if (!attributesEqual)
            return false;

        //gift cards
        if (product.IsGiftCard)
        {
            _productAttributeParser.GetGiftCardAttribute(attributesXml, out var giftCardRecipientName1, out var _, out var giftCardSenderName1, out var _, out var _);

            _productAttributeParser.GetGiftCardAttribute(quoteCartItem.AttributesXml, out var giftCardRecipientName2, out var _, out var giftCardSenderName2, out var _, out var _);

            var giftCardsAreEqual = giftCardRecipientName1.Equals(giftCardRecipientName2, StringComparison.InvariantCultureIgnoreCase)
                && giftCardSenderName1.Equals(giftCardSenderName2, StringComparison.InvariantCultureIgnoreCase);
            if (!giftCardsAreEqual)
                return false;
        }

        return true;
    }

    public virtual async Task<QuoteCartItem> FindQuoteCartItemInTheCartAsync(IList<QuoteCartItem> quoteCart,
        Product product,
        string attributesXml = "",
        decimal customerEnteredPrice = decimal.Zero,
        DateTime? rentalStartDate = null,
        DateTime? rentalEndDate = null)
    {
        ArgumentNullException.ThrowIfNull(quoteCart);

        return product == null
            ? throw new ArgumentNullException(nameof(product))
            : await quoteCart
            .FirstOrDefaultAwaitAsync(async sci => await ShoppingCartItemIsEqualAsync(sci, product, attributesXml));
    }

    #endregion

    #region Methods 

    public virtual async Task<IList<QuoteCartItem>> GetQuoteCartAsync(Customer customer,
        int storeId = 0, int? productId = null, DateTime? createdFromUtc = null, DateTime? createdToUtc = null)
    {
        ArgumentNullException.ThrowIfNull(customer);

        var items = _qciRepository.Table.Where(qci => qci.CustomerId == customer.Id);

        //filter shopping cart items by product
        if (productId > 0)
            items = items.Where(item => item.ProductId == productId);

        //filter shopping cart items by date
        if (createdFromUtc.HasValue)
            items = items.Where(item => createdFromUtc.Value <= item.CreatedOnUtc);
        if (createdToUtc.HasValue)
            items = items.Where(item => createdToUtc.Value >= item.CreatedOnUtc);

        var key = _staticCacheManager.PrepareKeyForDefaultCache(QuoteCartDefaults.QuoteCartItemsAllCacheKey, customer, storeId);

        return await _staticCacheManager.GetAsync(key, async () => await items.ToListAsync());
    }

    public virtual async Task<IList<string>> AddToQuoteCartAsync(Customer customer, Product product, int storeId, string attributesXml = null,
        int quantity = 1, DateTime? rentalStartDate = null, DateTime? rentalEndDate = null)
    {
        ArgumentNullException.ThrowIfNull(customer);

        ArgumentNullException.ThrowIfNull(product);

        var warnings = new List<string>();

        if (quantity <= 0)
        {
            warnings.Add(await _localizationService.GetResourceAsync("NopStation.Plugin.Misc.QuoteCart.Cart.Warnings.QtyError"));
            return warnings;
        }

        var cart = await GetQuoteCartAsync(customer, storeId);

        var quoteCartItem = await FindQuoteCartItemInTheCartAsync(cart, product, attributesXml);

        if (_quoteCartSettings.MaxQuoteItemCount > 0 && cart.Count + (quoteCartItem != null ? 0 : 1) > _quoteCartSettings.MaxQuoteItemCount)
        {
            warnings.Add(string.Format(await _localizationService.GetResourceAsync("NopStation.Plugin.Misc.QuoteCart.Cart.Warnings.MaxQuoteItemCount"), _quoteCartSettings.MaxQuoteItemCount));
            return warnings;
        }

        if (quoteCartItem != null)
        {
            var newQuantity = quoteCartItem.Quantity + quantity;

            quoteCartItem.AttributesXml = attributesXml;
            quoteCartItem.Quantity = newQuantity;
            quoteCartItem.UpdatedOnUtc = DateTime.UtcNow;

            await _qciRepository.UpdateAsync(quoteCartItem);
        }
        else
        {
            var now = DateTime.UtcNow;
            quoteCartItem = new QuoteCartItem
            {
                StoreId = storeId,
                ProductId = product.Id,
                AttributesXml = attributesXml,
                Quantity = quantity,
                CreatedOnUtc = now,
                UpdatedOnUtc = now,
                CustomerId = customer.Id
            };
            if (product.IsRental)
            {
                quoteCartItem.RentalStartDateUtc = rentalStartDate;
                quoteCartItem.RentalEndDateUtc = rentalEndDate;
            }

            await _qciRepository.InsertAsync(quoteCartItem);
        }

        return warnings;
    }

    public virtual async Task ClearCartAsync(Customer customer, int storeId)
    {
        if (customer == null)
            return;

        await _qciRepository.DeleteAsync(x => x.CustomerId == customer.Id && x.StoreId == storeId);

        await _staticCacheManager.RemoveByPrefixAsync(QuoteCartDefaults.QuoteCartItemsByCustomerPrefix, customer.Id);
    }

    public virtual async Task DeleteFromQuoteCartAsync(int itemId)
    {
        var item = _qciRepository.Table.Where(qci => qci.Id == itemId).FirstOrDefault();
        if (item != null)
            await _qciRepository.DeleteAsync(item);
    }

    public async Task UpdateQuoteCartAsync(QuoteCartItem cartItem)
    {
        await _qciRepository.UpdateAsync(cartItem);
    }

    public async Task<PermissionRecord> GetSendQuotePermissionRecordAsync()
    {
        return await _permissionRecordRepository.Table.FirstOrDefaultAsync(x => x.SystemName == QuoteCartPermissionProvider.SendQuoteRequest.SystemName);
    }

    public async Task<IList<int>> GetAllowedCustomerRoleIdsAsync()
    {
        var roleIds = from cr in _permissionRecordCustomerRoleMappingRepository.Table
                      join pr in _permissionRecordRepository.Table on cr.PermissionRecordId equals pr.Id
                      where pr.SystemName == QuoteCartPermissionProvider.SendQuoteRequest.SystemName
                      select cr.CustomerRoleId;

        return await roleIds.ToListAsync();
    }

    public async Task<string> ExportQuoteRequestItemsXmlAsync(IList<QuoteRequestItem> quoteRequestItems)
    {
        var settings = new XmlWriterSettings
        {
            Async = true,
            ConformanceLevel = ConformanceLevel.Auto,
            Encoding = Encoding.UTF8
        };

        await using var stringWriter = new StringWriter();
        await using var xmlWriter = XmlWriter.Create(stringWriter, settings);

        await xmlWriter.WriteStartDocumentAsync();
        await xmlWriter.WriteStartElementAsync("QuoteItems");
        await xmlWriter.WriteAttributeStringAsync("Version", NopVersion.CURRENT_VERSION);

        foreach (var requestItem in quoteRequestItems)
        {
            await xmlWriter.WriteStartElementAsync("QuoteItem");
            await xmlWriter.WriteElementStringAsync(nameof(requestItem.ProductId), null, requestItem.ProductId.ToString());
            await xmlWriter.WriteElementStringAsync(nameof(requestItem.CustomerId), null, requestItem.CustomerId.ToString());
            await xmlWriter.WriteElementStringAsync(nameof(requestItem.Quantity), null, requestItem.Quantity.ToString());
            await xmlWriter.WriteElementStringAsync(nameof(requestItem.QuoteRequestId), null, requestItem.QuoteRequestId.ToString());
            await xmlWriter.WriteElementStringAsync(nameof(requestItem.StoreId), null, requestItem.StoreId.ToString());
            await xmlWriter.WriteElementStringAsync(nameof(requestItem.DiscountedPrice), null, requestItem.DiscountedPrice.ToString());
            await xmlWriter.WriteElementStringAsync(nameof(requestItem.CreatedOnUtc), null, requestItem.CreatedOnUtc.ToString());
            await xmlWriter.WriteElementStringAsync(nameof(requestItem.UpdatedOnUtc), null, requestItem.UpdatedOnUtc.ToString());

            // attributes
            await xmlWriter.WriteStartElementAsync(nameof(requestItem.AttributesXml));
            await xmlWriter.WriteCDataAsync(requestItem.AttributesXml);
            await xmlWriter.WriteEndElementAsync();

            await xmlWriter.WriteEndElementAsync();
        }

        await xmlWriter.WriteEndElementAsync();
        await xmlWriter.WriteEndDocumentAsync();
        await xmlWriter.FlushAsync();

        return stringWriter.ToString();
    }

    #endregion
}
