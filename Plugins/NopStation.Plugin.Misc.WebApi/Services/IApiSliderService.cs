using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Misc.WebApi.Domains;

namespace NopStation.Plugin.Misc.WebApi.Services
{
    public interface IApiSliderService
    {
        Task DeleteApiSliderAsync(ApiSlider slider);

        Task DeleteApiSlidersAsync(IList<ApiSlider> sliders);

        Task InsertApiSliderAsync(ApiSlider slider);

        Task InsertApiSliderAsync(List<ApiSlider> sliders);

        Task UpdateApiSliderAsync(ApiSlider slider);

        Task<ApiSlider> GetApiSliderByIdAsync(int sliderId);

        Task<IPagedList<ApiSlider>> GetAllApiSlidersAsync(List<int> stids = null, int pageIndex = 0, int pageSize = int.MaxValue);

        Task<IList<ApiSlider>> GetActiveApiSlidersAsync(int maximumItems = 0);

        IList<ApiSlider> GetApiSliderByIds(int[] ids);
    }
}