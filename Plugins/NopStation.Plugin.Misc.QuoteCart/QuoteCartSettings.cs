using Nop.Core.Configuration;

namespace NopStation.Plugin.Misc.QuoteCart;

public class QuoteCartSettings : ISettings
{
    public bool EnableQuoteCart { get; set; }

    public bool EnableWhitelist { get; set; }

    public int MaxQuoteItemCount { get; set; }

    public bool WhitelistAllProducts { get; set; }

    public bool WhitelistAllCategories { get; set; }

    public bool WhitelistAllManufacturers { get; set; }

    public bool WhitelistAllVendors { get; set; }

    public bool ClearCartAfterSubmission { get; set; }

    public bool CustomerCanEnterPrice { get; set; }

    public bool CustomerCanCancelQuote { get; set; }

    public bool SubjectToAcl { get; set; }
}