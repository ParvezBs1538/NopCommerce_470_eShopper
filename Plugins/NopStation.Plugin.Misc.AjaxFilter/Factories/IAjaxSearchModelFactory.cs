using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using NopStation.Plugin.Misc.AjaxFilter.Models;

namespace NopStation.Plugin.Misc.AjaxFilter.Factories
{
    public interface IAjaxSearchModelFactory
    {
        Task<PublicInfoModel> PreparePublicInfoModelAsync(int categoryId, int manufacturer, RouteValueDictionary routeValues, List<RequestParams> requestParams);
        Task<PublicInfoModel> CompletePublicInfoModelAsync(PublicInfoModel model, SearchFilterResult query);
        Task<SearchModel> PrepareSearchModelAsync(PublicInfoModel publicInfoModel, string typ = "");
        Task<FilterPriceRangeModel> GetConvertedPriceRangeAsync(string price);
    }
}
