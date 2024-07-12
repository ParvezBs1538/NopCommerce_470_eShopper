using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using NopStation.Plugin.Misc.AdvancedSEO.Domains;

namespace NopStation.Plugin.Misc.AdvancedSEO.Services
{
    public interface  IManufacturerManufacturerSEOTemplateMappingService
    {

        Task DeleteManufacturerManufacturerSEOTemplateMappingAsync(ManufacturerManufacturerSEOTemplateMapping manufacturerManufacturerSEOTemplateMapping);

        Task DeleteManufacturerManufacturerSEOTemplateMappingsAsync(IList<ManufacturerManufacturerSEOTemplateMapping> manufacturerManufacturerSEOTemplateMappings);

        Task<IPagedList<ManufacturerManufacturerSEOTemplateMapping>> GetAllManufacturerManufacturerSEOTemplateMappingAsync(
            int manufacturerSEOTemplateId = 0,
            int manufacturerId = 0,
            int pageIndex = 0,
            int pageSize = int.MaxValue
            );

        Task<IPagedList<ManufacturerSEOTemplate>> GetAllMappedManufacturerSEOTemplateByManufacturerIdAsync(
            int manufacturerId,
            int storeId = 0,
            int pageIndex = 0,
            int pageSize = int.MaxValue
            );

        Task<ManufacturerManufacturerSEOTemplateMapping> GetManufacturerManufacturerSEOTemplateMappingAsync(
            int manufacturerSEOTemplateId,
            int manufacturerId
            );

        Task<ManufacturerManufacturerSEOTemplateMapping> GetManufacturerManufacturerSEOTemplateMappingByIdAsync(int manufacturerManufacturerSEOTemplateMappingId, bool includeDeleted = false);

        Task<IList<ManufacturerManufacturerSEOTemplateMapping>> GetManufacturerManufacturerSEOTemplateMappingByIdsAsync(int[] manufacturerManufacturerSEOTemplateMappingIds, bool includeDeleted = false);

        Task InsertManufacturerManufacturerSEOTemplateMappingAsync(ManufacturerManufacturerSEOTemplateMapping manufacturerManufacturerSEOTemplateMapping);

        Task UpdateManufacturerManufacturerSEOTemplateMappingAsync(ManufacturerManufacturerSEOTemplateMapping manufacturerManufacturerSEOTemplateMapping);

        Task<IPagedList<Manufacturer>> GetAllMappedCategoriesByMappingId(
            int manufacturerSEOTemplateId = 0,
            int pageIndex = 0,
            int pageSize = int.MaxValue
            );

        Task<IPagedList<Manufacturer>> GetAllMappedCategoriesByManufacturerId(
            int manufacturerId = 0,
            int pageIndex = 0,
            int pageSize = int.MaxValue
            );

        Task<IPagedList<Manufacturer>> GetAllNotMappedCategoriesByManufacturerSEOTemplateId(
            int manufacturerSEOTemplateId,
            string manufacturerName = null,
            int pageIndex = 0,
            int pageSize = int.MaxValue
            );

    }
}
