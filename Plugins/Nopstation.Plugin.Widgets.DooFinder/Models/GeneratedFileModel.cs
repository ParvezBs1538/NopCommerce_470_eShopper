using Nop.Web.Framework.Models;

namespace Nopstation.Plugin.Widgets.DooFinder.Models
{
    public record GeneratedFileModel : BaseNopModel
    {
        public string StoreName { get; set; }
        public string FileUrl { get; set; }
    }
}
