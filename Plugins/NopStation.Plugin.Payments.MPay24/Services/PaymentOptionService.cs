using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using NopStation.Plugin.Payments.MPay24.Domains;
using Nop.Services.Stores;
using NopStation.Plugin.Misc.Core.Caching;
using Nop.Core.Caching;
using NopStation.Plugin.Payments.MPay24.Services.Cache;

namespace NopStation.Plugin.Payments.MPay24.Services
{
    public class PaymentOptionService : IPaymentOptionService
    {
        private readonly IStaticCacheManager _cacheManager;
        private readonly IRepository<PaymentOption> _paymentOptionRepository;
        private readonly IStoreMappingService _storeMappingService;

        public PaymentOptionService(IStaticCacheManager cacheManager,
            IRepository<PaymentOption> paymentOptionRepository,
            IStoreMappingService storeMappingService)
        {
            _cacheManager = cacheManager;
            _paymentOptionRepository = paymentOptionRepository;
            _storeMappingService = storeMappingService;
        }

        public async Task<IPagedList<PaymentOption>> GetAllMPay24PaymentOptionsAsync(string name = "", string brand = "",
            bool? active = null, int storeId = 0, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var cacheKey = _cacheManager.PrepareKeyForDefaultCache(MPay24CacheDefaults.PaymentOptionAllKey,
                name, brand, active, storeId, pageIndex, pageSize);

            return await _cacheManager.GetAsync(cacheKey, async () =>
            {
                var query = from pa in _paymentOptionRepository.Table
                            where (string.IsNullOrWhiteSpace(brand) || pa.Brand.Contains(brand)) &&
                            (string.IsNullOrWhiteSpace(name) || pa.DisplayName.Contains(name) ||
                            pa.ShortName.Contains(name) || pa.Description.Contains(name)) &&
                            (!active.HasValue || pa.Active == active.Value)
                            select pa;

                if (storeId > 0)
                    query = await _storeMappingService.ApplyStoreMapping(query, storeId);

                query = query.OrderBy(x => x.DisplayOrder);

                return await query.ToPagedListAsync(pageIndex, pageSize);
            });
        }

        public async Task InsertPaymentOptionAsync(PaymentOption paymentOption)
        {
            await _paymentOptionRepository.InsertAsync(paymentOption);
        }

        public async Task InsertPaymentOptionAsync(IList<PaymentOption> paymentOptions)
        {
            await _paymentOptionRepository.InsertAsync(paymentOptions);
        }

        public async Task UpdatePaymentOptionAsync(PaymentOption paymentOption)
        {
            await _paymentOptionRepository.UpdateAsync(paymentOption);
        }

        public async Task DeletePaymentOptionAsync(PaymentOption paymentOption)
        {
            await _paymentOptionRepository.DeleteAsync(paymentOption);
        }

        public async Task<PaymentOption> GetPaymentOptionByIdAsync(int id)
        {
            return await _paymentOptionRepository.GetByIdAsync(id, cache =>
                _cacheManager.PrepareKeyForDefaultCache(NopStationEntityCacheDefaults<PaymentOption>.ByIdCacheKey, id));
        }

        public async Task<PaymentOption> GetPaymentOptionByShortNameAsync(string shortName)
        {
            var cacheKey = _cacheManager.PrepareKeyForDefaultCache(MPay24CacheDefaults.PaymentOptionByShortNameKey, shortName);

            return await _cacheManager.GetAsync(cacheKey, async () =>
            {
                return await _paymentOptionRepository.Table.FirstOrDefaultAsync(x => x.ShortName == shortName);
            });
        }

        //public async Task ManipulateOrderDetailsModelAsync(OrderDetailsModel orderDetailsModel)
        //{
        //    try
        //    {
        //        var allMPayPaymentOptions = await GetAllMPay24PaymentOptionsAsync();
        //        var order = await _orderService.GetOrderByIdAsync(orderDetailsModel.Id);
        //        var mPay24PaymentOption = await _genericAttributeService.GetAttributeAsync<string>(order, await _localizationService.GetResourceAsync("Plugins.Payments.MPay24.OrderMPay24PaymentOption"));

        //        var mPay24PaymentMethod = await _paymentPluginManager
        //            .LoadPluginBySystemNameAsync(MPay24PaymentDefaults.SystemName);
        //        var mPay24PaymentMethodName = mPay24PaymentMethod != null ? await _localizationService.GetLocalizedFriendlyNameAsync(mPay24PaymentMethod, (await _workContext.GetWorkingLanguageAsync()).Id) : MPay24PaymentDefaults.SystemName;

        //        if (orderDetailsModel.PaymentMethod.Equals(mPay24PaymentMethodName, StringComparison.InvariantCultureIgnoreCase) && !string.IsNullOrEmpty(mPay24PaymentOption) && allMPayPaymentOptions.Any(f => f.ShortName == mPay24PaymentOption))
        //        {
        //            orderDetailsModel.PaymentMethod = allMPayPaymentOptions.FirstOrDefault(f => f.ShortName == mPay24PaymentOption)?.DisplayName;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //    }
        //}
    }
}
