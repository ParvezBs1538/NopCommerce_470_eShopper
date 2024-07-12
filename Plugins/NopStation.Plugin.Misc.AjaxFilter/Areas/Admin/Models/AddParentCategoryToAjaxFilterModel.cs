using System.Collections.Generic;
using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Misc.AjaxFilter.Areas.Admin.Models
{
    public partial record AddParentCategoryToAjaxFilterModel : BaseNopModel
    {
        #region Ctor

        public AddParentCategoryToAjaxFilterModel()
        {
            SelectedParentCategoryIds = new List<int>();
        }
        #endregion

        #region Properties


        public IList<int> SelectedParentCategoryIds { get; set; }
        #endregion
    }
}
