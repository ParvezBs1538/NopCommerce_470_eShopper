using System;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Seo;
using Nop.Core.Domain.Stores;

namespace NopStation.Plugin.Widgets.Flipbooks.Domains
{
    public class Flipbook : BaseEntity, ISlugSupported, IAclSupported, ILocalizedEntity, IStoreMappingSupported, ISoftDeletedEntity
    {
        public string Name { get; set; }

        public bool IncludeInTopMenu { get; set; }

        public string MetaKeywords { get; set; }

        public string MetaDescription { get; set; }

        public string MetaTitle { get; set; }

        public int DisplayOrder { get; set; }

        public bool Active { get; set; }

        public DateTime? AvailableStartDateTimeUtc { get; set; }

        public DateTime? AvailableEndDateTimeUtc { get; set; }

        public bool Deleted { get; set; }

        public bool LimitedToStores { get; set; }

        public DateTime CreatedOnUtc { get; set; }

        public DateTime UpdatedOnUtc { get; set; }

        public bool SubjectToAcl { get; set; }
    }
}
