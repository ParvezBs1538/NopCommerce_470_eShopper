using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Blogs;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.News;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Polls;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Stores;
using Nop.Core.Domain.Vendors;
using Nop.Data;
using Nop.Services.Customers;
using NopStation.Plugin.CRM.Zoho.Domain;
using NopStation.Plugin.CRM.Zoho.Domain.Zoho;

namespace NopStation.Plugin.CRM.Zoho.Services
{
    public class MappingService : IMappingService
    {
        #region Fields

        private readonly int _pageSize = 100;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<BlogComment> _blogCommentRepository;
        private readonly ICustomerService _customerService;
        private readonly IRepository<CustomerCustomerRoleMapping> _customerCustomerRoleMappingRepository;
        private readonly IRepository<CustomerPassword> _customerPasswordRepository;
        private readonly IRepository<CustomerRole> _customerRoleRepository;
        private readonly IRepository<ForumPost> _forumPostRepository;
        private readonly IRepository<ForumTopic> _forumTopicRepository;
        private readonly IRepository<GenericAttribute> _gaRepository;
        private readonly IRepository<NewsComment> _newsCommentRepository;
        private readonly IRepository<ProductReview> _productReviewRepository;
        private readonly IRepository<ProductReviewHelpfulness> _productReviewHelpfulnessRepository;
        private readonly IRepository<PollVotingRecord> _pollVotingRecordRepository;
        private readonly IRepository<ShoppingCartItem> _shoppingCartRepository;
        private readonly IRepository<DataMapping> _dataMappingRepository;
        private readonly IRepository<UpdatableItem> _updatableItemRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<OrderItem> _orderItemRepository;
        private readonly IRepository<Shipment> _shipmentRepository;
        private readonly IRepository<ShipmentItem> _shipmentItemRepository;
        private readonly IRepository<Vendor> _vendorRepository;
        private readonly IRepository<Store> _storeRepository;
        private readonly IRepository<Customer> _customerRepository;

        #endregion

        #region Ctor

        public MappingService(IRepository<Product> productRepository,
            IRepository<BlogComment> blogCommentRepository,
            ICustomerService customerService,
            IRepository<CustomerCustomerRoleMapping> customerCustomerRoleMappingRepository,
            IRepository<CustomerPassword> customerPasswordRepository,
            IRepository<CustomerRole> customerRoleRepository,
            IRepository<ForumPost> forumPostRepository,
            IRepository<ForumTopic> forumTopicRepository,
            IRepository<GenericAttribute> gaRepository,
            IRepository<NewsComment> newsCommentRepository,
            IRepository<Order> orderRepository,
            IRepository<ProductReview> productReviewRepository,
            IRepository<ProductReviewHelpfulness> productReviewHelpfulnessRepository,
            IRepository<PollVotingRecord> pollVotingRecordRepository,
            IRepository<ShoppingCartItem> shoppingCartRepository,
            IRepository<DataMapping> dataMappingRepository,
            IRepository<UpdatableItem> updatableItemRepository,
            IRepository<OrderItem> orderItemRepository,
            IRepository<Shipment> shipmentRepository,
            IRepository<ShipmentItem> shipmentItemRepository,
            IRepository<Vendor> vendorRepository,
            IRepository<Store> storeRepository,
            IRepository<Customer> customerRepository)
        {
            _productRepository = productRepository;
            _blogCommentRepository = blogCommentRepository;
            _customerService = customerService;
            _customerCustomerRoleMappingRepository = customerCustomerRoleMappingRepository;
            _customerPasswordRepository = customerPasswordRepository;
            _customerRoleRepository = customerRoleRepository;
            _forumPostRepository = forumPostRepository;
            _forumTopicRepository = forumTopicRepository;
            _gaRepository = gaRepository;
            _newsCommentRepository = newsCommentRepository;
            _productReviewRepository = productReviewRepository;
            _productReviewHelpfulnessRepository = productReviewHelpfulnessRepository;
            _pollVotingRecordRepository = pollVotingRecordRepository;
            _shoppingCartRepository = shoppingCartRepository;
            _dataMappingRepository = dataMappingRepository;
            _updatableItemRepository = updatableItemRepository;
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _shipmentRepository = shipmentRepository;
            _shipmentItemRepository = shipmentItemRepository;
            _vendorRepository = vendorRepository;
            _storeRepository = storeRepository;
            _customerRepository = customerRepository;
        }

