using System.Threading.Tasks;
using NopStation.Plugin.Widgets.Popups.Models;

namespace NopStation.Plugin.Widgets.Popups.Factories;

public interface IPopupModelFactory
{
    Task<PopupPublicModel> PreparePopupPublicModelAsync();
}