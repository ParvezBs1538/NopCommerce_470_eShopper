using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.Flipbooks.Areas.Admin.Models
{
    public record FlipbookContentModel : BaseNopEntityModel
    {
        public FlipbookContentModel()
        {
            FlipbookContentProductSearchModel = new FlipbookContentProductSearchModel();
        }

        [NopResourceDisplayName("Admin.NopStation.Flipbooks.FlipbookContents.Fields.Flipbook")]
        public int FlipbookId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Flipbooks.FlipbookContents.Fields.Flipbook")]
        public string FlipbookName { get; set; }

        [UIHint("Picture")]
        [NopResourceDisplayName("Admin.NopStation.Flipbooks.FlipbookContents.Fields.Image")]
        public int ImageId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Flipbooks.FlipbookContents.Fields.Image")]
        public string ImageUrl { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Flipbooks.FlipbookContents.Fields.Content")]
        public string Content { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Flipbooks.FlipbookContents.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Flipbooks.FlipbookContents.Fields.IsImage")]
        public bool IsImage { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Flipbooks.FlipbookContents.Fields.RedirectUrl")]
        public string RedirectUrl { get; set; }

        public FlipbookContentProductSearchModel FlipbookContentProductSearchModel { get; set; }
    }
}
