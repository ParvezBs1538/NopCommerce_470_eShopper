using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Misc.AdvancedSEO.Domains;

namespace NopStation.Plugin.Misc.AdvancedSEO.Services
{
    public interface IManufacturerSEOTemplateService
    {
        Task DeleteManufacturerSEOTemplateAsync(ManufacturerSEOTemplate manufacturerSEOTemplate);

        Task DeleteManufacturerSEOTemplatesAsync(IList<ManufacturerSEOTemplate> manufacturerSEOTemplates);

        Task<IPagedList<ManufacturerSEOTemplate>> GetAllManufacturerSEOTemplateAsync(
            string name = null,
            int storeId = 0,
            bool? isGlobalTemplate = null,
            bool? isActive = null,
            bool includeDeleted = false,
            int pageIndex = 0,
            int pageSize = int.MaxValue
            );

        Task<ManufacturerSEOTemplate> GetManufacturerSEOTemplateByIdAsync(int manufacturerSEOTemplateId, bool includeDeleted = false);

        Task<IList<ManufacturerSEOTemplate>> GetManufacturerSEOTemplateByIdsAsync(int[] manufacturerSEOTemplateIds, bool includeDeleted = false);

        Task InsertManufacturerSEOTemplateAsync(ManufacturerSEOTemplate manufacturerSEOTemplate);

        Task UpdateManufacturerSEOTemplateAsync(ManufacturerSEOTemplate manufacturerSEOTemplate);

    }
}
