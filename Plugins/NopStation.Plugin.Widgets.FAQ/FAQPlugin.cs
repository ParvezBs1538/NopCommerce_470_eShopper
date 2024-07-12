using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Services;
using NopStation.Plugin.Widgets.FAQ.Components;

namespace NopStation.Plugin.Widgets.FAQ
{
    public class FAQPlugin : BasePlugin, IAdminMenuPlugin, INopStationPlugin, IWidgetPlugin
    {
        #region Fields

        private readonly IWebHelper _webHelper;
        private readonly INopStationCoreService _nopStationCoreService;
        private readonly IPermissionService _permissionService;
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;

        #endregion

        #region Ctor

        public FAQPlugin(IWebHelper webHelper,
            INopStationCoreService nopStationCoreService,
            IPermissionService permissionService,
            ILocalizationService localizationService,
            ISettingService settingService)
        {
            _webHelper = webHelper;
            _nopStationCoreService = nopStationCoreService;
            _permissionService = permissionService;
            _localizationService = localizationService;
            _settingService = settingService;
        }

        #endregion

        #region Methods

        public override async Task InstallAsync()
        {
            await _settingService.SaveSettingAsync(new FAQSettings()
            {
                IncludeInFooter = true,
                IncludeInTopMenu = true,
                FooterElementSelector = ".footer-block.customer-service .list"
            });

            await this.InstallPluginAsync(new FAQPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await this.UninstallPluginAsync(new FAQPermissionProvider());
            await base.UninstallAsync();
        }

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/FAQ/Configure";
        }

        public Task<IList<string>> GetWidgetZonesAsync()
        {
            return Task.FromResult<IList<string>>(
                new List<string> {
                    PublicWidgetZones.Footer,
                    PublicWidgetZones.HeaderMenuAfter,
                    PublicWidgetZones.MobHeaderMenuAfter
                });
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.FAQ.Menu.FAQ"),
                Visible = true,
                IconClass = "far fa-dot-circle",
            };

            if (await _permissionService.AuthorizeAsync(FAQPermissionProvider.ManageConfiguration))
            {
                var categoryIconItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.FAQ.Menu.Configuration"),
                    Url = "~/Admin/FAQ/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "FAQ.Configuration"
                };
                menuItem.ChildNodes.Add(categoryIconItem);
            }

