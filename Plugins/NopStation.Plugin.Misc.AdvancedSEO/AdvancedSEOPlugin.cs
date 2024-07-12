using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Services.Cms;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Services;

namespace NopStation.Plugin.Misc.AdvancedSEO
{
    public class AdvancedSEOPlugin : BasePlugin, IAdminMenuPlugin, INopStationPlugin
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly INopStationCoreService _nopStationCoreService;
        private readonly IPermissionService _permissionService;


        #endregion

        #region Ctor

        public AdvancedSEOPlugin(
            ILocalizationService localizationService,
            INopStationCoreService nopStationCoreService,
            IPermissionService permissionService
            )
        {
            _localizationService = localizationService;
            _nopStationCoreService = nopStationCoreService;
            _permissionService = permissionService;
        }

        #endregion

        #region Utilites



        #endregion

        #region Methods
        public override async Task InstallAsync()
        {
            await this.InstallPluginAsync(new AdvancedSEOPermissionProvider());

            await base.InstallAsync();
        }

        public override async Task UpdateAsync(string currentVersion, string targetVersion)
        {
            var keyValuePairs =  PluginResouces();
            foreach (var keyValuePair in keyValuePairs)
            {
                await _localizationService.AddOrUpdateLocaleResourceAsync(keyValuePair.Key, keyValuePair.Value);
            }
            await base.UpdateAsync(currentVersion, targetVersion);
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menu = new SiteMapNode()
            {
                Visible = true,
                IconClass = "far fa-dot-circle",
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.Plugin.Misc.AdvancedSEO.Menu.AdvancedSEO")
            };

            #region Configure

            //if (await _permissionService.AuthorizeAsync(AdvancedSEOPermissionProvider.ManageAdvancedSEOConfiguration))
            //{
            //    var dashboard = new SiteMapNode()
            //    {
            //        Visible = true,
            //        IconClass = "far fa-circle",
            //        Url = "~/Admin/AdvancedSEO/Configure",
            //        Title = await _localizationService.GetResourceAsync("Admin.NopStation.Plugin.Misc.AdvancedSEO.Menu.Configure"),
            //        SystemName = "NopStation.AdvancedSEO.Configure"
            //    };
            //    menu.ChildNodes.Add(dashboard);
            //}

            #endregion

            #region CategorySEOTemplate

            if (await _permissionService.AuthorizeAsync(AdvancedSEOPermissionProvider.ManageAdvancedSEOCategoryTemplates))
            {
                var productSEOTemplate = new SiteMapNode()
                {
                    Visible = true,
                    IconClass = "far fa-circle",
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Plugin.Misc.AdvancedSEO.Menu.CategorySEOTemplate"),
                    Url = "~/Admin/CategorySEOTemplate/List",
                    SystemName = "NopStation.AdvancedSEO.CategorySEOTemplate"
                };
                menu.ChildNodes.Add(productSEOTemplate);
            }

            #endregion

            #region ManufacturerSEOTemplate

            if (await _permissionService.AuthorizeAsync(AdvancedSEOPermissionProvider.ManageAdvancedSEOManufacturerTemplates))
            {
                var manufacturerSEOTemplateNode = new SiteMapNode()
                {
                    Visible = true,
                    IconClass = "far fa-circle",
                    Url = "~/Admin/ManufacturerSEOTemplate/List",
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Plugin.Misc.AdvancedSEO.Menu.ManufacturerSEOTemplate"),
                    SystemName = "NopStation.AdvancedSEO.ManufacturerSEOTemplate"
                };
                menu.ChildNodes.Add(manufacturerSEOTemplateNode);
            }
            #endregion

            #region ProductSEOTemplate

            if (await _permissionService.AuthorizeAsync(AdvancedSEOPermissionProvider.ManageAdvancedSEOProductTemplates))
            {
                var productSEOTemplateNode = new SiteMapNode()
                {
                    Visible = true,
                    IconClass = "far fa-circle",
                    Url = "~/Admin/ProductSEOTemplate/List",
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Plugin.Misc.AdvancedSEO.Menu.ProductSEOTemplate"),
                    SystemName = "NopStation.AdvancedSEO.ProductSEOTemplate"
                };
                menu.ChildNodes.Add(productSEOTemplateNode);
            }

            #endregion

            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/advanced-seo-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=advanced-seo",
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
            return (new Dictionary<string, string>
            {
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.Menu.AdvancedSEO"] = "Advanced SEO",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.Menu.Configure"] = "Configure",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.Menu.CategorySEOTemplate"] = "Category Template",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.Menu.ManufacturerSEOTemplate"] = "Manufacturer Template",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.Menu.ProductSEOTemplate"] = "Product Template",

                //CategorySEOTemplate
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.CategorySEOTemplate.Added"] = "Category SEO template added successfully",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.CategorySEOTemplate.Updated"] = "Category SEO template updated successfully",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.CategorySEOTemplate.Deleted"] = "Category SEO template deleted successfully",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.CategorySEOTemplate.Fields.TemplateName"] = "Template Name",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.CategorySEOTemplate.Fields.TemplateName.Hint"] = "Template Name",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.CategorySEOTemplate.Fields.TemplateRegex"] = "TemplateRegex",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.CategorySEOTemplate.Fields.TemplateRegex.Hint"] = "TemplateRegex",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.CategorySEOTemplate.Fields.SEOTitleTemplate"] = "Title template",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.CategorySEOTemplate.Fields.SEOTitleTemplate.Hint"] = "SEO title template",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.CategorySEOTemplate.Fields.SEODescriptionTemplate"] = "Description Template",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.CategorySEOTemplate.Fields.SEODescriptionTemplate.Hint"] = "SEO description template",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.CategorySEOTemplate.Fields.SEOKeywordsTemplate"] = "Keywords template",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.CategorySEOTemplate.Fields.SEOKeywordsTemplate.Hint"] = "SEO keywords template",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.CategorySEOTemplate.Fields.IsActive"] = "Is Active",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.CategorySEOTemplate.Fields.IsActive.Hint"] = "Is Active",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.CategorySEOTemplate.Fields.Priority"] = "Priority",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.CategorySEOTemplate.Fields.Priority.Hint"] = "Priority",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.CategorySEOTemplate.Fields.ApplyToAllCategory"] = "Apply to all Category",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.CategorySEOTemplate.Fields.ApplyToAllCategory.Hint"] = "Apply to all Category",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.CategorySEOTemplate.Fields.CreatedByCustomer"] = "Created by customer",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.CategorySEOTemplate.Fields.CreatedByCustomer.Hint"] = "Created by customer",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.CategorySEOTemplate.Fields.LastUpdatedByCustomer"] = "Last updated by customer",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.CategorySEOTemplate.Fields.LastUpdatedByCustomer.Hint"] = "Last updated by customer",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.CategorySEOTemplate.Fields.IncludeProductNamesOnKeyword"] = "Include product names on keyword",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.CategorySEOTemplate.Fields.IncludeProductNamesOnKeyword.Hint"] = "Include product names on keyword",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.CategorySEOTemplate.Fields.MaxNumberOfProductToInclude"] = "Max number of product",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.CategorySEOTemplate.Fields.MaxNumberOfProductToInclude.Hint"] = "Max number of product to include on product tag",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.CategorySEOTemplate.Fields.CreatedOn"] = "Created on",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.CategorySEOTemplate.Fields.CreatedOn.Hint"] = "Created on",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.CategorySEOTemplate.Fields.UpdatedOn"] = "Updated on",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.CategorySEOTemplate.Fields.UpdatedOn.Hint"] = "Updated on",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.CategorySEOTemplate.Fields.LimitedToStores"] = "Limited to stores",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.CategorySEOTemplate.Fields.LimitedToStores.Hint"] = "Limited to stores",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.CategorySEOTemplate.List.SearchTemplateName"] = "Template name",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.CategorySEOTemplate.List.SearchStatus"] = "Status",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.CategorySEOTemplate.List.SearchTemplateType"] = "Template type",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.CategorySEOTemplate.List.SearchStore"] = "Store",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.CategorySEOTemplate.List"] = "Category SEO List",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.CategorySEOTemplate"] = "Category SEO Template",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.CategorySEOTemplate.AddNew"] = "Add New Category SEO Template",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.CategorySEOTemplate.AddNew.Title"] = "Add New Category SEO Template",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.CategorySEOTemplate.BackToList"] = "Back to list",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.CategorySEOTemplate.Edit"] = "Edit Category SEO Template",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.CategorySEOTemplate.Edit.Title"] = "Edit Category SEO Template",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.CategorySEOTemplate.General"] = "General",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.CategorySEOTemplate.Format"] = "Seo Format",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.CategorySEOTemplate.EntityMap"] = "Category entity map",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.CategorySEOTemplates.List.SearchTemplateType"] = "Template type",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.CategorySEOTemplate.List.SearchStatus.All"] = "All",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.CategorySEOTemplate.List.SearchTemplateType.All"] = "All",

                //ManufacturerSEOTemplate
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerSEOTemplate.Added"] = "Manufacturer SEO template added successfully",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerSEOTemplate.Updated"] = "Manufacturer SEO template updated successfully",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerSEOTemplate.Deleted"] = "Manufacturer SEO template deleted successfully",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerSEOTemplate.Fields.TemplateName"] = "Template Name",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerSEOTemplate.Fields.TemplateName.Hint"] = "Template Name",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerSEOTemplate.Fields.TemplateRegex"] = "TemplateRegex",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerSEOTemplate.Fields.TemplateRegex.Hint"] = "TemplateRegex",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerSEOTemplate.Fields.SEOTitleTemplate"] = "Meta Title template",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerSEOTemplate.Fields.SEOTitleTemplate.Hint"] = "SEO title template",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerSEOTemplate.Fields.SEODescriptionTemplate"] = "Meta Description Template",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerSEOTemplate.Fields.SEODescriptionTemplate.Hint"] = "SEO description template",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerSEOTemplate.Fields.SEOKeywordsTemplate"] = "Meta Keywords template",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerSEOTemplate.Fields.SEOKeywordsTemplate.Hint"] = "SEO keywords template",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerSEOTemplate.Fields.IsActive"] = "Is Active",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerSEOTemplate.Fields.IsActive.Hint"] = "Is Active",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerSEOTemplate.Fields.Priority"] = "Priority",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerSEOTemplate.Fields.Priority.Hint"] = "Priority",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerSEOTemplate.Fields.ApplyToAllManufacturer"] = "Apply to all Manufacturer",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerSEOTemplate.Fields.ApplyToAllManufacturer.Hint"] = "Apply to all Manufacturer",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerSEOTemplate.Fields.CreatedByCustomer"] = "Created by customer",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerSEOTemplate.Fields.CreatedByCustomer.Hint"] = "Created by customer",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerSEOTemplate.Fields.LastUpdatedByCustomer"] = "Last updated by customer",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerSEOTemplate.Fields.LastUpdatedByCustomer.Hint"] = "Last updated by customer",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerSEOTemplate.Fields.IncludeProductNamesOnKeyword"] = "Include product names on keyword",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerSEOTemplate.Fields.IncludeProductNamesOnKeyword.Hint"] = "Include product names on keyword",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerSEOTemplate.Fields.MaxNumberOfProductToInclude"] = "Max number of product",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerSEOTemplate.Fields.MaxNumberOfProductToInclude.Hint"] = "Max number of product to include on product tag",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerSEOTemplate.Fields.CreatedOn"] = "Created on",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerSEOTemplate.Fields.CreatedOn.Hint"] = "Created on",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerSEOTemplate.Fields.UpdatedOn"] = "Updated on",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerSEOTemplate.Fields.UpdatedOn.Hint"] = "Updated on",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerSEOTemplate.Fields.LimitedToStores"] = "Limited to stores",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerSEOTemplate.Fields.LimitedToStores.Hint"] = "Limited to stores",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerSEOTemplate.List.SearchTemplateName"] = "Template name",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerSEOTemplate.List.SearchStatus"] = "Status",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerSEOTemplate.List.SearchTemplateType"] = "Template type",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerSEOTemplate.List.SearchStore"] = "Store",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerSEOTemplate.List"] = "Manufacturer SEO List",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerSEOTemplate"] = "Manufacturer SEO Template",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerSEOTemplate.AddNew"] = "Add New Manufacturer SEO Template",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerSEOTemplate.AddNew.Title"] = "Add New Manufacturer SEO Template",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerSEOTemplate.BackToList"] = "Back to list",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerSEOTemplate.Edit"] = "Edit Manufacturer SEO Template",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerSEOTemplate.Edit.Title"] = "Edit Manufacturer SEO Template",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerSEOTemplate.General"] = "General",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerSEOTemplate.Format"] = "Seo Format",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerSEOTemplate.EntityMap"] = "Manufacturer entity map",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerSEOTemplates.List.SearchTemplateType"] = "Template type",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerSEOTemplate.List.SearchStatus.All"] = "All",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerSEOTemplate.List.SearchTemplateType.All"] = "All",

                //ProductSEOTemplate
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductSEOTemplate.Added"] = "Product SEO template added successfully",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductSEOTemplate.Updated"] = "Product SEO template updated successfully",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductSEOTemplate.Deleted"] = "Product SEO template deleted successfully",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductSEOTemplate.Fields.TemplateName"] = "Template Name",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductSEOTemplate.Fields.TemplateName.Hint"] = "Template Name",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductSEOTemplate.Fields.TemplateRegex"] = "TemplateRegex",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductSEOTemplate.Fields.TemplateRegex.Hint"] = "TemplateRegex",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductSEOTemplate.Fields.SEOTitleTemplate"] = "Meta Title template",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductSEOTemplate.Fields.SEOTitleTemplate.Hint"] = "SEO title template",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductSEOTemplate.Fields.SEODescriptionTemplate"] = "Meta Description Template",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductSEOTemplate.Fields.SEODescriptionTemplate.Hint"] = "SEO description template",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductSEOTemplate.Fields.SEOKeywordsTemplate"] = "Meta Keywords template",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductSEOTemplate.Fields.SEOKeywordsTemplate.Hint"] = "SEO keywords template",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductSEOTemplate.Fields.IsActive"] = "Is Active",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductSEOTemplate.Fields.IsActive.Hint"] = "Is Active",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductSEOTemplate.Fields.Priority"] = "Priority",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductSEOTemplate.Fields.Priority.Hint"] = "Priority",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductSEOTemplate.Fields.ApplyToAllProduct"] = "Apply to all product",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductSEOTemplate.Fields.ApplyToAllProduct.Hint"] = "Apply to all product",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductSEOTemplate.Fields.CreatedByCustomer"] = "Created by customer",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductSEOTemplate.Fields.CreatedByCustomer.Hint"] = "Created by customer",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductSEOTemplate.Fields.LastUpdatedByCustomer"] = "Last updated by customer",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductSEOTemplate.Fields.LastUpdatedByCustomer.Hint"] = "Last updated by customer",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductSEOTemplate.Fields.IncludeProductTagsOnKeyword"] = "Include product tags on keyword",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductSEOTemplate.Fields.IncludeProductTagsOnKeyword.Hint"] = "Include product tags on keyword",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductSEOTemplate.Fields.IncludeCategoryNamesOnKeyword"] = "Include category names on keyword",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductSEOTemplate.Fields.IncludeCategoryNamesOnKeyword.Hint"] = "Include category names on keyword",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductSEOTemplate.Fields.IncludeManufacturerNamesOnKeyword"] = "Include manufacturer names on keyword",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductSEOTemplate.Fields.IncludeManufacturerNamesOnKeyword.Hint"] = "Include manufacturer names on keyword",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductSEOTemplate.Fields.IncludeVendorNamesOnKeyword"] = "Include vendor names on keyword",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductSEOTemplate.Fields.IncludeVendorNamesOnKeyword.Hint"] = "Include vendor names on keyword",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductSEOTemplate.Fields.CreatedOn"] = "Created on",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductSEOTemplate.Fields.CreatedOn.Hint"] = "Created on",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductSEOTemplate.Fields.UpdatedOn"] = "Updated on",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductSEOTemplate.Fields.UpdatedOn.Hint"] = "Updated on",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductSEOTemplate.Fields.LimitedToStores"] = "Limited to stores",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductSEOTemplate.Fields.LimitedToStores.Hint"] = "Limited to stores",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductSEOTemplate.List.SearchTemplateName"] = "Template name",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductSEOTemplate.List.SearchStatus"] = "Status",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductSEOTemplate.List.SearchTemplateType"] = "Template type",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductSEOTemplate.List.SearchStore"] = "Store",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductSEOTemplate.List"] = "Product SEO List",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductSEOTemplate"] = "Product SEO Template",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductSEOTemplate.AddNew"] = "Add New Product SEO Template",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductSEOTemplate.AddNew.Title"] = "Add New Product SEO Template",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductSEOTemplate.BackToList"] = "Back to list",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductSEOTemplate.Edit"] = "Edit Product SEO Template",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductSEOTemplate.Edit.Title"] = "Edit Product SEO Template",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductSEOTemplate.General"] = "General",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductSEOTemplate.Format"] = "Seo Format",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductSEOTemplate.EntityMap"] = "Product entity map",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductSEOTemplates.List.SearchTemplateType"] = "Template type",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductSEOTemplate.List.SearchStatus.All"] = "All",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductSEOTemplate.List.SearchTemplateType.All"] = "All",

                //SEOTemplateLocalized
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.SEOTemplateLocalized.Fields.SEOTitleTemplate"] = "Meta Title Format",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.SEOTemplateLocalized.Fields.SEOTitleTemplate.Hint"] = "Meta Title Format",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.SEOTemplateLocalized.Fields.SEODescriptionTemplate"] = "Meta Description Format",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.SEOTemplateLocalized.Fields.SEODescriptionTemplate.Hint"] = "Meta Description Format",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.SEOTemplateLocalized.Fields.SEOKeywordsTemplate"] = "Meta Keywords Format",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.SEOTemplateLocalized.Fields.SEOKeywordsTemplate.Hint"] = "Meta Keywords Format",

                //CategoryToMap
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.CategoryToMap.Fields.CategoryName"] = "Category Name",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.CategoryToMap.Fields.CategoryBreadCrumb"] = "Category Name Formated",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.CategoryToMap.AddCategoryToMap.Popup.Title"] = "Add category to template",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.CategoryToMap.AddCategoryToMap.Popup"] = "Add category to template",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.CategoryToMapSearch.Fields.CategoryName"] = "Category Name",

                //CategoryCategorySEOTemplateMapping
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.CategoryCategorySEOTemplateMapping.Fields.Category"] = "Category Name",
                //["Admin.NopStation.Plugin.Misc.AdvancedSEO.CategoryCategorySEOTemplateMapping.Fields.Category"] = "Category Name",


                //ManufacturerToMap
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerToMap.Fields.ManufacturerName"] = "Manufacturer Name",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerToMap.Fields.ManufacturerBreadCrumb"] = "Manufacturer Name Formated",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerToMap.AddManufacturerToMap.Popup.Title"] = "Add Manufacturer to template",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerToMap.AddManufacturerToMap.Popup"] = "Add Manufacturer to template",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerToMapSearch.Fields.ManufacturerName"] = "Manufacturer Name",

                //ManufacturerManufacturerSEOTemplateMapping
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerManufacturerSEOTemplateMapping.Fields.Manufacturer"] = "Manufacturer Name",

                //ProductToMap
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductToMap.Fields.ProductName"] = "Product Name",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductToMap.Fields.ProductBreadCrumb"] = "Product Name Formated",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductToMap.AddProductToMap.Popup.Title"] = "Add Product to template",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductToMap.AddProductToMap.Popup"] = "Add Product to template",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductToMapSearch.Fields.ProductName"] = "Product Name",

                //ProductProductSEOTemplateMapping
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductProductSEOTemplateMapping.Fields.Product"] = "Product Name",



                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.CategorySEOTemplate.AvailableCategoryMetaTitleTokens"] = "Token available for title",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.CategorySEOTemplate.AvailableCategoryMetaDescriptionTokens"] = "Token available for meta description",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.CategorySEOTemplate.AvailableCategoryMetaKeywordsTokens"] = "Token available for meta Keywords",


                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerSEOTemplate.AvailableManufacturerMetaTitleTokens"] = "Token available for title",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerSEOTemplate.AvailableManufacturerMetaDescriptionTokens"] = "Token available for meta description",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerSEOTemplate.AvailableManufacturerMetaKeywordsTokens"] = "Token available for meta Keywords",



                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductSEOTemplate.AvailableProductMetaTitleTokens"] = "Token available for title",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductSEOTemplate.AvailableProductMetaDescriptionTokens"] = "Token available for meta description",
                ["Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductSEOTemplate.AvailableProductMetaKeywordsTokens"] = "Token available for meta Keywords",


                //validator
                ["Admin.Configure.SEOTemplate.Fields.TemplateName.Required"] = "Template name required",
                ["Admin.Configure.SEOTemplate.Fields.SEOTitleTemplate.Required"] = "Seo title template can not be empty",
                ["Admin.Configure.SEOTemplate.Fields.SEODescriptionTemplate.Required"] = "Seo description template can not be empty",
                ["Admin.Configure.SEOTemplate.Fields.SEOKeywordsTemplate.Required"] = "Seo keywords template can not be empty",
                ["Admin.Configure.SEOTemplate.Fields.MaxNumberOfProductToInclude.Invalid"] = "Max Number Of Product To Include in keywords must be greater than 0 and less then 1000",

            }).ToList();
        }

        #endregion
    }
}
