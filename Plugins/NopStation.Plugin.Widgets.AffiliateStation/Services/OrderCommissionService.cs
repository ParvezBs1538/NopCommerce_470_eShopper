using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Affiliates;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Orders;
using Nop.Data;
using NopStation.Plugin.Widgets.AffiliateStation.Domains;
using NopStation.Plugin.Widgets.AffiliateStation.Services.Cache;

namespace NopStation.Plugin.Widgets.AffiliateStation.Services
{
    public class OrderCommissionService : IOrderCommissionService
    {
        #region Fields

        private readonly IRepository<Address> _addressRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<Affiliate> _affiliateRepository;
        private readonly IRepository<OrderCommission> _orderCommissionRepository;
        private readonly IStaticCacheManager _staticCacheManager;

        #endregion

        #region Ctor

        public OrderCommissionService(IRepository<Address> addressRepository,
            IRepository<Order> orderRepository,
            IRepository<Affiliate> affiliateRepository,
            IRepository<OrderCommission> orderCommissionRepository,
            IStaticCacheManager staticCacheManager)
        {
            _addressRepository = addressRepository;
            _orderRepository = orderRepository;
            _affiliateRepository = affiliateRepository;
            _orderCommissionRepository = orderCommissionRepository;
            _staticCacheManager = staticCacheManager;
        }

        #endregion

        #region Methods

        public async Task DeleteOrderCommissionAsync(OrderCommission orderCommission)
        {
            await _orderCommissionRepository.DeleteAsync(orderCommission);
        }

        public async Task InsertOrderCommissionAsync(OrderCommission orderCommission)
        {
            await _orderCommissionRepository.InsertAsync(orderCommission);
        }

        public async Task<OrderCommission> GetOrderCommissionByIdAsync(int id)
        {
            if (id == 0)
                return null;

            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(AffiliateStationCacheDefaults.OrderCommissionByIdKey, id);

            return await _staticCacheManager.GetAsync(cacheKey, async () =>
                await _orderCommissionRepository.Table.FirstOrDefaultAsync(x => x.OrderId == id));
        }

        public async Task<(IPagedList<OrderCommission>, decimal, decimal, decimal)> SearchOrderCommissionsAsync(bool loadCommission = false,
            string firstName = "", string lastName = "", int affiliateId = 0, int orderId = 0, IList<int> csIds = null, IList<int> osIds = null,
            IList<int> psIds = null, DateTime? startDateUtc = null, DateTime? endDateUtc = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            decimal totalCommission = 0;
            decimal payableCommission = 0;
            decimal paidCommission = 0;

            csIds ??= new List<int>();
            osIds ??= new List<int>();
            psIds ??= new List<int>();

            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(AffiliateStationCacheDefaults.OrderCommissionAllKey,
                loadCommission, firstName, lastName, affiliateId, orderId, csIds, osIds, psIds, startDateUtc, endDateUtc, pageIndex, pageSize);

            return await _staticCacheManager.GetAsync(cacheKey, async () =>
            {
                var query = from oc in _orderCommissionRepository.Table
                            join af in _affiliateRepository.Table on oc.AffiliateId equals af.Id
                            join o in _orderRepository.Table on oc.OrderId equals o.Id
                            join ad in _addressRepository.Table on af.AddressId equals ad.Id
                            where !af.Deleted &&
                            (string.IsNullOrWhiteSpace(firstName) || ad.FirstName.Contains(firstName)) &&
                            (string.IsNullOrWhiteSpace(lastName) || ad.LastName.Contains(lastName)) &&
                            (affiliateId == 0 || o.AffiliateId == affiliateId) &&
                            (orderId == 0 || o.Id == orderId) &&
                            (startDateUtc == null || o.CreatedOnUtc >= startDateUtc) &&
                            (endDateUtc == null || o.CreatedOnUtc <= endDateUtc) &&
                            (csIds.Count == 0 || csIds.Contains(oc.CommissionStatusId)) &&
                            (osIds.Count == 0 || osIds.Contains(o.OrderStatusId)) &&
                            (psIds.Count == 0 || psIds.Contains(o.PaymentStatusId))
                            orderby o.CreatedOnUtc descending
                            select oc;

                if (loadCommission)
                {
                    totalCommission = await query.SumAsync(x => x.TotalCommissionAmount);
                    payableCommission = (from oc in query
                                         join o in _orderRepository.Table on oc.OrderId equals o.Id
                                         where o.PaymentStatusId == 30
                                         select oc.TotalCommissionAmount).Sum();

                    paidCommission = (from oc in query
                                      join o in _orderRepository.Table on oc.OrderId equals o.Id
                                      where o.PaymentStatusId == 30 && (oc.CommissionStatusId == 30 || oc.CommissionStatusId == 20)
                                      select new
                                      {
                                          CommissionAmount = oc.CommissionStatusId == 20 ? oc.PartialPaidAmount : oc.TotalCommissionAmount
                                      }).Sum(x => x.CommissionAmount);

                    payableCommission -= paidCommission;
                }

                return (await query.ToPagedListAsync(pageIndex, pageSize), totalCommission, payableCommission, paidCommission);
            });
        }

        public async Task UpdateOrderCommissionAsync(OrderCommission orderCommission)
        {
            await _orderCommissionRepository.UpdateAsync(orderCommission);
        }

        #endregion
    }
}
