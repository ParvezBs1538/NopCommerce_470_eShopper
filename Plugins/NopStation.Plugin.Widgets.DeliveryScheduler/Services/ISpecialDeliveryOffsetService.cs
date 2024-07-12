using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Widgets.DeliveryScheduler.Domains;

namespace NopStation.Plugin.Widgets.DeliveryScheduler.Services
{
    public interface ISpecialDeliveryOffsetService
    {
        Task DeleteSpecialDeliveryOffsetAsync(SpecialDeliveryOffset specialDeliveryOffset);

        Task InsertSpecialDeliveryOffsetAsync(SpecialDeliveryOffset specialDeliveryOffset);

        Task UpdateSpecialDeliveryOffsetAsync(SpecialDeliveryOffset specialDeliveryOffset);

        Task<SpecialDeliveryOffset> GetSpecialDeliveryOffsetByIdCategoryAsync(int categoryId);

        Task<IPagedList<SpecialDeliveryOffset>> SearchSpecialDeliveryOffsetsAsync(int pageIndex = 0, int pageSize = int.MaxValue);
        
        Task<int> GetMaximumCategoryOffset(List<int> categoryIds);
    }
}