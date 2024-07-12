using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Media;
using NopStation.Plugin.Widgets.StoreLocator.Domain;

namespace NopStation.Plugin.Widgets.StoreLocator.Services
{
    public partial interface IStoreLocationService
    {
        Task InsertStoreLocationAsync(StoreLocation storeLocation);

        Task UpdateStoreLocationAsync(StoreLocation storeLocation);

        Task<IPagedList<StoreLocation>> SearchStoreLocationsAsync(string storeName = "", int stateId = 0,
            int countryId = 0, int storeId = 0, bool? active = null, bool? pickupPoint = null, int pageIndex = 0, 
            int pageSize = int.MaxValue);

        Task<StoreLocation> GetStoreLocationByIdAsync(int storeStoreId);

        Task DeleteStoreLocationAsync(StoreLocation storeLocation);

        Task DeleteStoreLocationPictureAsync(StoreLocationPicture storeLocationPicture);

        Task<IList<StoreLocationPicture>> GetStoreLocationPicturesByStoreLocationIdAsync(int storeLocationId);

        Task<StoreLocationPicture> GetStoreLocationPictureByIdAsync(int storeLocationPictureId);

        Task InsertStoreLocationPictureAsync(StoreLocationPicture storeLocationPicture);

        Task UpdateStoreLocationPictureAsync(StoreLocationPicture storeLocationPicture);

        Task<IDictionary<int, int[]>> GetStoreLocationsImagesIdsAsync(int[] storeLocationsIds);

        Task<IList<StoreLocationPicture>> GetStoreLocationPicturesAsync(int storeLocationsId);

        Task<Picture> GetDefaultPictureByStoreLocationIdAsync(int storeLocationsId);

        Task<IList<Picture>> GetPicturesByStoreLocationIdAsync(int storeLocationsId);
    }
}
