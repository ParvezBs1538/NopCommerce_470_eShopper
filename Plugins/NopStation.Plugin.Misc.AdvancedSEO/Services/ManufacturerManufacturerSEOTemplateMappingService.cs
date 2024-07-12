using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Data;
using Nop.Services.Stores;
using NopStation.Plugin.Misc.AdvancedSEO.Domains;

namespace NopStation.Plugin.Misc.AdvancedSEO.Services
{

    public class ManufacturerManufacturerSEOTemplateMappingService : IManufacturerManufacturerSEOTemplateMappingService
    {
        private readonly IRepository<Manufacturer> _manufacturerRepository;
        #region Fields

        private readonly IRepository<ManufacturerManufacturerSEOTemplateMapping> _manufacturerManufacturerSEOTemplateMappingRepository;
        private readonly IRepository<ManufacturerSEOTemplate> _manufacturerSEOTemplateRepository;
        private readonly IStoreMappingService _storeMappingService;

        #endregion

        #region Ctor

        public ManufacturerManufacturerSEOTemplateMappingService(
            IRepository<Manufacturer> manufacturerRepository,
            IRepository<ManufacturerManufacturerSEOTemplateMapping> manufacturerManufacturerSEOTemplateMappingRepository,
            IRepository<ManufacturerSEOTemplate> manufacturerSEOTemplateRepository,
            IStoreMappingService storeMappingService
            )
        {
            _manufacturerRepository = manufacturerRepository;
            _manufacturerManufacturerSEOTemplateMappingRepository = manufacturerManufacturerSEOTemplateMappingRepository;
            _manufacturerSEOTemplateRepository = manufacturerSEOTemplateRepository;
            _storeMappingService = storeMappingService;
        }

        #endregion

        #region Utilites



        #endregion

        #region Methods

        public async Task DeleteManufacturerManufacturerSEOTemplateMappingAsync(ManufacturerManufacturerSEOTemplateMapping manufacturerManufacturerSEOTemplateMapping)
        {
            if (manufacturerManufacturerSEOTemplateMapping == null)
                throw new ArgumentNullException(nameof(manufacturerManufacturerSEOTemplateMapping));

            await _manufacturerManufacturerSEOTemplateMappingRepository.DeleteAsync(manufacturerManufacturerSEOTemplateMapping);
        }


        public async Task DeleteManufacturerManufacturerSEOTemplateMappingsAsync(IList<ManufacturerManufacturerSEOTemplateMapping> manufacturerManufacturerSEOTemplateMappings)
        {
            if (manufacturerManufacturerSEOTemplateMappings == null)
                throw new ArgumentNullException(nameof(manufacturerManufacturerSEOTemplateMappings));

            await _manufacturerManufacturerSEOTemplateMappingRepository.DeleteAsync(manufacturerManufacturerSEOTemplateMappings);
        }

        public async Task<IPagedList<ManufacturerManufacturerSEOTemplateMapping>> GetAllManufacturerManufacturerSEOTemplateMappingAsync(
            int manufacturerSEOTemplateId = 0,
            int manufacturerId = 0,
            int pageIndex = 0,
            int pageSize = int.MaxValue
            )
        {
            if (pageSize == int.MaxValue)
                --pageSize;

            var query = _manufacturerManufacturerSEOTemplateMappingRepository.Table;

            if (manufacturerId > 0)
                query = query.Where(ccm => ccm.ManufacturerId == manufacturerId);

            if (manufacturerSEOTemplateId > 0)
                query = query.Where(ccm => ccm.ManufacturerSEOTemplateId == manufacturerSEOTemplateId);

            return await query.OrderByDescending(cst => cst.Id).ToPagedListAsync(pageIndex, pageSize);

        }

        public async Task<IPagedList<ManufacturerSEOTemplate>> GetAllMappedManufacturerSEOTemplateByManufacturerIdAsync(
            int manufacturerId,
            int storeId = 0,
            int pageIndex = 0,
            int pageSize = int.MaxValue
            )
        {
            if (pageSize == int.MaxValue)
                --pageSize;

            if (manufacturerId < 1)
                throw new ArgumentNullException(nameof(manufacturerId));

            var query = from mt in _manufacturerSEOTemplateRepository.Table
                        join mmtm in _manufacturerManufacturerSEOTemplateMappingRepository.Table on mt.Id equals mmtm.ManufacturerSEOTemplateId
                        where mmtm.ManufacturerId == manufacturerId && !mt.IsGlobalTemplate && mt.IsActive && !mt.Deleted
                        select mt;

            //apply store mapping constraints
            query = await _storeMappingService.ApplyStoreMapping(query, storeId);

            return await query.OrderBy(ct => ct.Id).OrderBy(ct => ct.Priority).ToPagedListAsync(pageIndex, pageSize);
        }


        //public async Task<ManufacturerSEOTemplate> GetAllMappedManufacturerSEOTemplateByManufacturerIdAsync(
        //    int manufacturerId
        //    )
        //{

        //    if (manufacturerId < 1)
        //        throw new ArgumentNullException(nameof(manufacturerId));

        //    var query = await _manufacturerManufacturerSEOTemplateMappingRepository.Table.FirstOrDefaultAsync(ccm => ccm.ManufacturerId == manufacturerId);

        //    return query;
        //}

        public async Task<ManufacturerManufacturerSEOTemplateMapping> GetManufacturerManufacturerSEOTemplateMappingAsync(
            int manufacturerSEOTemplateId,
            int manufacturerId
            )
        {
            if (manufacturerSEOTemplateId < 1)
                throw new ArgumentNullException(nameof(manufacturerSEOTemplateId));

            if (manufacturerId < 1)
                throw new ArgumentNullException(nameof(manufacturerId));

            var query = await _manufacturerManufacturerSEOTemplateMappingRepository.Table.FirstOrDefaultAsync(ccm => ccm.ManufacturerSEOTemplateId == manufacturerSEOTemplateId && ccm.ManufacturerId == manufacturerId);

            return query;
        }

