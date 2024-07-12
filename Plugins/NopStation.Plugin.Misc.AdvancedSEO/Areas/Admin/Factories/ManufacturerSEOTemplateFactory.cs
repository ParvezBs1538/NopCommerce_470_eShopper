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
using NopStation.Plugin.Misc.AdvancedSEO.Areas.Admin.Models.ManufacturerSEOTemplate;
using NopStation.Plugin.Misc.AdvancedSEO.Domains;
using NopStation.Plugin.Misc.AdvancedSEO.Services;
using Nop.Web.Framework.Extensions;
using System.Globalization;
using NopStation.Plugin.Misc.AdvancedSEO.Areas.Admin.Models;
using Nop.Services.Localization;
using Nop.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using NopStation.Plugin.Misc.AdvancedSEO.Areas.Admin.Models.ManufacturerManufacturerSEOTemplateMapping;
using NopStation.Plugin.Misc.AdvancedSEO.Helpers;

namespace NopStation.Plugin.Misc.AdvancedSEO.Areas.Admin.Factories
{
    public class ManufacturerSEOTemplateModelFactory : IManufacturerSEOTemplateModelFactory
    {
        #region Fields

        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly IManufacturerSEOTemplateService _manufacturerSEOTemplateService;
        private readonly ICustomerService _customerService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizedModelFactory _localizedModelFactory;
        private readonly ILocalizationService _localizationService;
        private readonly IManufacturerManufacturerSEOTemplateMappingModelFactory _manufacturerManufacturerSEOTemplateMappingModelFactory;
        private readonly ISEOTokenProvider _sEOTokenProvider;
        private readonly IStoreMappingSupportedModelFactory _storeMappingSupportedModelFactory;

        #endregion

        #region Ctor

        public ManufacturerSEOTemplateModelFactory(
            IBaseAdminModelFactory baseAdminModelFactory,
            IManufacturerSEOTemplateService manufacturerSEOTemplateService,
            ICustomerService customerService,
            IDateTimeHelper dateTimeHelper,
            ILocalizedModelFactory localizedModelFactory,
            ILocalizationService localizationService,
            IManufacturerManufacturerSEOTemplateMappingModelFactory manufacturerManufacturerSEOTemplateMappingModelFactory,
            ISEOTokenProvider sEOTokenProvider,
            IStoreMappingSupportedModelFactory storeMappingSupportedModelFactory
            )
        {
            _baseAdminModelFactory = baseAdminModelFactory;
            _manufacturerSEOTemplateService = manufacturerSEOTemplateService;
            _customerService = customerService;
            _dateTimeHelper = dateTimeHelper;
            _localizedModelFactory = localizedModelFactory;
            _localizationService = localizationService;
            _manufacturerManufacturerSEOTemplateMappingModelFactory = manufacturerManufacturerSEOTemplateMappingModelFactory;
            _sEOTokenProvider = sEOTokenProvider;
            _storeMappingSupportedModelFactory = storeMappingSupportedModelFactory;
        }

        #endregion

        #region Utilites



        #endregion

        #region Methods

        public async Task<ManufacturerSEOTemplateModel> PrepareManufacturerSEOTemplateModelAsync(ManufacturerSEOTemplateModel model, ManufacturerSEOTemplate manufacturerSEOTemplate,
            bool excludeProperties = false)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            Func<SEOTemplateLocalizedModel, int, Task> localizedModelConfiguration = null;

            if (manufacturerSEOTemplate != null)
            {
                model = manufacturerSEOTemplate.ToModel<ManufacturerSEOTemplateModel>();
                model.CreatedOn = (await _dateTimeHelper.ConvertToUserTimeAsync(manufacturerSEOTemplate.CreatedOnUtc, DateTimeKind.Utc)).ToString(CultureInfo.InvariantCulture);
                model.UpdatedOn = (await _dateTimeHelper.ConvertToUserTimeAsync(manufacturerSEOTemplate.UpdatedOnUtc, DateTimeKind.Utc)).ToString(CultureInfo.InvariantCulture);

                var lastUpdatedByCustomer = await _customerService.GetCustomerByIdAsync(manufacturerSEOTemplate.LastUpdatedByCustomerId);
                model.LastUpdatedByCustomer = lastUpdatedByCustomer?.Email ?? string.Empty;

                var createdByCustomer = await _customerService.GetCustomerByIdAsync(manufacturerSEOTemplate.CreatedByCustomerId);
                model.CreatedByCustomer = createdByCustomer?.Email ?? string.Empty;

                //define localized model configuration action
                localizedModelConfiguration = async (locale, languageId) =>
                {
                    locale.SEOTitleTemplate = await _localizationService.GetLocalizedAsync(manufacturerSEOTemplate, entity => entity.SEOTitleTemplate, languageId, false, false);
                    locale.SEODescriptionTemplate = await _localizationService.GetLocalizedAsync(manufacturerSEOTemplate, entity => entity.SEODescriptionTemplate, languageId, false, false);
                    locale.SEOKeywordsTemplate = await _localizationService.GetLocalizedAsync(manufacturerSEOTemplate, entity => entity.SEOKeywordsTemplate, languageId, false, false);
                };

                if (!excludeProperties)
                {
                    model.ManufacturerManufacturerSEOTemplateMappingSearchModel = await _manufacturerManufacturerSEOTemplateMappingModelFactory.PrepareManufacturerManufacturerSEOTemplateMappingSearchModelAsync(new ManufacturerManufacturerSEOTemplateMappingSearchModel(), manufacturerSEOTemplate);


                }
            }

