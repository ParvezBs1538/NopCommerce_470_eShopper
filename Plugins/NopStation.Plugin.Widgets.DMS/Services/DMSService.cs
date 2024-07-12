using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nop.Core.Caching;
using Nop.Core.Infrastructure;
using Nop.Services.Localization;
using Nop.Services.Logging;
using NopStation.Plugin.Widgets.DMS.Services.Cache;

namespace NopStation.Plugin.Widgets.DMS.Services
{
    public class DMSService : IDMSService
    {
        private readonly IStaticCacheManager _cacheManager;
        private readonly ILocalizationService _localizationService;
        private readonly INopFileProvider _fileProvider;
        private readonly ILogger _logger;

        public DMSService(IStaticCacheManager cacheManager,
            ILocalizationService localizationService,
            INopFileProvider fileProvider,
            ILogger logger)
        {
            _cacheManager = cacheManager;
            _localizationService = localizationService;
            _fileProvider = fileProvider;
            _logger = logger;
        }

        public async Task<Dictionary<string, string>> LoadAppStringResourcesAsync()
        {
            var resources = new Dictionary<string, string>();
            try
            {
                var filePath = _fileProvider.Combine(_fileProvider.MapPath("/Plugins/NopStation.Plugin.Widgets.DMS/"), "stringResources.json");

                if (_fileProvider.FileExists(filePath))
                {
                    var jsonstr = _fileProvider.ReadAllText(filePath, Encoding.UTF8);
                    var list = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonstr);
                    if (list != null && list.Any())
                        return list;
                }
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync(ex.Message, ex);
            }

            return resources;
        }

        public async Task<Dictionary<string, string>> LoadLocalizedResourcesAsync(int languageId)
        {
            var cacheKey = _cacheManager.PrepareKey(DMSCacheDefaults.StringResourceKey, languageId);
            return await _cacheManager.GetAsync(cacheKey, async () =>
            {
                var strings = await LoadAppStringResourcesAsync();
                var list = new Dictionary<string, string>();

                foreach (var keyValuePair in strings)
                {
                    list.Add(keyValuePair.Key, await _localizationService.GetResourceAsync(keyValuePair.Key, languageId));
                }

                return list;
            });
        }
    }
}
