using Nop.Core;
using Nop.Core.Domain.Common;

namespace NopStation.Plugin.Shipping.Redx.Domains
{
    public class RedxArea : BaseEntity, ISoftDeletedEntity
    {
        public string Name { get; set; }

        public int RedxAreaId { get; set; }

        public string PostCode { get; set; }

        public string DistrictName { get; set; }

        public int? StateProvinceId { get; set; }

        public bool Deleted { get; set; }
    }
}