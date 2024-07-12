using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Shipping.DHL.Areas.Admin.Models
{
    public record ConfigurationModel : BaseNopModel, ISettingsModel
    {
        public ConfigurationModel()
        {
            AvailableCurrencies = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.NopStation.DHL.Configuration.Fields.Url")]
        public string Url { get; set; }
        public bool Url_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DHL.Configuration.Fields.SiteId")]
        public string SiteId { get; set; }
        public bool SiteId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DHL.Configuration.Fields.Password")]
        public string Password { get; set; }
        public bool Password_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DHL.Configuration.Fields.AccountNumber")]
        public string AccountNumber { get; set; }
        public bool AccountNumber_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DHL.Configuration.Fields.Email")]
        public string Email { get; set; }
        public bool Email_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DHL.Configuration.Fields.Name")]
        public string Name { get; set; }
        public bool Name_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DHL.Configuration.Fields.Country")]
        public string Country { get; set; }
        public bool Country_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DHL.Configuration.Fields.CountryCode")]
        public string CountryCode { get; set; }
        public bool CountryCode_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DHL.Configuration.Fields.PostalCode")]
        public string PostalCode { get; set; }
        public bool PostalCode_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DHL.Configuration.Fields.City")]
        public string City { get; set; }
        public bool City_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DHL.Configuration.Fields.PhoneNumber")]
        public string PhoneNumber { get; set; }
        public bool PhoneNumber_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DHL.Configuration.Fields.DefaultHeight")]
        public int DefaultHeight { get; set; }
        public bool DefaultHeight_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DHL.Configuration.Fields.DefaultDepth")]
        public int DefaultDepth { get; set; }
        public bool DefaultDepth_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DHL.Configuration.Fields.DefaultWidth")]
        public int DefaultWidth { get; set; }
        public bool DefaultWidth_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DHL.Configuration.Fields.DefaultWeight")]
        public int DefaultWeight { get; set; }
        public bool DefaultWeight_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DHL.Configuration.Fields.PickupReadyByTime")]
        public string PickupReadyByTime { get; set; }
        public bool PickupReadyByTime_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DHL.Configuration.Fields.PickupCloseTime")]
        public string PickupCloseTime { get; set; }
        public bool PickupCloseTime_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DHL.Configuration.Fields.PickupPackageLocation")]
        public string PickupPackageLocation { get; set; }
        public bool PickupPackageLocation_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DHL.Configuration.Fields.Tracing")]
        public bool Tracing { get; set; }
        public bool Tracing_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DHL.Configuration.Fields.CompanyName")]
        public string CompanyName { get; set; }
        public bool CompanyName_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DHL.Configuration.Fields.CompanyAddress")]
        public string CompanyAddress { get; set; }
        public bool CompanyAddress_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DHL.Configuration.Fields.TrackingUrl")]
        public string TrackingUrl { get; set; }
        public bool TrackingUrl_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DHL.Configuration.Fields.Currency")]
        public int SelectedCurrencyId { get; set; }
        public bool SelectedCurrencyId_OverrideForStore { get; set; }
       
        [NopResourceDisplayName("Admin.NopStation.DHL.Configuration.Fields.Currency")]
        public int CurrencyId { get; set; }

        public IList<SelectListItem> AvailableCurrencies { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DHL.Configuration.Fields.CurrencyRate")]
        public decimal SelectedCurrencyRate { get; set; }
        public bool SelectedCurrencyRate_OverrideForStore { get; set; }

        public int ActiveStoreScopeConfiguration { get; set; }
    }
}