        #endregion

        #region Utilities

        protected IQueryable<TEntity> GetDifferentialQuery<TEntity>(EntityType entityType, IQueryable<TEntity> query)
            where TEntity : BaseZohoEntity
        {
            var diffQuery = from sc in _updatableItemRepository.Table
                            where sc.EntityTypeId == (int)entityType
                            select sc;

            return from q in query
                   join ui in diffQuery on q.Id equals ui.EntityId
                   select q;
        }

        protected IQueryable<DataMapping> GetMapQuery(EntityType entityType, int entityId = 0)
        {
            return from sc in _dataMappingRepository.Table
                   where sc.EntityTypeId == (int)entityType &&
                   (entityId == 0 || sc.EntityId == entityId)
                   select sc;
        }

        #endregion

        #region Methods

        #region Stores

        public async Task<ZohoStore> GetZohoStoreAsync(int storeId)
        {
            var query = from s in _storeRepository.Table
                        join sc in GetMapQuery(EntityType.Stores, storeId) on s.Id equals sc.EntityId into gj
                        from sub in gj.DefaultIfEmpty()
                        where s.Id == storeId
                        select new ZohoStore
                        {
                            Url = s.Url,
                            Id = s.Id,
                            ZohoId = sub == null ? null : sub.ZohoId,
                            Name = s.Name,
                            PhoneNumber = s.CompanyPhoneNumber
                        };

            return await query.FirstOrDefaultAsync();
        }

        public async Task<IPagedList<ZohoStore>> GetZohoStoresByStoreIdsAsync(
            IList<int> storeIds,
            bool update = false,
            int pageIndex = 0,
            int pageSize = int.MaxValue
            )
        {
            if (storeIds == null || !storeIds.Any())
                throw new ArgumentNullException(nameof(storeIds));

            if (pageSize == int.MaxValue)
                --pageSize;

            var stores = _storeRepository.Table.Where(p => storeIds.Contains(p.Id));
            var query = from s in stores
                        join sc in GetMapQuery(EntityType.Stores) on s.Id equals sc.EntityId into gj
                        from sub in gj.DefaultIfEmpty()
                        where sub == null != update
                        select new ZohoStore
                        {
                            Url = s.Url,
                            Id = s.Id,
                            ZohoId = sub == null ? null : sub.ZohoId,
                            Name = s.Name,
                            PhoneNumber = s.CompanyPhoneNumber
                        };

            return await query.ToPagedListAsync(pageIndex, _pageSize);
        }

        public async Task<IPagedList<ZohoStore>> GetZohoStoresAsync(SyncType syncType, bool update, int pageIndex = 0)
        {
            var query = from s in _storeRepository.Table
                        join sc in GetMapQuery(EntityType.Stores) on s.Id equals sc.EntityId into gj
                        from sub in gj.DefaultIfEmpty()
                        where sub == null != update
                        select new ZohoStore
                        {
                            Url = s.Url,
                            Id = s.Id,
                            ZohoId = sub == null ? null : sub.ZohoId,
                            Name = s.Name,
                            PhoneNumber = s.CompanyPhoneNumber
                        };

            if (syncType == SyncType.DifferentialSync)
                query = GetDifferentialQuery(EntityType.Stores, query);

            return await query.ToPagedListAsync(pageIndex, _pageSize);
        }

        #endregion

        #region Vendors

        public async Task<ZohoVendor> GetZohoVendorAsync(int vendorId)
        {
            var query = from v in _vendorRepository.Table
                        join sc in GetMapQuery(EntityType.Vendors, vendorId) on v.Id equals sc.EntityId into gj
                        from sub in gj.DefaultIfEmpty()
                        where v.Id == vendorId
                        select new ZohoVendor
                        {
                            Active = v.Active,
                            Email = v.Email,
                            AddressId = v.AddressId,
                            Id = v.Id,
                            Name = v.Name,
                            ZohoId = sub == null ? null : sub.ZohoId
                        };

            return await query.FirstOrDefaultAsync();
        }

