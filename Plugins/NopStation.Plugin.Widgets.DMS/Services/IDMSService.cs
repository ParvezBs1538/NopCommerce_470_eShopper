using System.Collections.Generic;
using System.Threading.Tasks;

namespace NopStation.Plugin.Widgets.DMS.Services
{
    public interface IDMSService
    {
        Task<Dictionary<string, string>> LoadAppStringResourcesAsync();

        Task<Dictionary<string, string>> LoadLocalizedResourcesAsync(int languageId);
    }
}