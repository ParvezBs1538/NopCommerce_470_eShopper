using System;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Widgets.AbandonedCarts.Models;

namespace NopStation.Plugin.Widgets.AbandonedCarts.Services
{
    public interface ICustomerAbandonmentInfoService
    {
        Task AddOrUpdateCustomerAbandonmentAsync(CustomerAbandonmentInfoModel customerAbandonment);
        Task<CustomerAbandonmentInfoModel> GetCustomerAbandonmentByCustomerIdAsync(int id);
        Task<CustomerAbandonmentInfoModel> GetCustomerAbandonmentByTokenAsync(string token);
        Task<IPagedList<CustomerAbandonmentInfoModel>> GetAllCustomerAbandonmentsAsync(string firstName = "",
                        string lastName = "",
                        string email = "",
                        int pageIndex = 0,
                        int pageSize = int.MaxValue,
                        int statusId = 0,
                        int customerId = 0,
                        int? productId = null,
                        DateTime? createdFromUtc = null,
                        DateTime? createdToUtc = null);
    }
}
