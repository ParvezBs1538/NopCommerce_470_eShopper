using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Customers;

namespace NopStation.Plugin.Misc.Core.Services;

public interface INopStationPluginManager
{
    Task<IList<INopStationPlugin>> LoadNopStationPluginsAsync(Customer customer = null, string pluginSystemName = "",
        int storeId = 0);

    Task<IPagedList<KeyValuePair<string, string>>> LoadPluginStringResourcesAsync(string pluginSystemName = "",
        string keyword = "", int languageId = 0, int storeId = 0, int pageIndex = 0, int pageSize = int.MaxValue);
}