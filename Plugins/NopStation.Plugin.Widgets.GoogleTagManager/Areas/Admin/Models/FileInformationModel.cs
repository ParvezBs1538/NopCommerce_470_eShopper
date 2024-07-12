using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.GoogleTagManager.Areas.Admin.Models
{
    public class FileInformationModel
    {
        [NopResourceDisplayName("Admin.NopStation.GoogleTagManager.ExportFileInformation.Fields.GAContainerId")]
        public string GAContainerId { get; set; }
    }
}
