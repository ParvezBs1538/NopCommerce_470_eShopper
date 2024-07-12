using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.Misc.AjaxFilter.Areas.Admin.Services;
using NopStation.Plugin.Misc.AjaxFilter.Components;
using NopStation.Plugin.Misc.AjaxFilter.Domains;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Services;

namespace NopStation.Plugin.Misc.AjaxFilter
{
    public class AjaxFilterPlugin : BasePlugin, IWidgetPlugin, IAdminMenuPlugin, INopStationPlugin
    {
        #region Fields

        private readonly ISettingService _settingService;
        private readonly INopFileProvider _nopFileProvider;
        private readonly INopDataProvider _nopDataProvider;
        private readonly INopStationCoreService _nopStationCoreService;
        private readonly IWebHelper _webHelper;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly IAjaxFilterParentCategoryService _ajaxFilterParentCategoryService;
        private readonly ICategoryService _categoryService;
        #endregion

        #region Ctor

        public AjaxFilterPlugin(
            INopFileProvider nopFileProvider,
            INopDataProvider nopDataProvider,
            INopStationCoreService nopStationCoreService,
            ISettingService settingService,
            IWebHelper webHelper,
            ILocalizationService localizationService,
            IAjaxFilterParentCategoryService ajaxFilterParentCategoryService,
            IPermissionService permissionProvider,
            ICategoryService categoryService)
        {
            _nopFileProvider = nopFileProvider;
            _nopDataProvider = nopDataProvider;
            _nopStationCoreService = nopStationCoreService;
            _settingService = settingService;
            _webHelper = webHelper;
            _localizationService = localizationService;
            _permissionService = permissionProvider;
            _ajaxFilterParentCategoryService = ajaxFilterParentCategoryService;
            _categoryService = categoryService;
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
            {
                using (var reader = new StreamReader(stream))
                {
                    var statement = "";
                    while ((statement = ReadNextStatementFromStream(reader)) != null)
                    {
                        statements.Add(statement);
                    }
                }
            }

            foreach (var stmt in statements)
            {
                await _nopDataProvider.ExecuteNonQueryAsync(stmt);
            }
        }

        #endregion

