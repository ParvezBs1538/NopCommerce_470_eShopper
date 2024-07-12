using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Topics;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.ExportImport;
using Nop.Services.ExportImport.Help;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Nop.Services.Topics;

namespace NopStation.Plugin.Widgets.ExportImportTopic.Services
{
    public partial class ExImManager : IExImManager
    {
        #region Fields

        private readonly CatalogSettings _catalogSettings;
        private readonly ICustomerService _customerService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IStoreService _storeService;
        private readonly IWorkContext _workContext;
        private readonly IAclService _aclService;
        private readonly IImportManager _importManager;
        private readonly ITopicService _topicService;
        private readonly ILocalizationService _localizationService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizedEntityService _localizedEntityService;

        #endregion

        #region Ctor

        public ExImManager(CatalogSettings catalogSettings,
            ICustomerService customerService,
            IGenericAttributeService genericAttributeService,
            IStoreMappingService storeMappingService,
            IStoreService storeService,
            IWorkContext workContext,
            IAclService aclService,
            IImportManager importManager,
            ITopicService topicService,
            ILocalizationService localizationService,
            IUrlRecordService urlRecordService,
            ILanguageService languageService,
            ILocalizedEntityService localizedEntityService)
        {
            _catalogSettings = catalogSettings;
            _customerService = customerService;
            _genericAttributeService = genericAttributeService;
            _storeMappingService = storeMappingService;
            _storeService = storeService;
            _workContext = workContext;
            _aclService = aclService;
            _importManager = importManager;
            _topicService = topicService;
            _localizationService = localizationService;
            _urlRecordService = urlRecordService;
            _languageService = languageService;
            _localizedEntityService = localizedEntityService;
        }

        #endregion

        #region Utilities

        protected async Task<string[]> GetNotExistingCustomerRolesAsync(string[] roleIdsNames)
        {
            if (roleIdsNames == null)
                throw new ArgumentNullException(nameof(roleIdsNames));

            var query = await _customerService.GetAllCustomerRolesAsync();
            var queryFilter = roleIdsNames.Distinct().ToArray();
            //filtering by name
            var filter = await query.Select(role => role.Name)
                .Where(role => queryFilter.Contains(role))
                .ToListAsync();
            queryFilter = queryFilter.Except(filter).ToArray();

            //if some names not found
            if (!queryFilter.Any())
                return queryFilter.ToArray();

            //filtering by IDs
            filter = await query.Select(role => role.Id.ToString())
                .Where(role => queryFilter.Contains(role))
                .ToListAsync();
            queryFilter = queryFilter.Except(filter).ToArray();

            return queryFilter.ToArray();
        }

        protected virtual async Task<object> GetLimitedToStoresAsync(Topic topic)
        {
            string limitedToStores = null;
            foreach (var storeMapping in await _storeMappingService.GetStoreMappingsAsync(topic))
            {
                var store = await _storeService.GetStoreByIdAsync(storeMapping.StoreId);

                limitedToStores += _catalogSettings.ExportImportRelatedEntitiesByName ? store.Name : store.Id.ToString();

                limitedToStores += ";";
            }

            return limitedToStores;
        }

        protected virtual async Task<object> GetSubjectToAclAsync(Topic topic)
        {
            string subjectToAcl = null;
            foreach (var aclRecord in await _aclService.GetAclRecordsAsync(topic))
            {
                var customerRole = await _customerService.GetCustomerRoleByIdAsync(aclRecord.CustomerRoleId);

                subjectToAcl += _catalogSettings.ExportImportRelatedEntitiesByName ? customerRole.Name : customerRole.Id.ToString();

                subjectToAcl += ";";
            }

            return subjectToAcl;
        }

        protected virtual async Task<bool> IgnoreExportTopicPropertyAsync()
        {
            try
            {
                return !await _genericAttributeService.GetAttributeAsync<bool>(await _workContext.GetCurrentCustomerAsync(), "topic-advanced-mode");
            }
            catch (ArgumentNullException)
            {
                return false;
            }
        }

        protected virtual async Task<bool> IgnoreExportLimitedToStoreAsync()
        {
            return _catalogSettings.IgnoreStoreLimitations ||
                   (await _storeService.GetAllStoresAsync()).Count == 1;
        }

