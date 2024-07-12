using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.ScheduleTasks;
using Nop.Data;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.ScheduleTasks;
using Nop.Services.Security;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.Misc.AlgoliaSearch.Extensions;
using NopStation.Plugin.Misc.AlgoliaSearch.Infrastructure;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Services;

namespace NopStation.Plugin.Misc.AlgoliaSearch
{
    public class AlgoliaSearchPlugin : BasePlugin, IAdminMenuPlugin, IMiscPlugin, INopStationPlugin
    {
        #region Fields

        private readonly IPermissionService _permissionService;
        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;
        private readonly IScheduleTaskService _scheduleTaskService;
        private readonly ISettingService _settingService;
        private readonly INopStationCoreService _nopStationCoreService;
        private readonly INopDataProvider _dataProvider;

        #endregion

        #region Ctor

        public AlgoliaSearchPlugin(IPermissionService permissionService,
            ILocalizationService localizationService,
            IWebHelper webHelper,
            IScheduleTaskService scheduleTaskService,
            ISettingService settingService,
            INopStationCoreService nopStationCoreService,
            INopDataProvider dataProvider)
        {
            _permissionService = permissionService;
            _localizationService = localizationService;
            _webHelper = webHelper;
            _scheduleTaskService = scheduleTaskService;
            _settingService = settingService;
            _nopStationCoreService = nopStationCoreService;
            _dataProvider = dataProvider;
        }


        #endregion

        #region Utilities

        protected virtual string ReadNextStatementFromStream(StreamReader reader)
        {
            var sb = new StringBuilder();
            while (true)
            {
                var lineOfText = reader.ReadLine();
                if (lineOfText == null)
                {
                    if (sb.Length > 0)
                    {
                        return sb.ToString();
                    }
                    return null;
                }
                if (lineOfText.TrimEnd(Array.Empty<char>()).ToUpper() == "GO")
                {
                    return sb.ToString();
                }
                sb.Append(lineOfText + Environment.NewLine);
            }
        }

        protected virtual async Task ExecuteSqlFileAsync(string path)
        {
            var statements = new List<string>();

            using (var stream = File.OpenRead(path))
            using (var reader = new StreamReader(stream))
            {
                var statement = "";
                while ((statement = ReadNextStatementFromStream(reader)) != null)
                    statements.Add(statement);
            }

            foreach (var stmt in statements)
                await _dataProvider.ExecuteNonQueryAsync(stmt);
        }

        #endregion

        #region Methods

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/AlgoliaSearch/Configure";
        }

