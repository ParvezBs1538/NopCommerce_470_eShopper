using Nop.Core;

namespace NopStation.Plugin.Misc.WebApi.Domains
{
    public partial class ApiCategoryIcon : BaseEntity
    {
        public int CategoryId { get; set; }

        public int CategoryBannerId { get; set; }
    }
}