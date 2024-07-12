using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Customers;
using Nop.Data;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using NopStation.Plugin.Widgets.AbandonedCarts.Domain;
using NopStation.Plugin.Widgets.AbandonedCarts.Models;

namespace NopStation.Plugin.Widgets.AbandonedCarts.Services
{
    public class CustomerAbandonmentInfoService : ICustomerAbandonmentInfoService
    {
        #region Constants

        private static CacheKey CustomerAbandonmentCacheKey => new("Nop.customerabandonment.all-{0}", CustomerAbandonmentPrefix);
        private static string CustomerAbandonmentPrefix => "Nop.customerabandonment.{0}";

        #endregion

        private readonly IRepository<CustomerAbandonmentInfo> _customerAbandonmentRepository;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<AbandonedCart> _abandonedCartRepository;

        public CustomerAbandonmentInfoService(IRepository<CustomerAbandonmentInfo> customerAbandonmentRepository,
            IStaticCacheManager staticCacheManager,
            IRepository<Customer> customerRepository,
            IRepository<AbandonedCart> abandonedCartRepository)
        {
            _customerAbandonmentRepository = customerAbandonmentRepository;
            _staticCacheManager = staticCacheManager;
            _customerRepository = customerRepository;
            _abandonedCartRepository = abandonedCartRepository;
        }

        public virtual async Task<CustomerAbandonmentInfoModel> GetCustomerAbandonmentByCustomerIdAsync(int customerId)
        {
            var query = from cs in _customerAbandonmentRepository.Table
                        where cs.CustomerId == customerId
                        select cs;

            var key = _staticCacheManager.PrepareKeyForDefaultCache(CustomerAbandonmentCacheKey, customerId);

            var customerAbandonment = await _staticCacheManager.GetAsync(key, async () => await query.FirstOrDefaultAsync());

            if (customerAbandonment != null)
                return customerAbandonment.ToModel<CustomerAbandonmentInfoModel>();

            return null;
        }

        public virtual async Task AddOrUpdateCustomerAbandonmentAsync(CustomerAbandonmentInfoModel customerAbandonment)
        {
            if (customerAbandonment == null)
                throw new ArgumentNullException();

            var query = _customerAbandonmentRepository.Table;

            if (customerAbandonment.Id > 0)
            {
                var oldcart = await query.Where(c => c.Id == customerAbandonment.Id).FirstOrDefaultAsync();

                oldcart = customerAbandonment.ToEntity<CustomerAbandonmentInfo>();

                await _customerAbandonmentRepository.UpdateAsync(oldcart);

                await _staticCacheManager.RemoveByPrefixAsync(CustomerAbandonmentPrefix, oldcart.CustomerId);
                return;
            }
            var existingCart = await query.Where(c => c.CustomerId == customerAbandonment.CustomerId).FirstOrDefaultAsync();

            if (existingCart != null)
            {
                existingCart = customerAbandonment.ToEntity<CustomerAbandonmentInfo>();
                await _customerAbandonmentRepository.UpdateAsync(existingCart);

                await _staticCacheManager.RemoveByPrefixAsync(CustomerAbandonmentPrefix, existingCart.CustomerId);
                return;
            }

            var customerAbandonmentt = new CustomerAbandonmentInfo();

            customerAbandonmentt = customerAbandonment.ToEntity<CustomerAbandonmentInfo>();

            await _customerAbandonmentRepository.InsertAsync(customerAbandonmentt);
        }

        public virtual async Task<CustomerAbandonmentInfoModel> GetCustomerAbandonmentByTokenAsync(string token)
        {
            var query = from cs in _customerAbandonmentRepository.Table
                        where cs.Token == token
                        select cs;

            var customerAbandonment = await query.FirstOrDefaultAsync();

            if (customerAbandonment != null)
                return customerAbandonment.ToModel<CustomerAbandonmentInfoModel>();

            return null;
        }

        public virtual async Task<IPagedList<CustomerAbandonmentInfoModel>> GetAllCustomerAbandonmentsAsync(string firstName = "",
            string lastName = "",
            string email = "",
            int pageIndex = 0,
            int pageSize = int.MaxValue,
            int statusId = 0,
            int customerId = 0,
            int? productId = null,
            DateTime? createdFromUtc = null,
            DateTime? createdToUtc = null)
        {
            var utcTimeDiff = DateTime.UtcNow - DateTime.Now;

            var customerQuery = _customerRepository.Table;
            if (customerId > 0)
                customerQuery = customerQuery.Where(c => c.Id == customerId);

            if (!string.IsNullOrWhiteSpace(firstName))
                customerQuery = customerQuery.Where(m => m.FirstName.Contains(firstName));
            if (!string.IsNullOrWhiteSpace(lastName))
                customerQuery = customerQuery.Where(m => m.LastName.Contains(lastName));
            if (!string.IsNullOrWhiteSpace(email))
                customerQuery = customerQuery.Where(m => m.Email.Contains(email));

            var cartQuery = _abandonedCartRepository.Table;
            if (productId > 0)
                cartQuery = cartQuery.Where(c => c.ProductId == productId);
            if (createdFromUtc != null)
                cartQuery = cartQuery.Where(c => c.StatusChangedOn.AddHours(-utcTimeDiff.TotalHours).Date >= createdFromUtc.Value.Date);
            if (createdToUtc != null)
                cartQuery = cartQuery.Where(c => c.StatusChangedOn.AddHours(-utcTimeDiff.TotalHours).Date <= createdToUtc.Value.Date);
            if (statusId > 0)
                cartQuery = cartQuery.Where(c => c.StatusId == statusId);

            var abandonmentList = from abd in _customerAbandonmentRepository.Table
                                  join cst in customerQuery on abd.CustomerId equals cst.Id
                                  join cart in cartQuery on abd.CustomerId equals cart.CustomerId
                                  select new CustomerAbandonmentInfoModel()
                                  {
                                      Id = abd.Id,
                                      CustomerName = cst.Email,
                                      CustomerId = cst.Id,
                                      StatusId = abd.StatusId,
                                      NotificationSentFrequency = abd.NotificationSentFrequency,
                                  };

            if (abandonmentList == null)
                return new PagedList<CustomerAbandonmentInfoModel>(new List<CustomerAbandonmentInfoModel>(), pageIndex, pageSize);

            pageSize = Math.Max(pageSize, 1);

            var count = await abandonmentList.CountAsync();
            var data = abandonmentList.ToList();
            data = data.DistinctBy(c => c.Id).ToList();
            data = data.Skip(pageIndex * pageSize).Take(pageSize).ToList();

            return new PagedList<CustomerAbandonmentInfoModel>(data, pageIndex, pageSize, count);
        }
    }
}