        public async Task<IPagedList<ZohoVendor>> GetZohoVendorsByVendorIdsAsync(
            IList<int> vendorIds,
            bool update = false,
            int pageIndex = 0,
            int pageSize = int.MaxValue
            )
        {
            if (vendorIds == null || !vendorIds.Any())
                throw new ArgumentNullException(nameof(vendorIds));

            if (pageSize == int.MaxValue)
                --pageSize;

            var vendors = _vendorRepository.Table.Where(p => vendorIds.Contains(p.Id));
            var query = from v in vendors
                        join sc in GetMapQuery(EntityType.Vendors) on v.Id equals sc.EntityId into gj
                        from sub in gj.DefaultIfEmpty()
                        where !v.Deleted && sub == null != update
                        select new ZohoVendor
                        {
                            Email = v.Email,
                            Id = v.Id,
                            ZohoId = sub == null ? null : sub.ZohoId,
                            Name = v.Name,
                            AddressId = v.AddressId,
                            Active = v.Active
                        };

            return await query.ToPagedListAsync(pageIndex, _pageSize);
        }

        public async Task<IPagedList<ZohoVendor>> GetZohoVendorsAsync(SyncType syncType, bool update, int pageIndex = 0)
        {
            var query = from v in _vendorRepository.Table
                        join sc in GetMapQuery(EntityType.Vendors) on v.Id equals sc.EntityId into gj
                        from sub in gj.DefaultIfEmpty()
                        where !v.Deleted && sub == null != update
                        select new ZohoVendor
                        {
                            Email = v.Email,
                            Id = v.Id,
                            ZohoId = sub == null ? null : sub.ZohoId,
                            Name = v.Name,
                            AddressId = v.AddressId,
                            Active = v.Active
                        };

            if (syncType == SyncType.DifferentialSync)
                query = GetDifferentialQuery(EntityType.Vendors, query);

            return await query.ToPagedListAsync(pageIndex, _pageSize);
        }

        #endregion

        #region Customers

        public async Task<ZohoCustomer> GetZohoCustomerAsync(int customerId)
        {
            var query = from c in _customerRepository.Table
                        join sc in GetMapQuery(EntityType.Customers, customerId) on c.Id equals sc.EntityId into gj
                        from sub in gj.DefaultIfEmpty()
                        where c.Id == customerId
                        select new ZohoCustomer
                        {
                            Email = c.Email,
                            Id = c.Id,
                            ZohoId = sub == null ? null : sub.ZohoId,
                            VendorId = c.VendorId
                        };

            return await query.FirstOrDefaultAsync();
        }

        public async Task<IPagedList<ZohoCustomer>> GetZohoCustomersAsync(SyncType syncType, bool update, int pageIndex = 0)
        {
            var query = from c in _customerRepository.Table
                        join sc in GetMapQuery(EntityType.Customers) on c.Id equals sc.EntityId into gj
                        from sub in gj.DefaultIfEmpty()
                        where !c.Deleted && c.Active && sub == null != update
                        select new ZohoCustomer
                        {
                            Email = c.Email,
                            Id = c.Id,
                            ZohoId = sub == null ? null : sub.ZohoId,
                            VendorId = c.VendorId
                        };

            if (syncType == SyncType.DifferentialSync)
                query = GetDifferentialQuery(EntityType.Customers, query);

            return await query.ToPagedListAsync(pageIndex, _pageSize);
        }

        public async Task<IPagedList<ZohoCustomer>> GetZohoCustomersByCustomerIdsAsync(
            IList<int> customerIds,
            bool update = false,
            int pageIndex = 0,
            int pageSize = int.MaxValue
            )
        {
            if (customerIds == null || !customerIds.Any())
                throw new ArgumentNullException(nameof(customerIds));

            if (pageSize == int.MaxValue)
                --pageSize;

            var customers = _customerRepository.Table.Where(c => customerIds.Contains(c.Id));
            var query = from c in customers
                        join sc in GetMapQuery(EntityType.Customers) on c.Id equals sc.EntityId into gj
                        from sub in gj.DefaultIfEmpty()
                        where !c.Deleted && c.Active && sub == null != update
                        select new ZohoCustomer
                        {
                            Email = c.Email,
                            Id = c.Id,
                            ZohoId = sub == null ? null : sub.ZohoId,
                            VendorId = c.VendorId
                        };

            return await query.ToPagedListAsync(pageIndex, _pageSize);
        }


