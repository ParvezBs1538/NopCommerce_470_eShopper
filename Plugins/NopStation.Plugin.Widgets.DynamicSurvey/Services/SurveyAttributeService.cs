using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Data;
using NopStation.Plugin.Widgets.DynamicSurvey.Domain;

namespace NopStation.Plugin.Widgets.DynamicSurvey.Services
{
    public partial class SurveyAttributeService : ISurveyAttributeService
    {
        #region Fields

        private readonly IRepository<Survey> _surveyRepository;
        private readonly IRepository<SurveyAttribute> _surveyAttributeRepository;
        private readonly IRepository<SurveyAttributeMapping> _surveyAttributeMappingRepository;
        private readonly IRepository<SurveyAttributeValue> _surveyAttributeValueRepository;
        private readonly IRepository<PredefinedSurveyAttributeValue> _predefinedSurveyAttributeValueRepository;
        private readonly IStaticCacheManager _staticCacheManager;

        #endregion

        #region Ctor

        public SurveyAttributeService(IRepository<Survey> surveyRepository,
            IRepository<SurveyAttribute> surveyAttributeRepository,
            IRepository<SurveyAttributeMapping> surveyAttributeMappingRepository,
            IRepository<SurveyAttributeValue> surveyAttributeValueRepository,
            IRepository<PredefinedSurveyAttributeValue> predefinedSurveyAttributeValueRepository,
            IStaticCacheManager staticCacheManager)
        {
            _surveyRepository = surveyRepository;
            _surveyAttributeRepository = surveyAttributeRepository;
            _surveyAttributeMappingRepository = surveyAttributeMappingRepository;
            _surveyAttributeValueRepository = surveyAttributeValueRepository;
            _predefinedSurveyAttributeValueRepository = predefinedSurveyAttributeValueRepository;
            _staticCacheManager = staticCacheManager;
        }

        #endregion

        #region Methods

        #region Survey attributes

        public virtual async Task DeleteSurveyAttributeAsync(SurveyAttribute surveyAttribute)
        {
            await _surveyAttributeRepository.DeleteAsync(surveyAttribute);
        }

        public virtual async Task DeleteSurveyAttributesAsync(IList<SurveyAttribute> surveyAttributes)
        {
            if (surveyAttributes == null)
                throw new ArgumentNullException(nameof(surveyAttributes));

            foreach (var surveyAttribute in surveyAttributes)
                await DeleteSurveyAttributeAsync(surveyAttribute);
        }

        public virtual async Task<IPagedList<SurveyAttribute>> GetAllSurveyAttributesAsync(int pageIndex = 0,
            int pageSize = int.MaxValue)
        {
            var surveyAttributes = await _surveyAttributeRepository.GetAllPagedAsync(query =>
            {
                return from pa in query
                       orderby pa.Name
                       select pa;
            }, pageIndex, pageSize);

            return surveyAttributes;
        }

        public virtual async Task<SurveyAttribute> GetSurveyAttributeByIdAsync(int surveyAttributeId)
        {
            return await _surveyAttributeRepository.GetByIdAsync(surveyAttributeId, cache => default);
        }

        public virtual async Task<IList<SurveyAttribute>> GetSurveyAttributeByIdsAsync(int[] surveyAttributeIds)
        {
            return await _surveyAttributeRepository.GetByIdsAsync(surveyAttributeIds);
        }

        public virtual async Task InsertSurveyAttributeAsync(SurveyAttribute surveyAttribute)
        {
            await _surveyAttributeRepository.InsertAsync(surveyAttribute);
        }

        public virtual async Task UpdateSurveyAttributeAsync(SurveyAttribute surveyAttribute)
        {
            await _surveyAttributeRepository.UpdateAsync(surveyAttribute);
        }

        public virtual async Task<int[]> GetNotExistingAttributesAsync(int[] attributeId)
        {
            if (attributeId == null)
                throw new ArgumentNullException(nameof(attributeId));

            var query = _surveyAttributeRepository.Table;
            var queryFilter = attributeId.Distinct().ToArray();
            var filter = await query.Select(a => a.Id)
                .Where(m => queryFilter.Contains(m))
                .ToListAsync();

            return queryFilter.Except(filter).ToArray();
        }

        #endregion

        #region Survey attributes mappings

        public virtual async Task DeleteSurveyAttributeMappingAsync(SurveyAttributeMapping surveyAttributeMapping)
        {
            await _surveyAttributeMappingRepository.DeleteAsync(surveyAttributeMapping);
        }

