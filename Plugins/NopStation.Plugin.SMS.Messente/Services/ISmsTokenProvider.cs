using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Stores;
using Nop.Core.Domain.Vendors;
using NopStation.Plugin.SMS.Messente.Domains;
using Nop.Services.Messages;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NopStation.Plugin.SMS.Messente.Services
{
    public interface ISmsTokenProvider
    {
        IEnumerable<string> GetTokenGroups(SmsTemplate smsTemplate);

        IEnumerable<string> GetListOfAllowedTokens(IEnumerable<string> tokenGroups);

        Task AddCustomerTokensAsync(IList<Token> commonTokens, Customer customer);

        Task AddStoreTokensAsync(IList<Token> tokens, Store store);

        Task AddOrderTokensAsync(IList<Token> commonTokens, Order order, int languageId, int vendorId = 0);

        Task AddShipmentTokensAsync(IList<Token> commonTokens, Shipment shipment, int languageId);

        void AddOTPTokens(IList<Token> tokens, string otp);

        Task AddOrderRefundedTokensAsync(IList<Token> commonTokens, Order order, decimal refundedAmount);

        Task AddOrderNoteTokensAsync(IList<Token> commonTokens, OrderNote orderNote);

        Task AddRecurringPaymentTokensAsync(IList<Token> commonTokens, RecurringPayment recurringPayment);

        Task AddNewsLetterSubscriptionTokensAsync(IList<Token> commonTokens, NewsLetterSubscription subscription);

        Task AddProductTokensAsync(IList<Token> commonTokens, Product product, int languageId);

        Task AddReturnRequestTokensAsync(IList<Token> commonTokens, ReturnRequest returnRequest, OrderItem orderItem);

        Task AddForumTopicTokensAsync(IList<Token> commonTokens, ForumTopic forumTopic,
            int? friendlyForumTopicPageIndex = null, int? appendedPostIdentifierAnchor = null);

        Task AddForumTokensAsync(IList<Token> commonTokens, Forum forum);

        Task AddForumPostTokensAsync(IList<Token> commonTokens, ForumPost forumPost);

        Task AddPrivateMessageTokensAsync(IList<Token> commonTokens, PrivateMessage privateMessage);

        Task AddVendorTokensAsync(IList<Token> commonTokens, Vendor vendor);

        Task AddGiftCardTokensAsync(IList<Token> commonTokens, GiftCard giftCard);

        Task AddProductReviewTokensAsync(IList<Token> commonTokens, ProductReview productReview);

        Task AddAttributeCombinationTokensAsync(IList<Token> commonTokens, ProductAttributeCombination combination, int languageId);

        Task AddBackInStockTokensAsync(IList<Token> commonTokens, BackInStockSubscription subscription);
    }
}