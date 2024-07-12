using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Misc.WebApi.Areas.Admin.Models
{
    /// <summary>
    /// Represents a locale resource list model
    /// </summary>
    public record LocaleResourceListModel : BasePagedListModel<LocaleResourceModel>
    {
    }
}