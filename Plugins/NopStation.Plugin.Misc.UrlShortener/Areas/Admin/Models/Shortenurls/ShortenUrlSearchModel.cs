using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.UrlShortener.Areas.Admin.Models.Shortenurls
{
    public record ShortenUrlSearchModel : BaseSearchModel
    {
        [NopResourceDisplayName("Admin.NopStation.UrlShortener.ShortenUrlSearch.Slug")]
        public string Slug { get; set; }


        [NopResourceDisplayName("Admin.NopStation.UrlShortener.ShortenUrlSearch.UrlEntityName")]
        public string UrlEntityName { get; set; }

        public IList<SelectListItem> AvailableUrlEntityNames { get; set; }
    }
}
