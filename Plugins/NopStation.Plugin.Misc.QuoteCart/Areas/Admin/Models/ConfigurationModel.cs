using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Areas.Admin.Models.Vendors;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.QuoteCart.Areas.Admin.Models;

public partial record ConfigurationModel : BaseNopModel, ISettingsModel, IAclSupportedModel
{
    public ConfigurationModel()
    {
        WhitelistedProductSearchModel = new ();
        WhitelistedCategorySearchModel = new ();
        WhitelistedManufacturerSearchModel = new ();
        WhitelistedVendorSearchModel = new ();
        AvailableCustomerRoles = [];
        SelectedCustomerRoleIds = [];
    }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.Configuration.Fields.EnableQuoteCart")]
    public bool EnableQuoteCart { get; set; }
    public bool EnableQuoteCart_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.Configuration.Fields.EnableWhitelist")]
    public bool EnableWhitelist { get; set; }
    public bool EnableWhitelist_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.Configuration.Fields.MaxQuoteItemCount")]
    [DisplayFormat(DataFormatString = "{0:n4}", ApplyFormatInEditMode = true)]
    public int MaxQuoteItemCount { get; set; }
    public bool MaxQuoteItemCount_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.Configuration.Fields.WhitelistAllProducts")]
    public bool WhitelistAllProducts { get; set; }
    public bool WhitelistAllProducts_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.Configuration.Fields.WhitelistAllCategories")]
    public bool WhitelistAllCategories { get; set; }
    public bool WhitelistAllCategories_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.Configuration.Fields.WhitelistAllManufacturers")]
    public bool WhitelistAllManufacturers { get; set; }
    public bool WhitelistAllManufacturers_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.Configuration.Fields.WhitelistAllVendors")]
    public bool WhitelistAllVendors { get; set; }
    public bool WhitelistAllVendors_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.Configuration.Fields.ClearCartAfterSubmission")]
    public bool ClearCartAfterSubmission { get; set; }
    public bool ClearCartAfterSubmission_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.Configuration.Fields.CustomerCanEnterPrice")]
    public bool CustomerCanEnterPrice { get; set; }
    public bool CustomerCanEnterPrice_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.Configuration.Fields.CustomerCanCancelQuote")]
    public bool CustomerCanCancelQuote { get; set; }
    public bool CustomerCanCancelQuote_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.QuoteCart.Configuration.Fields.SelectedCustomerRoleIds")]
    public IList<int> SelectedCustomerRoleIds { get; set; }
    public bool SelectedCustomerRoleIds_OverrideForStore { get; set; }
    public IList<SelectListItem> AvailableCustomerRoles { get; set; }

    public ProductSearchModel WhitelistedProductSearchModel { get; set; }

    public CategorySearchModel WhitelistedCategorySearchModel { get; set; }

    public ManufacturerSearchModel WhitelistedManufacturerSearchModel { get; set; }

    public VendorSearchModel WhitelistedVendorSearchModel { get; set; }

    public int ActiveStoreScopeConfiguration { get; set; }
}
