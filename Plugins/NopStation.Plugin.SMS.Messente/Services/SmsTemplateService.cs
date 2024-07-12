using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Stores;
using Nop.Data;
using NopStation.Plugin.SMS.Messente.Domains;
using Nop.Services.Caching;
using Nop.Services.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NopStation.Plugin.SMS.Messente.Services
{
    public class SmsTemplateService : ISmsTemplateService
    {
        #region Fields

        private readonly IRepository<SmsTemplate> _smsTemplateRepository;
        private readonly CatalogSettings _catalogSettings;
        private readonly IRepository<StoreMapping> _storeMappingRepository;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IStaticCacheManager _cacheManager;

        #endregion

        #region Ctor

        public SmsTemplateService(
            IRepository<SmsTemplate> smsTemplateRepository,
            IStaticCacheManager cacheManager,
            CatalogSettings catalogSettings,
            IRepository<StoreMapping> storeMappingRepository,
            IStoreMappingService storeMappingService)
        {
            _smsTemplateRepository = smsTemplateRepository;
            _cacheManager = cacheManager;
            _catalogSettings = catalogSettings;
            _storeMappingRepository = storeMappingRepository;
            _storeMappingService = storeMappingService;
        }

        #endregion

        #region Methods

        public async Task DeleteSmsTemplateAsync(SmsTemplate smsTemplate)
        {
            await _smsTemplateRepository.DeleteAsync(smsTemplate);

            await _cacheManager.RemoveByPrefixAsync(SmsDefaults.MessageTemplatesPrefixCacheKey);
        }

        public async Task InsertSmsTemplateAsync(SmsTemplate smsTemplate)
        {
            await _smsTemplateRepository.InsertAsync(smsTemplate);

            await _cacheManager.RemoveByPrefixAsync(SmsDefaults.MessageTemplatesPrefixCacheKey);
        }

        public async Task UpdateSmsTemplateAsync(SmsTemplate smsTemplate)
        {
            await _smsTemplateRepository.UpdateAsync(smsTemplate);

            await _cacheManager.RemoveByPrefixAsync(SmsDefaults.MessageTemplatesPrefixCacheKey);
        }

        public async Task<SmsTemplate> GetSmsTemplateByIdAsync(int smsTemplateId)
        {
            if (smsTemplateId == 0)
                return null;

            return await _smsTemplateRepository.GetByIdAsync(smsTemplateId, cache => default);
        }

        public async Task<IList<SmsTemplate>> GetAllSmsTemplatesAsync(int storeId)
        {
            var cache = new CacheKey(SmsDefaults.MessageTemplatesAllCacheKey);
            var key = _cacheManager.PrepareKeyForDefaultCache(cache, storeId);

            return await _cacheManager.GetAsync(key, () =>
            {
                var query = _smsTemplateRepository.Table;
                query = query.OrderBy(t => t.Name);

                if (storeId <= 0 || _catalogSettings.IgnoreStoreLimitations)
                    return query.ToList();

                //store mapping
                query = from t in query
                        join sm in _storeMappingRepository.Table
                            on new { c1 = t.Id, c2 = nameof(SmsTemplate) } equals new { c1 = sm.EntityId, c2 = sm.EntityName } into tSm
                        from sm in tSm.DefaultIfEmpty()
                        where !t.LimitedToStores || storeId == sm.StoreId
                        select t;

                query = query.Distinct().OrderBy(t => t.Name);

                return query.ToList();
            });
        }

        public async Task<IList<SmsTemplate>> GetSmsTemplatesByNameAsync(string messageTemplateName, int? storeId = null)
        {
            if (string.IsNullOrWhiteSpace(messageTemplateName))
                throw new ArgumentException(nameof(messageTemplateName));

            var cache = new CacheKey(SmsDefaults.MessageTemplatesByNameCacheKey);

            var key = _cacheManager.PrepareKeyForDefaultCache(cache, messageTemplateName, storeId ?? 0);

            return await _cacheManager.GetAsync(key, async () =>
            {
                //get message templates with the passed name
                var templates = _smsTemplateRepository.Table
                    .Where(messageTemplate => messageTemplate.Name.Equals(messageTemplateName))
                    .OrderBy(messageTemplate => messageTemplate.Id).ToList();

                //filter by the store
                if (storeId.HasValue && storeId.Value > 0)
                    templates = await (templates.WhereAwait(async messageTemplate => await _storeMappingService.AuthorizeAsync(messageTemplate, storeId.Value))).ToListAsync();

                return templates;
            });
        }

        #endregion
    }
}