        protected virtual async Task ImportLocalizedAsync(Topic topic, WorkbookMetadata<Topic> metadata, PropertyManager<Topic, Language> manager, int iRow, IList<Language> languages)
        {
            if (!metadata.LocalizedWorksheets.Any())
                return;

            var setSeName = metadata.LocalizedProperties.Any(p => p.PropertyName == "SeName");
            foreach (var language in languages)
            {
                var lWorksheet = metadata.LocalizedWorksheets.FirstOrDefault(ws => ws.Name.Equals(language.UniqueSeoCode, StringComparison.InvariantCultureIgnoreCase));
                if (lWorksheet == null)
                    continue;

                manager.CurrentLanguage = language;
                manager.ReadLocalizedFromXlsx(lWorksheet, iRow);

                foreach (var property in manager.GetLocalizedProperties)
                {
                    string localizedName = null;

                    switch (property.PropertyName)
                    {
                        case "Title":
                            localizedName = property.StringValue;
                            await _localizedEntityService.SaveLocalizedValueAsync(topic, m => m.Title, localizedName, language.Id);
                            break;
                        case "Body":
                            await _localizedEntityService.SaveLocalizedValueAsync(topic, m => m.Body, property.StringValue, language.Id);
                            break;
                        case "MetaKeywords":
                            await _localizedEntityService.SaveLocalizedValueAsync(topic, m => m.MetaKeywords, property.StringValue, language.Id);
                            break;
                        case "MetaDescription":
                            await _localizedEntityService.SaveLocalizedValueAsync(topic, m => m.MetaDescription, property.StringValue, language.Id);
                            break;
                        case "MetaTitle":
                            await _localizedEntityService.SaveLocalizedValueAsync(topic, m => m.MetaTitle, property.StringValue, language.Id);
                            break;
                        case "SeName":
                            //search engine name
                            if (setSeName)
                            {
                                var localizedSeName = await _urlRecordService.ValidateSeNameAsync(topic, property.StringValue, localizedName, false);
                                await _urlRecordService.SaveSlugAsync(topic, localizedSeName, language.Id);
                            }
                            break;
                    }
                }
            }
        }

        #endregion

        #region Methods

        public virtual async Task<byte[]> ExportToXlsxAsync(IList<Topic> topics)
        {
            var languages = await _languageService.GetAllLanguagesAsync(showHidden: true);

            var localizedProperties = new[]
            {
                new PropertyByName<Topic, Language>("Id", (p, l) => p.Id),
                new PropertyByName<Topic, Language>("Body", async (p, l) => await _localizationService.GetLocalizedAsync(p, x => x.Body, l.Id, false)),
                new PropertyByName<Topic, Language>("Title", async (p, l) => await _localizationService.GetLocalizedAsync(p, x => x.Title, l.Id, false)),
                new PropertyByName<Topic, Language>("MetaKeywords", async (p, l) => await _localizationService.GetLocalizedAsync(p, x => x.MetaKeywords, l.Id, false)),
                new PropertyByName<Topic, Language>("MetaDescription", async (p, l) => await _localizationService.GetLocalizedAsync(p, x => x.MetaDescription, l.Id, false)),
                new PropertyByName<Topic, Language>("MetaTitle", async (p, l) => await _localizationService.GetLocalizedAsync(p, x => x.MetaTitle, l.Id, false)),
                new PropertyByName<Topic, Language>("SeName", async (p, l) => await _urlRecordService.GetSeNameAsync(p, l.Id, returnDefaultValue: false), await IgnoreExportLimitedToStoreAsync())
            };

            //property manager 
            var manager = new PropertyManager<Topic, Language>(new[]
            {
                new PropertyByName<Topic, Language>("Id", (p, l) => p.Id),
                new PropertyByName<Topic, Language>("SystemName", (p, l) => p.SystemName, await IgnoreExportTopicPropertyAsync()),
                new PropertyByName<Topic, Language>("IncludeInSitemap", (p, l) => p.IncludeInSitemap, await IgnoreExportTopicPropertyAsync()),
                new PropertyByName<Topic, Language>("IncludeInTopMenu", (p, l) => p.IncludeInTopMenu),
                new PropertyByName<Topic, Language>("IncludeInFooterColumn1", (p, l) => p.IncludeInFooterColumn1),
                new PropertyByName<Topic, Language>("IncludeInFooterColumn2", (p, l) => p.IncludeInFooterColumn2),
                new PropertyByName<Topic, Language>("IncludeInFooterColumn3", (p, l) => p.IncludeInFooterColumn3),
                new PropertyByName<Topic, Language>("Published", (p, l) => p.Published),
                new PropertyByName<Topic, Language>("DisplayOrder", (p, l) => p.DisplayOrder),
                new PropertyByName<Topic, Language>("AccessibleWhenStoreClosed", (p, l) => p.AccessibleWhenStoreClosed, await IgnoreExportTopicPropertyAsync()),
                new PropertyByName<Topic, Language>("IsPasswordProtected", (p, l) => p.IsPasswordProtected, await IgnoreExportTopicPropertyAsync()),
                new PropertyByName<Topic, Language>("Password", (p, l) => p.Password, await IgnoreExportTopicPropertyAsync()),
                new PropertyByName<Topic, Language>("Title", (p, l) => p.Title),
                new PropertyByName<Topic, Language>("Body", (p, l) => p.Body),
                new PropertyByName<Topic, Language>("TopicTemplateId", (p, l) => p.TopicTemplateId, await IgnoreExportTopicPropertyAsync()),
                new PropertyByName<Topic, Language>("MetaKeywords", (p, l) => p.MetaKeywords, await IgnoreExportTopicPropertyAsync()),
                new PropertyByName<Topic, Language>("MetaDescription", (p, l) => p.MetaDescription, await IgnoreExportTopicPropertyAsync()),
                new PropertyByName<Topic, Language>("MetaTitle", (p, l) => p.MetaTitle, await IgnoreExportTopicPropertyAsync()),
                new PropertyByName<Topic, Language>("SubjectToAcl", async (p, l) => await GetSubjectToAclAsync(p), _catalogSettings.IgnoreAcl),
                new PropertyByName<Topic, Language>("IsSubjectToAcl", (p, l) => p.SubjectToAcl, _catalogSettings.IgnoreAcl),
                new PropertyByName<Topic, Language>("IsLimitedToStores", (p, l) => p.LimitedToStores, await IgnoreExportLimitedToStoreAsync()),
                new PropertyByName<Topic, Language>("LimitedToStores", async (p, l) =>  await GetLimitedToStoresAsync(p), await IgnoreExportLimitedToStoreAsync()),
            }, _catalogSettings, localizedProperties, languages);

            return await manager.ExportToXlsxAsync(topics);
        }

