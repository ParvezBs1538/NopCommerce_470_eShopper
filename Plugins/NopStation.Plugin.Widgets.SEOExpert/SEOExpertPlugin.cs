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
using NopStation.Plugin.Widgets.SEOExpert.Areas.Admin.Components;

namespace NopStation.Plugin.Widgets.SEOExpert
{
    public class SEOExpertPlugin : BasePlugin, INopStationPlugin, IAdminMenuPlugin, IWidgetPlugin
    {
        #region Fields

        private readonly INopStationCoreService _nopStationCoreService;
        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;

        public bool HideInWidgetList => false;

        #endregion

        #region Ctor

        public SEOExpertPlugin(INopStationCoreService nopStationCoreService,
            ILocalizationService localizationService,
            IWebHelper webHelper,
            IPermissionService permissionService,
            ISettingService settingService)
        {
            _nopStationCoreService = nopStationCoreService;
            _localizationService = localizationService;
            _webHelper = webHelper;
            _permissionService = permissionService;
            _settingService = settingService;
        }

        #endregion

        #region Methods

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/SEOExpert/Configure";
        }

        public override async Task InstallAsync()
        {
            var settings = new SEOExpertSettings()
            {
                Endpoint = "https://api.openai.com/v1/chat/completions",
                ModelName = "gpt-3.5-turbo",
                RequireAdminApproval = true
            };

            await _settingService.SaveSettingAsync(settings);

            await this.InstallPluginAsync(new SEOExpertPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            //settings
            await _settingService.DeleteSettingAsync<SEOExpertSettings>();

            await this.UninstallPluginAsync(new SEOExpertPermissionProvider());
            await base.UninstallAsync();
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.SEOExpert.Menu.Title"),
                Visible = true,
                IconClass = "far fa-dot-circle",
            };

            if (await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            {
                var configItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.SEOExpert.Menu.Configuration"),
                    Url = $"{_webHelper.GetStoreLocation()}Admin/SEOExpert/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "SEOExpert.Configuration"
                };
                menuItem.ChildNodes.Add(configItem);
            }

            var documentation = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                Url = "https://www.nop-station.com/seo-expert-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=seo-expert",
                Visible = true,
                IconClass = "far fa-circle",
                OpenUrlInNewTab = true
            };

            menuItem.ChildNodes.Add(documentation);

            await _nopStationCoreService.ManageSiteMapAsync(rootNode, menuItem, NopStationMenuType.Plugin);
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            return new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Admin.NopStation.SEOExpert.Menu.Title", "SEO expert"),
                new KeyValuePair<string, string>("Admin.NopStation.SEOExpert.Menu.Configuration", "Configuration"),

                new KeyValuePair<string, string>("Plugins.NopStation.SEOExpert.GenerateForAllProducts.Success", "SEO meta title, meta description and meta keywords are successfully generated for all products"),
                new KeyValuePair<string, string>("Plugins.NopStation.SEOExpert.GenerateForAllCategories.Success", "SEO meta title, meta description and meta keywords are successfully generated for all categories"),
                new KeyValuePair<string, string>("Plugins.NopStation.SEOExpert.GenerateForAllManufacturers.Success", "SEO meta title, meta description and meta keywords are successfully generated for all manufacturers"),
                new KeyValuePair<string, string>("Plugins.NopStation.SEOExpert.GenerateForProduct.Success", "SEO meta title, meta description and meta keywords are successfully generated for this product"),
                new KeyValuePair<string, string>("Plugins.NopStation.SEOExpert.GenerateForCategory.Success", "SEO meta title, meta description and meta keywords are successfully generated for this category"),
                new KeyValuePair<string, string>("Plugins.NopStation.SEOExpert.GenerateForManufacturer.Success", "SEO meta title, meta description and meta keywords are successfully generated for this manufacturer"),
                new KeyValuePair<string, string>("Plugins.NopStation.SEOExpert.GenerateForProduct.Failed", "SEO meta title, meta description and meta keywords are failed to generate for this product"),
                new KeyValuePair<string, string>("Plugins.NopStation.SEOExpert.GenerateForProduct.Failed.ProductNotFound", "Product not found!"),

