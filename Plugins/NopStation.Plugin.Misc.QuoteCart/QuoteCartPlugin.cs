using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Messages;
using Nop.Services.Cms;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Services;
using NopStation.Plugin.Misc.QuoteCart.Components;

namespace NopStation.Plugin.Misc.QuoteCart;

public class QuoteCartPlugin : BasePlugin, INopStationPlugin, IWidgetPlugin, IAdminMenuPlugin
{
    #region Fields

    private readonly ILocalizationService _localizationService;
    private readonly INopStationCoreService _nopStationCoreService;
    private readonly IPermissionService _permissionService;
    private readonly IWebHelper _webHelper;
    private readonly IMessageTemplateService _messageTemplateService;

    #endregion

    #region Ctor

    public QuoteCartPlugin(ILocalizationService localizationService,
        INopStationCoreService nopStationCoreService,
        IPermissionService permissionService,
        IWebHelper webHelper,
        IMessageTemplateService messageTemplateService)
    {
        _localizationService = localizationService;
        _nopStationCoreService = nopStationCoreService;
        _permissionService = permissionService;
        _webHelper = webHelper;
        _messageTemplateService = messageTemplateService;
    }

    #endregion

    #region Methods

    public override string GetConfigurationPageUrl()
    {
        return $"{_webHelper.GetStoreLocation()}Admin/QuoteCart/Configure";
    }

    public override async Task InstallAsync()
    {
        await this.InstallPluginAsync(new QuoteCartPermissionProvider());
        await SeedMessageTemplatesAsync();

        await base.InstallAsync();
    }

    public override async Task UninstallAsync()
    {
        await this.UninstallPluginAsync(new QuoteCartPermissionProvider());
        await base.UninstallAsync();
    }

    public async Task<IList<string>> GetWidgetZonesAsync()
    {
        await Task.CompletedTask;
        return [
            PublicWidgetZones.HeaderLinksAfter,
            PublicWidgetZones.ProductBoxAddinfoAfter,
            PublicWidgetZones.ProductDetailsOverviewBottom,
            PublicWidgetZones.AccountNavigationAfter
        ];
    }

    public static string GetWidgetViewComponentName(string widgetZone)
    {
        if (widgetZone == PublicWidgetZones.HeaderLinksAfter)
        {
            return QuoteCartDefaults.FLYOUT_QUOTE_CART_COMPONENT_NAME;
        }
        else if (widgetZone == PublicWidgetZones.ProductBoxAddinfoAfter)
        {
            return QuoteCartDefaults.ADD_QUOTE_COMPONENT_NAME;
        }
        else if (widgetZone == PublicWidgetZones.AccountNavigationAfter)
        {
            return QuoteCartDefaults.MY_REQUESTS_COMPONENT_NAME;
        }
        else if (widgetZone == PublicWidgetZones.AccountNavigationAfter)
        {
            return QuoteCartDefaults.MY_REQUESTS_COMPONENT_NAME;
        }
        else if (widgetZone == PublicWidgetZones.ProductDetailsOverviewBottom)
        {
            return QuoteCartDefaults.ADD_QUOTE_COMPONENT_NAME;
        }
        else
        {
            throw new NopException($"Widget zone {widgetZone} is not supported");
        }
    }

    public Type GetWidgetViewComponent(string widgetZone)
    {
        if (widgetZone == PublicWidgetZones.HeaderLinksAfter)
        {
            return typeof(FlyoutCartViewComponent);
        }
        else if (widgetZone == PublicWidgetZones.ProductBoxAddinfoAfter)
        {
            return typeof(AddQuoteViewComponent);
        }
        else if (widgetZone == PublicWidgetZones.AccountNavigationAfter)
        {
            return typeof(MyRequestsViewComponent);
        }
        else if (widgetZone == PublicWidgetZones.AccountNavigationAfter)
        {
            return typeof(MyRequestsViewComponent);
        }
        else if (widgetZone == PublicWidgetZones.ProductDetailsOverviewBottom)
        {
            return typeof(AddQuoteViewComponent);
        }
        else
        {
            throw new NopException($"Widget zone {widgetZone} is not supported");
        }
    }

