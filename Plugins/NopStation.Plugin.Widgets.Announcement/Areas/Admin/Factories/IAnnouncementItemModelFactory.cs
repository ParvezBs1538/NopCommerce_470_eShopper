using System.Threading.Tasks;
using NopStation.Plugin.Widgets.Announcement.Areas.Admin.Models;
using NopStation.Plugin.Widgets.Announcement.Domains;

namespace NopStation.Plugin.Widgets.Announcement.Areas.Admin.Factories;

public interface IAnnouncementItemModelFactory
{
    Task<ConfigurationModel> PrepareConfigurationModelAsync();

    AnnouncementItemSearchModel PrepareAnnouncementItemSearchModel(AnnouncementItemSearchModel searchModel);

    Task<AnnouncementItemListModel> PrepareAnnouncementItemListModelAsync(AnnouncementItemSearchModel searchModel);

    Task<AnnouncementItemModel> PrepareAnnouncementItemModelAsync(AnnouncementItemModel model, AnnouncementItem announcementItem, bool excludeProperties = false);
}