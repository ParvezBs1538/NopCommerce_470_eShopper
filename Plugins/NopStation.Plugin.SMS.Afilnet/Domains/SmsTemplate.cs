using Nop.Core;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Stores;

namespace NopStation.Plugin.SMS.Afilnet.Domains
{
    public class SmsTemplate : BaseEntity, ILocalizedEntity, IStoreMappingSupported, IAclSupported
    {
        public string Name { get; set; }

        public string Body { get; set; }

        public bool Active { get; set; }

        public bool LimitedToStores { get; set; }

        public bool SubjectToAcl { get; set; }
    }
}