    public async Task ManageSiteMapAsync(SiteMapNode rootNode)
    {
        var canManageConfiguration = await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.ManageConfiguration);
        var canManageForm = await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.ManageQuoteCartForm);
        var canManageRequest = await _permissionService.AuthorizeAsync(QuoteCartPermissionProvider.ManageQuoteRequest);

        if (!canManageConfiguration && !canManageForm && !canManageRequest)
            return;

        var menuItem = new SiteMapNode()
        {
            Title = await _localizationService.GetResourceAsync("Admin.NopStation.Plugin.Misc.QuoteCart.Menu.QuoteCart"),
            Visible = true,
            IconClass = "far fa-dot-circle",
        };

        if (canManageForm)
        {
            var formNode = new SiteMapNode()
            {
                Visible = true,
                IconClass = "far fa-circle",
                Url = "~/Admin/QuoteForm/List",
                SystemName = "QuoteCart.Forms",
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.Plugin.Misc.QuoteCart.Menu.QuoteCart.Forms"),
            };
            menuItem.ChildNodes.Add(formNode);

            var formAttributeNode = new SiteMapNode()
            {
                Visible = true,
                IconClass = "far fa-circle",
                Url = "~/Admin/FormAttribute/List",
                SystemName = "QuoteCart.FormAttributes",
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.Plugin.Misc.QuoteCart.Menu.QuoteCart.FormAttributes"),
            };
            menuItem.ChildNodes.Add(formAttributeNode);
        }

        if (canManageRequest)
        {
            var qtRequest = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.Plugin.Misc.QuoteCart.Menu.QuoteCart.QuoteRequests"),
                Url = "~/Admin/QCQuoteRequest/List",
                Visible = true,
                IconClass = "far fa-circle",
                SystemName = "QuoteCart.Requests"
            };
            menuItem.ChildNodes.Add(qtRequest);
        }

        if (canManageConfiguration)
        {
            var subNode = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.Plugin.Misc.QuoteCart.Menu.QuoteCart.MessageTemplates"),
                Url = "~/Admin/QuoteMessageTemplate/List",
                Visible = true,
                IconClass = "far fa-circle",
                SystemName = "QuoteCart.MessageTemplates"
            };
            menuItem.ChildNodes.Add(subNode);
        }

        if (canManageConfiguration)
        {

            var configItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.Plugin.Misc.QuoteCart.Menu.QuoteCart.Configuration"),
                Url = GetConfigurationPageUrl(),
                Visible = true,
                IconClass = "far fa-circle",
                SystemName = "QuoteCart.Configuration"
            };
            menuItem.ChildNodes.Add(configItem);
        }

        if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
        {
            var documentation = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                Url = "https://www.nop-station.com/quote-cart-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=quote-cart",
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
        var resources = new Dictionary<string, string>
        {
            // menu
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Menu.QuoteCart"] = "Quote cart",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Menu.QuoteCart.Forms"] = "Forms",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Menu.QuoteCart.Configuration"] = "Configuration",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Menu.QuoteCart.QuoteRequests"] = "Quote requests",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Menu.QuoteCart.MessageTemplates"] = "Message templates",

            // configuration
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Configuration"] = "Configuration",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Configuration.Updated"] = "Quote cart configuration updated successfully.",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Configuration.BlockTitle.Common"] = "Common",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Configuration.Fields.EnableQuoteCart"] = "Enable quote cart",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Configuration.Fields.EnableQuoteCart.Hint"] = "Enable quote cart functionality",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Configuration.Fields.EnableWhitelist"] = "Enable whitelist",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Configuration.Fields.EnableWhitelist.Hint"] = "Check to only allow whitelisted products to be added to quote cart",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Configuration.Fields.MaxQuoteItemCount"] = "Item limit per request",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Configuration.Fields.MaxQuoteItemCount.Hint"] = "Maximum number of item that can be added to quote cart. 0 (zero) for no limit",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Configuration.Fields.WhitelistAllProducts"] = "Whitelist all products",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Configuration.Fields.WhitelistAllProducts.Hint"] = "Check to whitelist all products",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Configuration.Fields.WhitelistAllCategories"] = "Whitelist all categories",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Configuration.Fields.WhitelistAllCategories.Hint"] = "Check to whitelist all categories",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Configuration.Fields.WhitelistAllManufacturers"] = "Whitelist all manufacturers",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Configuration.Fields.WhitelistAllManufacturers.Hint"] = "Check to whitelist all manufacturers",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Configuration.Fields.WhitelistAllVendors"] = "Whitelist all vendors",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Configuration.Fields.WhitelistAllVendors.Hint"] = "Check to whitelist all vendors",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Configuration.Fields.SelectedCustomerRoleIds"] = "Allowed customer roles",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Configuration.Fields.SelectedCustomerRoleIds.Hint"] = "Allow only selected customer roles to quote.",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Configuration.Fields.ClearCartAfterSubmission"] = "Clear cart after submission",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Configuration.Fields.ClearCartAfterSubmission.Hint"] = "Clear quote cart items after submitting quote",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Configuration.Fields.CustomerCanEnterPrice"] = "Allow customer to enter price",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Configuration.Fields.CustomerCanEnterPrice.Hint"] = "Allow customer to enter a custom price for negotiation.",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Configuration.Fields.CustomerCanCancelQuote"] = "Allow customer to cancel quote",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Configuration.Fields.CustomerCanCancelQuote.Hint"] = "Allow customer to cancel an existing quote request.",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Configuration.BlockTitle.WhitelistedProducts"] = "Whitelisted products",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Configuration.BlockTitle.WhitelistedCategories"] = "Whitelisted categories",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Configuration.BlockTitle.WhitelistedManufacturers"] = "Whitelisted manufacturers",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Configuration.BlockTitle.WhitelistedVendors"] = "Whitelisted vendors",

            // quote form
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Form.Fields.SearchActive"] = "Search active",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Form.Fields.SearchActive.Hint"] = "Search by active status",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.List.SearchActive.Active"] = "Active",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.List.SearchActive.Inactive"] = "Inactive",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Form.Fields.SearchStore"] = "Search store",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Form.Fields.SearchStore.Hint"] = "Search by store",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Form.Buttons.BackToList"] = "back to form list",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Form.AddNew"] = "Add new form",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Form.Tab.Info"] = "Info",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Form.Tab.FormFields"] = "Form fields",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Form.Fields.Title"] = "Title",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Form.Fields.Title.Hint"] = "Title of the form",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Form.Fields.Info"] = "Info",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Form.Fields.Info.Hint"] = "Info of the form to show bellow title",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Form.Fields.IsActive"] = "Active",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Form.Fields.IsActive.Hint"] = "Check to activate the form",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Form.Fields.DisplayOrder"] = "Display order",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Form.Fields.DisplayOrder.Hint"] = "The order this form will be shown",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Form.Fields.SendEmailToStoreOwner"] = "Send email to store owner",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Form.Fields.SendEmailToStoreOwner.Hint"] = "Send email to store owner on new request and reply",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Form.Fields.SendEmailToCustomer"] = "Send email to customer",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Form.Fields.SendEmailToCustomer.Hint"] = "Send email to customer on status change and reply",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Form.Fields.DefaultEmailAccountId"] = "Default email account",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Form.Fields.DefaultEmailAccountId.Hint"] = "Default email account that will be used to send emails",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Form.Fields.SubmitButtonText"] = "Submit button text",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Form.Fields.SubmitButtonText.Hint"] = "Text to be shown on submit button",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Form.Fields.StoreOwnerEmail"] = "Store owner email",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Form.Fields.StoreOwnerEmail.Hint"] = "Email that will be sent the updated for store owner",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Form.SelectedStoreIds"] = "Stores",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Form.SelectedStoreIds.Hint"] = "Stores that will be shown this form",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Form.Fields.ShowTermsAndConditionsCheckbox"] = "Show terms and conditions checkbox",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Form.Fields.ShowTermsAndConditionsCheckbox.Hint"] = "Show terms and conditions checkbox bellow form",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Form.Fields.TermsAndConditions"] = "Terms and conditions content",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Form.Fields.TermsAndConditions.Hint"] = "Terms and conditions to be shown as popup",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.BackToList"] = "Back to list",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.EditDetails"] = "Edit details",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Form.FormSaved"] = "Your form has been saved.",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Form.FormError"] = "Unable to saved your form.",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Form.Updated"] = "The form has been updated",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Form.Tab.SaveFormFirst"] = "You need to save your form before you can edit.",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Form.Deleted"] = "The form has been deleted.",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Form.Created"] = "The form has been created.",

            //quote forms
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Menu.QuoteCart.FormAttributes"] = "Form attributes",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Forms.Added"] = "The new form has been added successfully.",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Forms.AddNew"] = "Add a new form",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Forms.BackToList"] = "back to form list",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Forms.Copied"] = "The form has been copied successfully",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Forms.Info"] = "Form info",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Forms.Email"] = "Email",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Forms.Submissions"] = "Submissions",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Forms.Updated"] = "The form has been updated successfully.",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Forms.Deleted"] = "The form has been deleted successfully.",

            ["Admin.NopStation.Plugin.Misc.QuoteCart.Forms.List.SearchFormName"] = "Form name",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Forms.List.SearchFormName.Hint"] = "Name of the form",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Forms.List.SearchPublished"] = "Published",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Forms.List.SearchPublished.All"] = "All",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Forms.List.SearchPublished.Hint"] = "Search by a \"Published\" property.",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Forms.List.SearchPublished.PublishedOnly"] = "Published only",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Forms.List.SearchPublished.UnpublishedOnly"] = "Unpublished only",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Forms.List.SearchStore"] = "Store",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Forms.List.SearchStore.Hint"] = "Search by a specific store.",

            ["Admin.NopStation.Plugin.Misc.QuoteCart.FormAttributes.Fields.Description"] = "Description",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.FormAttributes.Fields.Description.Hint"] = "The description of the form attribute.",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.FormAttributes.Fields.Name"] = "Name",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.FormAttributes.Fields.Name.Hint"] = "The name of the form attribute.",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.FormAttributes.Fields.Name.Required"] = "Please provide a name.",

            ["Admin.NopStation.Plugin.Misc.QuoteCart.FormAttributes"] = "Form attributes",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.FormAttributes.Added"] = "The new attribute has been added successfully.",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.FormAttributes.AddNew"] = "Add a new form attribute",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.FormAttributes.BackToList"] = "back to form attribute list",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.FormAttributes.Deleted"] = "The attribute has been deleted successfully.",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.FormAttributes.Description"] = "Form attributes are quantifiable or descriptive aspects of a form (such as, color). For example, if you were to create an attribute for color, with the values of blue, green, yellow, and so on, you may want to apply this attribute to shirts, which you sell in various colors (you can adjust a price or weight for any of existing attribute values). You can add attributes to existing form on a form details page.",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.FormAttributes.EditAttributeDetails"] = "Edit form attribute details",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.FormAttributes.Updated"] = "The attribute has been updated successfully.",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.FormAttributes.Info"] = "Info",

            ["Admin.NopStation.Plugin.Misc.QuoteCart.FormAttributes.PredefinedValues"] = "Predefined values",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.FormAttributes.PredefinedValues.AddNew"] = "Add a new value",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.FormAttributes.PredefinedValues.EditValueDetails"] = "Edit value",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.FormAttributes.PredefinedValues.Fields.DisplayOrder"] = "Display order",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.FormAttributes.PredefinedValues.Fields.DisplayOrder.Hint"] = "The display order of the attribute value. 1 represents the first item in attribute value list.",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.FormAttributes.PredefinedValues.Fields.IsPreSelected"] = "Is pre-selected",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.FormAttributes.PredefinedValues.Fields.IsPreSelected.Hint"] = "Determines whether this attribute value is pre-selected for the customer.",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.FormAttributes.PredefinedValues.Fields.Name"] = "Name",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.FormAttributes.PredefinedValues.Fields.Name.Hint"] = "The attribute value name e.g. 'Blue' for Color attributes.",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.FormAttributes.PredefinedValues.Fields.Name.Required"] = "Please provide a name.",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.FormAttributes.PredefinedValues.Hint"] = "Predefined (default) values are helpful for a store owner when creating new forms. Then when you add the attribute to a form, you don't have to create the values again.",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.FormAttributes.PredefinedValues.SaveBeforeEdit"] = "You need to save the form attribute before you can add values for this page.",

            ["Admin.NopStation.Plugin.Misc.QuoteCart.FormAttributes.UsedByForms"] = "Used by forms",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.FormAttributes.UsedByForms.Hint"] = "Here you can see a list of forms which use this attribute.",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.FormAttributes.UsedByForms.Form"] = "Form",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.FormAttributes.UsedByForms.Published"] = "Published",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.Values.Fields.ImageSquaresPicture"] = "Picture",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.Values.Fields.ImageSquaresPicture.Hint"] = "Picture to be used in image square",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes"] = "Attributes",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.Added"] = "The new attribute has been added successfully.",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.AddNew"] = "Add a new attribute",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.AlreadyExists"] = "This attribute is already added to this form",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.BackToForm"] = "back to form details",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.Condition"] = "Condition",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.Condition.Attributes"] = "Attribute",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.Condition.Attributes.Hint"] = "Choose an attribute.",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.Condition.Description"] = "Conditional attributes appear if a previous attribute is selected, such as having an option for personalizing clothing with a name and only providing the text input box if the \"Personalize\" radio button is checked",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.Condition.EnableCondition"] = "Enable condition",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.Condition.EnableCondition.Hint"] = "Check to specify a condition (depending on another attribute) when this attribute should be enabled (visible).",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.Condition.SaveBeforeEdit"] = "You need to save the form attribute before you can edit conditional attributes.",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.Deleted"] = "The attribute has been deleted successfully.",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.EditAttributeDetails"] = "Edit form attribute",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.Fields.Attribute"] = "Attribute",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.Fields.Attribute.Hint"] = "Choose an attribute.",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.Fields.AttributeControlType"] = "Control type",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.Fields.AttributeControlType.Hint"] = "Choose how to display your attribute values.",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.Fields.DisplayOrder"] = "Display order",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.Fields.DisplayOrder.Hint"] = "The attribute display order. 1 represents the first item in the list.",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.Fields.IsRequired"] = "Is Required",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.Fields.IsRequired.Hint"] = "When an attribute is required, the customer must choose an appropriate attribute value before they can continue.",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.Fields.TextPrompt"] = "Text prompt",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.Fields.TextPrompt.Hint"] = "Enter text prompt (you can leave it empty).",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.Info"] = "Info",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.Updated"] = "The attribute has been updated successfully.",

            ["Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.ValidationRules"] = "Validation rules",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.ValidationRules.DefaultValue"] = "Default value",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.ValidationRules.DefaultValue.Hint"] = "Enter default value for attribute.",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.ValidationRules.FileAllowedExtensions"] = "Allowed file extensions",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.ValidationRules.FileAllowedExtensions.Hint"] = "Specify a comma-separated list of allowed file extensions. Leave empty to allow any file extension.",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.ValidationRules.FileMaximumSize"] = "Maximum file size (KB)",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.ValidationRules.FileMaximumSize.Hint"] = "Specify maximum file size in kilobytes. Leave empty to skip this validation.",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.ValidationRules.MaxLength"] = "Maximum length",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.ValidationRules.MaxLength.Hint"] = "Specify maximum length. Leave empty to skip this validation.",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.ValidationRules.MinLength"] = "Minimum length",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.ValidationRules.MinLength.Hint"] = "Specify minimum length. Leave empty to skip this validation.",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.ValidationRules.DefaultDate"] = "Default date",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.ValidationRules.DefaultDate.Hint"] = "Enter default date for attribute.",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.ValidationRules.MaxDate"] = "Maximum date",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.ValidationRules.MaxDate.Hint"] = "Specify maximum date. Leave empty to skip this validation.",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.ValidationRules.MinDate"] = "Minimum date",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.ValidationRules.MinDate.Hint"] = "Specify minimum date. Leave empty to skip this validation.",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.Values"] = "Values",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.Values.AddNew"] = "Add a new value",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.Values.AlreadyExistsInCombination"] = "This attribute value cannot be removed because it is already used in this combination: {0}.",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.Values.EditValueDetails"] = "Edit value",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.Values.Fields.ColorSquaresRgb"] = "RGB color",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.Values.Fields.ColorSquaresRgb.Hint"] = "Choose color to be used with the color squares attribute control.",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.Values.Fields.DisplayOrder"] = "Display order",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.Values.Fields.DisplayOrder.Hint"] = "The display order of the attribute value. 1 represents the first item in attribute value list.",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.Values.Fields.IsPreSelected"] = "Is pre-selected",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.Values.Fields.IsPreSelected.Hint"] = "Determines whether this attribute value is pre-selected for the customer.",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.Values.Fields.Name"] = "Name",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.Values.Fields.Name.Hint"] = "The attribute value name e.g. 'Blue' for Color attributes.",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.Values.Fields.Name.Required"] = "Please provide a name.",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.Values.SaveBeforeEdit"] = "You need to save the form attribute before you can add values for this form attribute page.",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.Hint"] = "Form attributes are quantifiable or descriptive aspects of a form (such as, color). For example, if you were to create an attribute for color, with the values of blue, green, yellow, and so on, you may want to apply this attribute to shirts, which you sell in various colors (you can adjust a price or weight for any of existing attribute values). " + "You can add attribute for your form using existing list of attributes, or if you need to create a new one go to Nop Station &gt; Plugins &gt; Form &gt; Attributes.",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.NoAttributesAvailable"] = "No form attributes available. Create at least one form attribute before mapping.",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.Forms.FormAttributes.SaveBeforeEdit"] = "You need to save the form before you can add attributes for this page.",

            // quote request
            ["Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequests.EditRequestDetails"] = "Edit quote request",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequest.ManageQuoteRequest"] = "Edit quote request - {0}",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequests.RequestDetails"] = "Request overview",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.FormattedGuestEmail"] = "{0} (Guest)",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequests.BackToList"] = "Back to request list",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequests.Fields.GoDirectlyToCustomRequestNumber"] = "Go directly to request #",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequests.Fields.GoDirectlyToCustomRequestNumber.Hint"] = "Go directly to request #.",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequests.Fields.Id"] = "Request #",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequests.Fields.Id.Hint"] = "Unique number of the request",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequests.Fields.RequestGuid"] = "Request GUID",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequests.Fields.RequestGuid.Hint"] = "Internal reference of request",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequests.Fields.Customer"] = "Customer",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequests.CustomerNotFound"] = "Customer not found",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequests.NotFound"] = "Quote request not found",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequests.Fields.Customer.Hint"] = "Customer that placed the request",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequests.Fields.Store"] = "Store",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequests.Fields.Store.Hint"] = "Store the request belongs to",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequests.Fields.RequestStatus"] = "Status",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequests.Fields.RequestStatus.Hint"] = "Status of the request",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequests.Fields.RequestType"] = "Request type",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequests.Fields.RequestType.Hint"] = "Type of request",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequests.Fields.ShareQuote"] = "Share quote",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequests.Fields.ShareQuote.Hint"] = "Share quote using link",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequests.Fields.CreatedOnUtc"] = "Created on",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequests.Fields.CreatedOnUtc.Hint"] = "Time the request has been created",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequests.Fields.UpdatedOnUtc"] = "Updated on",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequests.Fields.UpdatedOnUtc.Hint"] = "Time the order has been updated",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequests.Fields.StartDate"] = "Start date",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequests.Fields.StartDate.Hint"] = "Search starting from chosen date",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequests.Fields.EndDate"] = "End date",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequests.Fields.EndDate.Hint"] = "Search upto chosen date",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequests.Fields.FormId"] = "Search form",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequests.Fields.FormId.Hint"] = "Search by form of the request",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequests.Fields.CustomerEmail"] = "Search customer email",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequests.Fields.CustomerEmail.Hint"] = "Search by email of the customer",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequests.Fields.UserData.Hint"] = "User input for '{0}' field",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequests.Manage"] = "Manage quote requests",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequests.SendResponse"] = "Send response",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequests.Send"] = "Send",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequests.WriteResponseMessage"] = "Write response message",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequests.SendQuote"] = "Send quote",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequests.RestoreOriginal"] = "Restore original",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequests.RequestStatus.Cancel"] = "Cancel request",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequests.RequestStatus.Change"] = "Change status",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequests.GoToProductList"] = "Select from product list",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequests.ConvertToOrder"] = "Convert to order",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequests.Messages.You"] = "You",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequests.SendQuote.SendToStore"] = "Send to store owner",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequests.SendQuote.SendToCustomer"] = "Send to customer",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequests.CustomerInformation"] = "Customer information",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequests.Messages"] = "Messages",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequests.CartItems"] = "Quote cart items",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequests.SubmittedInformation"] = "Submitted information",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequests.Fields.ProductSku.Required"] = "Product is required",

            // request item
            ["Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequests.AddRequestItem"] = "Add item to quote request #{0}",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequests.EditRequestItem"] = "Edit quote request item",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequest.BackToRequest"] = "back to quote request",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequestItem.Fields.ProductId"] = "Product",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequestItem.Fields.ProductId.Hint"] = "Product for the quote request item",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequestItem.Fields.ProductSku"] = "Product Sku",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequestItem.Fields.ProductSku.Hint"] = "Sku of the product",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequestItem.Fields.ProductPrice"] = "Unit price",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequestItem.Fields.ProductPrice.Hint"] = "Unit price of the product",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequestItem.Fields.Quantity"] = "Quantity",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequestItem.Fields.Quantity.Hint"] = "Quantity of the requested item",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequestItem.Fields.DiscountedPrice"] = "Discounted price",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequestItem.Fields.DiscountedPrice.Hint"] = "Unit price of the product",

            // convert to order
            ["Admin.NopStation.Plugin.Misc.QuoteCart.ConvertToOrder.Addresses"] = "Addresses",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.ConvertToOrder.CartItems"] = "Cart items",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.ConvertToOrder.OrderInfo"] = "Order info",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.ConvertToOrder.ShippingMethodId"] = "Shipping option",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.ConvertToOrder.ShippingMethodId.Hint"] = "Shipping option to be used for the order",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.ConvertToOrder.OrderShippingFee"] = "Shipping fee",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.ConvertToOrder.OrderShippingFee.Hint"] = "Fee for shipping to be applied on the order",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.ConvertToOrder.PaymentMethodSystemName"] = "Payment method",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.ConvertToOrder.PaymentMethodSystemName.Hint"] = "Payment method to be used for the order",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.ConvertToOrder.PaymentMethodAdditionalFee"] = "Payment method additional fee",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.ConvertToOrder.PaymentMethodAdditionalFee.Hint"] = "Additional fee for payment to be applied on the order",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.ConvertToOrder.BillingAddress"] = "Billing address",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.ConvertToOrder.BillingAddress.Hint"] = "Billing address to be used for the order",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.ConvertToOrder.ShippingAddress"] = "Shipping address",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.ConvertToOrder.ShippingAddress.Hint"] = "Shipping address to be used for the order",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.ConvertToOrder.ShipToSameAddress"] = "Ship to same address",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.ConvertToOrder.ShipToSameAddress.Hint"] = "Ship to same address",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.ConvertToOrder.MarkAsPaid"] = "Mark as paid",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.ConvertToOrder.MarkAsPaid.Hint"] = "Mark the order as paid",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.ConvertToOrder.NewBillingAddress"] = "New billing address",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.ConvertToOrder.NewShippingAddress"] = "New shipping address",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.ConvertToOrder.PickUpInStore"] = "Pickup in store",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.ConvertToOrder.PickUpInStore.Hint"] = "Allow the customer to pickup from store",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.ConvertToOrder.ShippingRateComputationMethodSystemName"] = "Shipping provider",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.ConvertToOrder.ShippingRateComputationMethodSystemName.Hint"] = "Shipping method to use for the order",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.ConvertToOrder.AdditionalCharge"] = "Additional charge",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.ConvertToOrder.AdditionalCharge.Hint"] = "Additional charge to apply on order",
            ["Admin.NopStation.Plugin.Misc.QuoteCart.ConvertedToOrder.Success"] = "The quote request has been successfully converted to order.",

            // public quote cart
            ["NopStation.Plugin.Misc.QuoteCart.Cart.Flyout"] = "Quote cart ({0})",
            ["NopStation.Plugin.Misc.QuoteCart.Cart.AddButton"] = "Add to quote cart",
            ["NopStation.Plugin.Misc.QuoteCart.Cart.Warnings.QtyError"] = "Quantity should be greater than 0",
            ["NopStation.Plugin.Misc.QuoteCart.Cart.Added"] = "The product has been added to <a href='{0}'>quote cart</a>.",
            ["NopStation.Plugin.Misc.QuoteCart.Cart.FailToAdd"] = "The product could not be added to quote cart.",
            ["NopStation.Plugin.Misc.QuoteCart.Cart.FailToRemove"] = "The product could not be added to quote cart.",
            ["NopStation.Plugin.Misc.QuoteCart.QuoteCart"] = "Quote cart",
            ["NopStation.Plugin.Misc.QuoteCart.Cart.CartIsEmpty"] = "Your quote cart is empty!",
            ["NopStation.Plugin.Misc.QuoteCart.Cart.SKU"] = "SKU",
            ["NopStation.Plugin.Misc.QuoteCart.Cart.Image"] = "Image",
            ["NopStation.Plugin.Misc.QuoteCart.Cart.ProductName"] = "Product Name",
            ["NopStation.Plugin.Misc.QuoteCart.Cart.Product(s)"] = "Product(s)",
            ["NopStation.Plugin.Misc.QuoteCart.Cart.UnitPrice"] = "Price",
            ["NopStation.Plugin.Misc.QuoteCart.Cart.DiscountedPrice"] = "Discounted price",
            ["NopStation.Plugin.Misc.QuoteCart.Cart.Quantity"] = "Qty.",
            ["NopStation.Plugin.Misc.QuoteCart.Cart.ItemTotal"] = "Total",
            ["NopStation.Plugin.Misc.QuoteCart.Cart.Remove"] = "Remove",
            ["NopStation.Plugin.Misc.QuoteCart.Cart.SubTotal"] = "Subtotal",
            ["NopStation.Plugin.Misc.QuoteCart.Cart.Total"] = "Total",
            ["NopStation.Plugin.Misc.QuoteCart.QuoteRequest.Added"] = "Quote submitted",
            ["NopStation.Plugin.Misc.QuoteCart.QuoteRequest.Added.Message"] = "Your quote request has been recorded.",
            ["NopStation.Plugin.Misc.QuoteCart.QuoteRequest.FailedToAdd"] = "Your quote could not be added.",
            ["NopStation.Plugin.Misc.QuoteCart.Form.Buttons.SubmitQuote"] = "Submit Quote",
            ["NopStation.QuoteCart.FormAttribute.ValidationError.Required"] = "The '{0}' is required",
            ["NopStation.QuoteCart.FormAttribute.ValidationError.ReadOnly"] = "You can not change read-only values",
            ["NopStation.QuoteCart.FormAttribute.ValidationError.TextboxMinimumLength"] = "The '{0}' field can not have less than {1} characters",
            ["NopStation.QuoteCart.FormAttribute.ValidationError.TextboxMaximumLength"] = "The '{0}' field can not have more than {1} characters",
            ["NopStation.QuoteCart.FormAttribute.ValidationError.DatePickerMinimumDate"] = "The '{0}' field can not have date before {1}",
            ["NopStation.QuoteCart.FormAttribute.ValidationError.DatePickerMaximumDate"] = "The '{0}' field can not have date after {1}",
            ["NopStation.Plugin.Misc.QuoteCart.QuoteForm.Fields.GuestEmail"] = "Email",
            ["NopStation.Plugin.Misc.QuoteCart.QuoteForm.Fields.GuestEmail.Hint"] = "Email address for guests",
            ["NopStation.Plugin.Misc.QuoteCart.FormAttributes.DropdownList.DefaultItem"] = "Select option",
            ["NopStation.Plugin.Misc.QuoteCart.FormAttributes.NotAvailable"] = "Not available",
            ["NopStation.Plugin.Misc.QuoteCart.Cart.Warnings.MaxQuoteItemCount"] = "A maximum of {0} items can be added to quote cart.",

            // public quote request
            ["NopStation.Plugin.Misc.QuoteCart.QuoteRequests.MessageYou"] = "You",
            ["NopStation.Plugin.Misc.QuoteCart.QuoteRequests.WelcomeCustomer"] = "Dear customer",
            ["NopStation.Plugin.Misc.QuoteCart.QuoteRequests.ThankYou"] = "Thank you",
            ["NopStation.Plugin.Misc.QuoteCart.QuoteRequests.CustomerQuoteRequests"] = "Quote Requests",
            ["NopStation.Plugin.Misc.QuoteCart.QuoteRequests.RequestDetails"] = "Details",
            ["NopStation.Plugin.Misc.QuoteCart.QuoteRequests.RequestOverview"] = "Quote request details",
            ["NopStation.Plugin.Misc.QuoteCart.QuoteRequests.Fields.OrderStatus"] = "Order status",
            ["NopStation.Plugin.Misc.QuoteCart.QuoteRequests.Fields.RequestStatus"] = "Request status",
            ["NopStation.Plugin.Misc.QuoteCart.QuoteRequests.Fields.ProductsOverview"] = "{0} and {1} more products",
            ["NopStation.Plugin.Misc.QuoteCart.QuoteRequests.RequestStatus"] = "Request Status",
            ["NopStation.Plugin.Misc.QuoteCart.QuoteRequests.RequestTotal"] = "Estimated Totals",
            ["NopStation.Plugin.Misc.QuoteCart.QuoteRequests.RequestType"] = "Request Type",
            ["NopStation.Plugin.Misc.QuoteCart.QuoteRequests.RequestDate"] = "Request Date",
            ["NopStation.Plugin.Misc.QuoteCart.QuoteRequests.NoRequests"] = "No requests!",
            ["NopStation.Plugin.Misc.QuoteCart.QuoteRequests.RequestInformation"] = "Request information",
            ["NopStation.Plugin.Misc.QuoteCart.QuoteRequests.RequestNumber"] = "Request Number",
            ["NopStation.Plugin.Misc.QuoteCart.QuoteRequests.QuoteConversation"] = "Conversation",
            ["NopStation.Plugin.Misc.QuoteCart.QuoteRequests.QuoteConversationStart"] = "Start a new conversation for this quote request",
            ["NopStation.Plugin.Misc.QuoteCart.QuoteRequests.ItemLimitExceeded"] = "At most {0} items can be added.",
            ["NopStation.Plugin.Misc.QuoteCart.QuoteRequest.RequestInformation"] = "Quote request has been successfully processed.",
            ["NopStation.Plugin.Misc.QuoteCart.QuoteRequest.GoToQuote"] = "Go to quote request",
            ["NopStation.Plugin.Misc.QuoteCart.QuoteRequest.CopiedToClipboard"] = "Copied to clipboard.",
            ["Admin.Customers.Customers.Fields.FullName"] = "Customer name",
            ["Admin.Customers.Customers.Fields.FullName.Hint"] = "Name of the customer",
            ["NopStation.Plugin.Misc.QuoteCart.Remove"] = "Remove",
            ["NopStation.Plugin.Misc.QuoteCart.List.SearchActive.Manage"] = "Manage quote requests",
            ["NopStation.Plugin.Misc.QuoteCart.GuestEmail.Required"] = "Email is required !",
            ["NopStation.Plugin.Misc.QuoteCart.QuoteRequests.SendResponse"] = "Send response",
            ["NopStation.Plugin.Misc.QuoteCart.UpdateCart"] = "Update quote cart",
            // Message template descriptions
            [MESSAGE_TEMPLATE_DESCRIPTION_PREFIX + QuoteCartDefaults.REQUEST_CUSTOMER_STATUS_CHANGED_NOTIFICATION] = "This message template is used when status of a certain quote request updated by admin. The message is received by the customer that sent the request. You can set up this option by ticking the checkbox <b>Send email to customer</b> in QuoteCart - Forms - Edit.",
            [MESSAGE_TEMPLATE_DESCRIPTION_PREFIX + QuoteCartDefaults.REQUEST_NEW_REPLY_CUSTOMER_NOTIFICATION] = "This message template is used when a new message to the certain quote request is added by admin. The message is received by the customer that sent the request. You can set up this option by ticking the checkbox <b>Send email to customer</b> in QuoteCart - Forms - Edit.",
            [MESSAGE_TEMPLATE_DESCRIPTION_PREFIX + QuoteCartDefaults.REQUEST_NEW_REPLY_STORE_NOTIFICATION] = "This message template is used when a new message to the certain quote request is added by customer. The message is received by a store owner. You can set up this option by ticking the checkbox <b>Send email to store owner</b> in QuoteCart - Forms - Edit.",
            [MESSAGE_TEMPLATE_DESCRIPTION_PREFIX + QuoteCartDefaults.QUOTE_CUSTOMER_QUOTE_OFFER] = "This message template is used when the admin sends quote offer to author of the quote request. The message is received by the customer that sent the request. You can send this email by using the <b>Send quote</b> button in QuoteCart - Quote requests - Edit.",
            [MESSAGE_TEMPLATE_DESCRIPTION_PREFIX + QuoteCartDefaults.QUOTE_CUSTOMER_REQUEST_SUBMITTED_NOTIFICATION] = "This message template is used when a new quote request has been submitted. The message is received by the customer that sent the request. You can set up this option by ticking the checkbox <b>Send email to customer</b> in QuoteCart - Forms - Edit.",
            [MESSAGE_TEMPLATE_DESCRIPTION_PREFIX + QuoteCartDefaults.QUOTE_STORE_REQUEST_SUBMITTED_NOTIFICATION] = "This message template is used when a new quote request has been submitted. The message is received by a store owner. You can set up this option by ticking the checkbox <b>Send email to store owner</b> in QuoteCart - Forms - Edit.",
        };

        return resources.ToList();
    }

    #endregion

    #region Properties

    public bool HideInWidgetList => false;

    private const string MESSAGE_TEMPLATE_DESCRIPTION_PREFIX = "Admin.ContentManagement.MessageTemplates.Description.";

    #endregion

    #region Utilities

    protected async Task SeedMessageTemplatesAsync()
    {
        List<MessageTemplate> messageTemplates = [
            new ()
            {
                Name = QuoteCartDefaults.REQUEST_CUSTOMER_STATUS_CHANGED_NOTIFICATION,
                Subject = "Your request has been %QuoteRequest.Status% - %Store.Name%",
                Body =  @$"
                            <div>{Environment.NewLine}
                            %Customer.FullName%,{Environment.NewLine}
                            <br>{Environment.NewLine}
                            <br>{Environment.NewLine}
                            Your quote request has been updated! Please <a href='%QuoteRequest.Url%'>view it here</a>.{Environment.NewLine}
                            <br>{Environment.NewLine}
                            We look forward to hearing from you.
                            <br>{Environment.NewLine}
                            <br>{Environment.NewLine}
                            - %Store.Name%{Environment.NewLine}
                            </div>
                        ",
                IsActive = true
            },
            new ()
            {
                Name = QuoteCartDefaults.REQUEST_NEW_REPLY_CUSTOMER_NOTIFICATION,
                Subject = "You have a new reply to your quote request! - %Store.Name%",
                Body =  @$"
                            <div>{Environment.NewLine}
                            %Customer.FullName%,{Environment.NewLine}
                            <br>{Environment.NewLine}
                            <br>{Environment.NewLine}
                            %QuoteRequest.Message%{Environment.NewLine}
                            <br>{Environment.NewLine}
                            <br>{Environment.NewLine}
                            We have replied to your quote request! Please <a href='%QuoteRequest.Url%'>view it here</a>.{Environment.NewLine}
                            <br>{Environment.NewLine}
                            We look forward to hearing from you.
                            <br>{Environment.NewLine}
                            <br>{Environment.NewLine}
                            - %Store.Name%{Environment.NewLine}
                            </div>
                        ",
                IsActive = true
            },
            new ()
            {
                Name = QuoteCartDefaults.REQUEST_NEW_REPLY_STORE_NOTIFICATION,
                Subject = "%Customer.FullName% sent a reply to a quote request.",
                Body =  @$"
                            <div>{Environment.NewLine}
                            %Store.Name% has replied to Quote %QuoteRequest.ID% for %Customer.FullName% with:{Environment.NewLine}
                            <br>{Environment.NewLine}
                            <br>{Environment.NewLine}
                            %QuoteRequest.Message%{Environment.NewLine}
                            <br>{Environment.NewLine}
                            <a href='%QuoteRequest.Url%'>View it here</a>{Environment.NewLine}
                            </div>
                        ",
                IsActive = true
            },
            new ()
            {
                Name = QuoteCartDefaults.QUOTE_CUSTOMER_REQUEST_SUBMITTED_NOTIFICATION,
                Subject = "QuoteCart: Request Submitted",
                Body =  $@"%Customer.FullName%,{Environment.NewLine}
                            <br>{Environment.NewLine}
                            <br>{Environment.NewLine}
                            We have received your quote request and will respond shortly.{Environment.NewLine}
                            <br>{Environment.NewLine}
                            You can review your request and all replies <a href='%QuoteRequest.Url%'>here</a>.{Environment.NewLine}
                            <br>{Environment.NewLine}
                             Your response: {Environment.NewLine}
                            <br>{Environment.NewLine}
                            %QuoteRequest.FormAttributes%{Environment.NewLine}
                            <br>{Environment.NewLine}
                            <br>{Environment.NewLine}
                            Thank you,{Environment.NewLine}
                            <br>{Environment.NewLine}
                            %Store.Name%
                        ",
                IsActive = true
            },
            new ()
            {
                Name = QuoteCartDefaults.QUOTE_STORE_REQUEST_SUBMITTED_NOTIFICATION,
                Subject = "QuoteCart: A new request has been submitted",
                Body =  $@"
                            A new quote request was submitted to %Store.Name%{Environment.NewLine}
                            Quote ID: <b>%QuoteRequest.ID%</b>{Environment.NewLine}
                            <br>{Environment.NewLine}
                            Customer: <b>%Customer.FullName%</b>{Environment.NewLine}
                            <br>{Environment.NewLine}
                            Email: <b>%Customer.Email%</b>{Environment.NewLine}
                            <br>{Environment.NewLine}
                            Est. Value: <b>%QuoteOffer.TotalPrice%</b>{Environment.NewLine}
                            <br>{Environment.NewLine}
                            Form: <b>%QuoteRequest.FormAttributes%</b>{Environment.NewLine}
                            <br>{Environment.NewLine}
                            <br>{Environment.NewLine}
                            <a href=""%QuoteRequest.AdminUrl%"" target=""_blank"">View Request</a>.
                        ",
                IsActive = true
            },
            new ()
            {
                Name = QuoteCartDefaults.QUOTE_CUSTOMER_QUOTE_OFFER,
                Subject = "You have a quote offer from %Store.Name% !",
                Body = $@"%Customer.Username%,
                            <br>{Environment.NewLine}
                            <br>{Environment.NewLine}
                            We are pleased to offer you the following quote based on your <a href=""%QuoteRequest.Url%"">quote request #%QuoteRequest.ID%</a>:
                            <br>{Environment.NewLine}
                            %QuoteRequest.RequestProductsTable%
                            <br>{Environment.NewLine}
                            <br>{Environment.NewLine}
                            We look forward to your reply!
                            <br>
                            <br>
                            %Store.Name%
                        ",
                IsActive = true
            },
        ];

        foreach (var template in messageTemplates)
        {
            var foundTemplates = await _messageTemplateService.GetMessageTemplatesByNameAsync(template.Name);
            if (foundTemplates.Count <= 0)
                await _messageTemplateService.InsertMessageTemplateAsync(template);
            else
            {
                foreach (var foundTemplate in foundTemplates)
                {
                    foundTemplate.Subject = template.Subject;
                    foundTemplate.Body = template.Body;
                    await _messageTemplateService.UpdateMessageTemplateAsync(foundTemplate);
                }
            }
        }
    }

    #endregion
}
