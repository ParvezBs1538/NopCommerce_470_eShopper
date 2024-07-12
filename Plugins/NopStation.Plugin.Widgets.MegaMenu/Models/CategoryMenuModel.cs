using System.Collections.Generic;
using Nop.Web.Framework.Models;
using Nop.Web.Models.Media;

namespace NopStation.Plugin.Widgets.MegaMenu.Models;

public record CategoryMenuModel : BaseNopEntityModel
{
    public CategoryMenuModel()
    {
        SubCategories = new List<CategoryMenuModel>();
        PictureModel = new PictureModel();
        ParentPicture = new PictureModel();
    }

    public string Name { get; set; }
    public string SeName { get; set; }
    public int? NumberOfProducts { get; set; }
    public bool IncludeInTopMenu { get; set; }
    public PictureModel PictureModel { get; set; }
    public PictureModel ParentPicture { get; set; }
    public List<CategoryMenuModel> SubCategories { get; set; }
}