        public async Task<IPagedList<Manufacturer>> GetAllMappedCategoriesByManufacturerId(
            int manufacturerId = 0,
            int pageIndex = 0,
            int pageSize = int.MaxValue
            )
        {
            if (pageSize == int.MaxValue)
                --pageSize;

            var query = from c in _manufacturerRepository.Table
                        join ccm in _manufacturerManufacturerSEOTemplateMappingRepository.Table on c.Id equals ccm.ManufacturerId
                        where c.Id == manufacturerId && !c.Deleted
                        select c;

            return await query.OrderByDescending(cst => cst.Id).ToPagedListAsync(pageIndex, pageSize);
        }

        public async Task<IPagedList<Manufacturer>> GetAllMappedCategoriesByManufacturerSEOTemplateId(
            int manufacturerSEOTemplateId = 0,
            int pageIndex = 0,
            int pageSize = int.MaxValue
            )
        {
            if (pageSize == int.MaxValue)
                --pageSize;

            var query = from c in _manufacturerRepository.Table
                        join ccm in _manufacturerManufacturerSEOTemplateMappingRepository.Table on c.Id equals ccm.ManufacturerId
                        where ccm.ManufacturerSEOTemplateId == manufacturerSEOTemplateId && !c.Deleted
                        select c;

            return await query.OrderByDescending(cst => cst.Id).ToPagedListAsync(pageIndex, pageSize);
        }

        public async Task<IPagedList<Manufacturer>> GetAllNotMappedCategoriesByManufacturerSEOTemplateId(
            int manufacturerSEOTemplateId,
            string manufacturerName = null,
            int pageIndex = 0,
            int pageSize = int.MaxValue
            )
        {
            if (pageSize == int.MaxValue)
                --pageSize;

            var query = from c in _manufacturerRepository.Table
                        where !c.Deleted && (manufacturerName == null || manufacturerName == string.Empty || c.Name.Contains(manufacturerName))
                        select c;

            var mappedManufacturerIds = (from c in query
                                     select c.Id)
                                  .Except(from ccm in _manufacturerManufacturerSEOTemplateMappingRepository.Table
                                          where ccm.ManufacturerSEOTemplateId == manufacturerSEOTemplateId
                                          select ccm.ManufacturerId
                                          );
            query = from c in query
                    join mci in mappedManufacturerIds on c.Id equals mci
                    select c;

            return await query.OrderByDescending(cst => cst.Id).ToPagedListAsync(pageIndex, pageSize);
        }

        public async Task<IPagedList<Manufacturer>> GetAllMappedCategoriesByMappingId(
            int manufacturerManufacturerSEOTemplateMappingId = 0,
            int pageIndex = 0,
            int pageSize = int.MaxValue
            )
        {
            if (pageSize == int.MaxValue)
                --pageSize;

            var query = from c in _manufacturerRepository.Table
                        join ccm in _manufacturerManufacturerSEOTemplateMappingRepository.Table on c.Id equals ccm.ManufacturerId
                        where c.Id == manufacturerManufacturerSEOTemplateMappingId && !c.Deleted
                        select c;

            return await query.OrderByDescending(cst => cst.Id).ToPagedListAsync(pageIndex, pageSize);
        }

        public async Task<ManufacturerManufacturerSEOTemplateMapping> GetManufacturerManufacturerSEOTemplateMappingByIdAsync(int manufacturerManufacturerSEOTemplateMappingId, bool includeDeleted = false)
        {
            if (manufacturerManufacturerSEOTemplateMappingId < 1)
                throw new ArgumentNullException(nameof(manufacturerManufacturerSEOTemplateMappingId));

            return await _manufacturerManufacturerSEOTemplateMappingRepository.GetByIdAsync(manufacturerManufacturerSEOTemplateMappingId, includeDeleted: includeDeleted);
        }

        public async Task<IList<ManufacturerManufacturerSEOTemplateMapping>> GetManufacturerManufacturerSEOTemplateMappingByIdsAsync(int[] manufacturerManufacturerSEOTemplateMappingIds, bool includeDeleted = false)
        {
            return await _manufacturerManufacturerSEOTemplateMappingRepository.GetByIdsAsync(manufacturerManufacturerSEOTemplateMappingIds, includeDeleted: includeDeleted);
        }

        public async Task InsertManufacturerManufacturerSEOTemplateMappingAsync(ManufacturerManufacturerSEOTemplateMapping manufacturerManufacturerSEOTemplateMapping)
        {
            if (manufacturerManufacturerSEOTemplateMapping == null)
                throw new ArgumentNullException(nameof(manufacturerManufacturerSEOTemplateMapping));

            await _manufacturerManufacturerSEOTemplateMappingRepository.InsertAsync(manufacturerManufacturerSEOTemplateMapping);
        }

        public async Task UpdateManufacturerManufacturerSEOTemplateMappingAsync(ManufacturerManufacturerSEOTemplateMapping manufacturerManufacturerSEOTemplateMapping)
        {
            if (manufacturerManufacturerSEOTemplateMapping == null)
                throw new ArgumentNullException(nameof(manufacturerManufacturerSEOTemplateMapping));

            await _manufacturerManufacturerSEOTemplateMappingRepository.UpdateAsync(manufacturerManufacturerSEOTemplateMapping);
        }

        #endregion
    }
}
