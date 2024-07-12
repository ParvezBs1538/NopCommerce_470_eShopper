using System.Threading.Tasks;
using NopStation.Plugin.Shipping.Redx.Areas.Admin.Models;
using NopStation.Plugin.Shipping.Redx.Domains;

namespace NopStation.Plugin.Shipping.Redx.Areas.Admin.Factories
{
    public interface IRedxAreaModelFactory
    {
        Task<RedxAreaSearchModel> PrepareRedxAreaSearchModelAsync(RedxAreaSearchModel searchModel);

        Task<RedxAreaListModel> PrepareRedxAreaListModelAsync(RedxAreaSearchModel searchModel);

        Task<RedxAreaModel> PrepareRedxAreaModelAsync(RedxAreaModel model, RedxArea redxArea, bool excludeProperties = false);
    }
}