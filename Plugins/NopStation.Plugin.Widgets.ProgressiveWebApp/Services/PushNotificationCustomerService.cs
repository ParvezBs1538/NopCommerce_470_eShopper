using System.Collections.Generic;
using System.Linq;
using Nop.Core.Domain.Customers;
using Nop.Data;

namespace NopStation.Plugin.Widgets.ProgressiveWebApp.Services
{
    public class PushNotificationCustomerService : IPushNotificationCustomerService
    {
        #region Fields

        //private readonly IRepository<PushNotificationCustomer> _pushNotificationCustomerRepository;
        private readonly IRepository<Customer> _customerRepository;

        #endregion

        #region Ctor

        public PushNotificationCustomerService(
            //IRepository<PushNotificationCustomer> pushNotificationCustomerRepository,
            IRepository<Customer> customerRepository)
        {
            //_pushNotificationCustomerRepository = pushNotificationCustomerRepository;
            _customerRepository = customerRepository;
        }

        #endregion

        #region Methods

        public IList<Customer> GetCustomersByVendorId(int vendorId)
        {
            var query = _customerRepository.Table.Where(x => x.VendorId == vendorId);
            return query.ToList();
        }

        //public void DeletePushNotificationCustomer(PushNotificationCustomer pushNotificationCustomer)
        //{
        //    if (pushNotificationCustomer == null)
        //        throw new ArgumentNullException(nameof(pushNotificationCustomer));

        //    _pushNotificationCustomerRepository.Delete(pushNotificationCustomer);
        //}

        //public void InsertPushNotificationCustomer(PushNotificationCustomer pushNotificationCustomer)
        //{
        //    if (pushNotificationCustomer == null)
        //        throw new ArgumentNullException(nameof(pushNotificationCustomer));

        //    _pushNotificationCustomerRepository.Insert(pushNotificationCustomer);
        //}

        //public void UpdatePushNotificationCustomer(PushNotificationCustomer pushNotificationCustomer)
        //{
        //    if (pushNotificationCustomer == null)
        //        throw new ArgumentNullException(nameof(pushNotificationCustomer));

        //    _pushNotificationCustomerRepository.Update(pushNotificationCustomer);
        //}

        //public PushNotificationCustomer GetPushNotificationCustomerById(int pushNotificationCustomerId)
        //{
        //    if (pushNotificationCustomerId == 0)
        //        return null;

        //    return _pushNotificationCustomerRepository.GetById(pushNotificationCustomerId);
        //}

        //public IPagedList<PushNotificationCustomer> GetAllPushNotificationCustomers(int pageIndex = 0, int pageSize = int.MaxValue)
        //{
        //    var query = _pushNotificationCustomerRepository.Table;

        //    query = query.OrderByDescending(e => e.Id);

        //    return new PagedList<PushNotificationCustomer>(query, pageIndex, pageSize);
        //}

        #endregion
    }
}
