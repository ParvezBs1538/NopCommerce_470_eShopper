using System.Collections.Generic;
using Nop.Web.Models.Common;
using NopStation.Plugin.Misc.Core.Models.Api;

namespace NopStation.Plugin.Widgets.DMS.Models
{
    public class AppConfigurationModel
    {
        public AppConfigurationModel()
        {
            StringResources = new List<KeyValueApi>();
        }

        //public bool EnableSignatureUpload { get; set; }
        //public bool SignatureUploadRequired { get; set; }
        public bool StoreClosed { get; set; }

        //public bool GdprEnabled { get; set; }

        public bool AllowCustomersToDeleteAccount { get; set; }

        public bool EnabledProofOfDelivery { get; set; }

        public int ProofOfDeliveryTypeId { get; set; }

        public bool ProofOfDeliveryRequired { get; set; }

        public int OtpLength { get; set; }

        public bool Rtl { get; set; }

        public LanguageSelectorModel LanguageNavSelector { get; set; }

        public IList<KeyValueApi> StringResources { get; set; }
    }
}
