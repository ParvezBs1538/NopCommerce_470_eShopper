using System.Collections.Generic;
using System.Threading.Tasks;
using NopStation.Plugin.Misc.WebApi.Domains;

namespace NopStation.Plugin.Misc.WebApi.Services
{
    public interface IApiStringResourceService
    {
        Task DeleteApiStringResourceAsync(ApiStringResource apiStringResource);

        Task InsertApiStringResourceAsync(ApiStringResource apiStringResource);

        Task InsertApiStringResourceAsync(List<ApiStringResource> apiStringResources);

        Task UpdateApiStringResourceAsync(ApiStringResource apiStringResource);

        Task<ApiStringResource> GetApiStringResourceByIdAsync(int apiStringResourceId);

        Task<ApiStringResource> GetApiStringResourceByNameAsync(string resourceName);

        Task<Dictionary<string, KeyValuePair<string, string>>> GetAllResourceValuesAsync(int languageId);
    }
}