        /// <summary>
        /// Gets all customers
        /// </summary>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="affiliateId">Affiliate identifier</param>
        /// <param name="vendorId">Vendor identifier</param>
        /// <param name="customerRoleIds">A list of customer role identifiers to filter by (at least one match); pass null or empty list in order to load all customers; </param>
        /// <param name="email">Email; null to load all customers</param>
        /// <param name="username">Username; null to load all customers</param>
        /// <param name="firstName">First name; null to load all customers</param>
        /// <param name="lastName">Last name; null to load all customers</param>
        /// <param name="dayOfBirth">Day of birth; 0 to load all customers</param>
        /// <param name="monthOfBirth">Month of birth; 0 to load all customers</param>
        /// <param name="company">Company; null to load all customers</param>
        /// <param name="phone">Phone; null to load all customers</param>
        /// <param name="zipPostalCode">Phone; null to load all customers</param>
        /// <param name="ipAddress">IP address; null to load all customers</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="getOnlyTotalCount">A value in indicating whether you want to load only total number of records. Set to "true" if you don't want to load data from database</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the customers
        /// </returns>
        public virtual async Task<IPagedList<Customer>> GetAllCustomersForZohoSyncAsync(DateTime? createdFromUtc = null, DateTime? createdToUtc = null,
            int affiliateId = 0, int vendorId = 0, int[] customerRoleIds = null,
            string email = null, string username = null, string firstName = null, string lastName = null,
            int dayOfBirth = 0, int monthOfBirth = 0,
            string company = null, string phone = null, string zipPostalCode = null, string ipAddress = null,
            int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false, bool ignoreGuestCustomers = true)
        {
            var query = _customerRepository.Table;

            if (createdFromUtc.HasValue)
                query = query.Where(c => createdFromUtc.Value <= c.CreatedOnUtc);
            if (createdToUtc.HasValue)
                query = query.Where(c => createdToUtc.Value >= c.CreatedOnUtc);
            if (affiliateId > 0)
                query = query.Where(c => affiliateId == c.AffiliateId);
            if (vendorId > 0)
                query = query.Where(c => vendorId == c.VendorId);

            query = query.Where(c => !c.Deleted);

            if (customerRoleIds != null && customerRoleIds.Length > 0)
            {
                query = query.Join(_customerCustomerRoleMappingRepository.Table, x => x.Id, y => y.CustomerId,
                        (x, y) => new { Customer = x, Mapping = y })
                    .Where(z => customerRoleIds.Contains(z.Mapping.CustomerRoleId))
                    .Select(z => z.Customer)
                    .Distinct();
            }

            if (!string.IsNullOrWhiteSpace(email))
                query = query.Where(c => c.Email.Contains(email));
            if (!string.IsNullOrWhiteSpace(username))
                query = query.Where(c => c.Username.Contains(username));
            if (!string.IsNullOrWhiteSpace(firstName))
                query = query.Where(c => c.FirstName.Contains(firstName));

            if (!string.IsNullOrWhiteSpace(lastName))
                query = query.Where(c => c.LastName.Contains(lastName));

            //date of birth is stored as a string into database.
            //we also know that date of birth is stored in the following format YYYY-MM-DD (for example, 1983-02-18).
            //so let's search it as a string
            if (dayOfBirth > 0 && monthOfBirth > 0)
                query = query.Where(c => c.DateOfBirth.HasValue && c.DateOfBirth.Value.Day == dayOfBirth &&
                    c.DateOfBirth.Value.Month == monthOfBirth);
            else if (dayOfBirth > 0)
                query = query.Where(c => c.DateOfBirth.HasValue && c.DateOfBirth.Value.Day == dayOfBirth);
            else if (monthOfBirth > 0)
                query = query.Where(c => c.DateOfBirth.HasValue && c.DateOfBirth.Value.Month == monthOfBirth);

            //search by company
            if (!string.IsNullOrWhiteSpace(company))
                query = query.Where(c => c.Company.Contains(company));

            //search by phone
            if (!string.IsNullOrWhiteSpace(phone))
                query = query.Where(c => c.Phone.Contains(phone));

            //search by zip
            if (!string.IsNullOrWhiteSpace(zipPostalCode))
                query = query.Where(c => c.ZipPostalCode.Contains(zipPostalCode));

            //search by IpAddress
            if (!string.IsNullOrWhiteSpace(ipAddress) && CommonHelper.IsValidIpAddress(ipAddress))
            {
                query = query.Where(w => w.LastIpAddress == ipAddress);
            }

            if (ignoreGuestCustomers)
            {
                var guestRole = await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.GuestsRoleName);

                var allGuestCustomers = from guest in _customerRepository.Table
                                        join ccm in _customerCustomerRoleMappingRepository.Table on guest.Id equals ccm.CustomerId
                                        where ccm.CustomerRoleId == guestRole.Id
                                        select guest;

                var guestsToDelete = from guest in _customerRepository.Table
                                     join g in allGuestCustomers on guest.Id equals g.Id
                                     from sCart in _shoppingCartRepository.Table.Where(sci => sci.CustomerId == guest.Id).DefaultIfEmpty()
                                     from order in _orderRepository.Table.Where(o => o.CustomerId == guest.Id).DefaultIfEmpty()
                                     from blogComment in _blogCommentRepository.Table.Where(o => o.CustomerId == guest.Id).DefaultIfEmpty()
                                     from newsComment in _newsCommentRepository.Table.Where(o => o.CustomerId == guest.Id).DefaultIfEmpty()
                                     from productReview in _productReviewRepository.Table.Where(o => o.CustomerId == guest.Id).DefaultIfEmpty()
                                     from productReviewHelpfulness in _productReviewHelpfulnessRepository.Table.Where(o => o.CustomerId == guest.Id).DefaultIfEmpty()
                                     from pollVotingRecord in _pollVotingRecordRepository.Table.Where(o => o.CustomerId == guest.Id).DefaultIfEmpty()
                                     from forumTopic in _forumTopicRepository.Table.Where(o => o.CustomerId == guest.Id).DefaultIfEmpty()
                                     from forumPost in _forumPostRepository.Table.Where(o => o.CustomerId == guest.Id).DefaultIfEmpty()
                                     where (!false || sCart == null) &&
                                         order == null && blogComment == null && newsComment == null && productReview == null && productReviewHelpfulness == null &&
                                         pollVotingRecord == null && forumTopic == null && forumPost == null &&
                                         !guest.IsSystemAccount
                                     //&& (createdFromUtc == null || guest.CreatedOnUtc > createdFromUtc) 
                                     //&& (createdToUtc == null || guest.CreatedOnUtc < createdToUtc)
                                     select guest;


                query = query.Except(guestsToDelete);
            }

