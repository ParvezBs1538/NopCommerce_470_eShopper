using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using NopStation.Plugin.Widgets.DeliveryScheduler.Domains;

namespace NopStation.Plugin.Widgets.DeliveryScheduler.Services
{
    public class SpecialDeliveryOffsetService : ISpecialDeliveryOffsetService
    {
        #region Fields

        private readonly IRepository<SpecialDeliveryOffset> _specialDeliveryOffsetRepository;

        #endregion

        #region Ctor

        public SpecialDeliveryOffsetService(IRepository<SpecialDeliveryOffset> specialDeliveryOffsetRepository)
        {
            _specialDeliveryOffsetRepository = specialDeliveryOffsetRepository;
        }

        #endregion

        #region Methods

        public async Task DeleteSpecialDeliveryOffsetAsync(SpecialDeliveryOffset specialDeliveryOffset)
        {
            await _specialDeliveryOffsetRepository.DeleteAsync(specialDeliveryOffset);
        }

        public async Task InsertSpecialDeliveryOffsetAsync(SpecialDeliveryOffset specialDeliveryOffset)
        {
            await _specialDeliveryOffsetRepository.InsertAsync(specialDeliveryOffset);
        }

        public async Task UpdateSpecialDeliveryOffsetAsync(SpecialDeliveryOffset specialDeliveryOffset)
        {
            await _specialDeliveryOffsetRepository.UpdateAsync(specialDeliveryOffset);
        }

        public async Task<SpecialDeliveryOffset> GetSpecialDeliveryOffsetByIdCategoryAsync(int categoryId)
        {
            if (categoryId == 0)
                return null;

            return await _specialDeliveryOffsetRepository.Table.FirstOrDefaultAsync(x => x.CategoryId == categoryId);
        }

        public async Task<IPagedList<SpecialDeliveryOffset>> SearchSpecialDeliveryOffsetsAsync(int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _specialDeliveryOffsetRepository.Table;

            query = query.OrderByDescending(e => e.Id);

            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

        public async Task<int> GetMaximumCategoryOffset(List<int> categoryIds)
        {
            if (categoryIds == null || categoryIds.Count == 0)
                return 0;

            var query = from sdo in _specialDeliveryOffsetRepository.Table
                        where categoryIds.Contains(sdo.CategoryId)
                        select sdo;
            var items = await query.ToListAsync();

            if (items.Any())
                return items.Max(x => x.DaysOffset);

            return 0;
        }

        #endregion
    }
}
