using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using NopStation.Plugin.CRM.Zoho.Domain;

namespace NopStation.Plugin.CRM.Zoho.Services
{
    public class UpdatableItemService : IUpdatableItemService
    {
        private readonly IRepository<UpdatableItem> _updatableItemRepository;

        public UpdatableItemService(IRepository<UpdatableItem> updatableItemRepository)
        {
            _updatableItemRepository = updatableItemRepository;
        }

        public async Task<UpdatableItem> GetUpdatableItemByEntityTypeAndIdAsync(EntityType entityType, int entityId)
        {
            return await _updatableItemRepository.Table
                .FirstOrDefaultAsync(x => x.EntityTypeId == (int)entityType && x.EntityId == entityId);
        }

        public async Task InsertUpdatableItemAsync(UpdatableItem updatableItem)
        {
            await _updatableItemRepository.InsertAsync(updatableItem);
        }

        public async Task DeleteUpdatableItemAsync(EntityType entityType, List<int> ids = null)
        {
            var query = _updatableItemRepository.Table.Where(x => x.EntityTypeId == (int)entityType);

            if (ids != null && ids.Any())
                query = query.Where(x => ids.Contains(x.EntityId));

            var items = query.ToList();

            if (items.Any())
                await _updatableItemRepository.DeleteAsync(items);
        }

        public async Task<IPagedList<UpdatableItem>> GetAllUpdatableItemAsync(
            EntityType? entityType = null,
            int pageIndex = 0,
            int pageSize = int.MaxValue
            )
        {
            if (pageSize == int.MaxValue)
                --pageSize;

            var query = _updatableItemRepository.Table;

            if (entityType.HasValue)
                query.Where(x => x.EntityTypeId == (int)entityType.Value);

            return await query.ToPagedListAsync(pageIndex, pageSize);
        }
    }
}
