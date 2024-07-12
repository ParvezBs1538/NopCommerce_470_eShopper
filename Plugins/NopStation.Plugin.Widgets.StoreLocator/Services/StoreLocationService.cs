using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Media;
using Nop.Data;
using NopStation.Plugin.Widgets.StoreLocator.Domain;
using Nop.Services.Stores;

namespace NopStation.Plugin.Widgets.StoreLocator.Services
{
    public class StoreLocationService : IStoreLocationService
    {
        #region Fields

        private readonly IRepository<Address> _addressRepository;
        private readonly IRepository<StoreLocationPicture> _storeLocationPictureRepository;
        private readonly IRepository<Picture> _pictureRepository;
        private readonly IRepository<StoreLocation> _storeLocationRepository;
        private readonly IStoreMappingService _storeMappingService;

        #endregion

        #region Ctor

        public StoreLocationService(IRepository<Address> addressRepository,
            IRepository<StoreLocationPicture> storeLocationPictureRepository,
            IRepository<Picture> pictureRepository,
            IRepository<StoreLocation> storeLocationRepository,
            IStoreMappingService storeMappingService)
        {
            _addressRepository = addressRepository;
            _storeLocationPictureRepository = storeLocationPictureRepository;
            _pictureRepository = pictureRepository;
            _storeLocationRepository = storeLocationRepository;
            _storeMappingService = storeMappingService;
        }

        #endregion

        #region Methods

        #region Store locations

        public async Task InsertStoreLocationAsync(StoreLocation storeLocation)
        {
            await _storeLocationRepository.InsertAsync(storeLocation);
        }

        public async Task UpdateStoreLocationAsync(StoreLocation storeLocation)
        {
            await _storeLocationRepository.UpdateAsync(storeLocation);
        }

        public async Task<IPagedList<StoreLocation>> SearchStoreLocationsAsync(string storeName = "", int stateId = 0,
            int countryId = 0, int storeId = 0, bool? active = null, bool? pickupPoint = null, int pageIndex = 0,
            int pageSize = int.MaxValue)
        {
            var query = from s in _storeLocationRepository.Table
                        where (string.IsNullOrWhiteSpace(storeName) || s.Name.Contains(storeName)) &&
                        (pickupPoint == null || s.IsPickupPoint == pickupPoint.Value) &&
                        (active == null || s.Active == active.Value)
                        select s;

            if (countryId > 0 || stateId > 0)
            {
                query = from s in query
                        join a in _addressRepository.Table on s.AddressId equals a.Id
                        where (countryId == 0 || a.CountryId == countryId) &&
                        (stateId == 0 || a.StateProvinceId == stateId)
                        select s;
            }

            query = await _storeMappingService.ApplyStoreMapping(query, storeId);
            query = query.OrderBy(m => m.DisplayOrder);

            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

        public async Task<StoreLocation> GetStoreLocationByIdAsync(int storeId)
        {
            return await _storeLocationRepository.GetByIdAsync(storeId, cache => default);
        }

        public async Task DeleteStoreLocationAsync(StoreLocation storeLocation)
        {
            await _storeLocationRepository.DeleteAsync(storeLocation);
        }

        #endregion

        #region StoreLocation pictures

        public virtual async Task DeleteStoreLocationPictureAsync(StoreLocationPicture storeLocationPicture)
        {
            await _storeLocationPictureRepository.DeleteAsync(storeLocationPicture);
        }

        public virtual async Task<IList<StoreLocationPicture>> GetStoreLocationPicturesByStoreLocationIdAsync(int storeLocationId)
        {
            var query = from slp in _storeLocationPictureRepository.Table
                        where slp.StoreLocationId == storeLocationId
                        orderby slp.DisplayOrder, slp.Id
                        select slp;

            var storeLocationPictures = await query.ToListAsync();

            return storeLocationPictures;
        }

        public virtual async Task<StoreLocationPicture> GetStoreLocationPictureByIdAsync(int storeLocationPictureId)
        {
            return await _storeLocationPictureRepository.GetByIdAsync(storeLocationPictureId, cache => default);
        }

        public virtual async Task InsertStoreLocationPictureAsync(StoreLocationPicture storeLocationPicture)
        {
            await _storeLocationPictureRepository.InsertAsync(storeLocationPicture);
        }

        public virtual async Task UpdateStoreLocationPictureAsync(StoreLocationPicture storeLocationPicture)
        {
            await _storeLocationPictureRepository.UpdateAsync(storeLocationPicture);
        }

        public async Task<IDictionary<int, int[]>> GetStoreLocationsImagesIdsAsync(int[] storeLocationsIds)
        {
            var storeLocationPictures = await _storeLocationPictureRepository.Table
                .Where(p => storeLocationsIds.Contains(p.StoreLocationId))
                .ToListAsync();

            return storeLocationPictures.GroupBy(p => p.StoreLocationId).ToDictionary(p => p.Key, p => p.Select(p1 => p1.PictureId).ToArray());
        }

        public async Task<IList<StoreLocationPicture>> GetStoreLocationPicturesAsync(int storeLocationsId)
        {
            var storeLocationPictures = await _storeLocationPictureRepository.Table
                .Where(p => p.StoreLocationId == storeLocationsId)
                .OrderBy(x => x.DisplayOrder)
                .ToListAsync();

            return storeLocationPictures;
        }

        public async Task<IList<Picture>> GetPicturesByStoreLocationIdAsync(int storeLocationsId)
        {
            var picture = await (from slp in _storeLocationPictureRepository.Table
                                 join p in _pictureRepository.Table on slp.PictureId equals p.Id
                                 where slp.StoreLocationId == storeLocationsId
                                 orderby slp.DisplayOrder
                                 select p)
                    .ToListAsync();

            return picture;
        }

        public async Task<Picture> GetDefaultPictureByStoreLocationIdAsync(int storeLocationsId)
        {
            var picture = await (from slp in _storeLocationPictureRepository.Table
                                 join p in _pictureRepository.Table on slp.PictureId equals p.Id
                                 where slp.StoreLocationId == storeLocationsId
                                 orderby slp.DisplayOrder
                                 select p)
                    .FirstOrDefaultAsync();

            return picture;
        }

        #endregion

        #endregion
    }
}