            query = query.OrderByDescending(c => c.CreatedOnUtc);

            return await query.ToPagedListAsync(pageIndex, _pageSize, getOnlyTotalCount);
        }

        #endregion

        #region Products

        public async Task<ZohoProduct> GetZohoProductAsync(int productId)
        {
            var map = GetMapQuery(EntityType.Products, productId);
            var query = from p in _productRepository.Table
                        join sc in GetMapQuery(EntityType.Products, productId) on p.Id equals sc.EntityId into gj
                        from sub in gj.DefaultIfEmpty()
                        where p.Id == productId
                        select new ZohoProduct
                        {
                            Id = p.Id,
                            ZohoId = sub == null ? null : sub.ZohoId,
                            Name = p.Name,
                            AvailableEndDate = p.AvailableEndDateTimeUtc,
                            AvailableStartDate = p.AvailableStartDateTimeUtc,
                            Description = p.ShortDescription,
                            Price = p.Price,
                            Published = p.Published,
                            Sku = p.Sku,
                            StockQuantity = p.StockQuantity,
                            TaxCategoryId = p.TaxCategoryId,
                            VendorId = p.VendorId,
                            IsTaxExempt = p.IsTaxExempt
                        };

            return await query.FirstOrDefaultAsync();
        }

        public async Task<IPagedList<ZohoProduct>> GetZohoProductsByProductIdsAsync(
            IList<int> productIds,
            bool update = false,
            int pageIndex = 0,
            int pageSize = int.MaxValue
            )
        {
            if (productIds == null || !productIds.Any())
                throw new ArgumentNullException(nameof(productIds));

            if (pageSize == int.MaxValue)
                --pageSize;

            var products = _productRepository.Table.Where(p => productIds.Contains(p.Id));
            var query = from p in products
                        join sc in GetMapQuery(EntityType.Products) on p.Id equals sc.EntityId into gj
                        from sub in gj.DefaultIfEmpty()
                        where !p.Deleted && sub == null != update
                        select new ZohoProduct
                        {
                            Id = p.Id,
                            ZohoId = sub == null ? null : sub.ZohoId,
                            Name = p.Name,
                            AvailableEndDate = p.AvailableEndDateTimeUtc,
                            AvailableStartDate = p.AvailableStartDateTimeUtc,
                            Description = p.ShortDescription,
                            Price = p.Price,
                            Published = p.Published,
                            Sku = p.Sku,
                            StockQuantity = p.StockQuantity,
                            TaxCategoryId = p.TaxCategoryId,
                            VendorId = p.VendorId,
                            IsTaxExempt = p.IsTaxExempt
                        };

            return await query.ToPagedListAsync(pageIndex, _pageSize);
        }

