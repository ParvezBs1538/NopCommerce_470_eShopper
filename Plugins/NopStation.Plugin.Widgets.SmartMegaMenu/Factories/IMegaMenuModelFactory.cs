using System.Collections.Generic;
using System.Threading.Tasks;
using NopStation.Plugin.Widgets.SmartMegaMenu.Models;

namespace NopStation.Plugin.Widgets.SmartMegaMenu.Factories;

public interface IMegaMenuModelFactory
{
    Task<IList<MegaMenuModel>> PrepareMegaMenuModelsAsync(string widgetZones);
}
