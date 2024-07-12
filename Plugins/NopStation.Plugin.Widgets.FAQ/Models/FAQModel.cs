using System.Collections.Generic;
using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.FAQ.Models
{
    public record FAQModel : BaseNopModel
    {
        public FAQModel()
        {
            Categories = new List<FAQCategoryModel>();
            Items = new List<FAQItemModel>();
        }

        public IList<FAQCategoryModel> Categories { get; set; }

        public IList<FAQItemModel> Items { get; set; }
    }
}
