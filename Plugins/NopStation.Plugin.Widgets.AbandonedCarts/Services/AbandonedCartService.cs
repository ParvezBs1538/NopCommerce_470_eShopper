using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Data;
using Nop.Services.Configuration;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using NopStation.Plugin.Widgets.AbandonedCarts.Domain;
using NopStation.Plugin.Widgets.AbandonedCarts.Models;

namespace NopStation.Plugin.Widgets.AbandonedCarts.Services
{
    public class AbandonedCartService : IAbandonedCartService
    {
        #region Fields

        private readonly IRepository<AbandonedCart> _abandonedCartRepository;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<ShoppingCartItem> _shoppingCartRepository;
        private readonly IRepository<CustomerAbandonmentInfo> _customerAbandonmentInfoRepository;

        #endregion

        #region Ctors
        public AbandonedCartService(IRepository<AbandonedCart> abandonedCartRepository,
                                    IStoreContext storeContext,
                                    ISettingService settingService,
                                    IRepository<Customer> customerRepository,
                                    IRepository<Product> productRepository,
                                    IRepository<ShoppingCartItem> shoppingCartRepository,
                                    IRepository<CustomerAbandonmentInfo> customerAbandonmentInfoRepository)
        {
            _abandonedCartRepository = abandonedCartRepository;
            _storeContext = storeContext;
            _settingService = settingService;
            _customerRepository = customerRepository;
            _productRepository = productRepository;
            _shoppingCartRepository = shoppingCartRepository;
            _customerAbandonmentInfoRepository = customerAbandonmentInfoRepository;
        }

        #endregion

        #region Methods

        public virtual async Task AddOrUpdateAbandonedCartAsync(AbandonedCartModel cartModel)
        {
            if (cartModel == null)
                throw new ArgumentNullException();

            if (cartModel.Id > 0)
            {
                var oldAbandonedCart = await _abandonedCartRepository.GetByIdAsync(cartModel.Id);
                if (oldAbandonedCart == null)
                    throw new ArgumentNullException(nameof(cartModel));
                //map manually
                oldAbandonedCart.StatusId = cartModel.StatusId;
                oldAbandonedCart.StatusChangedOn = cartModel.StatusChangedOn;
                oldAbandonedCart.Quantity = cartModel.Quantity != 0 ? cartModel.Quantity : oldAbandonedCart.Quantity;
                oldAbandonedCart.UnitPrice = cartModel.UnitPrice ?? oldAbandonedCart.UnitPrice;
                oldAbandonedCart.TotalPrice = cartModel.TotalPrice ?? oldAbandonedCart.TotalPrice;
                oldAbandonedCart.AttributeInfo = cartModel.AttributeInfo ?? oldAbandonedCart.AttributeInfo;

                await _abandonedCartRepository.UpdateAsync(oldAbandonedCart);
                return;
            }

            var query = _abandonedCartRepository.Table;
            var oldCart = await query.Where(c => c.ShoppingCartItemId == cartModel.ShoppingCartItemId).FirstOrDefaultAsync();

            if (oldCart != null)
            {
                //map manually
                oldCart.StatusId = cartModel.StatusId;
                oldCart.StatusChangedOn = cartModel.StatusChangedOn;
                oldCart.Quantity = cartModel.Quantity != 0 ? cartModel.Quantity : oldCart.Quantity;
                oldCart.UnitPrice = cartModel.UnitPrice ?? oldCart.UnitPrice;
                oldCart.TotalPrice = cartModel.TotalPrice ?? oldCart.TotalPrice;
                oldCart.AttributeInfo = cartModel.AttributeInfo ?? oldCart.AttributeInfo;

                await _abandonedCartRepository.UpdateAsync(oldCart);
                return;
            }

            var oldCartByOrder = await query.Where(c => c.CustomerId == cartModel.CustomerId
                                    && c.ProductId == cartModel.ProductId &&
                                    cartModel.StatusId != (int)AbandonedStatus.Deleted &&
                                    cartModel.StatusId != (int)AbandonedStatus.Recovered).FirstOrDefaultAsync();

            if (oldCartByOrder != null)
            {
                //map manually
                oldCartByOrder.StatusId = cartModel.StatusId;
                oldCartByOrder.StatusChangedOn = cartModel.StatusChangedOn;

                await _abandonedCartRepository.UpdateAsync(oldCartByOrder);
                return;
            }

            var adandonedCartt = cartModel.ToEntity<AbandonedCart>();

            await _abandonedCartRepository.InsertAsync(adandonedCartt);
        }

