using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using NopStation.Plugin.Misc.WidgetManager.Areas.Admin.Models;

namespace NopStation.Plugin.Widgets.Announcement.Areas.Admin.Models;

public record AnnouncementItemModel : BaseNopEntityModel, ILocalizedModel<AnnouncementItemLocalizedModel>, IStoreMappingSupportedModel,
    IAclSupportedModel, IScheduleSupportedModel, ICustomerConditionSupportedModel
{
    public AnnouncementItemModel()
    {
        Locales = new List<AnnouncementItemLocalizedModel>();
        SelectedStoreIds = new List<int>();
        AvailableStores = new List<SelectListItem>();

        SelectedCustomerRoleIds = new List<int>();
        AvailableCustomerRoles = new List<SelectListItem>();

        Schedule = new ScheduleModel();
        CustomerConditionSearchModel = new CustomerConditionSearchModel();
    }

    [NopResourceDisplayName("Admin.NopStation.Announcement.AnnouncementItems.Fields.DisplayOrder")]
    public int DisplayOrder { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Announcement.AnnouncementItems.Fields.Name")]
    public string Name { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Announcement.AnnouncementItems.Fields.Title")]
    public string Title { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Announcement.AnnouncementItems.Fields.Description")]
    public string Description { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Announcement.AnnouncementItems.Fields.Color")]
    public string Color { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Announcement.AnnouncementItems.Fields.Active")]
    public bool Active { get; set; }

    public IList<AnnouncementItemLocalizedModel> Locales { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Announcement.AnnouncementItems.Fields.LimitedToStores")]
    public IList<int> SelectedStoreIds { get; set; }
    public IList<SelectListItem> AvailableStores { get; set; }

    public ScheduleModel Schedule { get; set; }

    public CustomerConditionSearchModel CustomerConditionSearchModel { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Announcement.AnnouncementItems.Fields.AclCustomerRoles")]
    public IList<int> SelectedCustomerRoleIds { get; set; }
    public IList<SelectListItem> AvailableCustomerRoles { get; set; }
}

public class AnnouncementItemLocalizedModel : ILocalizedLocaleModel
{
    public int LanguageId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Announcement.AnnouncementItems.Fields.Title")]
    public string Title { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Announcement.AnnouncementItems.Fields.Description")]
    public string Description { get; set; }
}
