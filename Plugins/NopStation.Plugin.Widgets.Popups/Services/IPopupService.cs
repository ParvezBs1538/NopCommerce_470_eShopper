using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Widgets.Popups.Domains;

namespace NopStation.Plugin.Widgets.Popups.Services;

public interface IPopupService
{
    Task<IPagedList<Popup>> GetAllPopupsAsync(string keywords = null, int storeId = 0, int productId = 0,
        bool overrideProduct = false, bool showHidden = false, bool? overridePublished = null,
        bool validScheduleOnly = false, DeviceType? deviceType = null, int pageIndex = 0, int pageSize = int.MaxValue);

    Task<Popup> GetPopupByIdAsync(int popupId);

    Task InsertPopupAsync(Popup popup);

    Task UpdatePopupAsync(Popup popup);

    Task DeletePopupAsync(Popup popup);
}