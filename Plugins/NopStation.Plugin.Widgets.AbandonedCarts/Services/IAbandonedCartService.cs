using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Customers;
using NopStation.Plugin.Widgets.AbandonedCarts.Domain;
using NopStation.Plugin.Widgets.AbandonedCarts.Models;

namespace NopStation.Plugin.Widgets.AbandonedCarts.Services
{
    public interface IAbandonedCartService
    {
        Task AddOrUpdateAbandonedCartAsync(AbandonedCartModel abandonmentCart);
        Task UpdateAbandonmentStatusAsync();
        Task<IList<Customer>> GetFirstAbandonedCustomersAsync();
        Task<IList<Customer>> GetSecondAbandonedCustomersAsync();
        Task<IList<ProductInfoModel>> GetProductsByCustomerAsync(int customerId);
        Task<IList<AbandonedCart>> GetAbandonedCartsByCustomerIdAsync(int customerId);
        Task BulkUpdateAbandonmentCarts(IList<AbandonedCart> abandonedCarts);
        Task<AbandonedCart> GetLastInactiveAbandonedCartByCustomerAsync(int customerId);
        Task<bool> IsCustomerOnlineByCustomerUsernameAsync(string username);
        Task<IPagedList<AbandonmentListViewModel>> GetAllAbandonmentsAsync(string firstName = "",
            string lastName = "",
            string email = "",
            int pageIndex = 0,
            int pageSize = int.MaxValue,
            int statusId = 0,
            int customerId = 0,
            int? productId = null,
            DateTime? createdFromUtc = null,
            DateTime? createdToUtc = null);
        Task<AbandonmentDetailsViewModel> GetAbandonedCartDetailByIdAsync(int id);
        Task<AbandonedCart> GetAbandonedCartByShoppingCartIdAsync(int shoppingCartItemId);
        Task<int> BulkDeleteAbandonedCartsAsync(AbandonmentMaintenanceModel maintenanceModel);
        Task<int> GetCustomerAbandonedCartsCountAsync(string firstName = "",
            string lastName = "",
            string email = "",
            int statusId = 0,
            int customerId = 0,
            int? productId = null,
            DateTime? createdFromUtc = null,
            DateTime? createdToUtc = null);
    }
}
