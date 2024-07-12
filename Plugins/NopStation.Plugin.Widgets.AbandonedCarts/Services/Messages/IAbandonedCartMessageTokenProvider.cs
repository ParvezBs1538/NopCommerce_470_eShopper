using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Stores;
using Nop.Services.Messages;
using NopStation.Plugin.Widgets.AbandonedCarts.Models;

namespace NopStation.Plugin.Widgets.AbandonedCarts.Services.Messages
{
    public interface IAbandonedCartMessageTokenProvider
    {
        Task AddCustomerTokensAsync(IList<Token> tokens, int customerId, string jwtToken);
        Task AddProductTokensAsync(IList<Token> tokens, IList<ProductInfoModel> productInfoModels, int languageId);
        Task AddStoreTokensAsync(IList<Token> tokens, Store store, EmailAccount emailAccount);
    }
}