        public virtual async Task UpdateAbandonmentStatusAsync()
        {
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var abandonedCartsSettings = await _settingService.LoadSettingAsync<AbandonmentCartSettings>(storeScope);
            var cutOffTime = abandonedCartsSettings.AbandonmentCutOffTime;
            var abdQuery = _abandonedCartRepository.Table;
            var abandonedCarts = from abd in abdQuery
                                 where abd.StatusId == (int)AbandonedStatus.InAction
                                     && abd.StatusChangedOn.AddMinutes(cutOffTime) <= DateTime.UtcNow
                                 select abd;

            var carts = abandonedCarts.ToList();

            for (var i = 0; i < carts.Count(); i++)
            {
                carts[i].StatusChangedOn = DateTime.UtcNow;
                carts[i].StatusId = (int)AbandonedStatus.Abandoned;
            }

            await _abandonedCartRepository.UpdateAsync(carts);
        }

        public virtual async Task<IList<Customer>> GetFirstAbandonedCustomersAsync()
        {
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var abandonedCartsSettings = await _settingService.LoadSettingAsync<AbandonmentCartSettings>(storeScope);
            var firstAbandonedDuration = abandonedCartsSettings.DurationAfterFirstAbandonment;
            var customerCutoffTime = abandonedCartsSettings.CustomerOnlineCutoffTime;
            var abdQuery = _abandonedCartRepository.Table;
            var customers = (from abd in abdQuery
                             join cs in _customerRepository.Table on abd.CustomerId equals cs.Id
                             where abd.StatusId == (int)AbandonedStatus.Abandoned
                                 && abd.FirstNotificationSentOn == DateTime.MinValue
                                 && abd.StatusChangedOn.AddMinutes(firstAbandonedDuration) <= DateTime.UtcNow
                                 && cs.LastActivityDateUtc < DateTime.UtcNow.AddMinutes(-customerCutoffTime)
                             select cs).Distinct().ToList();

            return customers;
        }

        public virtual async Task<IList<Customer>> GetSecondAbandonedCustomersAsync()
        {
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var abandonedCartsSettings = await _settingService.LoadSettingAsync<AbandonmentCartSettings>(storeScope);
            var secondAbandonedDuration = abandonedCartsSettings.DurationAfterSecondAbandonment;
            var customerCutoffTime = abandonedCartsSettings.CustomerOnlineCutoffTime;
            var dontSendSameDay = abandonedCartsSettings.DontSendSameDay;
            var abdQuery = _abandonedCartRepository.Table;
            var customers = new List<Customer>();
            if (dontSendSameDay)
            {
                customers = (from abd in abdQuery
                             join cs in _customerRepository.Table on abd.CustomerId equals cs.Id
                             join csAbd in _customerAbandonmentInfoRepository.Table on abd.CustomerId equals csAbd.CustomerId
                             where abd.StatusId == (int)AbandonedStatus.Abandoned
                                 && abd.FirstNotificationSentOn != DateTime.MinValue
                                 && abd.SecondNotificationSentOn == DateTime.MinValue
                                 && abd.StatusChangedOn.AddMinutes(secondAbandonedDuration) <= DateTime.UtcNow
                                 && cs.LastActivityDateUtc.AddMinutes(customerCutoffTime) < DateTime.UtcNow
                                 && csAbd.LastNotificationSentOn.Day != DateTime.UtcNow.Day
                                 && csAbd.StatusId != (int)CustomerAbandonmentStatus.Unsubscribed
                             select cs).Distinct().ToList();
            }
            else
            {
                customers = (from abd in abdQuery
                             join cs in _customerRepository.Table on abd.CustomerId equals cs.Id
                             join csAbd in _customerAbandonmentInfoRepository.Table on abd.CustomerId equals csAbd.CustomerId
                             where abd.StatusId == (int)AbandonedStatus.Abandoned
                                 && abd.FirstNotificationSentOn != DateTime.MinValue
                                 && abd.SecondNotificationSentOn == DateTime.MinValue
                                 && abd.StatusChangedOn.AddMinutes(secondAbandonedDuration) <= DateTime.UtcNow
                                 && cs.LastActivityDateUtc.AddMinutes(customerCutoffTime) < DateTime.UtcNow
                                 && csAbd.StatusId != (int)CustomerAbandonmentStatus.Unsubscribed
                             select cs).Distinct().ToList();
            }
            return customers;
        }

