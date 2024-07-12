using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Stores;
using Nop.Data;
using Nop.Services.Stores;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Domains;

namespace NopStation.Plugin.Widgets.ProgressiveWebApp.Services
{
    public class PushNotificationTemplateService : IPushNotificationTemplateService
    {
        #region Fields

        private readonly IRepository<PushNotificationTemplate> _pushNotificationTemplateRepository;
        private readonly CatalogSettings _catalogSettings;
        private readonly IRepository<StoreMapping> _storeMappingRepository;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IStaticCacheManager _cacheManager;

        #endregion

        #region Ctor

        public PushNotificationTemplateService(
            IRepository<PushNotificationTemplate> pushNotificationTemplateRepository,
            IStaticCacheManager cacheManager,
            CatalogSettings catalogSettings,
            IRepository<StoreMapping> storeMappingRepository,
            IStoreMappingService storeMappingService)
        {
            _pushNotificationTemplateRepository = pushNotificationTemplateRepository;
            _cacheManager = cacheManager;
            _catalogSettings = catalogSettings;
            _storeMappingRepository = storeMappingRepository;
            _storeMappingService = storeMappingService;
        }

        #endregion

        #region Methods

        public async Task DeletePushNotificationTemplateAsync(PushNotificationTemplate pushNotificationTemplate)
        {
            await _pushNotificationTemplateRepository.DeleteAsync(pushNotificationTemplate);

            await _cacheManager.RemoveByPrefixAsync(PushNotificationDefaults.MessageTemplatesPrefixCacheKey);
        }

        public async Task InsertPushNotificationTemplateAsync(PushNotificationTemplate pushNotificationTemplate)
        {
            await _pushNotificationTemplateRepository.InsertAsync(pushNotificationTemplate);

            await _cacheManager.RemoveByPrefixAsync(PushNotificationDefaults.MessageTemplatesPrefixCacheKey);
        }

        public async Task UpdatePushNotificationTemplateAsync(PushNotificationTemplate pushNotificationTemplate)
        {
            await _pushNotificationTemplateRepository.UpdateAsync(pushNotificationTemplate);

            await _cacheManager.RemoveByPrefixAsync(PushNotificationDefaults.MessageTemplatesPrefixCacheKey);
        }

        public async Task<PushNotificationTemplate> GetPushNotificationTemplateByIdAsync(int pushNotificationTemplateId)
        {
            if (pushNotificationTemplateId == 0)
                return null;

            return await _pushNotificationTemplateRepository.GetByIdAsync(pushNotificationTemplateId, cache => default);
        }

        public async Task<IList<PushNotificationTemplate>> GetAllPushNotificationTemplatesAsync(int storeId)
        {
            var cache = new CacheKey(PushNotificationDefaults.MessageTemplatesAllCacheKey, PushNotificationDefaults.MessageTemplatesPrefixCacheKey);
            var key = _cacheManager.PrepareKeyForDefaultCache(cache, storeId);

            return await _cacheManager.GetAsync(key, () =>
            {
                var query = _pushNotificationTemplateRepository.Table;
                query = query.OrderBy(t => t.Name);

                if (storeId <= 0 || _catalogSettings.IgnoreStoreLimitations)
                    return query.ToList();

                //store mapping
                query = from t in query
                        join sm in _storeMappingRepository.Table
                            on new { c1 = t.Id, c2 = nameof(PushNotificationTemplate) } equals new { c1 = sm.EntityId, c2 = sm.EntityName } into tSm
                        from sm in tSm.DefaultIfEmpty()
                        where !t.LimitedToStores || storeId == sm.StoreId
                        select t;

                query = query.Distinct().OrderBy(t => t.Name);

                return query.ToList();
            });
        }

        public async Task<IList<PushNotificationTemplate>> GetPushNotificationTemplatesByNameAsync(string messageTemplateName, int? storeId = null)
        {
            if (string.IsNullOrWhiteSpace(messageTemplateName))
                throw new ArgumentException(nameof(messageTemplateName));

            var cache = new CacheKey(PushNotificationDefaults.MessageTemplatesByNameCacheKey, PushNotificationDefaults.MessageTemplatesPrefixCacheKey);

            var key = _cacheManager.PrepareKeyForDefaultCache(cache, messageTemplateName, storeId ?? 0);

            return await _cacheManager.GetAsync(key, async () =>
            {
                //get message templates with the passed name
                var templates = _pushNotificationTemplateRepository.Table
                    .Where(messageTemplate => messageTemplate.Name.Equals(messageTemplateName))
                    .OrderBy(messageTemplate => messageTemplate.Id).ToList();

                //filter by the store
                if (storeId.HasValue && storeId.Value > 0)
                    templates = await templates.WhereAwait(async messageTemplate => await _storeMappingService.AuthorizeAsync(messageTemplate, storeId.Value)).ToListAsync();

                return templates;
            });
        }

        #endregion
    }
}
