using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.CRM.Zoho.Domain;

namespace NopStation.Plugin.CRM.Zoho.Services
{
    public interface IUpdatableItemService
    {
        Task<UpdatableItem> GetUpdatableItemByEntityTypeAndIdAsync(EntityType entityType, int entityId);

        Task InsertUpdatableItemAsync(UpdatableItem updatableItem);

        Task DeleteUpdatableItemAsync(EntityType entityType, List<int> ids = null);

        Task<IPagedList<UpdatableItem>> GetAllUpdatableItemAsync(
            EntityType? entityType = null,
            int pageIndex = 0,
            int pageSize = int.MaxValue
            );
    }
}