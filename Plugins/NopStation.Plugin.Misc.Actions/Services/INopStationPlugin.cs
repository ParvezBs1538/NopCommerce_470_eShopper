using System.Collections.Generic;
using Nop.Services.Plugins;

namespace NopStation.Plugin.Misc.Core.Services;

public interface INopStationPlugin : IPlugin
{
    List<KeyValuePair<string, string>> PluginResouces();
}
