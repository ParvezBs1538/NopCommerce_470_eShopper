using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Media;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Services;
using NopStation.Plugin.Widgets.DynamicSurvey.Components;

namespace NopStation.Plugin.Widgets.DynamicSurvey
{
    public class DynamicSurveyPlugin : BasePlugin, IAdminMenuPlugin, INopStationPlugin, IWidgetPlugin
    {
        private readonly IWebHelper _webHelper;
        private readonly INopStationCoreService _nopStationCoreService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly CatalogSettings _catalogSettings;
        private readonly MediaSettings _mediaSettings;

        public bool HideInWidgetList => false;

        public DynamicSurveyPlugin(IWebHelper webHelper,
            ILocalizationService localizationService,
            INopStationCoreService nopStationCoreService,
            IPermissionService permissionService,
            ISettingService settingService,
            CatalogSettings catalogSettings,
            MediaSettings mediaSettings)
        {
            _webHelper = webHelper;
            _nopStationCoreService = nopStationCoreService;
            _localizationService = localizationService;
            _permissionService = permissionService;
            _settingService = settingService;
            _catalogSettings = catalogSettings;
            _mediaSettings = mediaSettings;
        }

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/DynamicSurvey/Configure";
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.DynamicSurvey.Menu.DynamicSurvey"),
                Visible = true,
                IconClass = "far fa-dot-circle",
            };

