using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Widgets.AffiliateStation.Domains;

namespace NopStation.Plugin.Widgets.AffiliateStation.Services
{
    public interface IOrderCommissionService
    {
        Task DeleteOrderCommissionAsync(OrderCommission orderCommission);

        Task InsertOrderCommissionAsync(OrderCommission orderCommission);

        Task UpdateOrderCommissionAsync(OrderCommission orderCommission);

        Task<OrderCommission> GetOrderCommissionByIdAsync(int orderCommissionId);

        Task<(IPagedList<OrderCommission>, decimal, decimal, decimal)> SearchOrderCommissionsAsync(bool loadCommission = false,
           string firstName = "", string lastName = "", int affiliateId = 0, int orderId = 0, IList<int> csIds = null, IList<int> osIds = null,
           IList<int> psIds = null, DateTime? startDateUtc = null, DateTime? endDateUtc = null, int pageIndex = 0, int pageSize = int.MaxValue);
    }
}