        public virtual async Task<IList<ProductInfoModel>> GetProductsByCustomerAsync(int customerId)
        {
            var query = _productRepository.Table;

            IQueryable<ProductInfoModel> products = from pr in query
                                                    join abd in _abandonedCartRepository.Table on pr.Id equals abd.ProductId
                                                    join shp in _shoppingCartRepository.Table on abd.ShoppingCartItemId equals shp.Id
                                                    where abd.CustomerId == customerId && abd.StatusId == (int)AbandonedStatus.Abandoned &&
                                                            abd.IsSoftDeleted == false
                                                    select new ProductInfoModel()
                                                    {
                                                        ProductId = pr.Id,
                                                        ProductName = pr.Name,
                                                        ProductSku = pr.Sku,
                                                        ProductQuantity = shp.Quantity,
                                                        CartUpdatedOn = shp.UpdatedOnUtc
                                                    };

            return await products.ToListAsync();
        }

        public virtual async Task<IList<AbandonedCart>> GetAbandonedCartsByCustomerIdAsync(int customerId)
        {
            var abandonedCarts = from abd in _abandonedCartRepository.Table
                                 where abd.CustomerId == customerId && abd.StatusId == (int)AbandonedStatus.Abandoned &&
                                      abd.IsSoftDeleted == false
                                 select abd;

            return await abandonedCarts.ToListAsync();
        }

        public virtual async Task BulkUpdateAbandonmentCarts(IList<AbandonedCart> abandonedCarts)
        {
            await _abandonedCartRepository.UpdateAsync(abandonedCarts);
        }

        public virtual async Task<AbandonedCart> GetLastInactiveAbandonedCartByCustomerAsync(int customerId)
        {
            var abandonedCart = await (from abd in _abandonedCartRepository.Table
                                       where abd.CustomerId == customerId && abd.StatusId == (int)AbandonedStatus.InAction &&
                                           abd.IsSoftDeleted == false
                                       orderby abd.StatusChangedOn
                                       select abd).FirstOrDefaultAsync();

            return abandonedCart;
        }

        public virtual async Task<bool> IsCustomerOnlineByCustomerUsernameAsync(string username)
        {
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var abandonedCartsSettings = await _settingService.LoadSettingAsync<AbandonmentCartSettings>(storeScope);

            var query = _customerRepository.Table;
            var customer = await query.Where(c => c.LastActivityDateUtc > DateTime.UtcNow.AddMinutes(-abandonedCartsSettings.CustomerOnlineCutoffTime)
                                                && !c.Deleted && c.Username == username).FirstOrDefaultAsync();

            return customer != null;
        }