            if (await _permissionService.AuthorizeAsync(DynamicSurveyPermissionProvider.ManageDynamicSurvey))
            {
                var configItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.DynamicSurvey.Menu.Configuration"),
                    Url = "~/Admin/DynamicSurvey/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "DynamicSurvey.Configuration"
                };
                menuItem.ChildNodes.Add(configItem);
                var surveyItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.DynamicSurvey.Menu.Surveys"),
                    Url = "~/Admin/Survey/List",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "Surveys"
                };
                menuItem.ChildNodes.Add(surveyItem);

                var attrItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.DynamicSurvey.Menu.SurveyAttributes"),
                    Url = "~/Admin/SurveyAttribute/List",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "Survey attributes"
                };
                menuItem.ChildNodes.Add(attrItem);

                var surveySubmissionItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.DynamicSurvey.Menu.Submissions"),
                    Url = "~/Admin/SurveySubmission/List",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "Surveys submissions"
                };
                menuItem.ChildNodes.Add(surveySubmissionItem);
            }

            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/dynamic-survey-documentation?utm_source=admin-panel&utm_medium=surveys&utm_campaign=dynamic-survey",
                    Visible = true,
                    IconClass = "far fa-circle",
                    OpenUrlInNewTab = true
                };
                menuItem.ChildNodes.Add(documentation);
            }

            await _nopStationCoreService.ManageSiteMapAsync(rootNode, menuItem, NopStationMenuType.Plugin);
        }

        public override async Task InstallAsync()
        {
            await _settingService.SaveSettingAsync(new DynamicSurveySettings
            {
                CountDisplayedYearsDatePicker = _catalogSettings.CountDisplayedYearsDatePicker,
                CaptchaEnabled = true,
                ImageSquarePictureSize = _mediaSettings.ImageSquarePictureSize,
                MinimumIntervalToSubmitSurvey = 60
            });

            await this.InstallPluginAsync(new DynamicSurveyPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await this.UninstallPluginAsync(new DynamicSurveyPermissionProvider());
            await base.UninstallAsync();
        }

        public override async Task UpdateAsync(string currentVersion, string targetVersion)
        {
            await _localizationService.AddOrUpdateLocaleResourceAsync(PluginResouces().ToDictionary(x => x.Key, x => x.Value));
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            var list = new Dictionary<string, string>
            {
                ["Admin.NopStation.DynamicSurvey.Menu.DynamicSurvey"] = "Survey",
                ["Admin.NopStation.DynamicSurvey.Menu.Configuration"] = "Configuration",
                ["Admin.NopStation.DynamicSurvey.Menu.Surveys"] = "Surveys",
                ["Admin.NopStation.DynamicSurvey.Menu.SurveyAttributes"] = "Attributes",
                ["Admin.NopStation.DynamicSurvey.Menu.Submissions"] = "Submissions",

                ["Admin.NopStation.DynamicSurvey.Configuration"] = "Survey settings",
                ["Admin.NopStation.DynamicSurvey.Configuration.Fields.CaptchaEnabled"] = "CAPTCHA enabled",
                ["Admin.NopStation.DynamicSurvey.Configuration.Fields.CaptchaEnabled.Hint"] = "Check to enable CAPTCHA.",
                ["Admin.NopStation.DynamicSurvey.Configuration.Fields.MinimumIntervalToSubmitSurvey"] = "Minimum interval to re-submit survey",
                ["Admin.NopStation.DynamicSurvey.Configuration.Fields.MinimumIntervalToSubmitSurvey.Hint"] = "Minimum interval to submit same survey in seconds.",
                ["Admin.NopStation.DynamicSurvey.Configuration.Fields.ImageSquarePictureSize"] = "Image options size",
                ["Admin.NopStation.DynamicSurvey.Configuration.Fields.ImageSquarePictureSize.Hint"] = "Size of survey form’s image options in px.",
                ["Admin.NopStation.DynamicSurvey.Configuration.Fields.CountDisplayedYearsDatePicker"] = "Count displayed years datepicker",
                ["Admin.NopStation.DynamicSurvey.Configuration.Fields.CountDisplayedYearsDatePicker.Hint"] = "Number of years before and after current year to show on date picker.",

                ["Admin.NopStation.DynamicSurvey.Surveys"] = "Surveys",
                ["Admin.NopStation.DynamicSurvey.Surveys.ExportNoSurvey"] = "No survey to export",
                ["Admin.NopStation.DynamicSurvey.Surveys.Copy"] = "Copy survey",
                ["Admin.NopStation.DynamicSurvey.Surveys.Copy.Name"] = "New survey name",
                ["Admin.NopStation.DynamicSurvey.Surveys.Copy.Name.Hint"] = "The name of the new survey.",
                ["Admin.NopStation.DynamicSurvey.Surveys.Copy.Name.New"] = "{0} - copy",
                ["Admin.NopStation.DynamicSurvey.Surveys.Copy.Published"] = "Published",
                ["Admin.NopStation.DynamicSurvey.Surveys.Copy.Published.Hint"] = "Check to mark a survey as published.",
                ["Admin.NopStation.DynamicSurvey.Surveys.EditSurveyDetails"] = "Edit survey details",
                ["Admin.NopStation.DynamicSurvey.Surveys.Fields.AclCustomerRoles"] = "Customer roles",
                ["Admin.NopStation.DynamicSurvey.Surveys.Fields.AclCustomerRoles.Hint"] = "Choose one or several customer roles i.e. administrators, vendors, guests, who will be able to see this survey in catalog. If you don't need this option just leave this field empty. In order to use this functionality, you have to disable the following setting: Configuration &gt; Settings &gt; Catalog &gt; Ignore ACL rules (sitewide).",
                ["Admin.NopStation.DynamicSurvey.Surveys.Fields.EndDateUtc"] = "End date",
                ["Admin.NopStation.DynamicSurvey.Surveys.Fields.EndDateUtc.Hint"] = "The end of the survey's availability in Coordinated Universal Time (UTC).",
                ["Admin.NopStation.DynamicSurvey.Surveys.Fields.StartDateUtc"] = "Start date",
                ["Admin.NopStation.DynamicSurvey.Surveys.Fields.StartDateUtc.Hint"] = "The start of the survey's availability in Coordinated Universal Time (UTC).",
                ["Admin.NopStation.DynamicSurvey.Surveys.Fields.DisplayOrder"] = "Display order",
                ["Admin.NopStation.DynamicSurvey.Surveys.Fields.DisplayOrder.Hint"] = "Display order of the survey. 1 represents the top of the list.",
                ["Admin.NopStation.DynamicSurvey.Surveys.Fields.ToEmailAddresses"] = "To",
                ["Admin.NopStation.DynamicSurvey.Surveys.Fields.ToEmailAddresses.Hint"] = "The recipients for this e-mail message.",
                ["Admin.NopStation.DynamicSurvey.Surveys.Fields.BccEmailAddresses"] = "BCC",
                ["Admin.NopStation.DynamicSurvey.Surveys.Fields.BccEmailAddresses.Hint"] = "The blind carbon copy (BCC) recipients for this e-mail message.",
                ["Admin.NopStation.DynamicSurvey.Surveys.Fields.LimitedToStores"] = "Limited to stores",
                ["Admin.NopStation.DynamicSurvey.Surveys.Fields.LimitedToStores.Hint"] = "Option to limit this survey to a certain store. If you have multiple stores, choose one or several from the list. If you don't use this option just leave this field empty. In order to use this functionality, you have to disable the following setting: Configuration &gt; Catalog settings &gt; Ignore \"limit per store\" rules (sitewide).",
                ["Admin.NopStation.DynamicSurvey.Surveys.Fields.MetaDescription"] = "Meta description",
                ["Admin.NopStation.DynamicSurvey.Surveys.Fields.MetaDescription.Hint"] = "Meta description to be added to survey page header.",
                ["Admin.NopStation.DynamicSurvey.Surveys.Fields.MetaKeywords"] = "Meta keywords",
                ["Admin.NopStation.DynamicSurvey.Surveys.Fields.MetaKeywords.Hint"] = "Meta keywords to be added to survey page header.",
                ["Admin.NopStation.DynamicSurvey.Surveys.Fields.MetaTitle"] = "Meta title",
                ["Admin.NopStation.DynamicSurvey.Surveys.Fields.MetaTitle.Hint"] = "Override the page title. The default is the name of the survey.",
                ["Admin.NopStation.DynamicSurvey.Surveys.Fields.Name"] = "Survey name",
                ["Admin.NopStation.DynamicSurvey.Surveys.Fields.Name.Hint"] = "The name of the survey.",
                ["Admin.NopStation.DynamicSurvey.Surveys.Fields.SystemName"] = "System name",
                ["Admin.NopStation.DynamicSurvey.Surveys.Fields.SystemName.Hint"] = "System name of this survey.",
                ["Admin.NopStation.DynamicSurvey.Surveys.Fields.Name.Required"] = "Please provide a name.",
                ["Admin.NopStation.DynamicSurvey.Surveys.Fields.Description"] = "Survey description",
                ["Admin.NopStation.DynamicSurvey.Surveys.Fields.Description.Hint"] = "The description of the survey.",
                ["Admin.NopStation.DynamicSurvey.Surveys.Fields.EmailAccountId"] = "Email account",
                ["Admin.NopStation.DynamicSurvey.Surveys.Fields.EmailAccountId.Hint"] = "The email account that will be used to send this survey.",
                ["Admin.NopStation.DynamicSurvey.Surveys.Fields.IncludeInTopMenu"] = "Include in top menu",
                ["Admin.NopStation.DynamicSurvey.Surveys.Fields.IncludeInTopMenu.Hint"] = "Check to include this survey in the top menu.",
                ["Admin.NopStation.DynamicSurvey.Surveys.Fields.EnableEmailing"] = "Enable emailing",
                ["Admin.NopStation.DynamicSurvey.Surveys.Fields.EnableEmailing.Hint"] = "Check to enable emailing.",
                ["Admin.NopStation.DynamicSurvey.Surveys.Fields.SendImmediately"] = "Send immediately",
                ["Admin.NopStation.DynamicSurvey.Surveys.Fields.SendImmediately.Hint"] = "Send message immediately.",
                ["Admin.NopStation.DynamicSurvey.Surveys.Fields.DelayPeriodId"] = "Delay period",
                ["Admin.NopStation.DynamicSurvey.Surveys.Fields.DelayBeforeSend"] = "Delay send",
                ["Admin.NopStation.DynamicSurvey.Surveys.Fields.DelayBeforeSend.Hint"] = "A delay before sending the message.",
                ["Admin.NopStation.DynamicSurvey.Surveys.Fields.AllowMultipleSubmissions"] = "Allow multiple submissions",
                ["Admin.NopStation.DynamicSurvey.Surveys.Fields.AllowMultipleSubmissions.Hint"] = "Check to allow multiple submissions.",
                ["Admin.NopStation.DynamicSurvey.Surveys.Fields.Published"] = "Published",
                ["Admin.NopStation.DynamicSurvey.Surveys.Fields.Published.Hint"] = "Check to publish this survey (visible in store). Uncheck to unpublish (survey not available in store).",
                ["Admin.NopStation.DynamicSurvey.Surveys.Fields.SeName"] = "Search engine friendly page name",
                ["Admin.NopStation.DynamicSurvey.Surveys.Fields.SeName.Hint"] = "Set a search engine friendly page name e.g. 'the-best-survey' to make your page URL 'http://www.yourStore.com/the-best-survey'. Leave empty to generate it automatically based on the name of the survey.",
                ["Admin.NopStation.DynamicSurvey.Surveys.Fields.EmailAccount.Standard"] = "Standard",

                ["Admin.NopStation.DynamicSurvey.Surveys.Added"] = "The new survey has been added successfully.",
                ["Admin.NopStation.DynamicSurvey.Surveys.AddNew"] = "Add a new survey",
                ["Admin.NopStation.DynamicSurvey.Surveys.BackToList"] = "back to survey list",
                ["Admin.NopStation.DynamicSurvey.Surveys.Copied"] = "The survey has been copied successfully",
                ["Admin.NopStation.DynamicSurvey.Surveys.Info"] = "Survey info",
                ["Admin.NopStation.DynamicSurvey.Surveys.Email"] = "Email",
                ["Admin.NopStation.DynamicSurvey.Surveys.Submissions"] = "Submissions",
                ["Admin.NopStation.DynamicSurvey.Surveys.Updated"] = "The survey has been updated successfully.",
                ["Admin.NopStation.DynamicSurvey.Surveys.Deleted"] = "The survey has been deleted successfully.",

                ["Admin.NopStation.DynamicSurvey.Surveys.List.SearchSurveyName"] = "Survey name",
                ["Admin.NopStation.DynamicSurvey.Surveys.List.SearchSurveyName.Hint"] = "A survey name.",
                ["Admin.NopStation.DynamicSurvey.Surveys.List.SearchPublished"] = "Published",
                ["Admin.NopStation.DynamicSurvey.Surveys.List.SearchPublished.All"] = "All",
                ["Admin.NopStation.DynamicSurvey.Surveys.List.SearchPublished.Hint"] = "Search by a \"Published\" property.",
                ["Admin.NopStation.DynamicSurvey.Surveys.List.SearchPublished.PublishedOnly"] = "Published only",
                ["Admin.NopStation.DynamicSurvey.Surveys.List.SearchPublished.UnpublishedOnly"] = "Unpublished only",
                ["Admin.NopStation.DynamicSurvey.Surveys.List.SearchStore"] = "Store",
                ["Admin.NopStation.DynamicSurvey.Surveys.List.SearchStore.Hint"] = "Search by a specific store.",

                ["Admin.NopStation.DynamicSurvey.SurveyAttributes.Fields.Description"] = "Description",
                ["Admin.NopStation.DynamicSurvey.SurveyAttributes.Fields.Description.Hint"] = "The description of the survey attribute.",
                ["Admin.NopStation.DynamicSurvey.SurveyAttributes.Fields.Name"] = "Name",
                ["Admin.NopStation.DynamicSurvey.SurveyAttributes.Fields.Name.Hint"] = "The name of the survey attribute.",
                ["Admin.NopStation.DynamicSurvey.SurveyAttributes.Fields.Name.Required"] = "Please provide a name.",

                ["Admin.NopStation.DynamicSurvey.SurveyAttributes"] = "Survey attributes",
                ["Admin.NopStation.DynamicSurvey.SurveyAttributes.Added"] = "The new attribute has been added successfully.",
                ["Admin.NopStation.DynamicSurvey.SurveyAttributes.AddNew"] = "Add a new survey attribute",
                ["Admin.NopStation.DynamicSurvey.SurveyAttributes.BackToList"] = "back to survey attribute list",
                ["Admin.NopStation.DynamicSurvey.SurveyAttributes.Deleted"] = "The attribute has been deleted successfully.",
                ["Admin.NopStation.DynamicSurvey.SurveyAttributes.Description"] = "Survey attributes are quantifiable or descriptive aspects of a survey (such as, color). For example, if you were to create an attribute for color, with the values of blue, green, yellow, and so on, you may want to apply this attribute to shirts, which you sell in various colors (you can adjust a price or weight for any of existing attribute values). You can add attributes to existing survey on a survey details page.",
                ["Admin.NopStation.DynamicSurvey.SurveyAttributes.EditAttributeDetails"] = "Edit survey attribute details",
                ["Admin.NopStation.DynamicSurvey.SurveyAttributes.Updated"] = "The attribute has been updated successfully.",
                ["Admin.NopStation.DynamicSurvey.SurveyAttributes.Info"] = "Info",

                ["Admin.NopStation.DynamicSurvey.SurveyAttributes.PredefinedValues"] = "Predefined values",
                ["Admin.NopStation.DynamicSurvey.SurveyAttributes.PredefinedValues.AddNew"] = "Add a new value",
                ["Admin.NopStation.DynamicSurvey.SurveyAttributes.PredefinedValues.EditValueDetails"] = "Edit value",
                ["Admin.NopStation.DynamicSurvey.SurveyAttributes.PredefinedValues.Fields.DisplayOrder"] = "Display order",
                ["Admin.NopStation.DynamicSurvey.SurveyAttributes.PredefinedValues.Fields.DisplayOrder.Hint"] = "The display order of the attribute value. 1 represents the first item in attribute value list.",
                ["Admin.NopStation.DynamicSurvey.SurveyAttributes.PredefinedValues.Fields.IsPreSelected"] = "Is pre-selected",
                ["Admin.NopStation.DynamicSurvey.SurveyAttributes.PredefinedValues.Fields.IsPreSelected.Hint"] = "Determines whether this attribute value is pre-selected for the customer.",
                ["Admin.NopStation.DynamicSurvey.SurveyAttributes.PredefinedValues.Fields.Name"] = "Name",
                ["Admin.NopStation.DynamicSurvey.SurveyAttributes.PredefinedValues.Fields.Name.Hint"] = "The attribute value name e.g. 'Blue' for Color attributes.",
                ["Admin.NopStation.DynamicSurvey.SurveyAttributes.PredefinedValues.Fields.Name.Required"] = "Please provide a name.",
                ["Admin.NopStation.DynamicSurvey.SurveyAttributes.PredefinedValues.Hint"] = "Predefined (default) values are helpful for a store owner when creating new surveys. Then when you add the attribute to a survey, you don't have to create the values again.",
                ["Admin.NopStation.DynamicSurvey.SurveyAttributes.PredefinedValues.SaveBeforeEdit"] = "You need to save the survey attribute before you can add values for this page.",

                ["Admin.NopStation.DynamicSurvey.SurveyAttributes.UsedBySurveys"] = "Used by surveys",
                ["Admin.NopStation.DynamicSurvey.SurveyAttributes.UsedBySurveys.Hint"] = "Here you can see a list of surveys which use this attribute.",
                ["Admin.NopStation.DynamicSurvey.SurveyAttributes.UsedBySurveys.Survey"] = "Survey",
                ["Admin.NopStation.DynamicSurvey.SurveyAttributes.UsedBySurveys.Published"] = "Published",

                ["Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes"] = "Attributes",
                ["Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.Added"] = "The new attribute has been added successfully.",
                ["Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.AddNew"] = "Add a new attribute",
                ["Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.AlreadyExists"] = "This attribute is already added to this survey",
                ["Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.BackToSurvey"] = "back to survey details",
                ["Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.Condition"] = "Condition",
                ["Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.Condition.Attributes"] = "Attribute",
                ["Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.Condition.Attributes.Hint"] = "Choose an attribute.",
                ["Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.Condition.Description"] = "Conditional attributes appear if a previous attribute is selected, such as having an option for personalizing clothing with a name and only providing the text input box if the \"Personalize\" radio button is checked",
                ["Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.Condition.EnableCondition"] = "Enable condition",
                ["Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.Condition.EnableCondition.Hint"] = "Check to specify a condition (depending on another attribute) when this attribute should be enabled (visible).",
                ["Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.Condition.SaveBeforeEdit"] = "You need to save the survey attribute before you can edit conditional attributes.",
                ["Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.Deleted"] = "The attribute has been deleted successfully.",
                ["Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.EditAttributeDetails"] = "Edit survey attribute",
                ["Admin.NopStation.DynamicSurvey.SurveyAttributes.Attributes.Values.Fields.Name.Required"] = "Name is required",
                ["Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.Fields.Attribute"] = "Attribute",
                ["Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.Fields.Attribute.Hint"] = "Choose an attribute.",
                ["Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.Fields.AttributeControlType"] = "Control type",
                ["Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.Fields.AttributeControlType.Hint"] = "Choose how to display your attribute values.",
                ["Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.Fields.DisplayOrder"] = "Display order",
                ["Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.Fields.DisplayOrder.Hint"] = "The attribute display order. 1 represents the first item in the list.",
                ["Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.Fields.IsRequired"] = "Is Required",
                ["Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.Fields.IsRequired.Hint"] = "When an attribute is required, the customer must choose an appropriate attribute value before they can continue.",
                ["Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.Fields.TextPrompt"] = "Text prompt",
                ["Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.Fields.TextPrompt.Hint"] = "Enter text prompt (you can leave it empty).",
                ["Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.Info"] = "Info",
                ["Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.Updated"] = "The attribute has been updated successfully.",

                ["Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.ValidationRules"] = "Validation rules",
                ["Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.ValidationRules.DefaultValue"] = "Default value",
                ["Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.ValidationRules.DefaultValue.Hint"] = "Enter default value for attribute.",
                ["Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.ValidationRules.FileAllowedExtensions"] = "Allowed file extensions",
                ["Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.ValidationRules.FileAllowedExtensions.Hint"] = "Specify a comma-separated list of allowed file extensions. Leave empty to allow any file extension. e.g. - doc,docx,pdf",
                ["Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.ValidationRules.FileMaximumSize"] = "Maximum file size (KB)",
                ["Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.ValidationRules.FileMaximumSize.Hint"] = "Specify maximum file size in kilobytes. Leave empty to skip this validation.",
                ["Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.ValidationRules.MaxLength"] = "Maximum length",
                ["Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.ValidationRules.MaxLength.Hint"] = "Specify maximum length. Leave empty to skip this validation.",
                ["Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.ValidationRules.MinLength"] = "Minimum length",
                ["Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.ValidationRules.MinLength.Hint"] = "Specify minimum length. Leave empty to skip this validation.",
                ["Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.ValidationRules.DefaultDate"] = "Default date",
                ["Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.ValidationRules.DefaultDate.Hint"] = "Enter default date for attribute.",
                ["Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.ValidationRules.MaxDate"] = "Maximum date",
                ["Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.ValidationRules.MaxDate.Hint"] = "Specify maximum date. Leave empty to skip this validation.",
                ["Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.ValidationRules.MinDate"] = "Minimum date",
                ["Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.ValidationRules.MinDate.Hint"] = "Specify minimum date. Leave empty to skip this validation.",
                ["Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.Values"] = "Values",
                ["Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.Values.AddNew"] = "Add a new value",
                ["Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.Values.AlreadyExistsInCombination"] = "This attribute value cannot be removed because it is already used in this combination: {0}.",
                ["Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.Values.EditValueDetails"] = "Edit value",
                ["Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.Values.Fields.ColorSquaresRgb"] = "RGB color",
                ["Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.Values.Fields.ColorSquaresRgb.Hint"] = "Choose color to be used with the color squares attribute control.",
                ["Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.Values.Fields.DisplayOrder"] = "Display order",
                ["Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.Values.Fields.DisplayOrder.Hint"] = "The display order of the attribute value. 1 represents the first item in attribute value list.",
                ["Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.Values.Fields.IsPreSelected"] = "Is pre-selected",
                ["Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.Values.Fields.IsPreSelected.Hint"] = "Determines whether this attribute value is pre-selected for the customer.",
                ["Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.Values.Fields.Name"] = "Name",
                ["Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.Values.Fields.Name.Hint"] = "The attribute value name e.g. 'Blue' for Color attributes.",
                ["Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.Values.Fields.Name.Required"] = "Please provide a name.",
                ["Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.Values.SaveBeforeEdit"] = "You need to save the survey attribute before you can add values for this survey attribute page.",
                ["Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.Hint"] = "Survey attributes are quantifiable or descriptive aspects of a survey (such as, color). For example, if you were to create an attribute for color, with the values of blue, green, yellow, and so on, you may want to apply this attribute to shirts, which you sell in various colors (you can adjust a price or weight for any of existing attribute values). " + "You can add attribute for your survey using existing list of attributes, or if you need to create a new one go to Nop Station &gt; Plugins &gt; Survey &gt; Attributes.",
                ["Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.NoAttributesAvailable"] = "No survey attributes available. Create at least one survey attribute before mapping.",
                ["Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.SaveBeforeEdit"] = "You need to save the survey before you can add attributes for this page.",

                ["Admin.NopStation.DynamicSurvey.SurveySubmissions.Fields.Attribute"] = "Attribute",
                ["Admin.NopStation.DynamicSurvey.SurveySubmissions.Guest"] = "Guest",
                ["Admin.NopStation.DynamicSurvey.Surveys.SurveyAttributes.Values.Fields.ImageSquaresPicture"] = "Picture",

                ["NopStation.DynamicSurvey.Surveys.SurveyAttributes.DropdownList.DefaultItem"] = "Please select",
                ["NopStation.DynamicSurvey.Surveys.SurveyAttributes.NotAvailable"] = "Not available",
                ["NopStation.DynamicSurvey.MinSubmissionInterval"] = "Please wait several seconds before creating a new response (already placed another response moments ago).",
                ["NopStation.DynamicSurvey.YourEnquiryHasBeenSent"] = "Your response has been successfully sent to the store owner.",
                ["NopStation.DynamicSurvey.YourEnquiryAlreadySent"] = "Your response already sent to the store owner.",
                ["NopStation.DynamicSurvey.Surveys.SubmitButton"] = "Submit",
                ["NopStation.DynamicSurvey.SelectAttribute"] = "Please select {0}",
                ["NopStation.DynamicSurvey.TextboxMaximumLength"] = "{0}: maximum length is {1} chars",
                ["NopStation.DynamicSurvey.TextboxMinimumLength"] = "{0}: minimum length is {1} chars",
                ["NopStation.DynamicSurvey.DatepicketMaximumDate"] = "{0}: maximum date is {1}",
                ["NopStation.DynamicSurvey.DatepicketMinimumDate"] = "{0}: minimum date is {1}",

                ["Admin.NopStation.DynamicSurvey.SurveySubmissions.Fields.CreatedOn"] = "Submitted on",
                ["Admin.NopStation.DynamicSurvey.SurveySubmissions.Fields.CreatedOn.Hint"] = "The time the submission was made",
                ["Admin.NopStation.DynamicSurvey.SurveySubmissions.Fields.CreatedOnFrom"] = "Submitted on from",
                ["Admin.NopStation.DynamicSurvey.SurveySubmissions.Fields.CreatedOnFrom.Hint"] = "Search from the date the submission was made",
                ["Admin.NopStation.DynamicSurvey.SurveySubmissions.Fields.CreatedOnTo"] = "Submitted on to",
                ["Admin.NopStation.DynamicSurvey.SurveySubmissions.Fields.CreatedOnTo.Hint"] = "Search to the date the submission was made",
                ["Admin.NopStation.DynamicSurvey.SurveySubmissions.Fields.CustomerEmail"] = "Customer email",
                ["Admin.NopStation.DynamicSurvey.SurveySubmissions.Fields.CustomerEmail.Hint"] = "Search by customer email",
                ["Admin.NopStation.DynamicSurvey.SurveySubmissions.Fields.SurveyId"] = "Survey",
                ["Admin.NopStation.DynamicSurvey.SurveySubmissions.Fields.SurveyId.Hint"] = "Search by survey",
                ["Admin.NopStation.DynamicSurvey.SurveySubmissions.Details"] = "Survey submission details",
                ["Admin.NopStation.DynamicSurvey.SurveySubmissions.BackToList"] = "back to submission list",
                ["Admin.NopStation.DynamicSurvey.SurveySubmissions.Fields.SurveyName"] = "Survey name",
                ["Admin.NopStation.DynamicSurvey.SurveySubmissions.Fields.SurveyName.Hint"] = "Name of the survey",
                ["Admin.NopStation.DynamicSurvey.SurveySubmissions.Deleted"] = "Survey submission(s) has been deleted successfully.",
                ["Admin.NopStation.DynamicSurvey.SurveySubmissions.Info"] = "Submission info",
                ["Admin.NopStation.DynamicSurvey.SurveySubmissions.UserData"] = "Submitted data"
            }.ToList();

            return list;
        }

        public Task<IList<string>> GetWidgetZonesAsync()
        {
            var zones = new List<string>
            {
                PublicWidgetZones.HeaderMenuAfter,
                PublicWidgetZones.MobHeaderMenuAfter
            };
            zones.AddRange(SurveyHelper.GetWidgetZones());

            return Task.FromResult<IList<string>>(zones);
        }

        public Type GetWidgetViewComponent(string widgetZone)
        {
            if (widgetZone == PublicWidgetZones.HeaderMenuAfter || widgetZone == PublicWidgetZones.MobHeaderMenuAfter)
                return typeof(DynamicSurveyMenuViewComponent);

            return typeof(DynamicSurveyViewComponent);
        }
    }
}