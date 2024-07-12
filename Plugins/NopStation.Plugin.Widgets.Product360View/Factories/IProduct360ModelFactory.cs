using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using NopStation.Plugin.Widgets.Product360View.Models;

namespace Nop.Plugin.Misc.AbandonedCarts.Factories
{
    public interface IProduct360ModelFactory
    {
        Task<Product360PictureListModel> PrepareProduct360PictureListModelAsync(Picture360SearchModel searchModel, Product product);
        Task<ImageSetting360Model> PrepareImageSetting360ModelAsync(int productId);
        Task<Product360Model> PrepareImage360DetailsModelAsync(int productId);
    }
}
