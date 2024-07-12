using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Orders;
using Nop.Data;

namespace NopStation.Plugin.Widgets.ProductRibbon.Services
{
    public class BestSellerService : IBestSellerService
    {
        #region Fields

        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<OrderItem> _orderItemRepository;
        private readonly ProductRibbonSettings _productRibbonSettings;
        private readonly IStoreContext _storeContext;
        private readonly IStaticCacheManager _cacheManager;

        #endregion

        #region Ctor

        public BestSellerService(IRepository<Order> orderRepository,
            IRepository<OrderItem> orderItemRepository,
            ProductRibbonSettings productRibbonSettings,
            IStoreContext storeContext,
            IStaticCacheManager cacheManager)
        {
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _productRibbonSettings = productRibbonSettings;
            _storeContext = storeContext;
            _cacheManager = cacheManager;
        }

        #endregion

        public virtual async Task<BestsellersReportLine> BestSellerReportAsync(int productId)
        {
            var storeId = !_productRibbonSettings.BestSellStoreWise ? 0 : (await _storeContext.GetCurrentStoreAsync()).Id;
            var sids = _productRibbonSettings.BestSellShippingStatusIds ?? new List<int>();
            var pids = _productRibbonSettings.BestSellPaymentStatusIds ?? new List<int>();
            var oids = _productRibbonSettings.BestSellOrderStatusIds ?? new List<int>();
            var days = _productRibbonSettings.SoldInDays;

            var cacheKey = _cacheManager.PrepareKeyForDefaultCache(ProductRibbonDefaults.BestSellerKey, productId, storeId, sids, pids, oids, days);

            return await _cacheManager.GetAsync(cacheKey, () =>
            {
                var createdFromUtc = DateTime.UtcNow.AddDays(-days);

                var bestSellers = from orderItem in _orderItemRepository.Table
                                  join o in _orderRepository.Table on orderItem.OrderId equals o.Id
                                  where (storeId == 0 || storeId == o.StoreId) &&
                                      createdFromUtc <= o.CreatedOnUtc &&
                                      (!sids.Any() || sids.Contains(o.ShippingStatusId)) &&
                                      (!pids.Any() || pids.Contains(o.PaymentStatusId)) &&
                                      (!oids.Any() || oids.Contains(o.OrderStatusId)) &&
                                      orderItem.ProductId == productId
                                  select orderItem;

                var bsReport =
                    //group by products
                    from orderItem in bestSellers
                    group orderItem by orderItem.ProductId into g
                    select new BestsellersReportLine
                    {
                        ProductId = g.Key,
                        TotalAmount = g.Sum(x => x.PriceExclTax),
                        TotalQuantity = g.Sum(x => x.Quantity)
                    };

                return bsReport.FirstOrDefault();
            });
        }
    }
}
