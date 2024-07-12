using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Misc.Core.Models.Api;
using NopStation.Plugin.Misc.WebApi.Services;

namespace NopStation.Plugin.Misc.WebApi.Factories
{
    public class CommonApiModelFactory : ICommonApiModelFactory
    {
        private readonly IWorkContext _workContext;
        private readonly IApiStringResourceService _apiStringResourceService;

        public CommonApiModelFactory(IWorkContext workContext,
            IApiStringResourceService apiStringResourceService)
        {
            _workContext = workContext;
            _apiStringResourceService = apiStringResourceService;
        }

        public async Task<IList<KeyValueApi>> GetStringRsourcesAsync(int? languageId = null)
        {
            var langId = languageId ?? (await _workContext.GetWorkingLanguageAsync()).Id;
            var model = new List<KeyValueApi>();

            var resources = await _apiStringResourceService.GetAllResourceValuesAsync(langId);
            foreach (var resource in resources)
            {
                model.Add(new KeyValueApi()
                {
                    Key = resource.Key,
                    Value = string.IsNullOrWhiteSpace(resource.Value.Value) ? resource.Key : resource.Value.Value
                });
            }

            return model;
        }
    }
}
