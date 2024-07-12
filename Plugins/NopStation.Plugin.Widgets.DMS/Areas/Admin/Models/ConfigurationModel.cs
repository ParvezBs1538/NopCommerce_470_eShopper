using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.DMS.Areas.Admin.Models
{
    public record ConfigurationModel : BaseNopModel, ISettingsModel
    {
        #region Ctor
        public ConfigurationModel()
        {
            AvailablePaperSizes = new List<SelectListItem>();
            AvailableProofOfDeliveryType = new List<SelectListItem>();
            AvailableLocationUpdateIntervals = new List<SelectListItem>();
        }

        #endregion

        #region Properties

        public int ActiveStoreScopeConfiguration { get; set; }

        #region Common

        //[NopResourceDisplayName("Admin.NopStation.DMS.Configuration.Fields.EnableSignatureUpload")]
        //public bool EnableSignatureUpload { get; set; }
        //public bool EnableSignatureUpload_OverrideForStore { get; set; }

        //[NopResourceDisplayName("Admin.NopStation.DMS.Configuration.Fields.SignatureUploadRequired")]
        //public bool SignatureUploadRequired { get; set; }
        //public bool SignatureUploadRequired_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DMS.Configuration.Fields.AllowShippersToSelectPageSize")]
        public bool AllowShippersToSelectPageSize { get; set; }
        public bool AllowShippersToSelectPageSize_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DMS.Configuration.Fields.PageSizeOptions")]
        public string PageSizeOptions { get; set; }
        public bool PageSizeOptions_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DMS.Configuration.Fields.ShipmentPageSize")]
        public int ShipmentPageSize { get; set; }
        public bool ShipmentPageSize_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DMS.Configuration.Fields.UseAjaxLoading")]
        public bool UseAjaxLoading { get; set; }
        public bool UseAjaxLoading_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DMS.Configuration.Fields.AllowCustomersToDeleteAccount")]
        public bool AllowCustomersToDeleteAccount { get; set; }
        public bool AllowCustomersToDeleteAccount_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DMS.Configuration.Fields.LocationUpdateIntervalInSeconds")]
        public int LocationUpdateIntervalInSeconds { get; set; }
        public bool LocationUpdateIntervalInSeconds_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DMS.Configuration.Fields.GeoMapId")]
        public int GeoMapId { get; set; }
        public bool GeoMapId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DMS.Configuration.Fields.GoogleMapApiKey")]
        public string GoogleMapApiKey { get; set; }
        public bool GoogleMapApiKey_OverrideForStore { get; set; }

        #endregion

        #region Security

        [NopResourceDisplayName("Admin.NopStation.DMS.Configuration.Fields.EnableJwtSecurity")]
        public bool EnableJwtSecurity { get; set; }
        public bool EnableJwtSecurity_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DMS.Configuration.Fields.TokenKey")]
        public string TokenKey { get; set; }
        public bool TokenKey_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DMS.Configuration.Fields.TokenSecret")]
        public string TokenSecret { get; set; }
        public bool TokenSecret_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DMS.Configuration.Fields.CheckIat")]
        public bool CheckIat { get; set; }
        public bool CheckIat_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DMS.Configuration.Fields.TokenSecondsValid")]
        public int TokenSecondsValid { get; set; }
        public bool TokenSecondsValid_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DMS.Configuration.Fields.NST")]
        public string NST { get; set; }

        #endregion

        #region Packaging Slip Setting

        [NopResourceDisplayName("Admin.NopStation.DMS.Configuration.Fields.DefaultPackagingSlipPaperSizeId")]
        public int DefaultPackagingSlipPaperSizeId { get; set; }
        public bool DefaultPackagingSlipPaperSizeId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DMS.Configuration.Fields.PrintPackagingSlipLandscape")]
        public bool PrintPackagingSlipLandscape { get; set; }
        public bool PrintPackagingSlipLandscape_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DMS.Configuration.Fields.PrintProductsOnPackagingSlip")]
        public bool PrintProductsOnPackagingSlip { get; set; }
        public bool PrintProductsOnPackagingSlip_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DMS.Configuration.Fields.PrintWeightInfoOnPackagingSlip")]
        public bool PrintWeightInfoOnPackagingSlip { get; set; }
        public bool PrintWeightInfoOnPackagingSlip_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DMS.Configuration.Fields.PrintEachPackagingSlipInNewPage")]
        public bool PrintEachPackagingSlipInNewPage { get; set; }
        public bool PrintEachPackagingSlipInNewPage_OverrideForStore { get; set; }

        public IList<SelectListItem> AvailablePaperSizes { get; set; }

        #endregion

        #region ProofOfDelivery

        [NopResourceDisplayName("Admin.NopStation.DMS.Configuration.Fields.EnabledProofOfDelivery")]
        public bool EnabledProofOfDelivery { get; set; }
        public bool EnabledProofOfDelivery_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DMS.Configuration.Fields.ProofOfDeliveryType")]
        public int ProofOfDeliveryTypeId { get; set; }
        public bool ProofOfDeliveryTypeId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DMS.Configuration.Fields.ProofOfDeliveryRequired")]
        public bool ProofOfDeliveryRequired { get; set; }
        public bool ProofOfDeliveryRequired_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DMS.Configuration.Fields.ProofOfDeliveryImageMaxSize")]
        public int ProofOfDeliveryImageMaxSize { get; set; }
        public bool ProofOfDeliveryImageMaxSize_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DMS.Configuration.Fields.OtpLength")]
        public int OtpLength { get; set; }
        public bool OtpLength_OverrideForStore { get; set; }

        public IList<SelectListItem> AvailableProofOfDeliveryType { get; set; }

        public IList<SelectListItem> AvailableLocationUpdateIntervals { get; set; }

        #endregion

        #endregion

    }
}
