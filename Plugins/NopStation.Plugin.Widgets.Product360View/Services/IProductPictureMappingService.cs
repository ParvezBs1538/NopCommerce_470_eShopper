using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Media;
using NopStation.Plugin.Widgets.Product360View.Domain;

namespace NopStation.Plugin.Widgets.Product360View.Services
{
    public interface IProductPictureMappingService
    {
        Task<ProductPictureMapping360> GetPictureMappingByIdAsync(int id);
        Task<List<ProductPictureMapping360>> GetPictureMappingsByProductIdAsync(int productId, bool isPanorama = false);
        Task InsertPictureMappingAsync(ProductPictureMapping360 pictureMapping);
        Task UpdatePictureMappingAsync(ProductPictureMapping360 pictureMapping);
        Task DeletePictureMappingAsync(ProductPictureMapping360 pictureMapping);
        Task<IList<Picture>> Get360PicturesByProductIdAsync(int productId, bool isPanorama = false);
        Task<int> GetLastPictureOrderByProductIdAsync(int productId, bool isPanorama = false);
    }
}
