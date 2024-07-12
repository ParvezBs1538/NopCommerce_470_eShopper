using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Caching;
using Nop.Data;
using Nop.Services.Localization;
using NopStation.Plugin.Misc.WebApi.Domains;
using NopStation.Plugin.Misc.WebApi.Infrastructure.Cache;

namespace NopStation.Plugin.Misc.WebApi.Services
{
    public class ApiStringResourceService : IApiStringResourceService
    {
        #region Fields

        private readonly IRepository<ApiStringResource> _apiStringResourceRepository;
        private readonly ILocalizationService _localizationService;
        private readonly IStaticCacheManager _cacheManager;

        #endregion

        #region Ctor

        public ApiStringResourceService(IRepository<ApiStringResource> apiStringResourceRepository,
            ILocalizationService localizationService,
            IStaticCacheManager staticCacheManager)
        {
            _apiStringResourceRepository = apiStringResourceRepository;
            _localizationService = localizationService;
            _cacheManager = staticCacheManager;
        }

        #endregion

        #region Utilities

        private static Dictionary<string, KeyValuePair<int, string>> ResourceValuesToDictionary(IEnumerable<ApiStringResource> locales)
        {
            //format: <name, <id, value>>
            var dictionary = new Dictionary<string, KeyValuePair<int, string>>();
            foreach (var locale in locales)
            {
                var resourceName = locale.ResourceName.ToLowerInvariant();
                if (!dictionary.ContainsKey(resourceName))
                    dictionary.Add(resourceName, new KeyValuePair<int, string>(locale.Id, locale.ResourceName));
            }

            return dictionary;
        }

        #endregion

        #region Methods

        public async Task DeleteApiStringResourceAsync(ApiStringResource apiStringResource)
        {
            if (apiStringResource == null)
                throw new ArgumentNullException(nameof(apiStringResource));

            await _apiStringResourceRepository.DeleteAsync(apiStringResource);

            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.StringResourcePrefixCacheKey);
        }

        public async Task InsertApiStringResourceAsync(ApiStringResource apiStringResource)
        {
            if (apiStringResource == null)
                throw new ArgumentNullException(nameof(apiStringResource));

            await _apiStringResourceRepository.InsertAsync(apiStringResource);

            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.StringResourcePrefixCacheKey);
        }

        public async Task InsertApiStringResourceAsync(List<ApiStringResource> apiStringResources)
        {
            if (apiStringResources == null)
                throw new ArgumentNullException(nameof(apiStringResources));

            await _apiStringResourceRepository.InsertAsync(apiStringResources);

            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.StringResourcePrefixCacheKey);
        }

        public async Task UpdateApiStringResourceAsync(ApiStringResource apiStringResource)
        {
            if (apiStringResource == null)
                throw new ArgumentNullException(nameof(apiStringResource));

            await _apiStringResourceRepository.UpdateAsync(apiStringResource);

            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.StringResourcePrefixCacheKey);
        }

        public async Task<ApiStringResource> GetApiStringResourceByIdAsync(int apiStringResourceId)
        {
            if (apiStringResourceId == 0)
                return null;

            return await _apiStringResourceRepository.GetByIdAsync(apiStringResourceId, cache => default);
        }

        public async Task<ApiStringResource> GetApiStringResourceByNameAsync(string resourceKey)
        {
            if (resourceKey == null)
                resourceKey = string.Empty;
            resourceKey = resourceKey.Trim().ToLowerInvariant();

            var query = from l in _apiStringResourceRepository.Table
                        orderby l.ResourceName
                        where l.ResourceName.ToLower() == resourceKey
                        select l;

            return await query.FirstOrDefaultAsync();
        }

        public async Task<Dictionary<string, KeyValuePair<string, string>>> GetAllResourceValuesAsync(int languageId)
        {
            var localeResources = await _localizationService.GetAllResourceValuesAsync(languageId, loadPublicLocales: null);

            var key = string.Format(ApiModelCacheDefaults.StringResourceKey, languageId);
            return await _cacheManager.GetAsync(new CacheKey(key, ApiModelCacheDefaults.StringResourcePrefixCacheKey), () =>
            {
                var query = from l in _apiStringResourceRepository.Table
                            orderby l.ResourceName
                            select l;
                var appResources = ResourceValuesToDictionary(query);

                var resources = new Dictionary<string, KeyValuePair<string, string>>();
                foreach (var item in appResources)
                {
                    if (localeResources.TryGetValue(item.Key, out var value))
                        resources.Add(item.Key, new KeyValuePair<string, string>($"{item.Value.Key}__{value.Key}__{languageId}", value.Value));
                    else
                        resources.Add(item.Key, new KeyValuePair<string, string>($"{item.Value.Key}__0__{languageId}", ""));
                }

                return resources;
            });
        }

        #endregion
    }
}
