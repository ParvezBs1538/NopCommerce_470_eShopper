using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Stores;

namespace NopStation.Plugin.Widgets.FAQ.Domains
{
    public class FAQCategory : BaseEntity, ILocalizedEntity, IStoreMappingSupported, ISoftDeletedEntity
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public int DisplayOrder { get; set; }

        public bool Published { get; set; }

        public bool Deleted { get; set; }

        public bool LimitedToStores { get; set; }
    }
}
