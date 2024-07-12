using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Data;
using NopStation.Plugin.Misc.WebApi.Domains;
using NopStation.Plugin.Misc.WebApiNotification.Domains;

namespace NopStation.Plugin.Misc.WebApiNotification.Services
{
    public class WebApiNotificationCampaignService : IPushNotificationCampaignService
    {
        #region Fields

        private readonly IRepository<WebApiNotificationCampaign> _notificationCampaignRepository;
        private readonly IRepository<ApiDevice> _apiDeviceRepository;
        private readonly IRepository<CustomerCustomerRoleMapping> _customerCustomerRoleRepository;
        private readonly CatalogSettings _catalogSettings;

        #endregion

        #region Ctor

        public WebApiNotificationCampaignService(IRepository<ApiDevice> apiDeviceRepository,
            IRepository<WebApiNotificationCampaign> notificationCampaignRepository,
            IRepository<CustomerCustomerRoleMapping> customerCustomerRoleRepository,
            CatalogSettings catalogSettings)
        {
            _notificationCampaignRepository = notificationCampaignRepository;
            _apiDeviceRepository = apiDeviceRepository;
            _customerCustomerRoleRepository = customerCustomerRoleRepository;
            _catalogSettings = catalogSettings;
        }

        #endregion

        #region Methods

        public virtual async Task<IPagedList<WebApiNotificationCampaign>> GetAllPushNotificationCampaignsAsync(string keyword = "",
            DateTime? searchFrom = null, DateTime? searchTo = null, bool? addedToQueueStatus = null,
            int storeId = 0, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _notificationCampaignRepository.Table.Where(x => !x.Deleted);

            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(x => x.Name.Contains(keyword) || x.Title.Contains(keyword) || x.Body.Contains(keyword));
            if (searchFrom.HasValue)
                query = query.Where(x => x.SendingWillStartOnUtc >= searchFrom.Value);
            if (searchTo.HasValue)
                query = query.Where(x => x.SendingWillStartOnUtc <= searchTo.Value);
            if (addedToQueueStatus.HasValue)
                query = query.Where(x => x.AddedToQueueOnUtc.HasValue == addedToQueueStatus.Value);

            if (storeId > 0 && !_catalogSettings.IgnoreStoreLimitations)
            {
                query = query.Where(x => x.LimitedToStoreId == storeId);
            }

            query = query.OrderByDescending(x => x.Id);

            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

        public virtual async Task InsertPushNotificationCampaignAsync(WebApiNotificationCampaign campaign)
        {
            await _notificationCampaignRepository.InsertAsync(campaign);
        }

        public async Task<WebApiNotificationCampaign> GetPushNotificationCampaignByIdAsync(int id)
        {
            return await _notificationCampaignRepository.GetByIdAsync(id, cache => default);
        }

        public async Task UpdatePushNotificationCampaignAsync(WebApiNotificationCampaign campaign)
        {
            await _notificationCampaignRepository.UpdateAsync(campaign);
        }

        public async Task DeletePushNotificationCampaignAsync(WebApiNotificationCampaign campaign)
        {
            await _notificationCampaignRepository.DeleteAsync(campaign);
        }

        public async Task<IPagedList<ApiDevice>> GetCampaignDevicesAsync(WebApiNotificationCampaign campaign, int pageIndex = 0, int pageSize = 100)
        {
            var query = from nad in _apiDeviceRepository.Table
                        select nad;

            if (campaign.LimitedToStoreId > 0)
                query = from nad in query
                        where nad.StoreId == campaign.LimitedToStoreId
                        select nad;

            if (!string.IsNullOrEmpty(campaign.CustomerRoles))
            {
                var customerCustomerRoleIdMapping = from ccm in _customerCustomerRoleRepository.Table
                                                    where campaign.CustomerRoles.Contains(ccm.CustomerRoleId.ToString())
                                                    select ccm.CustomerId;

                query = from nad in query
                        where customerCustomerRoleIdMapping.Contains(nad.CustomerId)
                        select nad;
            }

            if (!string.IsNullOrEmpty(campaign.DeviceTypes))
                query = from nad in query
                        where campaign.DeviceTypes.Contains(nad.DeviceTypeId.ToString())
                        select nad;

            query = query.ToList().OrderByDescending(ad => ad.CreatedOnUtc).DistinctBy(ad => ad.SubscriptionId).AsQueryable();

            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

        #endregion
    }
}