        public async Task<IPagedList<ZohoProduct>> GetZohoProductsAsync(SyncType syncType, bool update, int pageIndex = 0)
        {
            var query = from p in _productRepository.Table
                        join sc in GetMapQuery(EntityType.Products) on p.Id equals sc.EntityId into gj
                        from sub in gj.DefaultIfEmpty()
                        where !p.Deleted && sub == null != update
                        select new ZohoProduct
                        {
                            Id = p.Id,
                            ZohoId = sub == null ? null : sub.ZohoId,
                            Name = p.Name,
                            AvailableEndDate = p.AvailableEndDateTimeUtc,
                            AvailableStartDate = p.AvailableStartDateTimeUtc,
                            Description = p.ShortDescription,
                            Price = p.Price,
                            Published = p.Published,
                            Sku = p.Sku,
                            StockQuantity = p.StockQuantity,
                            TaxCategoryId = p.TaxCategoryId,
                            VendorId = p.VendorId,
                            IsTaxExempt = p.IsTaxExempt
                        };

            if (syncType == SyncType.DifferentialSync)
                query = GetDifferentialQuery(EntityType.Products, query);

            return await query.ToPagedListAsync(pageIndex, _pageSize);
        }

        #endregion

        #region Orders

        public async Task<ZohoOrder> GetZohoOrderAsync(int orderId)
        {
            var query = from o in _orderRepository.Table
                        join sc in GetMapQuery(EntityType.Orders, orderId) on o.Id equals sc.EntityId into gj
                        from sub in gj.DefaultIfEmpty()
                        where o.Id == orderId
                        select new ZohoOrder
                        {
                            Id = o.Id,
                            ZohoId = sub == null ? null : sub.ZohoId,
                            CustomerCurrencyCode = o.CustomerCurrencyCode,
                            CustomerId = o.CustomerId,
                            OrderTax = o.OrderTax,
                            CurrencyRate = o.CurrencyRate,
                            BillingAddressId = o.BillingAddressId,
                            ShippingMethod = o.ShippingMethod,
                            OrderStatusId = o.OrderStatusId,
                            OrderTotal = o.OrderTotal,
                            ShippingAddressId = o.ShippingAddressId,
                            OrderSubtotalExclTax = o.OrderSubtotalExclTax,
                            StoreId = o.StoreId,
                            OrderDiscount = o.OrderDiscount
                        };

            return await query.FirstOrDefaultAsync();
        }

        public async Task<IPagedList<ZohoOrder>> GetZohoOrdersByOrderIdsAsync(
            IList<int> orderIds,
            bool update = false,
            int pageIndex = 0,
            int pageSize = int.MaxValue
            )
        {
            if (orderIds == null || !orderIds.Any())
                throw new ArgumentNullException(nameof(orderIds));

            if (pageSize == int.MaxValue)
                --pageSize;

            var orders = _orderRepository.Table.Where(p => orderIds.Contains(p.Id));
            var query = from o in orders
                        join sc in GetMapQuery(EntityType.Orders) on o.Id equals sc.EntityId into gj
                        from sub in gj.DefaultIfEmpty()
                        where !o.Deleted && sub == null != update
                        select new ZohoOrder
                        {
                            Id = o.Id,
                            ZohoId = sub == null ? null : sub.ZohoId,
                            CustomerCurrencyCode = o.CustomerCurrencyCode,
                            CustomerId = o.CustomerId,
                            OrderTax = o.OrderTax,
                            CurrencyRate = o.CurrencyRate,
                            BillingAddressId = o.BillingAddressId,
                            ShippingMethod = o.ShippingMethod,
                            OrderStatusId = o.OrderStatusId,
                            OrderTotal = o.OrderTotal,
                            ShippingAddressId = o.ShippingAddressId,
                            OrderSubtotalExclTax = o.OrderSubtotalExclTax,
                            StoreId = o.StoreId,
                            OrderDiscount = o.OrderDiscount
                        };

            return await query.ToPagedListAsync(pageIndex, _pageSize);
        }