        public override async Task InstallAsync()
        {
            var settings = new AlgoliaSearchSettings()
            {
                AllowCategoryFilter = true,
                AllowCustomersToSelectPageSize = true,
                AllowedSortingOptions = new List<int>()
                {
                    (int)AlgoliaSortingEnum.Position,
                    (int)AlgoliaSortingEnum.NameAsc,
                    (int)AlgoliaSortingEnum.NameDesc,
                    (int)AlgoliaSortingEnum.PriceAsc,
                    (int)AlgoliaSortingEnum.PriceDesc,
                    (int)AlgoliaSortingEnum.CreatedOn
                },
                AllowManufacturerFilter = true,
                AllowPriceRangeFilter = true,
                AllowRatingFilter = true,
                AllowSpecificationFilter = true,
                AllowProductSorting = true,
                AllowProductViewModeChanging = true,
                AllowVendorFilter = true,
                AutoCompleteListSize = 3,
                EnableAutoComplete = true,
                EnableMultilingualSearch = false,
                HidePoweredByAlgolia = false,
                MaximumCategoriesShowInFilter = 10,
                MaximumAttributesShowInFilter = 10,
                MaximumManufacturersShowInFilter = 10,
                MaximumSpecificationsShowInFilter = 10,
                MaximumVendorsShowInFilter = 10,
                ShowProductsCount = true,
                SearchPagePageSizeOptions = "6,12,18,30",
                AllowAttributeFilter = false,
                SearchPageProductsPerPage = 12,
                SearchTermMinimumLength = 3,
                ShowProductImagesInSearchAutoComplete = true,
                DefaultViewMode = "grid"
            };
            await _settingService.SaveSettingAsync(settings);

            await this.InstallPluginAsync(new AlgoliaSearchPermissionProvider());

            var task = await _scheduleTaskService.GetTaskByTypeAsync(AlgoliaDefaults.ScheduleTaskType);
            if (task == null)
            {
                await _scheduleTaskService.InsertTaskAsync(new ScheduleTask()
                {
                    Enabled = true,
                    Name = "Update algolia items",
                    Seconds = 3600,
                    Type = AlgoliaDefaults.ScheduleTaskType,
                    StopOnError = false
                });
            }

            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            if (await _scheduleTaskService.GetTaskByTypeAsync(AlgoliaDefaults.ScheduleTaskType) is ScheduleTask scheduleTask)
                await _scheduleTaskService.DeleteTaskAsync(scheduleTask);

            await this.UninstallPluginAsync(new AlgoliaSearchPermissionProvider());
            await base.UninstallAsync();
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menu = new SiteMapNode()
            {
                Visible = true,
                IconClass = "far fa-dot-circle",
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.AlgoliaSearch.Menu.AlgoliaSearch")
            };

            if (await _permissionService.AuthorizeAsync(AlgoliaSearchPermissionProvider.ManageConfiguration))
            {
                var settings = new SiteMapNode()
                {
                    Visible = true,
                    IconClass = "far fa-circle",
                    Url = "~/Admin/AlgoliaSearch/Configure",
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.AlgoliaSearch.Menu.Configuration"),
                    SystemName = "AlgoliaSearch.Configuration"
                };
                menu.ChildNodes.Add(settings);
            }

            if (await _permissionService.AuthorizeAsync(AlgoliaSearchPermissionProvider.ManageUploadProducts))
            {
                var updatableItems = new SiteMapNode()
                {
                    Visible = true,
                    IconClass = "far fa-circle",
                    Url = "~/Admin/AlgoliaSearch/UpdatableItem",
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.AlgoliaSearch.Menu.UpdatableItems"),
                    SystemName = "AlgoliaSearch.UpdatableItems"
                };
                menu.ChildNodes.Add(updatableItems);
                var uploadProduct = new SiteMapNode()
                {
                    Visible = true,
                    IconClass = "far fa-circle",
                    Url = "~/Admin/AlgoliaSearch/UploadProduct",
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.AlgoliaSearch.Menu.UploadProducts"),
                    SystemName = "AlgoliaSearch.UploadProducts"
                };
                menu.ChildNodes.Add(uploadProduct);
            }
            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/algolia-search-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=algolia-search",
                    Visible = true,
                    IconClass = "far fa-circle",
                    OpenUrlInNewTab = true
                };
                menu.ChildNodes.Add(documentation);
            }

            await _nopStationCoreService.ManageSiteMapAsync(rootNode, menu, NopStationMenuType.Plugin);
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            var list = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.Fields.EnableAutoComplete", "Enabled auto complete"),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.Fields.EnableAutoComplete.Hint", "Check to enable auto complete in search box."),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.Fields.AutoCompleteListSize", "Auto complete list size"),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.Fields.AutoCompleteListSize.Hint", "Enter the number of products that will be showm in auto complete list."),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.Fields.ShowProductImagesInSearchAutoComplete", "Show product images in search auto complete"),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.Fields.ShowProductImagesInSearchAutoComplete.Hint", "Check to show product images in search auto complete."),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.Fields.ApplicationId", "Application id"),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.Fields.ApplicationId.Hint", "The algolia application id. Click 'Update index' if the value of this property is changed."),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.Fields.SearchOnlyKey", "Search only key"),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.Fields.SearchOnlyKey.Hint", "The algolia search only key. Click 'Update index' if the value of this property is changed."),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.Fields.AdminKey", "Admin key"),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.Fields.AdminKey.Hint", "The algolia admin key. Click 'Update index' if the value of this property is changed."),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.Fields.MonitoringKey", "Monitoring key"),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.Fields.MonitoringKey.Hint", "The algolia aonitoring key. Click 'Update index' if the value of this property is changed."),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.Fields.UsageKey", "Usage key"),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.Fields.UsageKey.Hint", "The algolia usage key. Click 'Update index' if the value of this property is changed."),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.Fields.SearchTermMinimumLength", "Search term minimum length"),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.Fields.SearchTermMinimumLength.Hint", "The search term minimum query length."),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.Fields.AllowCustomersToSelectPageSize", "Allow customers to select page size"),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.Fields.AllowCustomersToSelectPageSize.Hint", "Check to allow customers to select page size."),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.Fields.SearchPagePageSizeOptions", "Search page age size options"),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.Fields.SearchPagePageSizeOptions.Hint", "The search page page size options."),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.Fields.SearchPageProductsPerPage", "Search page products per page"),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.Fields.SearchPageProductsPerPage.Hint", "The page size of search page."),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.Fields.AllowProductViewModeChanging", "Allow product view mode changing"),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.Fields.AllowProductViewModeChanging.Hint", "Check to allow customers to change product view mode."),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.Fields.DefaultViewMode", "Default view mode"),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.Fields.DefaultViewMode.Hint", "Select search page default product view mode."),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.Fields.AllowCategoryFilter", "Allow category filter"),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.Fields.AllowCategoryFilter.Hint", "Check to allow customers to filter by product categories."),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.Fields.MaximumCategoriesShowInFilter", "Maximum categories show in filter"),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.Fields.MaximumCategoriesShowInFilter.Hint", "Maximum number of categories show in filter."),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.Fields.AllowVendorFilter", "Allow vendor filter"),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.Fields.AllowVendorFilter.Hint", "Check to allow customers to filter by product vendors."),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.Fields.MaximumVendorsShowInFilter", "Maximum vendors show in filter"),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.Fields.MaximumVendorsShowInFilter.Hint", "Maximum number of vendors show in filter."),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.Fields.AllowManufacturerFilter", "Allow manufacturer filter"),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.Fields.AllowManufacturerFilter.Hint", "Check to allow customers to filter by product manufacturers."),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.Fields.MaximumManufacturersShowInFilter", "Maximum manufacturers show in filter"),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.Fields.MaximumManufacturersShowInFilter.Hint", "Maximum number of manufacturers show in filter."),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.Fields.AllowSpecificationFilter", "Allow specification filter"),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.Fields.AllowSpecificationFilter.Hint", "Check to allow customers to filter by product specifications."),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.Fields.MaximumSpecificationsShowInFilter", "Maximum specifications show in filter"),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.Fields.MaximumSpecificationsShowInFilter.Hint", "Maximum number of specifications show in filter."),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.Fields.AllowAttributeFilter", "Allow attribute filter"),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.Fields.AllowAttributeFilter.Hint", "Check to allow customers to filter by product attributes."),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.Fields.MaximumAttributesShowInFilter", "Maximum attributes show in filter"),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.Fields.MaximumAttributesShowInFilter.Hint", "Maximum number of attributes show in filter."),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.Fields.AllowProductSorting", "Allow product sorting"),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.Fields.AllowProductSorting.Hint", "Check to allow customers to sort products. Click 'Update index' if the value of this property is changed."),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.Fields.AllowedSortingOptions", "Allowed sorting options"),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.Fields.AllowedSortingOptions.Hint", "Allowed sorting options. Click 'Update index' if the value of this property is changed."),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.Fields.AllowPriceRangeFilter", "Allow price range filter"),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.Fields.AllowPriceRangeFilter.Hint", "Check to allow customers to filter by product price range."),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.Fields.AllowRatingFilter", "Allow rating filter"),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.Fields.AllowRatingFilter.Hint", "Check to allow customers to filter by product rating."),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.Fields.ShowProductsCount", "Show products count"),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.Fields.ShowProductsCount.Hint", "Check to show products count on product search page."),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.Fields.HidePoweredByAlgolia", "Hide powered by algolia"),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.Fields.HidePoweredByAlgolia.Hint", "Check to hide powered by algolia from search result footer. Please don't hide it when you are using community plan."),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.Fields.EnableMultilingualSearch", "Enable Multilingual Search"),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.Fields.EnableMultilingualSearch.Hint", "Enable this if you want to search on different languages."),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.Fields.EnableMultilingualSearch.Warnings", "If you enable this make sure your languages are supported by the algolia. For more informatio <a href=\"https://www.algolia.com/doc/guides/managing-results/optimize-search-results/handling-natural-languages-nlp/in-depth/supported-languages/\" >click Here.</a>"),

                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.Fields.ApplicationId.Required", "The 'Application id' is required."),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.Fields.SearchOnlyKey.Required", "The 'Search only key' is required."),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.Fields.AdminKey.Required", "The 'Admin key' is required."),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.Fields.UsageKey.Required", "The 'Usage key' is required."),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.Fields.MonitoringKey.Required", "The 'Monitoring key' is required."),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.Fields.MinimumQueryLength.GreaterThanZero", "'Minimum query length' must be greater than '0'."),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.Fields.DefaultViewMode.Required", "The 'Default view mode' is required."),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.Fields.SearchPagePageSizeOptions.Required", "The 'Search page page size options' is required."),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.Fields.SearchPagePageSizeOptions.InvalidPageSizeOptions", "Invalid page size options."),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.Fields.SearchPageProductsPerPage.GreaterThanZero", "'Page size' must be greater than '0'."),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.Fields.MaximumCategoriesShowInFilter.GreaterThanZero", "'Maximum categories show in filter' must be greater than '0'."),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.Fields.MaximumVendorsShowInFilter.GreaterThanZero", "'Maximum vendors show in filter' must be greater than '0'."),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.Fields.MaximumManufacturersShowInFilter.GreaterThanZero", "'Maximum manufacturers show in filter' must be greater than '0'."),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.Fields.MaximumSpecificationsShowInFilter.GreaterThanZero", "'Maximum specifications show in filter' must be greater than '0'."),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.Fields.AllowedSortOptions.Required", "The 'Allowed sorting options' is required."),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.Fields.AutoCompleteListSize.GreaterThanZero", "'Auto complete list size' must be greater than '0'."),

                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.UploadProduct.FromId", "From id"),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.UploadProduct.FromId.Hint", "The from product id for upload to algolia."),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.UploadProduct.ToId", "To id"),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.UploadProduct.ToId.Hint", "The to product id for upload to algolia."),

                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Menu.AlgoliaSearch", "Algolia search"),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Menu.Configuration", "Configuration"),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Menu.UpdatableItems", "Updatable items"),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Menu.UploadProducts", "Upload products"),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.Title", "Algolia settings"),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.IndexCleared", "Algolia index has been cleared successfully."),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.IndexUpdated", "Algolia index has been updated successfully."),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.ClearIndex", "Clear index"),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.UpdateIndex", "Update index"),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.Update", "Update"),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.UpdateIndexTitle", "Update algolia index settings"),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.ClearIndexWarning", "Are you sure want to clear algolia index?"),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.BlockTitle.Credential", "Algolia credential"),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.BlockTitle.Search", "Product search"),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.BlockTitle.Filter", "Product filter"),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.Configuration.AlgoliaIndexUpdateFailed", "Failed to update algolia index. Please check error message in system log."),

                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.UploadProduct.Title", "Upload products to algolia"),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.UploadProduct.Upload", "Upload"),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.UploadProduct.IndexCleared", "Algolia index has been cleared successfully."),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.UploadProduct.UploadCompleted", "Product upload has been completed."),

                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.UpdatableItem.Name", "Name"),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.UpdatableItem.UpdatedBy", "Updated by"),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.UpdatableItem.UpdatedOn", "Updated on"),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.UpdatableItem.Title", "Updatable items"),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.UpdatableItem.UpdateAll", "Update all"),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.UpdatableItem.BlockTitle.Product", "Product"),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.UpdatableItem.BlockTitle.Category", "Category"),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.UpdatableItem.BlockTitle.Manufacturer", "Manufacturer"),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.UpdatableItem.BlockTitle.Vendor", "Vendor"),

                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.UpdateIndices.ResetSearchableAttributeSettings", "Reset searchable attribute settings"),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.UpdateIndices.ResetSearchableAttributeSettings.Hint", "Reset searchable attribute settings of algolia index."),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.UpdateIndices.ResetFacetedAttributeSettings", "Reset faceted attribute settings"),
                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.UpdateIndices.ResetFacetedAttributeSettings.Hint", "Reset faceted attribute settings of algolia index."),

                new KeyValuePair<string, string>("Enums.NopStation.Plugin.Misc.AlgoliaSearch.Infrastructure.AlgoliaSortingEnum.CreatedOn", "Created on"),
                new KeyValuePair<string, string>("Enums.NopStation.Plugin.Misc.AlgoliaSearch.Infrastructure.AlgoliaSortingEnum.NameAsc", "Name: A to Z"),
                new KeyValuePair<string, string>("Enums.NopStation.Plugin.Misc.AlgoliaSearch.Infrastructure.AlgoliaSortingEnum.NameDesc", "Name: Z to A"),
                new KeyValuePair<string, string>("Enums.NopStation.Plugin.Misc.AlgoliaSearch.Infrastructure.AlgoliaSortingEnum.Position", "Position"),
                new KeyValuePair<string, string>("Enums.NopStation.Plugin.Misc.AlgoliaSearch.Infrastructure.AlgoliaSortingEnum.PriceAsc", "Price: Low to High"),
                new KeyValuePair<string, string>("Enums.NopStation.Plugin.Misc.AlgoliaSearch.Infrastructure.AlgoliaSortingEnum.PriceDesc", "Price: High to Low"),

                new KeyValuePair<string, string>("NopStation.AlgoliaSearch.Filterings.Categories", "Categories"),
                new KeyValuePair<string, string>("NopStation.AlgoliaSearch.Filterings.Manufacturers", "Manufacturers"),
                new KeyValuePair<string, string>("NopStation.AlgoliaSearch.Filterings.Vendors", "Vendors"),
                new KeyValuePair<string, string>("NopStation.AlgoliaSearch.Filterings.Ratings", "Ratings"),
                new KeyValuePair<string, string>("NopStation.AlgoliaSearch.Filterings.Price", "Price"),

                new KeyValuePair<string, string>("NopStation.AlgoliaSearch.Filterings.Ratings.OneStar", "One star"),
                new KeyValuePair<string, string>("NopStation.AlgoliaSearch.Filterings.Ratings.TwoStar", "Two star"),
                new KeyValuePair<string, string>("NopStation.AlgoliaSearch.Filterings.Ratings.ThreeStar", "Three star"),
                new KeyValuePair<string, string>("NopStation.AlgoliaSearch.Filterings.Ratings.FourStar", "Four star"),
                new KeyValuePair<string, string>("NopStation.AlgoliaSearch.Filterings.Ratings.FiveStar", "Five star"),

                new KeyValuePair<string, string>("Admin.NopStation.AlgoliaSearch.UploadProduct.UpdatedAllItems", "Items updated successfully."),
                new KeyValuePair<string, string>("NopStation.AlgoliaSearch.EnterSearchMinimumLength", "Enter minimum {0} character(s)."),
                new KeyValuePair<string, string>("NopStation.AlgoliaSearch.NoSearchResultFor", "No search result for")
            };

            return list;
        }

        #endregion
    }
}
