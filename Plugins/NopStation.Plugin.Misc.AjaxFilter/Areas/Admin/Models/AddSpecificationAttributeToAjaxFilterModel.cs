using System.Collections.Generic;
using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Misc.AjaxFilter.Areas.Admin.Models
{
    public partial record AddSpecificationAttributeToAjaxFilterModel : BaseNopModel
    {
        #region Ctor

        public AddSpecificationAttributeToAjaxFilterModel()
        {
            SelectedSpecificationAttributeIds = new List<int>();
        }
        #endregion

        #region Properties


        public IList<int> SelectedSpecificationAttributeIds { get; set; }

        #endregion
    }
}