        public async Task<IPagedList<ZohoOrder>> GetZohoOrdersAsync(SyncType syncType, bool update, int pageIndex = 0)
        {
            var query = from o in _orderRepository.Table
                        join sc in GetMapQuery(EntityType.Orders) on o.Id equals sc.EntityId into gj
                        from sub in gj.DefaultIfEmpty()
                        where !o.Deleted && sub == null != update
                        select new ZohoOrder
                        {
                            Id = o.Id,
                            ZohoId = sub == null ? null : sub.ZohoId,
                            CustomerCurrencyCode = o.CustomerCurrencyCode,
                            CustomerId = o.CustomerId,
                            OrderTax = o.OrderTax,
                            CurrencyRate = o.CurrencyRate,
                            BillingAddressId = o.BillingAddressId,
                            ShippingMethod = o.ShippingMethod,
                            OrderStatusId = o.OrderStatusId,
                            OrderTotal = o.OrderTotal,
                            ShippingAddressId = o.ShippingAddressId,
                            OrderSubtotalExclTax = o.OrderSubtotalExclTax,
                            StoreId = o.StoreId,
                            OrderDiscount = o.OrderDiscount
                        };

            if (syncType == SyncType.DifferentialSync)
                query = GetDifferentialQuery(EntityType.Orders, query);

            return await query.ToPagedListAsync(pageIndex, _pageSize);
        }

        #endregion

        #region Shipments

        public async Task<ZohoShipment> GetZohoShipmentAsync(int shipmentId)
        {
            var query = from o in _shipmentRepository.Table
                        join sc in GetMapQuery(EntityType.Shipments, shipmentId) on o.Id equals sc.EntityId into gj
                        from sub in gj.DefaultIfEmpty()
                        where o.Id == shipmentId
                        select new ZohoShipment
                        {
                            Id = o.Id,
                            ZohoId = sub == null ? null : sub.ZohoId,
                            DeliveryDateUtc = o.DeliveryDateUtc,
                            OrderId = o.OrderId,
                            ShippedDateUtc = o.ShippedDateUtc,
                            TotalWeight = o.TotalWeight,
                            TrackingNumber = o.TrackingNumber
                        };

            return await query.FirstOrDefaultAsync();
        }

        public async Task<IPagedList<ZohoShipment>> GetZohoShipmentsByShipmentIdsAsync(
            IList<int> shipmentIds,
            bool update = false,
            int pageIndex = 0,
            int pageSize = int.MaxValue
            )
        {
            if (shipmentIds == null || !shipmentIds.Any())
                throw new ArgumentNullException(nameof(shipmentIds));

            if (pageSize == int.MaxValue)
                --pageSize;

            var shipments = _shipmentRepository.Table.Where(p => shipmentIds.Contains(p.Id));
            var query = from o in shipments
                        join sc in GetMapQuery(EntityType.Shipments) on o.Id equals sc.EntityId into gj
                        from sub in gj.DefaultIfEmpty()
                        where sub == null != update
                        select new ZohoShipment
                        {
                            Id = o.Id,
                            ZohoId = sub == null ? null : sub.ZohoId,
                            DeliveryDateUtc = o.DeliveryDateUtc,
                            OrderId = o.OrderId,
                            ShippedDateUtc = o.ShippedDateUtc,
                            TotalWeight = o.TotalWeight,
                            TrackingNumber = o.TrackingNumber
                        };

            return await query.ToPagedListAsync(pageIndex, _pageSize);
        }

        public async Task<IPagedList<ZohoShipment>> GetZohoShipmentsAsync(SyncType syncType, bool update, int pageIndex = 0)
        {
            var query = from o in _shipmentRepository.Table
                        join sc in GetMapQuery(EntityType.Shipments) on o.Id equals sc.EntityId into gj
                        from sub in gj.DefaultIfEmpty()
                        where sub == null != update
                        select new ZohoShipment
                        {
                            Id = o.Id,
                            ZohoId = sub == null ? null : sub.ZohoId,
                            DeliveryDateUtc = o.DeliveryDateUtc,
                            OrderId = o.OrderId,
                            ShippedDateUtc = o.ShippedDateUtc,
                            TotalWeight = o.TotalWeight,
                            TrackingNumber = o.TrackingNumber
                        };

            if (syncType == SyncType.DifferentialSync)
                query = GetDifferentialQuery(EntityType.Shipments, query);

            return await query.ToPagedListAsync(pageIndex, _pageSize);
        }

        #endregion

        #region Shipment Items

