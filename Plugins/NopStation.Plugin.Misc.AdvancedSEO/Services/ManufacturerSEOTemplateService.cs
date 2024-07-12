using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using Nop.Services.Stores;
using NopStation.Plugin.Misc.AdvancedSEO.Domains;

namespace NopStation.Plugin.Misc.AdvancedSEO.Services
{
    public class ManufacturerSEOTemplateService : IManufacturerSEOTemplateService
    {
        #region Fields

        private readonly IRepository<ManufacturerSEOTemplate> _manufacturerSEOTemplateRepository;
        private readonly IStoreMappingService _storeMappingService;

        #endregion

        #region Ctor

        public ManufacturerSEOTemplateService(
            IRepository<ManufacturerSEOTemplate> manufacturerSEOTemplateRepository,
            IStoreMappingService storeMappingService
            )
        {
            _manufacturerSEOTemplateRepository = manufacturerSEOTemplateRepository;
            _storeMappingService = storeMappingService;
        }

        #endregion

        #region Utilites



        #endregion

        #region Methods

        public async Task DeleteManufacturerSEOTemplateAsync(ManufacturerSEOTemplate manufacturerSEOTemplate)
        {
            if (manufacturerSEOTemplate == null)
                throw new ArgumentNullException(nameof(manufacturerSEOTemplate));

            await _manufacturerSEOTemplateRepository.DeleteAsync(manufacturerSEOTemplate);
        }


        public async Task DeleteManufacturerSEOTemplatesAsync(IList<ManufacturerSEOTemplate> manufacturerSEOTemplates)
        {
            if (manufacturerSEOTemplates == null)
                throw new ArgumentNullException(nameof(manufacturerSEOTemplates));

            await _manufacturerSEOTemplateRepository.DeleteAsync(manufacturerSEOTemplates);
        }

        public async Task<IPagedList<ManufacturerSEOTemplate>> GetAllManufacturerSEOTemplateAsync(
            string name = null,
            int storeId = 0,
            bool? isGlobalTemplate = null,
            bool? isActive = null,
            bool includeDeleted = false,
            int pageIndex = 0,
            int pageSize = int.MaxValue
            )
        {
            if (pageSize == int.MaxValue)
                --pageSize;

            var query = _manufacturerSEOTemplateRepository.Table;


            //apply store mapping constraints
            query = await _storeMappingService.ApplyStoreMapping(query, storeId);

            if (name != null && !string.IsNullOrEmpty(name))
                query = query.Where(cst => cst.TemplateName.Contains(name));

            if (isActive.HasValue)
                query = query.Where(cst => cst.IsActive == isActive.Value);

            if (isGlobalTemplate.HasValue)
                query = query.Where(cst => cst.IsGlobalTemplate == isGlobalTemplate.Value);

            if (!includeDeleted)
                query = query.Where(cst => cst.Deleted == false);


            return await query.OrderBy(cst => cst.Id).OrderBy(cst => cst.Priority).ToPagedListAsync(pageIndex, pageSize);

        }

        public async Task<ManufacturerSEOTemplate> GetManufacturerSEOTemplateByIdAsync(int manufacturerSEOTemplateId, bool includeDeleted = false)
        {
            if (manufacturerSEOTemplateId < 1)
                throw new ArgumentNullException(nameof(manufacturerSEOTemplateId));

            return await _manufacturerSEOTemplateRepository.GetByIdAsync(manufacturerSEOTemplateId, includeDeleted: includeDeleted);
        }

        public async Task<IList<ManufacturerSEOTemplate>> GetManufacturerSEOTemplateByIdsAsync(int[] manufacturerSEOTemplateIds, bool includeDeleted = false)
        {
            return await _manufacturerSEOTemplateRepository.GetByIdsAsync(manufacturerSEOTemplateIds, includeDeleted: includeDeleted);
        }

        public async Task InsertManufacturerSEOTemplateAsync(ManufacturerSEOTemplate manufacturerSEOTemplate)
        {
            if (manufacturerSEOTemplate == null)
                throw new ArgumentNullException(nameof(manufacturerSEOTemplate));

            await _manufacturerSEOTemplateRepository.InsertAsync(manufacturerSEOTemplate);
        }

        public async Task UpdateManufacturerSEOTemplateAsync(ManufacturerSEOTemplate manufacturerSEOTemplate)
        {
            if (manufacturerSEOTemplate == null)
                throw new ArgumentNullException(nameof(manufacturerSEOTemplate));

            await _manufacturerSEOTemplateRepository.UpdateAsync(manufacturerSEOTemplate);
        }

        #endregion
    }
}
