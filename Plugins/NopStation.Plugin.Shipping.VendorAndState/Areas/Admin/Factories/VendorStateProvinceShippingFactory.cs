using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using NopStation.Plugin.Shipping.VendorAndState.Areas.Admin.Models;
using NopStation.Plugin.Shipping.VendorAndState.Services;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Shipping;
using Nop.Services.Vendors;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Models.Extensions;

namespace NopStation.Plugin.Shipping.VendorAndState.Areas.Admin.Factories
{
    public class VendorStateProvinceShippingFactory : IVendorStateProvinceShippingFactory
    {
        private readonly IVendorService _vendorService;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly IShippingService _shippingService;
        private readonly IVendorShippingService _vendorShippingService;
        private readonly ILocalizationService _localizationService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IVendorStateProvinceShippingService _vendorStateProvinceShippingService;

        public VendorStateProvinceShippingFactory(IVendorService vendorService,
            IBaseAdminModelFactory baseAdminModelFactory,
            IShippingService shippingService,
            IVendorShippingService vendorShippingService,
            ILocalizationService localizationService,
            IStateProvinceService stateProvinceService,
            IVendorStateProvinceShippingService vendorStateProvinceShippingService)
        {
            _vendorService = vendorService;
            _baseAdminModelFactory = baseAdminModelFactory;
            _shippingService = shippingService;
            _vendorShippingService = vendorShippingService;
            _localizationService = localizationService;
            _stateProvinceService = stateProvinceService;
            _vendorStateProvinceShippingService = vendorStateProvinceShippingService;
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

        public async Task<VendorStateProvinceShippingListModel> PrepareVendorStateProvinceShippingListModelAsync(VendorStateProvinceShippingSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var stateProvinces = (await _stateProvinceService.GetStateProvincesByCountryIdAsync(searchModel.SearchCountryId))
                .ToPagedList(searchModel);

            var model = await new VendorStateProvinceShippingListModel().PrepareToGridAsync(searchModel, stateProvinces, () =>
            {
                return stateProvinces.SelectAwait(async stateProvince =>
                {
                    var m = new VendorStateProvinceShippingModel();

                    var vss = await _vendorStateProvinceShippingService.GetVendorStateProvinceShippingByVendorIdAndShippingMethodIdAsync(
                        searchModel.VendorId, searchModel.ShippingMethodId, stateProvince.Id);

                    if (vss != null)
                    {
                        m = vss.ToModel<VendorStateProvinceShippingModel>();
                        m.IsSet = true;
                    }

                    m.StateProvince = stateProvince.Name;
                    m.ComplexId = $"{stateProvince.Id}___{searchModel.VendorId}___{searchModel.ShippingMethodId}";

                    return m;
                });
            });

            return model;
        }
    }
}
