using System.Collections.Generic;
using System.Threading.Tasks;
using NopStation.Plugin.Widgets.Announcement.Domains;

namespace NopStation.Plugin.Widgets.Announcement.Services;

public interface IAnnouncementItemService
{
    Task DeleteAnnouncementItemAsync(AnnouncementItem announcementItem);

    Task InsertAnnouncementItemAsync(AnnouncementItem announcementItem);

    Task UpdateAnnouncementItemAsync(AnnouncementItem announcementItem);

    Task<AnnouncementItem> GetAnnouncementItemByIdAsync(int announcementItemId);

    Task<IList<AnnouncementItem>> GetAllAnnouncementItemsAsync(string keywords = null, bool showHidden = false, bool? overridePublished = false,
        bool validScheduleOnly = false, int storeId = 0);
}