        public virtual async Task<IPagedList<AbandonmentListViewModel>> GetAllAbandonmentsAsync(string firstName = "",
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


            var abandonmentList = from abd in cartQuery
                                  join cst in customerQuery on abd.CustomerId equals cst.Id
                                  join prd in _productRepository.Table on abd.ProductId equals prd.Id
                                  where abd.IsSoftDeleted == false
                                  select new AbandonmentListViewModel()
                                  {
                                      Id = abd.Id,
                                      CustomerName = cst.Email,
                                      CustomerId = cst.Id,
                                      ProductName = prd.Name,
                                      ProductId = prd.Id,
                                      ProductSku = prd.Sku,
                                      StatusId = abd.StatusId,
                                      StatusChangedOn = abd.StatusChangedOn,
                                      FirstNotificationSentOn = abd.FirstNotificationSentOn,
                                      SecondNotificationSentOn = abd.SecondNotificationSentOn,
                                      Quantity = abd.Quantity,
                                      UnitPrice = abd.UnitPrice,
                                      TotalPrice = abd.TotalPrice,
                                      AttributeInfo = abd.AttributeInfo
                                  };

            if (statusId > 0)
                abandonmentList = abandonmentList.Where(c => c.StatusId == statusId);

            if (abandonmentList == null)
                return null;
            return await abandonmentList.ToPagedListAsync(pageIndex, pageSize);
        }

        public virtual async Task<AbandonmentDetailsViewModel> GetAbandonedCartDetailByIdAsync(int id)
        {
            var abandonmentList = from abd in _abandonedCartRepository.Table
                                  join cst in _customerRepository.Table on abd.CustomerId equals cst.Id
                                  join prd in _productRepository.Table on abd.ProductId equals prd.Id
                                  where abd.IsSoftDeleted == false && abd.Id == id
                                  select new AbandonmentDetailsViewModel()
                                  {
                                      AbandonmentInfo = new AbandonmentListViewModel()
                                      {
                                          Id = abd.Id,
                                          CustomerName = cst.FirstName + " " + cst.LastName,
                                          CustomerId = cst.Id,
                                          ProductName = prd.Name,
                                          ProductSku = prd.Sku,
                                          StatusId = abd.StatusId,
                                          StatusName = Enum.GetName(typeof(AbandonedStatus), abd.StatusId),
                                          StatusChangedOn = abd.StatusChangedOn,
                                          FirstNotificationSentOn = abd.FirstNotificationSentOn,
                                          SecondNotificationSentOn = abd.SecondNotificationSentOn,
                                          Quantity = abd.Quantity,
                                          UnitPrice = abd.UnitPrice,
                                          TotalPrice = abd.TotalPrice,
                                          AttributeInfo = abd.AttributeInfo
                                      }
                                  };

            return await abandonmentList.FirstOrDefaultAsync();
        }

        public virtual async Task<AbandonedCart> GetAbandonedCartByShoppingCartIdAsync(int shoppingCartItemId)
        {
            var query = _abandonedCartRepository.Table;
            return await query.Where(c => c.ShoppingCartItemId == shoppingCartItemId).FirstOrDefaultAsync();
        }

        public virtual async Task<int> BulkDeleteAbandonedCartsAsync(AbandonmentMaintenanceModel maintenanceModel)
        {
            var utcTimeDiff = DateTime.UtcNow - DateTime.Now;

            var query = _abandonedCartRepository.Table;
            query = query.Where(c => c.StatusId == maintenanceModel.StatusId && c.StatusChangedOn.AddHours(-utcTimeDiff.TotalHours).Date <= maintenanceModel.LastActivityBefore.Date);
            var carts = await query.ToListAsync();

            await _abandonedCartRepository.DeleteAsync(carts);
            return carts.Count;
        }

        public virtual async Task<int> GetCustomerAbandonedCartsCountAsync(string firstName = "",
            string lastName = "",
            string email = "",
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

            var quantityList = await (from abd in cartQuery
                                      join cst in customerQuery on abd.CustomerId equals cst.Id
                                      where abd.IsSoftDeleted == false
                                      select abd.Quantity).ToListAsync();
            return quantityList.Sum();
        }

        #endregion
    }
}