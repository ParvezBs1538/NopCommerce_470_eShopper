using System.Threading.Tasks;
using NopStation.Plugin.Widgets.Popups.Areas.Admin.Models;
using NopStation.Plugin.Widgets.Popups.Domains;

namespace NopStation.Plugin.Widgets.Popups.Areas.Admin.Factories;

public interface IPopupModelFactory
{
    Task<PopupSearchModel> PreparePopupSearchModelAsync(PopupSearchModel searchModel);

    Task<PopupListModel> PreparePopupListModelAsync(PopupSearchModel searchModel);

    Task<PopupModel> PreparePopupModelAsync(PopupModel model, Popup popup, bool excludeProperties = false);
}