                new KeyValuePair<string, string>("Admin.NopStation.SEOExpert.GenerateForAll", "Generate(All products)"),
                new KeyValuePair<string, string>("Admin.NopStation.SEOExpert.Generate", "Generate SEO"),
                new KeyValuePair<string, string>("Admin.NopStation.SEOExpert.Generate.All", "Generate(All)"),
                new KeyValuePair<string, string>("Admin.NopStation.SEOExpert.Generate.Selected", "Generate(Selected)"),
                new KeyValuePair<string, string>("Admin.NopStation.SEOExpert.Generate.Title", "Generate seo through artificial intellegence"),
                new KeyValuePair<string, string>("Admin.NopStation.SEOExpert.Generate.Apply", "Apply"),
                new KeyValuePair<string, string>("Admin.NopStation.SEOExpert.Generate.Regenerate", "Regenerate"),
                new KeyValuePair<string, string>("Admin.NopStation.SEOExpert.Configuration", "SEO Expert - Configuration"),

                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.SEOExpert.Configuration.Fields.OpenAIApiKey", "API key"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.SEOExpert.Configuration.Fields.OpenAIApiKey.Hint", "API key generated in openai account"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.SEOExpert.Configuration.Fields.Endpoint", "Endpoint url"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.SEOExpert.Configuration.Fields.Endpoint.Hint", "Endpoint url"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.SEOExpert.Configuration.Fields.ModelName", "Model"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.SEOExpert.Configuration.Fields.ModelName.Hint", "Model name that we are using"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.SEOExpert.Configuration.Fields.RequireAdminApproval", "Require admin approval"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.SEOExpert.Configuration.Fields.RequireAdminApproval.Hint", "Require admin approval for applying the seo contents on product"),

                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.SEOExpert.Configuration.Fields.AdditionalInfoWithName", "Additional info with name"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.SEOExpert.Configuration.Fields.AdditionalInfoWithName.Hint", "Additional info with product's name will be sent while seo generating"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.SEOExpert.Configuration.Fields.AdditionalInfoWithShortDescription", "Additional info with short description"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.SEOExpert.Configuration.Fields.AdditionalInfoWithShortDescription.Hint", "Additional info with product's short description will be sent while seo generating"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.SEOExpert.Configuration.Fields.AdditionalInfoWithFullDescription", "Additional info with full description"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.SEOExpert.Configuration.Fields.AdditionalInfoWithFullDescription.Hint", "Additional info with product's full description will be sent while seo generating"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.SEOExpert.Configuration.Fields.RegenerateConditionId", "Generate if not exists"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.SEOExpert.Configuration.Fields.RegenerateConditionId.Hint", "When to generate meta title, meta description and meta keywords"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.SEOExpert.Configuration.Fields.Temperature", "Temperature"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.SEOExpert.Configuration.Fields.Temperature.Hint", "Set temperature for model."),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.SEOExpert.Configuration.Fields.Temperature.Warnings.Notification", "Tempareture should be between 0 to 1. For more information visit <a href=\"https://platform.openai.com/docs/api-reference/chat/create#chat/create-temperature\">click here.</a>"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.SEOExpert.Configuration.Fields.Temperature.RangeValidation", "Tempareture should be between 0 to 1"),
                new KeyValuePair<string, string>("Admin.Plugins.NopStation.SEOExpert.GenerateFor.Failed", "Failed to generate SEO."),

                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.SEOExpert.Configuration.Fields.EnableListGeneration", "Enable all generation"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.SEOExpert.Configuration.Fields.EnableListGeneration.Hint", "Enable this for generation seo for all option in list page."),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Widgets.SEOExpert.Configuration.Fields.EnableListGeneration.Warnings.Notification", "If enable this you can generate seo for all list records.It we be time comsuming and Costly."),

                new KeyValuePair<string, string>("Enums.NopStation.Plugin.Widgets.SEOExpert.Domains.RegenerateCondition.RegenerateIfNotExistMetaTitle", "Meta title missing"),
                new KeyValuePair<string, string>("Enums.NopStation.Plugin.Widgets.SEOExpert.Domains.RegenerateCondition.RegenerateIfNotExistMetaKeywords", "Meta keywords missing"),
                new KeyValuePair<string, string>("Enums.NopStation.Plugin.Widgets.SEOExpert.Domains.RegenerateCondition.RegenerateIfNotExistMetaDescription", "Meta description missing"),
            };
        }

        public Task<IList<string>> GetWidgetZonesAsync()
        {
            return Task.FromResult<IList<string>>(new List<string>
            {
                AdminWidgetZones.ProductDetailsButtons,
                AdminWidgetZones.ProductListButtons,
                AdminWidgetZones.CategoryDetailsButtons,
                AdminWidgetZones.CategoryListButtons,
                AdminWidgetZones.ManufacturerDetailsButtons,
                AdminWidgetZones.ManufacturerListButtons
            });
        }

        public Type GetWidgetViewComponent(string widgetZone)
        {
            if (widgetZone == AdminWidgetZones.ProductDetailsButtons || widgetZone == AdminWidgetZones.CategoryDetailsButtons || widgetZone == AdminWidgetZones.ManufacturerDetailsButtons)
            {
                return typeof(DetailsPageSEOViewComponent);
            }
            else if (widgetZone == AdminWidgetZones.ProductListButtons || widgetZone == AdminWidgetZones.CategoryListButtons || widgetZone == AdminWidgetZones.ManufacturerListButtons)
            {
                return typeof(ListPageSEOViewComponent);
            }
            return null;
        }

        #endregion
    }
}
