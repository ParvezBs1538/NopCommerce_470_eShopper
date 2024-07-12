using System.Threading.Tasks;
using NopStation.Plugin.Widgets.MegaMenu.Models;

namespace NopStation.Plugin.Widgets.MegaMenu.Factories;

public interface IMegaMenuModelFactory
{
    Task<MegaMenuModel> PrepareMegaMenuModelAsync();
}