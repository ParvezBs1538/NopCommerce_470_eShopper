using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Payments.NetsEasy.Areas.Admin.Models
{
    public record ConfigurationModel : BaseNopModel
    {
        public ConfigurationModel()
        {
            SelectedCountryIds = new List<int>();
            AvailableCountries = new List<SelectListItem>();
        }

        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Admin.NopStation.NetsEasy.Configuration.Fields.EnsureRecurringInterval")]
        public bool EnsureRecurringInterval { get; set; }
        public bool EnsureRecurringInterval_OverrideForStore { get; set; }
        [NopResourceDisplayName("Admin.NopStation.NetsEasy.Configuration.Fields.TestMode")]
        public bool TestMode { get; set; }
        public bool TestMode_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.NetsEasy.Configuration.Fields.CheckoutKey")]
        public string CheckoutKey { get; set; }
        public bool CheckoutKey_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.NetsEasy.Configuration.Fields.SecretKey")]
        public string SecretKey { get; set; }
        public bool SecretKey_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.NetsEasy.Configuration.Fields.AdditionalFeePercentage")]
        public bool AdditionalFeePercentage { get; set; }
        public bool AdditionalFeePercentage_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.NetsEasy.Configuration.Fields.AdditionalFee")]
        public decimal AdditionalFee { get; set; }
        public bool AdditionalFee_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.NetsEasy.Configuration.Fields.CheckoutPageUrl")]
        public string CheckoutPageUrl { get; set; }
        public bool CheckoutPageUrl_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.NetsEasy.Configuration.Fields.ShowB2B")]
        public bool ShowB2B { get; set; }
        public bool ShowB2B_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.NetsEasy.Configuration.Fields.EnableLog")]
        public bool EnableLog { get; set; }
        public bool EnableLog_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.NetsEasy.Configuration.Fields.TransactMode")]
        public int TransactModeId { get; set; }

        public SelectList TransactModeValues { get; set; }
        public bool TransactModeId_OverrideForStore { get; set; }

        public int IntegrationTypeId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.NetsEasy.Configuration.Fields.IntegrationType")]
        public SelectList IntegrationTypeValues { get; set; }
        public bool IntegrationTypeId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.NetsEasy.Configuration.Fields.Countries")]
        [UIHint("MultiSelect")]
        public IList<int> SelectedCountryIds { get; set; }
        public bool SelectedCountryIds_OverrideForStore { get; set; }

        public IList<SelectListItem> AvailableCountries { get; set; }
    }
}