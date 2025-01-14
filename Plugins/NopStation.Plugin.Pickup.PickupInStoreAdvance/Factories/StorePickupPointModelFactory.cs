﻿using System;
using System.Linq;
using System.Threading.Tasks;
using NopStation.Plugin.Pickup.PickupInStoreAdvance.Areas.Admin.Models;
using NopStation.Plugin.Pickup.PickupInStoreAdvance.Services;
using Nop.Services.Localization;
using Nop.Services.Stores;
using Nop.Web.Framework.Models.Extensions;

namespace NopStation.Plugin.Pickup.PickupInStoreAdvance.Factories
{
    /// <summary>
    /// Represents store pickup point models factory implementation
    /// </summary>
    public class StorePickupPointModelFactory : IStorePickupPointModelFactory
    {
        #region Fields

        private readonly IStorePickupPointService _storePickupPointService;
        private readonly ILocalizationService _localizationService;
        private readonly IStoreService _storeService;

        #endregion

        #region Ctor

        public StorePickupPointModelFactory(IStorePickupPointService storePickupPointService,
            ILocalizationService localizationService, IStoreService storeService)
        {
            _storePickupPointService = storePickupPointService;
            _localizationService = localizationService;
            _storeService = storeService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Prepare store pickup point list model
        /// </summary>
        /// <param name="searchModel">Store pickup point search model</param>
        /// <returns>Store pickup point list model</returns>
        public async Task<StorePickupPointListModel> PrepareStorePickupPointListModelAsync(StorePickupPointSearchModel searchModel)
        {
            var pickupPoints = await _storePickupPointService.GetAllStorePickupPointsAsync(pageIndex: searchModel.Page - 1,
                pageSize: searchModel.PageSize);
            var model = new StorePickupPointListModel().PrepareToGrid(searchModel, pickupPoints, () =>
            {
                return pickupPoints.Select(point =>
                {
                    var store = _storeService.GetStoreByIdAsync(point.StoreId).Result;
                    return new StorePickupPointModel
                    {
                        Id = point.Id,
                        Name = point.Name,
                        Active = point.Active,
                        OpeningHours = point.OpeningHours,
                        PickupFee = point.PickupFee,
                        DisplayOrder = point.DisplayOrder,
                        StoreName = store?.Name ?? (point.StoreId == 0
                                        ? _localizationService.GetResourceAsync(
                                            "Admin.Configuration.Settings.StoreScope.AllStores").Result
                                        : string.Empty)
                    };
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare store pickup point search model
        /// </summary>
        /// <param name="searchModel">Store pickup point search model</param>
        /// <returns>Store pickup point search model</returns>
        public Task<StorePickupPointSearchModel> PrepareStorePickupPointSearchModelAsync(StorePickupPointSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare page parameters
            searchModel.SetGridPageSize();

            return Task.FromResult(searchModel);
        }

        #endregion
    }
}