            if (await _permissionService.AuthorizeAsync(FAQPermissionProvider.ManageItems))
            {
                var categoryIconItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.FAQ.Menu.Items"),
                    Url = "~/Admin/FAQItem/List",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "FAQ.Items"
                };
                menuItem.ChildNodes.Add(categoryIconItem);
            }

            if (await _permissionService.AuthorizeAsync(FAQPermissionProvider.ManageCategories))
            {
                var categoryIconItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.FAQ.Menu.Categories"),
                    Url = "~/Admin/FAQCategory/List",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "FAQ.Categories"
                };
                menuItem.ChildNodes.Add(categoryIconItem);
            }
            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/faq-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=faq",
                    Visible = true,
                    IconClass = "far fa-circle",
                    OpenUrlInNewTab = true
                };
                menuItem.ChildNodes.Add(documentation);
            }

            await _nopStationCoreService.ManageSiteMapAsync(rootNode, menuItem, NopStationMenuType.Plugin);
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            var list = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Admin.NopStation.FAQ.Menu.FAQ", "FAQ"),
                new KeyValuePair<string, string>("Admin.NopStation.FAQ.Menu.Configuration", "Configuration"),
                new KeyValuePair<string, string>("Admin.NopStation.FAQ.Menu.Items", "Items"),
                new KeyValuePair<string, string>("Admin.NopStation.FAQ.Menu.Categories", "Categories"),

                new KeyValuePair<string, string>("Admin.NopStation.FAQ.Configuration", "FAQ settings"),
                new KeyValuePair<string, string>("Admin.NopStation.FAQ.Configuration.Fields.EnablePlugin", "Enable plugin"),
                new KeyValuePair<string, string>("Admin.NopStation.FAQ.Configuration.Fields.EnablePlugin.Hint", "Determines whether the plugin is enabled or not."),
                new KeyValuePair<string, string>("Admin.NopStation.FAQ.Configuration.Fields.IncludeInTopMenu", "Include in top menu"),
                new KeyValuePair<string, string>("Admin.NopStation.FAQ.Configuration.Fields.IncludeInTopMenu.Hint", "Determines whether to include in top menu or not."),
                new KeyValuePair<string, string>("Admin.NopStation.FAQ.Configuration.Fields.IncludeInFooter", "Include in footer"),
                new KeyValuePair<string, string>("Admin.NopStation.FAQ.Configuration.Fields.IncludeInFooter.Hint", "Determines whether to include in footer or not."),
                new KeyValuePair<string, string>("Admin.NopStation.FAQ.Configuration.Fields.FooterElementSelector", "Footer element selector"),
                new KeyValuePair<string, string>("Admin.NopStation.FAQ.Configuration.Fields.FooterElementSelector.Hint", "Enter footer element selector."),

                new KeyValuePair<string, string>("Admin.NopStation.FAQ.FAQCategories.List", "FAQ categories"),
                new KeyValuePair<string, string>("Admin.NopStation.FAQ.FAQCategories.EditDetails", "Edit FAQ category details"),
                new KeyValuePair<string, string>("Admin.NopStation.FAQ.FAQCategories.AddNew", "Add new FAQ category"),
                new KeyValuePair<string, string>("Admin.NopStation.FAQ.FAQCategories.BackToList", "back to category list"),

                new KeyValuePair<string, string>("Admin.NopStation.FAQ.FAQCategories.List.SearchName", "Name"),
                new KeyValuePair<string, string>("Admin.NopStation.FAQ.FAQCategories.List.SearchName.Hint", "Search name."),

                new KeyValuePair<string, string>("Admin.NopStation.FAQ.FAQCategories.Created", "FAQ category created successfully."),
                new KeyValuePair<string, string>("Admin.NopStation.FAQ.FAQCategories.Updated", "FAQ category updated successfully."),
                new KeyValuePair<string, string>("Admin.NopStation.FAQ.FAQCategories.Deleted", "FAQ category deleted successfully."),

                new KeyValuePair<string, string>("Admin.NopStation.FAQ.FAQCategories.Fields.Name", "Name"),
                new KeyValuePair<string, string>("Admin.NopStation.FAQ.FAQCategories.Fields.Name.Hint", "FAQ category name."),
                new KeyValuePair<string, string>("Admin.NopStation.FAQ.FAQCategories.Fields.Description", "Description"),
                new KeyValuePair<string, string>("Admin.NopStation.FAQ.FAQCategories.Fields.Description.Hint", "FAQ category description."),
                new KeyValuePair<string, string>("Admin.NopStation.FAQ.FAQCategories.Fields.Published", "Published"),
                new KeyValuePair<string, string>("Admin.NopStation.FAQ.FAQCategories.Fields.Published.Hint", "Determines the FAQ category is published or not."),
                new KeyValuePair<string, string>("Admin.NopStation.FAQ.FAQCategories.Fields.DisplayOrder", "Display order"),
                new KeyValuePair<string, string>("Admin.NopStation.FAQ.FAQCategories.Fields.Description.Hint", "Display order of the FAQ category. 1 represents the top of the list."),
                new KeyValuePair<string, string>("Admin.NopStation.FAQ.FAQCategories.Fields.SelectedStoreIds", "Limited to stores"),
                new KeyValuePair<string, string>("Admin.NopStation.FAQ.FAQCategories.Fields.SelectedStoreIds.Hint", "Option to limit this FAQ category to a certain store. If you have multiple stores, choose one or several from the list. If you don't use this option just leave this field empty."),

                new KeyValuePair<string, string>("Admin.NopStation.FAQ.FAQCategories.Fields.Name.Required", "Category name is required"),
                new KeyValuePair<string, string>("Admin.NopStation.FAQ.FAQCategories.Fields.Description.Required", "Category description is required"),

                new KeyValuePair<string, string>("Admin.NopStation.FAQ.FAQItem.Fields.Question.Required", "Question is required"),
                new KeyValuePair<string, string>("Admin.NopStation.FAQ.FAQItem.Fields.Answer.Required", "Answer is required"),

                new KeyValuePair<string, string>("Admin.NopStation.FAQ.FAQItems.List", "FAQ items"),
                new KeyValuePair<string, string>("Admin.NopStation.FAQ.FAQItems.EditDetails", "Edit FAQ item details"),
                new KeyValuePair<string, string>("Admin.NopStation.FAQ.FAQItems.AddNew", "Add new FAQ item"),
                new KeyValuePair<string, string>("Admin.NopStation.FAQ.FAQItems.BackToList", "back to item list"),

                new KeyValuePair<string, string>("Admin.NopStation.FAQ.FAQItems.List.SearchKeyword", "Keyword"),
                new KeyValuePair<string, string>("Admin.NopStation.FAQ.FAQItems.List.SearchKeyword.Hint", "Search keyword."),
                new KeyValuePair<string, string>("Admin.NopStation.FAQ.FAQItems.List.SearchCategory", "Category"),
                new KeyValuePair<string, string>("Admin.NopStation.FAQ.FAQItems.List.SearchCategory.Hint", "Search category."),
                new KeyValuePair<string, string>("Admin.NopStation.FAQ.FAQItems.List.SearchCategory", "Category"),
                new KeyValuePair<string, string>("Admin.NopStation.FAQ.FAQItems.List.SearchCategory.All", "All"),

                new KeyValuePair<string, string>("Admin.NopStation.FAQ.FAQItems.Created", "FAQ item created successfully."),
                new KeyValuePair<string, string>("Admin.NopStation.FAQ.FAQItems.Updated", "FAQ item updated successfully."),
                new KeyValuePair<string, string>("Admin.NopStation.FAQ.FAQItems.Deleted", "FAQ item deleted successfully."),

                new KeyValuePair<string, string>("Admin.NopStation.FAQ.FAQItems.Fields.Question", "Question"),
                new KeyValuePair<string, string>("Admin.NopStation.FAQ.FAQItems.Fields.Question.Hint", "FAQ item question."),
                new KeyValuePair<string, string>("Admin.NopStation.FAQ.FAQItems.Fields.Answer", "Answer"),
                new KeyValuePair<string, string>("Admin.NopStation.FAQ.FAQItems.Fields.Answer.Hint", "FAQ item answer."),
                new KeyValuePair<string, string>("Admin.NopStation.FAQ.FAQItems.Fields.Published", "Published"),
                new KeyValuePair<string, string>("Admin.NopStation.FAQ.FAQItems.Fields.Published.Hint", "Determines the FAQ item is published or not."),
                new KeyValuePair<string, string>("Admin.NopStation.FAQ.FAQItems.Fields.Permalink", "Permalink"),
                new KeyValuePair<string, string>("Admin.NopStation.FAQ.FAQItems.Fields.Permalink.Hint", "FAQ item permalink."),
                new KeyValuePair<string, string>("Admin.NopStation.FAQ.FAQItems.Fields.FAQTags", "Tags"),
                new KeyValuePair<string, string>("Admin.NopStation.FAQ.FAQItems.Fields.FAQTags.Hint", "FAQ item tags."),
                new KeyValuePair<string, string>("Admin.NopStation.FAQ.FAQItems.Fields.FAQTags.Placeholder", "FAQ tags"),
                new KeyValuePair<string, string>("Admin.NopStation.FAQ.FAQItems.Fields.DisplayOrder", "Display order"),
                new KeyValuePair<string, string>("Admin.NopStation.FAQ.FAQItems.Fields.Description.Hint", "Display order of the FAQ item. 1 represents the top of the list."),
                new KeyValuePair<string, string>("Admin.NopStation.FAQ.FAQItems.Fields.SelectedStoreIds", "Limited to stores"),
                new KeyValuePair<string, string>("Admin.NopStation.FAQ.FAQItems.Fields.SelectedStoreIds.Hint", "Option to limit this FAQ item to a certain store. If you have multiple stores, choose one or several from the list. If you don't use this option just leave this field empty."),
                new KeyValuePair<string, string>("Admin.NopStation.FAQ.FAQItems.Fields.SelectedCategoryIds", "Limited to faq categories"),
                new KeyValuePair<string, string>("Admin.NopStation.FAQ.FAQItems.Fields.SelectedCategoryIds.Hint", "Select faq categories from the list."),

                new KeyValuePair<string, string>("NopStation.FAQ.FindFAQ", "FAQ"),
                new KeyValuePair<string, string>("NopStation.FAQ.Others", "Others"),
                new KeyValuePair<string, string>("PageTitle.NopStation.FAQ", "FAQ"),
                new KeyValuePair<string, string>("Account.NopStation.FAQ", "FAQ")
            };

            return list;
        }

        public Type GetWidgetViewComponent(string widgetZone)
        {
            return typeof(FAQViewComponent);
        }

        #endregion

        #region Properties

        public bool HideInWidgetList => false;

        #endregion
    }
}
