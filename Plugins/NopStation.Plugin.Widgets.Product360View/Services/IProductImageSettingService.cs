using System.Threading.Tasks;
using NopStation.Plugin.Widgets.Product360View.Domain;
using NopStation.Plugin.Widgets.Product360View.Models;

namespace NopStation.Plugin.Widgets.Product360View.Services
{
    public interface IProductImageSettingService
    {
        Task<ProductImageSetting360> GetImageSettingByIdAsync(int id);
        Task<ProductImageSetting360> GetImageSettingByProductIdAsync(int productId);
        Task AddOrUpdateImageSettingAsync(ImageSetting360Model setting);
        Task DeleteImageSettingAsync(ProductImageSetting360 setting);
    }
}
