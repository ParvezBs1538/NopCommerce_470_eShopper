using System.Collections.Generic;
using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.FAQ.Models
{
    public record FAQItemModel : BaseNopEntityModel
    {
        public FAQItemModel()
        {
            Tags = new List<TagModel>();
            MappedCategoryIds = new List<int>();
        }

        public string Question { get; set; }

        public string Answer { get; set; }

        public string Permalink { get; set; }

        public IList<int> MappedCategoryIds { get; set; }

        public IList<TagModel> Tags { get; set; }


        public record TagModel : BaseNopEntityModel
        {
            public string Name { get; set; }
        }
    }
}
