using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.Customers;
using Nop.Services.Customers;
using Nop.Services.Plugins;

namespace NopStation.Plugin.Misc.Core.Services;

public class SmsPluginManager : PluginManager<ISmsPlugin>, ISmsPluginManager
{
    #region Ctor

    public SmsPluginManager(ICustomerService customerService,
        IPluginService pluginService) : base(customerService, pluginService)
    {
    }

    #endregion

    #region Methods

    public virtual async Task<IList<ISmsPlugin>> LoadSmsPluginsAsync(Customer customer = null, string pluginSystemName = "",
        int storeId = 0)
    {
        //get loaded plugins according to passed system names
        return (await LoadAllPluginsAsync(customer, storeId))
            .Where(plugin => string.IsNullOrWhiteSpace(pluginSystemName) ||
                plugin.PluginDescriptor.SystemName.Equals(pluginSystemName))
            .ToList();
    }

    #endregion
}
