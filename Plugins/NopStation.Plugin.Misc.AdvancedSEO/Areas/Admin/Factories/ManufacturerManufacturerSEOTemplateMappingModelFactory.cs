using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core.Domain.Logging;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Models.Extensions;
using Nop.Web.Framework.Factories;
using NopStation.Plugin.Misc.AdvancedSEO.Areas.Admin.Models.ManufacturerManufacturerSEOTemplateMapping;
using NopStation.Plugin.Misc.AdvancedSEO.Domains;
using NopStation.Plugin.Misc.AdvancedSEO.Services;
using Nop.Web.Framework.Extensions;
using System.Globalization;
using NopStation.Plugin.Misc.AdvancedSEO.Areas.Admin.Models;
using Nop.Services.Localization;
using Nop.Services;
using Nop.Services.Catalog;
using NopStation.Plugin.Misc.AdvancedSEO.Areas.Admin.Models.ManufacturerToMap;
using Nop.Core.Domain.Catalog;

namespace NopStation.Plugin.Misc.AdvancedSEO.Areas.Admin.Factories
{
    public class ManufacturerManufacturerSEOTemplateMappingModelFactory : IManufacturerManufacturerSEOTemplateMappingModelFactory
    {
        #region Fields

        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly IManufacturerService _manufacturerService;
        private readonly IManufacturerManufacturerSEOTemplateMappingService _manufacturerManufacturerSEOTemplateMappingService;
        private readonly ICustomerService _customerService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizedModelFactory _localizedModelFactory;
        private readonly ILocalizationService _localizationService;
        private readonly IStoreMappingSupportedModelFactory _storeMappingSupportedModelFactory;

        #endregion

        #region Ctor

        public ManufacturerManufacturerSEOTemplateMappingModelFactory(
            IBaseAdminModelFactory baseAdminModelFactory,
            IManufacturerService manufacturerService,
            IManufacturerManufacturerSEOTemplateMappingService manufacturerManufacturerSEOTemplateMappingService,
            ICustomerService customerService,
            IDateTimeHelper dateTimeHelper,
            ILocalizedModelFactory localizedModelFactory,
            ILocalizationService localizationService,
            IStoreMappingSupportedModelFactory storeMappingSupportedModelFactory
            )
        {
            _baseAdminModelFactory = baseAdminModelFactory;
            _manufacturerService = manufacturerService;
            _manufacturerManufacturerSEOTemplateMappingService = manufacturerManufacturerSEOTemplateMappingService;
            _customerService = customerService;
            _dateTimeHelper = dateTimeHelper;
            _localizedModelFactory = localizedModelFactory;
            _localizationService = localizationService;
            _storeMappingSupportedModelFactory = storeMappingSupportedModelFactory;
        }

        #endregion

        #region Utilites



        #endregion

        #region Methods 

        #region ManufacturerManufacturerSEOTemplateMapping

        public async Task<ManufacturerManufacturerSEOTemplateMappingModel> PrepareManufacturerManufacturerSEOTemplateMappingModelAsync(ManufacturerManufacturerSEOTemplateMappingModel model, ManufacturerManufacturerSEOTemplateMapping manufacturerManufacturerSEOTemplateMapping,
            bool excludeProperties = false)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (manufacturerManufacturerSEOTemplateMapping != null)
            {
                var manufacturer = await _manufacturerService.GetManufacturerByIdAsync(manufacturerManufacturerSEOTemplateMapping.ManufacturerId);
                if(manufacturer == null)
                    throw new ArgumentNullException(nameof(manufacturer));

                model = manufacturerManufacturerSEOTemplateMapping.ToModel<ManufacturerManufacturerSEOTemplateMappingModel>();
                model.ManufacturerName = manufacturer.Name;
            }
            return model;
        }

