using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Customers;

namespace NopStation.Plugin.Misc.Core.Services;

public interface ISmsPluginManager
{
    Task<IList<ISmsPlugin>> LoadSmsPluginsAsync(Customer customer = null, string pluginSystemName = "", int storeId = 0);
}