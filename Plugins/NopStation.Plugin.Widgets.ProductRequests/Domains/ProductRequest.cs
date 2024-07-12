using System;
using Nop.Core;
using Nop.Core.Domain.Common;

namespace NopStation.Plugin.Widgets.ProductRequests.Domains
{
    public class ProductRequest : BaseEntity, ISoftDeletedEntity
    {
        public int CustomerId { get; set; }

        public string Name { get; set; }

        public string Link { get; set; }

        public string Description { get; set; }

        public string AdminComment { get; set; }

        public int StoreId { get; set; }

        public bool Deleted { get; set; }

        public DateTime CreatedOnUtc { get; set; }
    }
}