        /// <summary>
        /// Prepare manufacturerManufacturerSEOTemplateMapping search model
        /// </summary>
        /// <param name="searchModel">ManufacturerManufacturerSEOTemplateMapping search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the manufacturerManufacturerSEOTemplateMapping search model
        /// </returns>
        public virtual async Task<ManufacturerManufacturerSEOTemplateMappingSearchModel> PrepareManufacturerManufacturerSEOTemplateMappingSearchModelAsync(
            ManufacturerManufacturerSEOTemplateMappingSearchModel searchModel, ManufacturerSEOTemplate manufacturerSEOTemplate)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (manufacturerSEOTemplate == null)
                throw new ArgumentNullException(nameof(manufacturerSEOTemplate));


            searchModel.ManufacturerSEOTemplateId = manufacturerSEOTemplate.Id;

            //prepare page parameters
            searchModel.SetGridPageSize();

            await Task.CompletedTask;
            return searchModel;
        }

        public virtual async Task<ManufacturerManufacturerSEOTemplateMappingListModel> PrepareManufacturerManufacturerSEOTemplateMappingListModelAsync(ManufacturerManufacturerSEOTemplateMappingSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get manufacturerManufacturerSEOTemplateMappings
            var manufacturerManufacturerSEOTemplateMappings = await _manufacturerManufacturerSEOTemplateMappingService.GetAllManufacturerManufacturerSEOTemplateMappingAsync(
                manufacturerSEOTemplateId : searchModel.ManufacturerSEOTemplateId,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.PageSize);

            //prepare grid model
            var model = await new ManufacturerManufacturerSEOTemplateMappingListModel().PrepareToGridAsync(searchModel, manufacturerManufacturerSEOTemplateMappings, () =>
            {
                return manufacturerManufacturerSEOTemplateMappings.SelectAwait(async manufacturerManufacturerSEOTemplateMapping =>
                {
                    return await PrepareManufacturerManufacturerSEOTemplateMappingModelAsync(new ManufacturerManufacturerSEOTemplateMappingModel(), manufacturerManufacturerSEOTemplateMapping, true);
                });
            });

            return model;
        }

        #endregion

        #region ManufacturerToMap



        /// <summary>
        /// Prepare manufacturerManufacturerSEOTemplateMapping search model
        /// </summary>
        /// <param name="searchModel">ManufacturerManufacturerSEOTemplateMapping search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the manufacturerManufacturerSEOTemplateMapping search model
        /// </returns>
        public virtual async Task<ManufacturerToMapSearchModel> PrepareManufacturerToMapSearchModelAsync(
            ManufacturerToMapSearchModel searchModel, ManufacturerSEOTemplate manufacturerSEOTemplate)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (manufacturerSEOTemplate == null)
                throw new ArgumentNullException(nameof(manufacturerSEOTemplate));

            searchModel.ManufacturerSEOTemplateId = manufacturerSEOTemplate.Id;

            //prepare page parameters
            searchModel.SetGridPageSize();

            await Task.CompletedTask;
            return searchModel;
        }

        public Task<ManufacturerToMapModel> PrepareManufacturerToMapModelAsync(Manufacturer manufacturer,
            bool excludeProperties = false)
        {
            if (manufacturer == null)
                throw new ArgumentNullException(nameof(manufacturer));

            return Task.FromResult(new ManufacturerToMapModel(manufacturer));
        }

        public virtual async Task<ManufacturerToMapListModel> PrepareManufacturerToMapListModelAsync(ManufacturerToMapSearchModel searchModel)
        {
            if(searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get Not Mapped categoties
            var categories = await _manufacturerManufacturerSEOTemplateMappingService.GetAllNotMappedCategoriesByManufacturerSEOTemplateId(
                manufacturerName: searchModel.ManufacturerName,
                manufacturerSEOTemplateId : searchModel.ManufacturerSEOTemplateId,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.PageSize);

            //prepare grid model
            var model = await new ManufacturerToMapListModel().PrepareToGridAsync(searchModel, categories, () =>
            {
                return categories.SelectAwait(async manufacturer =>
                {
                    return await PrepareManufacturerToMapModelAsync(manufacturer, true);
                });
            });

            return model;
        }

        #endregion

        #endregion
    }
}
