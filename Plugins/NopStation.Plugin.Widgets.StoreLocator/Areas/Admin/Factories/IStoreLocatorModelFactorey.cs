using System.Threading.Tasks;
using NopStation.Plugin.Widgets.StoreLocator.Areas.Admin.Models;
using NopStation.Plugin.Widgets.StoreLocator.Domain;

namespace NopStation.Plugin.Widgets.StoreLocator.Areas.Admin.Factories
{
    public interface IStoreLocatorModelFactorey
    {
        Task<StoreLocationSearchModel> PrepareStoreLocationSearchModelAsync(StoreLocationSearchModel searchModel);

        Task<StoreLocationListModel> PrepareStoreLocationListModelAsync(StoreLocationSearchModel searchModel);

        Task<StoreLocationModel> PrepareStoreLocationModelAsync(StoreLocationModel model, StoreLocation storeLocation, bool excludeProperties = false);

        Task<StoreLocationPictureListModel> PrepareStoreLocationPictureListModelAsync(StoreLocationPictureSearchModel searchModel, StoreLocation storeLocation);
    }
}