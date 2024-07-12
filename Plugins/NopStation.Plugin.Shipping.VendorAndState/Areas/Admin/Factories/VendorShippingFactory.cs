using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using NopStation.Plugin.Shipping.VendorAndState.Areas.Admin.Models;
using NopStation.Plugin.Shipping.VendorAndState.Domain;
using NopStation.Plugin.Shipping.VendorAndState.Services;
using Nop.Services.Localization;
using Nop.Services.Shipping;
using Nop.Services.Vendors;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Models.Extensions;

namespace NopStation.Plugin.Shipping.VendorAndState.Areas.Admin.Factories
{
    public class VendorShippingFactory : IVendorShippingFactory
    {
        private readonly IVendorService _vendorService;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly IShippingService _shippingService;
        private readonly IVendorShippingService _vendorShippingService;
        private readonly ILocalizationService _localizationService;

        public VendorShippingFactory(IVendorService vendorService,
            IBaseAdminModelFactory baseAdminModelFactory,
            IShippingService shippingService,
            IVendorShippingService vendorShippingService,
            ILocalizationService localizationService)
        {
            _vendorService = vendorService;
            _baseAdminModelFactory = baseAdminModelFactory;
            _shippingService = shippingService;
            _vendorShippingService = vendorShippingService;
            _localizationService = localizationService;
        }

        protected async Task PrepareShippingMethodsAsync(IList<SelectListItem> items, bool addDefaultItem = false)
        {
            var shippingMethods = await _shippingService.GetAllShippingMethodsAsync();
            foreach (var shippingMethod in shippingMethods)
            {
                items.Add(new SelectListItem
                {
                    Text = shippingMethod.Name,
                    Value = shippingMethod.Id.ToString()
                });
            }

            if (addDefaultItem)
            {
                items.Insert(0, new SelectListItem
                {
                    Text = await _localizationService.GetResourceAsync("Admin.NopStation.ShippingByVendor.ShippingMethods.All"),
                    Value = "0"
                });
            }
        }

        public async Task<VendorShippingListModel> PrepareVendorShippingListModelAsync(VendorShippingSearchModel searchModel)
        {
            var vendorShippings = await _vendorShippingService.GetAllVendorShippingsAsync(shippingMethodId: searchModel.SearchShippingMethodId,
                vendorId: searchModel.SearchVendorId, pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            var model = await new VendorShippingListModel().PrepareToGridAsync(searchModel, vendorShippings, () =>
            {
                return vendorShippings.SelectAwait(async vendorShipping =>
                {
                    return await PrepareVendorShippingModelAsync(null, vendorShipping, true);
                });
            });

            return model;
        }

        public async Task<VendorShippingModel> PrepareVendorShippingModelAsync(VendorShippingModel model, VendorShipping vendorShipping, bool excludeProperties = false)
        {
            if (vendorShipping != null)
            {
                //fill in model values from the entity
                if (model == null)
                {
                    model = vendorShipping.ToModel<VendorShippingModel>();
                    var shippingMethod = await _shippingService.GetShippingMethodByIdAsync(vendorShipping.ShippingMethodId);
                    model.ShippingMethod = shippingMethod?.Name;
                    var vendor = await _vendorService.GetVendorByIdAsync(model.VendorId);
                    model.VendorName = vendor?.Name;
                }
            }

            if (!excludeProperties)
            {
                await PrepareShippingMethodsAsync(model.AvailableShippingMethods);
                await _baseAdminModelFactory.PrepareCountriesAsync(model.VendorStateProvinceShippingSearchModel.AvailableCountries, false);
                model.VendorStateProvinceShippingSearchModel.SetGridPageSize();
            }

            return model;
        }

        public async Task<VendorShippingSearchModel> PrepareVendorShippingSearchModelAsync(VendorShippingSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            await PrepareShippingMethodsAsync(searchModel.AvailableShippingMethods, true);

            var vendors = await _vendorService.GetAllVendorsAsync();
            searchModel.AvailableVendors = vendors.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Id.ToString()
            }).ToList();
            searchModel.AvailableVendors.Insert(0, new SelectListItem
            {
                Text = await _localizationService.GetResourceAsync("Admin.NopStation.ShippingByVendor.Vendors.All"),
                Value = "0"
            });

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }
    }
}
