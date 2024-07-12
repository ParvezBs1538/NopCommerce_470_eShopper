using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Stores;

namespace NopStation.Plugin.Misc.AdvancedSEO.Domains
{
    public class SEOTemplateBaseEntity: BaseEntity, ISoftDeletedEntity, IStoreMappingSupported, ILocalizedEntity
    {
        public string TemplateName { get; set; }

        public string SEOTitleTemplate { get; set; }

        public string SEODescriptionTemplate { get; set; }

        public string SEOKeywordsTemplate { get; set; }

        public string TemplateRegex { get; set; }

        public bool IsActive { get; set; }

        public bool Deleted { get; set; }

        public int Priority { get; set; }

        public bool IsGlobalTemplate { get; set; }

        public int CreatedByCustomerId { get; set; }

        public int LastUpdatedByCustomerId { get; set; }

        public DateTime CreatedOnUtc { get; set; }

        public DateTime UpdatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity is limited/restricted to certain stores
        /// </summary>
        public bool LimitedToStores { get; set; }
    }
}
