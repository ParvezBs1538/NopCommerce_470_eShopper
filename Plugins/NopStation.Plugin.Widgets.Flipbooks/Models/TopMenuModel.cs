using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.Flipbooks.Models
{
    public record TopMenuModel
    {
        public TopMenuModel()
        {
            Flipbooks = new List<FlipbookModel>();
        }

        public IList<FlipbookModel> Flipbooks { get; set; }

        public record FlipbookModel : BaseNopEntityModel
        {
            public string Name { get; set; }
            public string SeName { get; set; }
        }
    }
}
