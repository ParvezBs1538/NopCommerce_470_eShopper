using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using NopStation.Plugin.Misc.AdvancedSEO.Areas.Admin.Models.ManufacturerManufacturerSEOTemplateMapping;
using NopStation.Plugin.Misc.AdvancedSEO.Areas.Admin.Models.ManufacturerToMap;
using NopStation.Plugin.Misc.AdvancedSEO.Domains;

namespace NopStation.Plugin.Misc.AdvancedSEO.Areas.Admin.Factories
{
    public interface IManufacturerManufacturerSEOTemplateMappingModelFactory
    {
        Task<ManufacturerManufacturerSEOTemplateMappingModel> PrepareManufacturerManufacturerSEOTemplateMappingModelAsync(ManufacturerManufacturerSEOTemplateMappingModel model, ManufacturerManufacturerSEOTemplateMapping manufacturerSEOTemplate,
            bool excludeProperties = false);

        Task<ManufacturerManufacturerSEOTemplateMappingSearchModel> PrepareManufacturerManufacturerSEOTemplateMappingSearchModelAsync(
            ManufacturerManufacturerSEOTemplateMappingSearchModel searchModel, ManufacturerSEOTemplate manufacturerSEOTemplate);

        Task<ManufacturerManufacturerSEOTemplateMappingListModel> PrepareManufacturerManufacturerSEOTemplateMappingListModelAsync(ManufacturerManufacturerSEOTemplateMappingSearchModel searchModel);

        #region ManufacturerToMap

        Task<ManufacturerToMapSearchModel> PrepareManufacturerToMapSearchModelAsync(
            ManufacturerToMapSearchModel searchModel, ManufacturerSEOTemplate manufacturerSEOTemplate);

        Task<ManufacturerToMapModel> PrepareManufacturerToMapModelAsync(Manufacturer manufacturer,
            bool excludeProperties = false);

        Task<ManufacturerToMapListModel> PrepareManufacturerToMapListModelAsync(ManufacturerToMapSearchModel searchModel);

        #endregion

    }
}