        public override async Task InstallAsync()
        {
            await this.InstallPluginAsync(new AjaxFilterPermissionProvider());

            var dataSettings = DataSettingsManager.LoadSettings();
            if (dataSettings != null)
            {
                if (dataSettings.DataProvider == DataProviderType.SqlServer)
                {
                    await ExecuteSqlFileAsync(_nopFileProvider.Combine(_nopFileProvider.MapPath("/Plugins/NopStation.Plugin.Misc.AjaxFilter/Install/"), "MSSQL_nop_splitstring_to_table.sql"));
                    await ExecuteSqlFileAsync(_nopFileProvider.Combine(_nopFileProvider.MapPath("/Plugins/NopStation.Plugin.Misc.AjaxFilter/Install/"), "MSSQL_AjaxFilter_StoredProcedure.sql"));
                    await ExecuteSqlFileAsync(_nopFileProvider.Combine(_nopFileProvider.MapPath("/Plugins/NopStation.Plugin.Misc.AjaxFilter/Install/"), "MSSQL_AjaxFilterProduct_StoredProcedure.sql"));
                }
                if (dataSettings.DataProvider == DataProviderType.MySql)
                {
                    await ExecuteSqlFileAsync(_nopFileProvider.Combine(_nopFileProvider.MapPath("/Plugins/NopStation.Plugin.Misc.AjaxFilter/Install/"), "MySQL_nop_splitstring_to_table.sql"));
                    await ExecuteSqlFileAsync(_nopFileProvider.Combine(_nopFileProvider.MapPath("/Plugins/NopStation.Plugin.Misc.AjaxFilter/Install/"), "MySQL_AjaxFilter_StoredProcedure.sql"));
                    await ExecuteSqlFileAsync(_nopFileProvider.Combine(_nopFileProvider.MapPath("/Plugins/NopStation.Plugin.Misc.AjaxFilter/Install/"), "MySQL_AjaxFilterProduct_StoredProcedure.sql"));
                }
            }

            var categories = await _categoryService.GetAllCategoriesAsync();
            foreach (var category in categories)
            {
                var ajaxfilterCategory = new AjaxFilterParentCategory()
                {
                    CategoryId = category.Id,
                    EnableManufactureFiltering = true,
                    EnablePriceRangeFiltering = true,
                    EnableSearchForManufacturers = true,
                    EnableSearchForSpecifications = true,
                    EnableSpecificationAttributeFiltering = true,
                    EnableVendorFiltering = false
                };
                await _ajaxFilterParentCategoryService.InsertParentCategoryAsync(ajaxfilterCategory);
            }

            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await this.UninstallPluginAsync(new AjaxFilterPermissionProvider());
            var dataSettings = DataSettingsManager.LoadSettings();
            await _nopDataProvider.ExecuteNonQueryAsync(@"DROP PROCEDURE AjaxFilterProduct");
            await _nopDataProvider.ExecuteNonQueryAsync(@"DROP PROCEDURE AjaxFilter");
            if (dataSettings.DataProvider == DataProviderType.SqlServer)
            {
                await _nopDataProvider.ExecuteNonQueryAsync(@"DROP FUNCTION nop_splitstring_to_table");
            }
            if (dataSettings.DataProvider == DataProviderType.MySql)
            {
                await _nopDataProvider.ExecuteNonQueryAsync(@"DROP PROCEDURE nop_splitstring_to_table");
            }
            await base.UninstallAsync();
        }

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/AjaxFilter/Configure";
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            var resourceList = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AjaxFilter.Configuration","Ajax filter configuration"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AjaxFilter.Configuration.Blocktitle.SpecificationAttribute","Specification attribute"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AjaxFilter.Resources.AddFromExistingSpecificationAttributes","Add from existing specification attribute"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AjaxFilter.Resources.AddFromExistingCategories","Add from existing categories"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AjaxFilter.CategoryList","AjaxFilter category list"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AjaxFilter.Menu.Ajaxfilter","AjaxFilter"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AjaxFilter.Menu.List","List"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AjaxFilter.Menu.Configuration","Configuration"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AjaxFilter.Menu.Documentation","Documentation"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AjaxFilter.Configuration.Fields.EnableFilter", "Enable filter"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AjaxFilter.Configuration.Fields.EnableFilter.Hint", "Enable AjaxFilter in catalogue page to filter products"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AjaxFilter.Configuration.Fields.EnableProductCount", "Enable product count"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AjaxFilter.Configuration.Fields.EnableProductCount.Hint", "Enables product count while filtering"),

                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AjaxFilter.Configuration.Fields.EnableProductAttributeFilter", "Enable product attribute filter"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AjaxFilter.Configuration.Fields.EnableProductAttributeFilter.Hint", "Consider filtering product attributes too"),

                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AjaxFilter.Configuration.Fields.CloseProductAttributeFilterByDefualt", "Close product attribute filter by default"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AjaxFilter.Configuration.Fields.CloseProductAttributeFilterByDefualt.Hint", "Control for this filter will be closed by default"),

                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AjaxFilter.Configuration.Fields.HideManufacturerProductCount", "Hide manufacturer product count"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AjaxFilter.Configuration.Fields.HideManufacturerProductCount.Hint", "Hides manufacturer count while filtering"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AjaxFilter.Configuration.Fields.MaxDisplayForCategories", "Max categories to display"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AjaxFilter.Configuration.Fields.MaxDisplayForCategories.Hint", "Max categories number to display filters"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AjaxFilter.Configuration.Fields.MaxDisplayForManufacturers", "Max manufacturer to display"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AjaxFilter.Configuration.Fields.MaxDisplayForManufacturers.Hint", "Max manufacturer number to display filters"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AjaxFilter.Configuration.Fields.MaxDisplayForSpecificationAttributes", "Max specification attribute to display"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AjaxFilter.Configuration.Fields.MaxDisplayForSpecificationAttributes.Hint", "Max specification attribute number to display"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AjaxFilter.Configuration.Fields.EnablePriceRangeFilter", "Enable price range filter"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AjaxFilter.Configuration.Fields.EnablePriceRangeFilter.Hint", "Enable filtering with price range for products"),

                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AjaxFilter.Configuration.Fields.ClosePriceRangeFilterByDefualt", "Close price range filter by default"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AjaxFilter.Configuration.Fields.ClosePriceRangeFilterByDefualt.Hint", "Control for this filter will be closed by default"),

                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AjaxFilter.Configuration.Fields.EnableSpecificationAttributeFilter", "Enable specification attribute filter"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AjaxFilter.Configuration.Fields.EnableSpecificationAttributeFilter.Hint", "Considers specification attributes to be filtered"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AjaxFilter.Configuration.Fields.CloseSpecificationAttributesFilterByDefualt", "Close specification filter by default"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AjaxFilter.Configuration.Fields.SearchSpecificationAttributeId", "Search specification attribute"),

                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AjaxFilter.Configuration.Fields.EnableManufacturerFilter", "Enable manufacturer filter"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AjaxFilter.Configuration.Fields.EnableManufacturerFilter.Hint", "Enables manufacturer to be filtered"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AjaxFilter.Configuration.Fields.CloseManufactureFilterByDefualt", "Close manufacturer filter by default"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AjaxFilter.Configuration.Fields.CloseManufactureFilterByDefualt.Hint", "Control for this filter will be closed by default"),

                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AjaxFilter.Configuration.Fields.NotPositive", "Not positive value"),

                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AjaxFilter.Configuration.Fields.MaxSpecificationAttributesToDisplay", "Max specification attributes to display"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AjaxFilter.Configuration.Fields.MaxSpecificationAttributesToDisplay.Hint", "Max specification attributes that will display"),

                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AjaxFilter.Configuration.Fields.CloseSpecificationAttributeByDefault", "Close specification attribute by default"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AjaxFilter.Configuration.Fields.CloseSpecificationAttributeByDefault.Hint", "Specification attribute control will be closed by default"),

                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AjaxFilter.Configuration.Fields.HideProductCount", "Hide product count"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AjaxFilter.Configuration.Fields.HideProductCount.Hint", "Product counts for this specification attribute will be hidden"),

                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AjaxFilter.Configuration.Fields.EnableProductRatingsFilter", "Enable product ratings filter"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AjaxFilter.Configuration.Fields.EnableProductRatingsFilter.Hint", "Enables filtering with product rating review."),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AjaxFilter.Configuration.Fields.CloseProductRatingsFilterByDefualt", "Close product ratings filter by default"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AjaxFilter.Configuration.Fields.CloseProductRatingsFilterByDefualt.Hint", "Control for this filter will be closed by default"),

                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AjaxFilter.Configuration.Fields.EnableProductTagsFilter", "Enable product tags filter"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AjaxFilter.Configuration.Fields.EnableProductTagsFilter.Hint", "Enable filtering with product tags"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AjaxFilter.Configuration.Fields.CloseProductTagsFilterByDefualt", "Close product tags filter by default"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AjaxFilter.Configuration.Fields.CloseProductTagsFilterByDefualt.Hint", "Control for this filter will be closed by default"),


                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AjaxFilter.Configuration.Fields.EnableStockFilter", "Enable stock filter"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AjaxFilter.Configuration.Fields.CloseProductStockFilterByDefualt", "Close product stock filter by default"),

                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AjaxFilter.Configuration.Fields.EnableMiscFilter", "Enable miscellaneous filter"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AjaxFilter.Configuration.Fields.EnableMiscFilter.Hint", "Enable miscellaneous filters(On sale,Free shipping, Tax excempt, New product)"),

                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AjaxFilter.Configuration.Fields.CloseMiscFilterByDefualt", "Close miscellaneous filter by default"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AjaxFilter.Configuration.Fields.CloseMiscFilterByDefualt.Hint", "Control for this filter will be closed by default"),

                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AjaxFilter.Parentcategory.AddFromExistingCategories","Add from existing categories"),
                new KeyValuePair<string, string>("Admin.Configuration.Parentcategories.Fields.Name","Categories"),
                new KeyValuePair<string, string>("Admin.Configuration.SpecificationAttributes.Fields.Name","Specification attributes"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AjaxFilter.Configuration.Fields.EnableCategoryFilter", "Enable category filter"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AjaxFilter.Configuration.Fields.CloseCategoryFilterByDefualt", "Close category filter by default"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AjaxFilter.BackToList","Back to list"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AjaxFilter.SearchParentCategoryName","Search parent category name"),

                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AjaxFilter.SearchSpecificationAttributeName","Search specification attribute"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AjaxFilter.SearchSpecificationAttributeName.Hint","Search specification by its name"),

                new KeyValuePair<string, string>("NopStation.Plugin.Misc.AjaxFilter.Filters.Pricerange", "Price range"),
                new KeyValuePair<string, string>("NopStation.Plugin.Misc.AjaxFilter.Filters.Stock", "Stock"),
                new KeyValuePair<string, string>("NopStation.Plugin.Misc.AjaxFilter.Filters.ProductTags", "Product tags"),
                new KeyValuePair<string, string>("NopStation.Plugin.Misc.AjaxFilter.Filters.ProductRating", "Product rating"),
                new KeyValuePair<string, string>("NopStation.Plugin.Misc.AjaxFilter.Filters.ProductRating.Up", " & Up"),
                new KeyValuePair<string, string>("NopStation.Plugin.Misc.AjaxFilter.Filters.Product", "Product"),
                new KeyValuePair<string, string>("NopStation.Plugin.Misc.AjaxFilter.Filters.Product.NewProduct", "New product"),
                new KeyValuePair<string, string>("NopStation.Plugin.Misc.AjaxFilter.Filters.Product.TaxExcempt", "Tax excempt"),
                new KeyValuePair<string, string>("NopStation.Plugin.Misc.AjaxFilter.Filters.Product.FreeShipping", "Free shipping"),
                new KeyValuePair<string, string>("NopStation.Plugin.Misc.AjaxFilter.Filters.ProductFilter", "Filter Products"),
                new KeyValuePair<string, string>("NopStation.Plugin.Misc.AjaxFilter.Filters.Miscellaneous", "Miscellaneous"),
                new KeyValuePair<string, string>("NopStation.Plugin.Misc.AjaxFilter.Filters.Clear", "Clear"),
                new KeyValuePair<string, string>("NopStation.Plugin.Misc.AjaxFilter.Filters.NoResults", "No products found"),
                new KeyValuePair<string, string>("Nopstation.Ajaxfilter.Filters.PriceRange", "Price range"),
                new KeyValuePair<string, string>("NopStation.Plugin.Misc.AjaxFilter.Filters.Manufacturers", "Manufacturers"),
                new KeyValuePair<string, string>("NopStation.Plugin.Misc.AjaxFilter.Filters.Vendors", "Vendors"),
                new KeyValuePair<string, string>("NopStation.Plugin.Misc.AjaxFilter.Filters.CreatedOnAsc", "Created on (Old - New)"),
                new KeyValuePair<string, string>("NopStation.Plugin.Misc.AjaxFilter.Filters.CreatedOnDesc", "Created on (New - Old)"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AjaxFilter.Configuration.Specification","Specification attribute filter configuration - Ajax filter"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AjaxFilter.Configuration.Specification.BackToConfigure","Back to ajax filter configuration")
            };

            return resourceList;
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.Plugin.Misc.AjaxFilter.Menu.AjaxFilter"),
                Visible = true,
                IconClass = "far fa-dot-circle",
            };

            if (await _permissionService.AuthorizeAsync(AjaxFilterPermissionProvider.ManageAjaxFilter))
            {
                var configItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Plugin.Misc.AjaxFilter.Menu.Configuration"),
                    Url = "/Admin/AjaxFilter/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "AjaxFilter.Configuration"
                };
                menuItem.ChildNodes.Add(configItem);
            }

            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/ajax-filter-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=ajax-filter",
                    Visible = true,
                    IconClass = "far fa-circle",
                    OpenUrlInNewTab = true
                };
                menuItem.ChildNodes.Add(documentation);
            }

            await _nopStationCoreService.ManageSiteMapAsync(rootNode, menuItem, NopStationMenuType.Plugin);
        }

        public Task<IList<string>> GetWidgetZonesAsync()
        {
            return Task.FromResult<IList<string>>(new List<string> { PublicWidgetZones.LeftSideColumnBefore });
        }

        public Type GetWidgetViewComponent(string widgetZone)
        {
            return typeof(CatalogFiltersViewComponent);
        }

        public bool HideInWidgetList => false;
    }
}
