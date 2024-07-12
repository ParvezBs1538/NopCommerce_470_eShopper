using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Localization;
using Nop.Data;
using Nop.Services.Customers;
using Nop.Services.ExportImport.Help;
using Nop.Services.Security;
using Nop.Services.Stores;
using NopStation.Plugin.Misc.WidgetManager.Services;
using NopStation.Plugin.Widgets.DynamicSurvey.Domain;

namespace NopStation.Plugin.Widgets.DynamicSurvey.Services
{
    public class SurveyService : ISurveyService
    {
        #region Fields

        private readonly IRepository<Survey> _surveyRepository;
        private readonly IRepository<SurveyAttributeMapping> _surveyAttributeMappingRepository;
        private readonly IRepository<SurveySubmission> _surveySubmissionRepository;
        private readonly IRepository<SurveySubmissionAttribute> _surveySubmissionAttributeRepository;
        private readonly IRepository<Customer> _customerRepository;
        private readonly CatalogSettings _catalogSettings;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IStoreService _storeService;
        private readonly ISurveyAttributeFormatter _surveyAttributeFormatter;
        private readonly ISurveyAttributeParser _surveyAttributeParser;
        private readonly IWorkContext _workContext;
        private readonly IAclService _aclService;
        private readonly IWidgetZoneService _widgetZoneMappingService;
        private readonly ICustomerService _customerService;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IScheduleService _scheduleMappingService;
        private readonly IConditionService _conditionMappingService;

        #endregion

        #region Ctor

        public SurveyService(IRepository<Survey> surveyRepository,
            IRepository<SurveyAttributeMapping> surveyAttributeMappingRepository,
            IRepository<SurveySubmission> surveySubmissionRepository,
            IRepository<SurveySubmissionAttribute> surveySubmissionAttributeRepository,
            IRepository<Customer> customerRepository,
            CatalogSettings catalogSettings,
            IStoreMappingService storeMappingService,
            IStoreService storeService,
            ISurveyAttributeFormatter surveyAttributeFormatter,
            ISurveyAttributeParser surveyAttributeParser,
            IWorkContext workContext,
            IAclService aclService,
            IWidgetZoneService widgetZoneMappingService,
            ICustomerService customerService,
            IStaticCacheManager staticCacheManager,
            IScheduleService scheduleMappingService,
            IConditionService conditionMappingService)
        {
            _surveyRepository = surveyRepository;
            _surveyAttributeMappingRepository = surveyAttributeMappingRepository;
            _surveySubmissionRepository = surveySubmissionRepository;
            _surveySubmissionAttributeRepository = surveySubmissionAttributeRepository;
            _customerRepository = customerRepository;
            _catalogSettings = catalogSettings;
            _storeMappingService = storeMappingService;
            _storeService = storeService;
            _surveyAttributeFormatter = surveyAttributeFormatter;
            _surveyAttributeParser = surveyAttributeParser;
            _workContext = workContext;
            _aclService = aclService;
            _widgetZoneMappingService = widgetZoneMappingService;
            _customerService = customerService;
            _staticCacheManager = staticCacheManager;
            _scheduleMappingService = scheduleMappingService;
            _conditionMappingService = conditionMappingService;
        }

        #endregion

        #region Methods

        #region Surveys

        public async Task DeleteSurveyAsync(Survey survey)
        {
            await _surveyRepository.DeleteAsync(survey);
        }

        public async Task DeleteSurveysAsync(IList<Survey> surveys)
        {
            await _surveyRepository.DeleteAsync(surveys);
        }

        public async Task InsertSurveyAsync(Survey survey)
        {
            await _surveyRepository.InsertAsync(survey);
        }

        public async Task UpdateSurveyAsync(Survey survey)
        {
            await _surveyRepository.UpdateAsync(survey);
        }

        public async Task<Survey> GetSurveyByIdAsync(int surveyId)
        {
            if (surveyId == 0)
                return null;

            return await _surveyRepository.GetByIdAsync(surveyId, default);
        }

        public async Task<IList<Survey>> GetSurveysByIdsAsync(int[] surveyIds)
        {
            return await _surveyRepository.GetByIdsAsync(surveyIds, cache => default, false);
        }