            if (!excludeProperties)
            {
                //prepare model stores
                await _storeMappingSupportedModelFactory.PrepareModelStoresAsync(model, manufacturerSEOTemplate, excludeProperties);
                model.Locales = await _localizedModelFactory.PrepareLocalizedModelsAsync(localizedModelConfiguration);

                model.AvailableManufacturerMetaTitleTokens.AddRange(await _sEOTokenProvider.GetListOfAllowedTokensAsync(SEOTokenGroupNames.ManufacturerMateTitleTokens));
                model.AvailableManufacturerMetaDescriptionTokens.AddRange(await _sEOTokenProvider.GetListOfAllowedTokensAsync(SEOTokenGroupNames.ManufacturerDescriptionTokens));
                model.AvailableManufacturerMetaKeywordsTokens.AddRange(await _sEOTokenProvider.GetListOfAllowedTokensAsync(SEOTokenGroupNames.ManufacturerKeywordsTokens));
            }

            return model;
        }

        /// <summary>
        /// Prepare manufacturerSEOTemplate search model
        /// </summary>
        /// <param name="searchModel">ManufacturerSEOTemplate search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the manufacturerSEOTemplate search model
        /// </returns>
        public virtual async Task<ManufacturerSEOTemplateSearchModel> PrepareManufacturerSEOTemplateSearchModelAsync(ManufacturerSEOTemplateSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare available stores
            await _baseAdminModelFactory.PrepareStoresAsync(searchModel.AvailableStores);

            searchModel.AvailableStatus = (await SEOTemplateStatusType.Active.ToSelectListAsync()).ToList();
            searchModel.AvailableStatus.Insert(0, new SelectListItem { Value = "0", Text = await _localizationService.GetResourceAsync("Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerSEOTemplate.List.SearchStatus.All") });

            searchModel.AvailableTemplateType = (await SEOTemplateType.GlobalTemplate.ToSelectListAsync()).ToList();
            searchModel.AvailableTemplateType.Insert(0, new SelectListItem { Value = "0", Text = await _localizationService.GetResourceAsync("Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerSEOTemplate.List.SearchTemplateType.All") });

            searchModel.HideStoresList = searchModel.AvailableStores.SelectionIsNotPossible();


            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        public virtual async Task<ManufacturerSEOTemplateListModel> PrepareManufacturerSEOTemplateListModelAsync(ManufacturerSEOTemplateSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            bool? isGlobalTemplate = null;
            if (searchModel.SearchTemplateType > 0)
            {
                if (searchModel.SearchTemplateType == (int)SEOTemplateType.GlobalTemplate)
                    isGlobalTemplate = true;

                else if (searchModel.SearchTemplateType == (int)SEOTemplateType.EntityMappingSpecific)
                    isGlobalTemplate = false;
            }
            bool? isActive = null;
            if (searchModel.SearchStatus > 0)
            {
                if (searchModel.SearchStatus == (int)SEOTemplateStatusType.Active)
                    isActive = true;

                else if (searchModel.SearchStatus == (int)SEOTemplateStatusType.Inactive)
                    isActive = false;
            }

            //get manufacturerSEOTemplates
            var manufacturerSEOTemplates = await _manufacturerSEOTemplateService.GetAllManufacturerSEOTemplateAsync(
                name: searchModel.SearchTemplateName,
                storeId: searchModel.SearchStoreId,
                isGlobalTemplate: isGlobalTemplate,
                isActive: isActive,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.PageSize);

            //prepare grid model
            var model = await new ManufacturerSEOTemplateListModel().PrepareToGridAsync(searchModel, manufacturerSEOTemplates, () =>
            {
                return manufacturerSEOTemplates.SelectAwait(async manufacturerSEOTemplate =>
                {
                    ////fill in model values from the entity
                    //var manufacturerSEOTemplateModel = manufacturerSEOTemplate.ToModel<ManufacturerSEOTemplateModel>();
                    //manufacturerSEOTemplateModel.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(manufacturerSEOTemplate.CreatedOnUtc, DateTimeKind.Utc);
                    //manufacturerSEOTemplateModel.UpdatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(manufacturerSEOTemplate.UpdatedOnUtc, DateTimeKind.Utc);
                    return await PrepareManufacturerSEOTemplateModelAsync(new ManufacturerSEOTemplateModel(), manufacturerSEOTemplate, true);
                });
            });

            return model;
        }
        #endregion
    }
}
