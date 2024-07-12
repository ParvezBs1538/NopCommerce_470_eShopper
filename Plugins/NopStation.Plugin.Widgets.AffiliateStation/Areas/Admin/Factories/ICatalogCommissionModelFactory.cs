using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Widgets.AffiliateStation.Areas.Admin.Models;

namespace NopStation.Plugin.Widgets.AffiliateStation.Areas.Admin.Factories
{
    public interface ICatalogCommissionModelFactory
    {
        Task<CatalogCommissionSearchModel> PrepareCatalogCommissionSearchModelAsync(SearchType searchType);
        Task<CatalogCommissionListModel> PrepareCatalogCommissionListModelAsync(CatalogCommissionSearchModel searchModel);
        Task<CatalogCommissionModel> PrepareCatalogCommissionModelAsync(CatalogCommissionModel model, BaseEntity baseEntity);
    }
}