using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Web.Models.Catalog;

namespace NopStation.Plugin.Widgets.GoogleTagManager.Services
{
    public interface IGTMService
    {
        Task<string> GetProductDetailsAsync(int productId, int quantity, int index = 0);
        Task<string> PrepareProductItemsAsync(IList<ShoppingCartItem> cart);
        Task<string> PrepareOrderItemsAsync(int orderId);
        Task<string> PrepareRemoveFromCartEcommerceAsync(ShoppingCartItem cartItem);
        Task<string> GetProductIdAsync(int productId);
        Task<string> GetPurchaseEcommerceScriptAsync(int orderId);
        Task<string> GetCategoriesAsync(int productId);
        Task<string> GetCustomerScriptAsync(int customerId);
        Task<string> GetCategoryEcommerceScript(IList<ProductOverviewModel> products);
        Task<string> PrepareShoppingCartScriptAsync(Customer customer, int storeId);
    }
}