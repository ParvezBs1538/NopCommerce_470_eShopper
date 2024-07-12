using System;
using System.Linq;
using System.Threading.Tasks;
using NopStation.Plugin.Shipping.Redx.Areas.Admin.Models;
using NopStation.Plugin.Shipping.Redx.Domains;
using NopStation.Plugin.Shipping.Redx.Services;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Models.Extensions;
using Nop.Services.Directory;
using Nop.Web.Areas.Admin.Factories;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace NopStation.Plugin.Shipping.Redx.Areas.Admin.Factories
{
    public class RedxAreaModelFactory : IRedxAreaModelFactory
    {
        private readonly IRedxAreaService _redxAreaService;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly ICountryService _countryService;

        public RedxAreaModelFactory(IRedxAreaService redxAreaService,
            IBaseAdminModelFactory baseAdminModelFactory,
            ICountryService countryService)
        {
            _redxAreaService = redxAreaService;
            _baseAdminModelFactory = baseAdminModelFactory;
            _countryService = countryService;
        }

        public async Task<RedxAreaSearchModel> PrepareRedxAreaSearchModelAsync(RedxAreaSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            searchModel.SetGridPageSize();

            var country = await _countryService.GetCountryByTwoLetterIsoCodeAsync("BD");
            await _baseAdminModelFactory.PrepareStatesAndProvincesAsync(searchModel.AvailableStateProvinces, country?.Id);

            return searchModel;
        }

        public async Task<RedxAreaListModel> PrepareRedxAreaListModelAsync(RedxAreaSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var stateId = searchModel.SearchStateProvinceId == 0 ? (int?)null : searchModel.SearchStateProvinceId;

            var redxAreas = await _redxAreaService.GetRedxAreasAsync(
                stateProvinceId: stateId,
                loadUnmapped: false,
                distName: searchModel.SearchDisctrictName,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.PageSize);

            var model = await new RedxAreaListModel().PrepareToGridAsync(searchModel, redxAreas, () =>
            {
                return redxAreas.SelectAwait(async area =>
                {
                    return await PrepareRedxAreaModelAsync(null, area, true);
                });
            });

            return model;
        }

        public async Task<RedxAreaModel> PrepareRedxAreaModelAsync(RedxAreaModel model, RedxArea redxArea,
            bool excludeProperties = false)
        {
            if (redxArea != null)
            {
                if (model == null)
                {
                    model = redxArea.ToModel<RedxAreaModel>();
                }
            }

            if (!excludeProperties)
            {
                var country = await _countryService.GetCountryByTwoLetterIsoCodeAsync("BD");
                await _baseAdminModelFactory.PrepareStatesAndProvincesAsync(model.AvailableStateProvinces,
                    country?.Id, false);

                model.AvailableStateProvinces.Insert(0, new SelectListItem
                {
                    Value = "",
                    Text = "--"
                });
            }

            return model;
        }
    }
}