        public async Task<ZohoShipmentItem> GetZohoShipmentItemAsync(int shipmentItemId)
        {
            var query = from si in _shipmentItemRepository.Table
                        join oi in _orderItemRepository.Table on si.OrderItemId equals oi.Id
                        join sc in GetMapQuery(EntityType.ShipmentItems, shipmentItemId) on si.Id equals sc.EntityId into gj
                        from sub in gj.DefaultIfEmpty()
                        where si.Id == shipmentItemId
                        select new ZohoShipmentItem
                        {
                            Id = si.Id,
                            ZohoId = sub == null ? null : sub.ZohoId,
                            ProductId = oi.ProductId,
                            Quantity = si.Quantity,
                            ShipmentId = si.ShipmentId
                        };

            return await query.FirstOrDefaultAsync();
        }

        public async Task<IPagedList<ZohoShipmentItem>> GetZohoShipmentItemsByShipmentItemIdsAsync(
            IList<int> productIds,
            bool update = false,
            int pageIndex = 0,
            int pageSize = int.MaxValue
            )
        {
            if (productIds == null || !productIds.Any())
                throw new ArgumentNullException(nameof(productIds));

            if (pageSize == int.MaxValue)
                --pageSize;

            var shipmentItem = _shipmentItemRepository.Table.Where(p => productIds.Contains(p.Id));
            var query = from si in shipmentItem
                        join oi in _orderItemRepository.Table on si.OrderItemId equals oi.Id
                        join sc in GetMapQuery(EntityType.ShipmentItems) on si.Id equals sc.EntityId into gj
                        from sub in gj.DefaultIfEmpty()
                        where sub == null != update
                        select new ZohoShipmentItem
                        {
                            Id = si.Id,
                            ZohoId = sub == null ? null : sub.ZohoId,
                            ProductId = oi.ProductId,
                            Quantity = si.Quantity,
                            ShipmentId = si.ShipmentId
                        };

            return await query.ToPagedListAsync(pageIndex, _pageSize);
        }

        public async Task<IPagedList<ShipmentItem>> GetAllShipmentItemsAsync(
            int? shipmentId = null,
            bool update = false,
            int pageIndex = 0,
            int pageSize = int.MaxValue
            )
        {
            if (pageSize == int.MaxValue)
                --pageSize;

            var query = _shipmentItemRepository.Table;
            if (shipmentId.HasValue)
                query = query.Where(x => x.ShipmentId == shipmentId.Value);

            return await query.ToPagedListAsync(pageIndex, _pageSize);
        }

        public async Task<IPagedList<ZohoShipmentItem>> GetZohoShipmentItemsAsync(SyncType syncType, bool update, int pageIndex = 0)
        {
            var query = from si in _shipmentItemRepository.Table
                        join oi in _orderItemRepository.Table on si.OrderItemId equals oi.Id
                        join sc in GetMapQuery(EntityType.ShipmentItems) on si.Id equals sc.EntityId into gj
                        from sub in gj.DefaultIfEmpty()
                        where sub == null != update
                        select new ZohoShipmentItem
                        {
                            Id = si.Id,
                            ZohoId = sub == null ? null : sub.ZohoId,
                            ProductId = oi.ProductId,
                            Quantity = si.Quantity,
                            ShipmentId = si.ShipmentId
                        };

            if (syncType == SyncType.DifferentialSync)
                query = GetDifferentialQuery(EntityType.ShipmentItems, query);

            return await query.ToPagedListAsync(pageIndex, _pageSize);
        }

        #endregion

        #region Data mapping

        public async Task InsertDataMappingAsync(List<DataMapping> dataMappings)
        {
            await _dataMappingRepository.InsertAsync(dataMappings);
        }

        public async Task<DataMapping> GetDataMappingByEntityIdAsync(EntityType entityType, int id)
        {
            return await _dataMappingRepository.Table
                .FirstOrDefaultAsync(x => x.EntityTypeId == (int)entityType && x.EntityId == id);
        }

        public async Task<IPagedList<DataMapping>> GetAllDataMappingAsync(
            EntityType? entityType = null,
            int pageIndex = 0,
            int pageSize = int.MaxValue
            )
        {
            if (pageSize == int.MaxValue)
                --pageSize;

            var query = _dataMappingRepository.Table;

            if (entityType.HasValue)
                query.Where(x => x.EntityTypeId == (int)entityType.Value);

            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

        #endregion

        #endregion
    }
}