        public async Task<IList<Survey>> SearchSurveysAsync(string keywords = null, int storeId = 0, bool? includedInTopMenu = null,
            bool showHidden = false, bool? overridePublished = null, string widgetZone = null, bool validScheduleOnly = false)
        {
            var surveys = await _surveyRepository.GetAllAsync(async query =>
            {
                if (!showHidden)
                    query = query.Where(s => s.Published);
                else if (overridePublished.HasValue)
                    query = query.Where(s => s.Published == overridePublished.Value);

                //apply store mapping constraints
                query = await _storeMappingService.ApplyStoreMapping(query, storeId);

                //apply widget zone mapping constraints
                query = await _widgetZoneMappingService.ApplyWidgetZoneMappingAsync(query, widgetZone);

                //apply ACL constraints
                if (!showHidden)
                {
                    var customer = await _workContext.GetCurrentCustomerAsync();
                    query = await _aclService.ApplyAcl(query, customer);

                    query = await _conditionMappingService.ApplyCustomerConditionMappingAsync(query, customer.Id);
                }

                if (!string.IsNullOrWhiteSpace(keywords))
                    query = query.Where(s => s.Name.Contains(keywords) || s.Description.Contains(keywords));

                if (includedInTopMenu.HasValue)
                    query = query.Where(s => s.IncludeInTopMenu == includedInTopMenu.Value);

                if (validScheduleOnly)
                    query = await _scheduleMappingService.ApplyScheduleMappingAsync(query);

                query = query.Where(c => !c.Deleted);

                return query.OrderBy(c => c.DisplayOrder);
            }, cache =>
                cache.PrepareKeyForDefaultCache(
                    DynamicSurveyDefaults.SurveysAllCacheKey,
                    keywords,
                    storeId,
                    showHidden,
                    includedInTopMenu,
                    overridePublished,
                    widgetZone,
                    validScheduleOnly,
                    DateTime.UtcNow.Date)
                );

            if (validScheduleOnly)
                return await surveys.WhereAwait(s => new ValueTask<bool>(_scheduleMappingService.IsValidByDateTime(s))).ToListAsync();

            return surveys;
        }

        public virtual async Task<Survey> GetSurveyBySystemNameAsync(string systemName, int storeId = 0, bool showHidden = false)
        {
            if (string.IsNullOrEmpty(systemName))
                return null;

            var customer = await _workContext.GetCurrentCustomerAsync();
            var customerRoleIds = await _customerService.GetCustomerRoleIdsAsync(customer);

            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(DynamicSurveyDefaults.SurveyBySystemNameCacheKey, systemName, storeId, customerRoleIds);

            var survey = await _staticCacheManager.GetAsync(cacheKey, async () =>
            {
                var query = _surveyRepository.Table;

                if (!showHidden)
                    query = query.Where(s => s.Published);

                //apply store mapping constraints
                query = await _storeMappingService.ApplyStoreMapping(query, storeId);

                //apply ACL constraints
                if (!showHidden)
                    query = await _aclService.ApplyAcl(query, customerRoleIds);

                return query.Where(s => s.SystemName == systemName)
                    .OrderBy(s => s.Id)
                    .FirstOrDefault();
            });

            return survey;
        }

        public virtual async Task UpdateSurveyStoreMappingsAsync(Survey survey, IList<int> limitedToStoresIds)
        {
            survey.LimitedToStores = limitedToStoresIds.Any();

            var existingStoreMappings = await _storeMappingService.GetStoreMappingsAsync(survey);
            var allStores = await _storeService.GetAllStoresAsync();
            foreach (var store in allStores)
            {
                if (limitedToStoresIds.Contains(store.Id))
                {
                    //new store
                    if (!existingStoreMappings.Any(sm => sm.StoreId == store.Id))
                        await _storeMappingService.InsertStoreMappingAsync(survey, store.Id);
                }
                else
                {
                    //remove store
                    var storeMappingToDelete = existingStoreMappings.FirstOrDefault(sm => sm.StoreId == store.Id);
                    if (storeMappingToDelete != null)
                        await _storeMappingService.DeleteStoreMappingAsync(storeMappingToDelete);
                }
            }
        }

