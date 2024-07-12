using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Stores;
using Nop.Core.Events;
using Nop.Data;
using Nop.Services.Stores;
using NopStation.Plugin.Misc.WebApiNotification.Domains;

namespace NopStation.Plugin.Misc.WebApiNotification.Services
{
    public class WebApiNotificationTemplateService : IPushNotificationTemplateService
    {
        #region Fields

        private readonly IStaticCacheManager _cacheManager;
        private readonly IRepository<WebApiNotificationTemplate> _pushNotificationTemplateRepository;
        private readonly CatalogSettings _catalogSettings;
        private readonly IRepository<StoreMapping> _storeMappingRepository;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IStoreContext _storeContext;
        private readonly IEventPublisher _eventPublisher;

        #endregion

        #region Ctor

        public WebApiNotificationTemplateService(IStaticCacheManager cacheManager,
            IRepository<WebApiNotificationTemplate> pushNotificationTemplateRepository,
            CatalogSettings catalogSettings,
            IRepository<StoreMapping> storeMappingRepository,
            IStoreMappingService storeMappingService,
            IStoreContext storeContext,
            IEventPublisher eventPublisher)
        {
            _pushNotificationTemplateRepository = pushNotificationTemplateRepository;
            _cacheManager = cacheManager;
            _catalogSettings = catalogSettings;
            _storeMappingRepository = storeMappingRepository;
            _storeMappingService = storeMappingService;
            _storeContext = storeContext;
            _eventPublisher = eventPublisher;
        }

        #endregion

        #region Methods

        public async Task DeletePushNotificationTemplateAsync(WebApiNotificationTemplate pushNotificationTemplate)
        {
            await _pushNotificationTemplateRepository.DeleteAsync(pushNotificationTemplate);
            await _cacheManager.RemoveByPrefixAsync(WebApiNotificationDefaults.MessageTemplatesPrefixCacheKey);
        }

        public async Task InsertPushNotificationTemplateAsync(WebApiNotificationTemplate pushNotificationTemplate)
        {
            await _pushNotificationTemplateRepository.InsertAsync(pushNotificationTemplate);
            await _cacheManager.RemoveByPrefixAsync(WebApiNotificationDefaults.MessageTemplatesPrefixCacheKey);
        }

        public async Task UpdatePushNotificationTemplateAsync(WebApiNotificationTemplate pushNotificationTemplate)
        {
            await _pushNotificationTemplateRepository.UpdateAsync(pushNotificationTemplate);
            await _cacheManager.RemoveByPrefixAsync(WebApiNotificationDefaults.MessageTemplatesPrefixCacheKey);
        }

        public async Task<WebApiNotificationTemplate> GetPushNotificationTemplateByIdAsync(int pushNotificationTemplateId)
        {
            if (pushNotificationTemplateId == 0)
                return null;

            return await _pushNotificationTemplateRepository.GetByIdAsync(pushNotificationTemplateId, cache => default);
        }

        public async Task<IPagedList<WebApiNotificationTemplate>> GetAllPushNotificationTemplatesAsync(string keyword = null, bool? active = null,
            int storeId = 0, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var key = _cacheManager.PrepareKeyForDefaultCache(WebApiNotificationDefaults.MessageTemplatesAllCacheKey,
                keyword, active, storeId, pageIndex, pageSize);
            return await _cacheManager.GetAsync(key, () =>
            {
                var query = _pushNotificationTemplateRepository.Table;

                if (!string.IsNullOrWhiteSpace(keyword))
                    query = query.Where(x => x.Name.Contains(keyword) || x.Title.Contains(keyword) || x.Body.Contains(keyword));

                if (active.HasValue)
                    query = query.Where(x => x.Active == active.Value);

                if (storeId > 0 && !_catalogSettings.IgnoreStoreLimitations)
                {
                    var storeMappings = _storeMappingRepository.Table
                        .Where(x => x.EntityName == nameof(WebApiNotificationTemplate) && x.StoreId == storeId)
                        .ToList();

                    query = query.Where(x => !x.LimitedToStores || storeMappings.Any(y => y.EntityId == x.Id));
                }

                query = query.OrderBy(t => t.Name);

                return query.ToPagedListAsync(pageIndex, pageSize);
            });
        }

        public async Task<IList<WebApiNotificationTemplate>> GetPushNotificationTemplatesByNameAsync(string messageTemplateName, int storeId = 0)
        {
            if (string.IsNullOrWhiteSpace(messageTemplateName))
                return new List<WebApiNotificationTemplate>();

            var key = _cacheManager.PrepareKeyForDefaultCache(WebApiNotificationDefaults.MessageTemplatesByNameCacheKey,
                messageTemplateName, storeId);
            return await _cacheManager.GetAsync(key, async () =>
            {
                var templates = _pushNotificationTemplateRepository.Table
                    .Where(messageTemplate => messageTemplate.Name.Equals(messageTemplateName))
                    .OrderBy(messageTemplate => messageTemplate.Id).ToList();

                if (storeId > 0)
                    templates = await templates.WhereAwait(async messageTemplate => await _storeMappingService.AuthorizeAsync(messageTemplate, storeId)).ToListAsync();

                return templates;
            });
        }

        public virtual IList<WebApiNotificationTemplate> GetTemplatesByIds(int[] templateIds)
        {
            if (templateIds == null || templateIds.Length == 0)
                return new List<WebApiNotificationTemplate>();

            var query = from o in _pushNotificationTemplateRepository.Table
                        where templateIds.Contains(o.Id)
                        select o;
            var templates = query.ToList();
            //sort by passed identifiers
            var sortedTemplates = new List<WebApiNotificationTemplate>();
            foreach (var id in templateIds)
            {
                var template = templates.Find(x => x.Id == id);
                if (template != null)
                    sortedTemplates.Add(template);
            }

            return sortedTemplates;
        }

        #endregion
    }
}
