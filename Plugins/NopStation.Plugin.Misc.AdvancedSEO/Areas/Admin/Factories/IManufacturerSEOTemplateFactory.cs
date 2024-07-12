using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NopStation.Plugin.Misc.AdvancedSEO.Areas.Admin.Models.ManufacturerSEOTemplate;
using NopStation.Plugin.Misc.AdvancedSEO.Domains;

namespace NopStation.Plugin.Misc.AdvancedSEO.Areas.Admin.Factories
{
    public interface IManufacturerSEOTemplateModelFactory
    {
        Task<ManufacturerSEOTemplateModel> PrepareManufacturerSEOTemplateModelAsync(ManufacturerSEOTemplateModel model, ManufacturerSEOTemplate manufacturerSEOTemplate,
            bool excludeProperties = false);

        Task<ManufacturerSEOTemplateSearchModel> PrepareManufacturerSEOTemplateSearchModelAsync(ManufacturerSEOTemplateSearchModel searchModel);

        Task<ManufacturerSEOTemplateListModel> PrepareManufacturerSEOTemplateListModelAsync(ManufacturerSEOTemplateSearchModel searchModel);

    }
}