        public virtual async Task<IPagedList<Survey>> GetSurveysBySurveyAttributeIdAsync(int surveyAttributeId,
            int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = from p in _surveyRepository.Table
                        join pam in _surveyAttributeMappingRepository.Table on p.Id equals pam.SurveyId
                        where
                            pam.SurveyAttributeId == surveyAttributeId &&
                            !p.Deleted
                        orderby p.Name
                        select p;

            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

        #endregion

        #region Survey export

        public async Task<byte[]> ExportSurveysToXlsxAsync(IList<SurveySubmission> surveySubmissions)
        {
            //property array
            var properties = new[]
            {
                new PropertyByName<SurveySubmission,Language>("SubmissionId", (ss, l) => ss.Id),
                new PropertyByName<SurveySubmission,Language>("SurveyId", (ss, l) => ss.SurveyId),
                new PropertyByName<SurveySubmission,Language>("CustomerId", (ss, l) => ss.CustomerId),
                new PropertyByName<SurveySubmission,Language>("CreatedOnUtc", (ss, l) => ss.CreatedOnUtc),
            };

            var submittedProperties = new List<string>();

            var submittedValues = new Dictionary<int, IDictionary<string, List<string>>>();

            foreach (var surveySubmission in surveySubmissions)
            {
                var parsedValues = await _surveyAttributeFormatter.GetAttributeValuesAsync(surveySubmission.AttributeXml);
                submittedValues.Add(surveySubmission.Id, parsedValues);
                submittedProperties.AddRange(parsedValues.Keys);
            }

            string getPropertyValue(SurveySubmission ss, string ppName) => submittedValues[ss.Id] == null || !submittedValues[ss.Id].ContainsKey(ppName) ? string.Empty : string.Join(", ", submittedValues[ss.Id][ppName]);

            properties = properties.Concat(submittedProperties.Distinct().Select(p => new PropertyByName<SurveySubmission, Language>(p, (ss, l) => getPropertyValue(ss, p)))).ToArray();

            var propertyManager = new PropertyManager<SurveySubmission, Language>(properties, _catalogSettings);

            return await propertyManager.ExportToXlsxAsync(surveySubmissions);
        }

        #endregion

        #region Survey submissions

        public async Task DeleteSurveySubmissionAsync(SurveySubmission surveySubmission)
        {
            await _surveySubmissionRepository.DeleteAsync(surveySubmission);
        }

        public async Task DeleteSurveySubmissionsAsync(IList<SurveySubmission> surveySubmissions)
        {
            await _surveySubmissionRepository.DeleteAsync(surveySubmissions);
        }

        public async Task InsertSurveySubmissionAsync(SurveySubmission surveySubmission)
        {
            await _surveySubmissionRepository.InsertAsync(surveySubmission);
        }

        public async Task UpdateSurveySubmissionAsync(SurveySubmission surveySubmission)
        {
            await _surveySubmissionRepository.UpdateAsync(surveySubmission);
        }

        public async Task<SurveySubmission> GetSurveySubmissionByIdAsync(int surveySubmissionId)
        {
            if (surveySubmissionId == 0)
                return null;

            return await _surveySubmissionRepository.GetByIdAsync(surveySubmissionId, cache => default);
        }

        public async Task<IList<SurveySubmission>> GetSurveySubmissionsByIdsAsync(int[] surveySubmissionIds)
        {
            return await _surveySubmissionRepository.GetByIdsAsync(surveySubmissionIds, cache => default);
        }

        public virtual async Task<IList<SurveySubmission>> GetSurveySubmissionsBySurveyIdAsync(int surveyId, int customerId = 0, bool loadOnlyOne = false)
        {
            var query = from ss in _surveySubmissionRepository.Table
                        join s in _surveyRepository.Table on ss.SurveyId equals s.Id
                        where
                            ss.SurveyId == surveyId &&
                            (customerId == 0 || ss.CustomerId == customerId)
                        orderby ss.CreatedOnUtc descending
                        select ss;

            if (loadOnlyOne)
                return await query.Take(1).ToListAsync();

            return await query.ToListAsync();
        }

        public virtual async Task<IPagedList<SurveySubmission>> GetAllSurveySubmissionsAsync(int surveyId = 0, int customerId = 0, string customerEmail = null, DateTime? dateStart = null, DateTime? dateEnd = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _surveySubmissionRepository.Table;

            if (surveyId > 0)
                query = query.Where(ss => ss.SurveyId == surveyId);

            if (customerId > 0)
                query = query.Where(ss => ss.CustomerId == customerId);

            if (!string.IsNullOrEmpty(customerEmail) && customerId == 0)
                query = from ss in query
                        join c in _customerRepository.Table on ss.CustomerId equals c.Id
                        where c.Email.Contains(customerEmail)
                        select ss;

            if (dateStart.HasValue)
                query = query.Where(ss => dateStart.Value <= ss.CreatedOnUtc);

            if (dateEnd.HasValue)
                query = query.Where(ss => dateEnd.Value >= ss.CreatedOnUtc);

            query = query.OrderByDescending(ss => ss.CreatedOnUtc);

            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

        #endregion

        #region Survey submission attributes

        public async Task DeleteSurveySubmissionAttributeAsync(SurveySubmissionAttribute surveySubmissionAttribute)
        {
            await _surveySubmissionAttributeRepository.DeleteAsync(surveySubmissionAttribute);
        }

        public async Task DeleteSurveySubmissionAttributesAsync(IList<SurveySubmissionAttribute> surveySubmissionAttributes)
        {
            await _surveySubmissionAttributeRepository.DeleteAsync(surveySubmissionAttributes);
        }

        public async Task InsertSurveySubmissionAttributeAsync(SurveySubmissionAttribute surveySubmissionAttribute)
        {
            await _surveySubmissionAttributeRepository.InsertAsync(surveySubmissionAttribute);
        }

        public async Task UpdateSurveySubmissionAttributeAsync(SurveySubmissionAttribute surveySubmissionAttribute)
        {
            await _surveySubmissionAttributeRepository.UpdateAsync(surveySubmissionAttribute);
        }

        public async Task<SurveySubmissionAttribute> GetSurveySubmissionAttributeByIdAsync(int surveySubmissionAttributeId)
        {
            if (surveySubmissionAttributeId == 0)
                return null;

            return await _surveySubmissionAttributeRepository.GetByIdAsync(surveySubmissionAttributeId, default);
        }

        public virtual async Task<IList<SurveySubmissionAttribute>> GetSurveySubmissionAttributesBySurveySubmissionIdAsync(int surveySubmissionId)
        {
            var query = from ssa in _surveySubmissionAttributeRepository.Table
                        join ss in _surveySubmissionRepository.Table on ssa.SurveySubmissionId equals ss.Id
                        where
                            ssa.SurveySubmissionId == surveySubmissionId
                        orderby ss.CreatedOnUtc descending
                        select ssa;

            return await query.ToListAsync();
        }

        #endregion

        #endregion
    }
}
