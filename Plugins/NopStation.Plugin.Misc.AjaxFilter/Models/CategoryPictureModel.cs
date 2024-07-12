using Nop.Web.Models.Media;

namespace NopStation.Plugin.Misc.AjaxFilter.Models
{
    public record CategoryPictureModel : PictureModel
    {
        public int ProductId { get; set; }
    }
}
