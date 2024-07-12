using System.Collections.Generic;
using System.Threading.Tasks;
using NopStation.Plugin.Misc.AjaxFilter.Models;

namespace NopStation.Plugin.Misc.AjaxFilter.Factories
{
    public interface ICategoryImageFactory
    {
        Task<IList<CategoryPictureModel>> GetProductImages(IList<int> productIds);
    }
}
