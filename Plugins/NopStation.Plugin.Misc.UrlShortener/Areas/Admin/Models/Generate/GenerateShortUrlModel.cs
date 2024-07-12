using System.Collections.Generic;
using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Misc.UrlShortener.Areas.Admin.Models.Generate
{
    public record GenerateShortUrlModel : BaseNopModel
    {
        #region Ctor

        public GenerateShortUrlModel()
        {
            SelectedUrlRecordIds = new List<int>();
        }

        public IList<int> SelectedUrlRecordIds { get; set; }

        #endregion
    }
}
