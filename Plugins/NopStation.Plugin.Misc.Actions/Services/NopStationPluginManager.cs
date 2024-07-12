using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Localization;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Plugins;

namespace NopStation.Plugin.Misc.Core.Services;

public class NopStationPluginManager : PluginManager<INopStationPlugin>, INopStationPluginManager
{
    #region Fields

    private readonly ILocalizationService _localizationService;

    #endregion

    #region Ctor

    public NopStationPluginManager(IPluginService pluginService,
        ILocalizationService localizationService,
        ICustomerService customerService) : base(customerService, pluginService)
    {
        _localizationService = localizationService;
    }

    #endregion

    #region Methods

    public virtual async Task<IList<INopStationPlugin>> LoadNopStationPluginsAsync(Customer customer = null, string pluginSystemName = "",
        int storeId = 0)
    {
        //get loaded plugins according to passed system names
        return (await LoadAllPluginsAsync(customer, storeId))
            .Where(plugin => string.IsNullOrWhiteSpace(pluginSystemName) ||
                plugin.PluginDescriptor.SystemName.Equals(pluginSystemName))
            .ToList();
    }

    public virtual async Task<IPagedList<KeyValuePair<string, string>>> LoadPluginStringResourcesAsync(string pluginSystemName = "",
        string keyword = "", int languageId = 0, int storeId = 0, int pageIndex = 0, int pageSize = int.MaxValue)
    {
        var nopStationPlugins = await LoadNopStationPluginsAsync(pluginSystemName: pluginSystemName, storeId: storeId);
        var originnalResources = nopStationPlugins.SelectMany(x => x.PluginResouces()
            .Where(y => string.IsNullOrWhiteSpace(keyword) || y.Key.Contains(keyword, StringComparison.InvariantCultureIgnoreCase)))
            .OrderBy(x => x.Key)
            .ToList();

        var total = originnalResources.Count;
        originnalResources = originnalResources.Skip(pageIndex * pageSize).Take(pageSize).ToList();

        var pagedResources = new List<KeyValuePair<string, string>>();
        foreach (var item in originnalResources)
        {
            var resource = await _localizationService.GetResourceAsync(item.Key, languageId, false, "", true);
            if (string.IsNullOrWhiteSpace(resource))
            {
                resource = item.Value;
                await _localizationService.InsertLocaleStringResourceAsync(new LocaleStringResource()
                {
                    ResourceName = item.Key,
                    ResourceValue = item.Value,
                    LanguageId = languageId
                });
            }
            pagedResources.Add(new KeyValuePair<string, string>(item.Key, resource));
        }

        return new PagedList<KeyValuePair<string, string>>(pagedResources, pageIndex, pageSize, total);
    }

    #endregion
}
