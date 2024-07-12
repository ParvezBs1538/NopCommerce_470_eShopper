using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Widgets.AffiliateStation.Domains;

namespace NopStation.Plugin.Widgets.AffiliateStation.Services
{
    public interface ICatalogCommissionService
    {
        Task DeleteCatalogCommissionAsync(CatalogCommission catalogCommission);

        Task InsertCatalogCommissionAsync(CatalogCommission catalogCommission);

        Task UpdateCatalogCommissionAsync(CatalogCommission catalogCommission);

        Task<CatalogCommission> GetCatalogCommissionByIdAsync(int catalogCommissionId);

        Task<CatalogCommission> GetCatalogCommissionByEntityAsync(BaseEntity baseEntity);

        Task<IPagedList<CatalogCommission>> GetAllCatalogCommissionsAsync(string entityName = null,
            int entityId = 0, int pageIndex = 0, int pageSize = int.MaxValue);
    }
}