using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.PrevNextProduct.Models;

public class PublicInfoModel
{
    public PublicInfoModel()
    {
        Next = new ProductModel();
        Previous = new ProductModel();
    }

    public ProductModel Next { get; set; }

    public ProductModel Previous { get; set; }

    public record ProductModel : BaseNopEntityModel
    {
        public bool HasProduct { get; set; }

        public string Name { get; set; }

        public string SeName { get; set; }
    }
}