        public virtual async Task ImportFromXlsxAsync(Stream stream)
        {
            using var workbook = new XLWorkbook(stream);
            // get the first worksheet in the workbook
            var worksheet = workbook.Worksheets.FirstOrDefault();
            if (worksheet == null)
                throw new NopException("No worksheet found");

            var languages = await _languageService.GetAllLanguagesAsync(showHidden: true);

            //the columns
            var metadata = ImportManager.GetWorkbookMetadata<Topic>(workbook, languages);
            var defaultWorksheet = metadata.DefaultWorksheet;
            var defaultProperties = metadata.DefaultProperties;
            var localizedProperties = metadata.LocalizedProperties;

            var manager = new PropertyManager<Topic, Language>(defaultProperties, _catalogSettings, localizedProperties, languages);

            var tempProperty = manager.GetDefaultProperty("SystemName");
            var systemNameCellNum = tempProperty?.PropertyOrderPosition ?? -1;

            //performance optimization, load all stores in one SQL request
            var allStores = await _storeService.GetAllStoresAsync();

            //performance optimization, load all customer roles in one SQL request
            var allCustomerRoles = await _customerService.GetAllCustomerRolesAsync(true);

            var iRow = 2;
            var setSeName = defaultProperties.Any(p => p.PropertyName == "SeName");

            while (true)
            {
                var allColumnsAreEmpty = manager.GetDefaultProperties
                    .Select(property => worksheet.Row(iRow).Cell(property.PropertyOrderPosition))
                    .All(cell => cell?.Value == null || string.IsNullOrEmpty(cell.Value.ToString()));

                if (allColumnsAreEmpty)
                    break;

                manager.ReadDefaultFromXlsx(worksheet, iRow);

                var topic = systemNameCellNum > 0 ? await _topicService.GetTopicBySystemNameAsync(manager.GetDefaultProperty("SystemName").StringValue) : null;

                var isNew = topic == null;
                topic ??= new Topic();

                var seName = string.Empty;

                foreach (var property in manager.GetDefaultProperties)
                {
                    switch (property.PropertyName)
                    {
                        case "SystemName":
                            topic.SystemName = property.StringValue;
                            break;
                        case "IncludeInSitemap":
                            topic.IncludeInSitemap = property.BooleanValue;
                            break;
                        case "IncludeInTopMenu":
                            topic.IncludeInTopMenu = property.BooleanValue;
                            break;
                        case "IncludeInFooterColumn1":
                            topic.IncludeInFooterColumn1 = property.BooleanValue;
                            break;
                        case "IncludeInFooterColumn2":
                            topic.IncludeInFooterColumn2 = property.BooleanValue;
                            break;
                        case "IncludeInFooterColumn3":
                            topic.IncludeInFooterColumn3 = property.BooleanValue;
                            break;
                        case "Published":
                            topic.Published = property.BooleanValue;
                            break;
                        case "DisplayOrder":
                            topic.DisplayOrder = property.IntValue;
                            break;
                        case "AccessibleWhenStoreClosed":
                            topic.AccessibleWhenStoreClosed = property.BooleanValue;
                            break;
                        case "IsPasswordProtected":
                            topic.IsPasswordProtected = property.BooleanValue;
                            break;
                        case "Password":
                            topic.Password = property.StringValue;
                            break;
                        case "Title":
                            topic.Title = property.StringValue;
                            break;
                        case "Body":
                            topic.Body = property.StringValue;
                            break;
                        case "TopicTemplateId":
                            topic.TopicTemplateId = property.IntValue;
                            break;
                        case "MetaKeywords":
                            topic.MetaKeywords = property.StringValue;
                            break;
                        case "MetaDescription":
                            topic.MetaDescription = property.StringValue;
                            break;
                        case "MetaTitle":
                            topic.MetaTitle = property.StringValue;
                            break;
                        case "IsLimitedToStores":
                            topic.LimitedToStores = property.BooleanValue;
                            break;
                        case "IsSubjectToAcl":
                            topic.SubjectToAcl = property.BooleanValue;
                            break;
                        case "SeName":
                            seName = property.StringValue;
                            break;
                    }
                }

                if (isNew)
                    await _topicService.InsertTopicAsync(topic);
                else
                    await _topicService.UpdateTopicAsync(topic);

                //search engine name
                if (setSeName)
                    await _urlRecordService.SaveSlugAsync(topic, await _urlRecordService.ValidateSeNameAsync(topic, seName, topic.Title, true), 0);

                tempProperty = manager.GetDefaultProperty("LimitedToStores");
                if (tempProperty != null)
                {
                    var limitedToStoresList = tempProperty.StringValue;

                    var importedStores = topic.LimitedToStores ? limitedToStoresList.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => allStores.FirstOrDefault(store => store.Name == x.Trim())?.Id ?? int.Parse(x.Trim())).ToList() : new List<int>();

                    topic.LimitedToStores = importedStores.Any();
                    await _topicService.UpdateTopicAsync(topic);

                    var existingStoreMappings = await _storeMappingService.GetStoreMappingsAsync(topic);
                    foreach (var store in allStores)
                    {
                        if (importedStores.Contains(store.Id))
                        {
                            //new store
                            if (!existingStoreMappings.Any(sm => sm.StoreId == store.Id))
                                await _storeMappingService.InsertStoreMappingAsync(topic, store.Id);
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

                tempProperty = manager.GetDefaultProperty("SubjectToAcl");
                if (tempProperty != null)
                {
                    var subjectToAclList = tempProperty.StringValue;

                    var importedRoles = topic.SubjectToAcl ? subjectToAclList.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => allStores.FirstOrDefault(store => store.Name == x.Trim())?.Id ?? int.Parse(x.Trim())).ToList() : new List<int>();

                    topic.SubjectToAcl = importedRoles.Any();
                    await _topicService.UpdateTopicAsync(topic);

                    var existingAclRecords = await _aclService.GetAclRecordsAsync(topic);
                    foreach (var customerRole in allCustomerRoles)
                    {
                        if (importedRoles.Contains(customerRole.Id))
                        {
                            //new role
                            if (!existingAclRecords.Any(acl => acl.CustomerRoleId == customerRole.Id))
                                await _aclService.InsertAclRecordAsync(topic, customerRole.Id);
                        }
                        else
                        {
                            //remove role
                            var aclRecordToDelete = existingAclRecords.FirstOrDefault(acl => acl.CustomerRoleId == customerRole.Id);
                            if (aclRecordToDelete != null)
                                await _aclService.DeleteAclRecordAsync(aclRecordToDelete);
                        }
                    }
                }

                //save manufacturer localized data
                await ImportLocalizedAsync(topic, metadata, manager, iRow, languages);

                iRow++;
            }
        }

        #endregion
    }
}