        public virtual async Task<IList<SurveyAttributeMapping>> GetSurveyAttributeMappingsBySurveyIdAsync(int surveyId)
        {
            var allCacheKey = _staticCacheManager.PrepareKeyForDefaultCache(DynamicSurveyDefaults.SurveyAttributeMappingsBySurveyCacheKey, surveyId);

            var query = from pam in _surveyAttributeMappingRepository.Table
                        orderby pam.DisplayOrder, pam.Id
                        where pam.SurveyId == surveyId
                        select pam;

            var attributes = await _staticCacheManager.GetAsync(allCacheKey, async () => await query.ToListAsync()) ?? new List<SurveyAttributeMapping>();

            return attributes;
        }

        public virtual async Task<SurveyAttributeMapping> GetSurveyAttributeMappingByIdAsync(int surveyAttributeMappingId)
        {
            return await _surveyAttributeMappingRepository.GetByIdAsync(surveyAttributeMappingId, cache => default);
        }

        public virtual async Task InsertSurveyAttributeMappingAsync(SurveyAttributeMapping surveyAttributeMapping)
        {
            await _surveyAttributeMappingRepository.InsertAsync(surveyAttributeMapping);
        }

        public virtual async Task UpdateSurveyAttributeMappingAsync(SurveyAttributeMapping surveyAttributeMapping)
        {
            await _surveyAttributeMappingRepository.UpdateAsync(surveyAttributeMapping);
        }

        #endregion

        #region Survey attribute values

        public virtual async Task DeleteSurveyAttributeValueAsync(SurveyAttributeValue surveyAttributeValue)
        {
            await _surveyAttributeValueRepository.DeleteAsync(surveyAttributeValue);
        }

        public virtual async Task<IList<SurveyAttributeValue>> GetSurveyAttributeValuesAsync(int surveyAttributeMappingId)
        {
            var key = _staticCacheManager.PrepareKeyForDefaultCache(DynamicSurveyDefaults.SurveyAttributeValuesByAttributeCacheKey, surveyAttributeMappingId);

            var query = from pav in _surveyAttributeValueRepository.Table
                        orderby pav.DisplayOrder, pav.Id
                        where pav.SurveyAttributeMappingId == surveyAttributeMappingId
                        select pav;
            var surveyAttributeValues = await _staticCacheManager.GetAsync(key, async () => await query.ToListAsync());

            return surveyAttributeValues;
        }

        public virtual async Task<SurveyAttributeValue> GetSurveyAttributeValueByIdAsync(int surveyAttributeValueId)
        {
            return await _surveyAttributeValueRepository.GetByIdAsync(surveyAttributeValueId, cache => default);
        }

        public virtual async Task InsertSurveyAttributeValueAsync(SurveyAttributeValue surveyAttributeValue)
        {
            await _surveyAttributeValueRepository.InsertAsync(surveyAttributeValue);
        }

        public virtual async Task UpdateSurveyAttributeValueAsync(SurveyAttributeValue surveyAttributeValue)
        {
            await _surveyAttributeValueRepository.UpdateAsync(surveyAttributeValue);
        }

        #endregion

        #region Predefined survey attribute values

        public virtual async Task DeletePredefinedSurveyAttributeValueAsync(PredefinedSurveyAttributeValue psav)
        {
            await _predefinedSurveyAttributeValueRepository.DeleteAsync(psav);
        }

        public virtual async Task<IList<PredefinedSurveyAttributeValue>> GetPredefinedSurveyAttributeValuesAsync(int surveyAttributeId)
        {
            var key = _staticCacheManager.PrepareKeyForDefaultCache(DynamicSurveyDefaults.PredefinedSurveyAttributeValuesByAttributeCacheKey, surveyAttributeId);

            var query = from ppav in _predefinedSurveyAttributeValueRepository.Table
                        orderby ppav.DisplayOrder, ppav.Id
                        where ppav.SurveyAttributeId == surveyAttributeId
                        select ppav;

            var values = await _staticCacheManager.GetAsync(key, async () => await query.ToListAsync());

            return values;
        }

        public virtual async Task<PredefinedSurveyAttributeValue> GetPredefinedSurveyAttributeValueByIdAsync(int id)
        {
            return await _predefinedSurveyAttributeValueRepository.GetByIdAsync(id, cache => default);
        }

        public virtual async Task InsertPredefinedSurveyAttributeValueAsync(PredefinedSurveyAttributeValue psav)
        {
            await _predefinedSurveyAttributeValueRepository.InsertAsync(psav);
        }

        public virtual async Task UpdatePredefinedSurveyAttributeValueAsync(PredefinedSurveyAttributeValue psav)
        {
            await _predefinedSurveyAttributeValueRepository.UpdateAsync(psav);
        }

        #endregion

        #endregion
    }
}