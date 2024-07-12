using Nop.Core.Configuration;

namespace NopStation.Plugin.Widgets.DMS
{
    public class DMSSettings : ISettings
    {
        #region Common

        public bool AllowShippersToSelectPageSize { get; set; }

        //public bool EnableSignatureUpload { get; set; }

        //public bool SignatureUploadRequired { get; set; }

        public int ShipmentPageSize { get; set; }

        public string PageSizeOptions { get; set; }

        public bool UseAjaxLoading { get; set; }

        public bool AllowCustomersToDeleteAccount { get; set; }

        public int LocationUpdateIntervalInSeconds { get; set; }
        public int GeoMapId { get; set; }
        public string GoogleMapApiKey { get; set; }

        #endregion

        #region Security

        public bool EnableJwtSecurity { get; set; }

        public string TokenKey { get; set; }

        public string TokenSecret { get; set; }

        public bool CheckIat { get; set; }

        public int TokenSecondsValid { get; set; }

        #endregion

        #region Packaging Slip Setting

        public int DefaultPackagingSlipPaperSizeId { get; set; }

        public bool PrintPackagingSlipLandscape { get; set; }

        public bool PrintProductsOnPackagingSlip { get; set; }

        public bool PrintWeightInfoOnPackagingSlip { get; set; }

        public bool PrintEachPackagingSlipInNewPage { get; set; }

        #endregion

        #region Proof of delivery

        public bool EnabledProofOfDelivery { get; set; }

        public int ProofOfDeliveryTypeId { get; set; }

        public bool ProofOfDeliveryRequired { get; set; }

        public int OtpLength { get; set; }

        public int ProofOfDeliveryImageMaxSize { get; set; }

        #region TwilioSms

        //public string TwilioAccountSid { get; set; }

        //public string TwilioAuthToken { get; set; }

        //public string TwilioPhoneNumber { get; set; }


        //public int TwilioOtpValidForMinutes { get; set; }

        //public bool CheckPhoneNumberRegex { get; set; }

        //public string PhoneNumberRegex { get; set; }

        //public bool CheckIntlDialCode { get; set; }

        //public string IntlDialCode { get; set; }

        //public int RemoveFirstNDigitsWhenLocalNumber { get; set; }

        #endregion

        #endregion
    }
}
