using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;

namespace NopStation.Plugin.Widgets.VendorShop.Services
{
    public interface IVendorCategoryService
    {
        Task<IList<Category>> GetAllCategoriesByVendorId(int vendorId);
    }
}