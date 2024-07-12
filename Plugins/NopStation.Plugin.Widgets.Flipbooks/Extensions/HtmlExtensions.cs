using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Infrastructure;
using NopStation.Plugin.Misc.Core.Helpers;
using Nop.Services.Localization;
using Nop.Web.Framework.Extensions;
using Nop.Web.Framework.UI.Paging;
using Nop.Web.Models.Common;

namespace NopStation.Plugin.Widgets.Flipbooks.Extensions
{
    public static class HtmlExtensions
    {
        //we have two pagers:
        //The first one can have custom routes
        //The second one just adds query string parameter
        public static async Task<IHtmlContent> PagerForCatalogAsync<TModel>(this IHtmlHelper<TModel> html, PagerModel model)
        {
            if (model.TotalRecords == 0)
                return new HtmlString("");

            var localizationService = NopInstance.Load<ILocalizationService>();

            var links = new StringBuilder();
            if (model.ShowTotalSummary && (model.TotalPages > 0))
            {
                links.Append("<li class=\"total-summary\">");
                links.Append(string.Format(await model.GetCurrentPageTextAsync(), model.PageIndex + 1, model.TotalPages, model.TotalRecords));
                links.Append("</li>");
            }
            if (model.ShowPagerItems && (model.TotalPages > 1))
            {
                if (model.ShowFirst)
                {
                    //first page
                    if ((model.PageIndex >= 3) && (model.TotalPages > model.IndividualPagesDisplayedCount))
                    {
                        model.RouteValues.PageNumber = 1;

                        links.Append("<li class=\"first-page\">");
                        if (model.UseRouteLinks)
                        {
                            var link = html.RouteLink(await model.GetFirstButtonTextAsync(), model.RouteActionName, model.RouteValues, new { title = await localizationService.GetResourceAsync("Pager.FirstPageTitle") });
                            links.Append(link.ToString());
                        }
                        else
                        {
                            var link = html.ActionLink(await model.GetFirstButtonTextAsync(), model.RouteActionName, model.RouteValues, new { title = await localizationService.GetResourceAsync("Pager.FirstPageTitle") });
                            links.Append(link.ToString());
                        }
                        links.Append("</li>");
                    }
                }
                if (model.ShowPrevious)
                {
                    //previous page
                    if (model.PageIndex > 0)
                    {
                        model.RouteValues.PageNumber = (model.PageIndex);

                        links.Append("<li class=\"previous-page\">");
                        if (model.UseRouteLinks)
                        {
                            var link = html.RouteLink(await model.GetPreviousButtonTextAsync(), model.RouteActionName, model.RouteValues, new { title = await localizationService.GetResourceAsync("Pager.PreviousPageTitle") });
                            links.Append(link.ToString());
                        }
                        else
                        {
                            var link = html.ActionLink(await model.GetPreviousButtonTextAsync(), model.RouteActionName, model.RouteValues, new { title = await localizationService.GetResourceAsync("Pager.PreviousPageTitle") });
                            links.Append(link.ToString());
                        }
                        links.Append("</li>");
                    }
                }
                if (model.ShowIndividualPages)
                {
                    //individual pages
                    var firstIndividualPageIndex = model.GetFirstIndividualPageIndex();
                    var lastIndividualPageIndex = model.GetLastIndividualPageIndex();
                    for (var i = firstIndividualPageIndex; i <= lastIndividualPageIndex; i++)
                    {
                        if (model.PageIndex == i)
                        {
                            links.AppendFormat("<li class=\"current-page\"><span>{0}</span></li>", (i + 1));
                        }
                        else
                        {
                            model.RouteValues.PageNumber = (i + 1);

                            links.Append("<li class=\"individual-page\">");
                            if (model.UseRouteLinks)
                            {
                                var link = html.RouteLink((i + 1).ToString(), model.RouteActionName, model.RouteValues, new { title = string.Format(await localizationService.GetResourceAsync("Pager.PageLinkTitle"), (i + 1)) });
                                links.Append(link.ToString());
                            }
                            else
                            {
                                var link = html.ActionLink((i + 1).ToString(), model.RouteActionName, model.RouteValues, new { title = string.Format(await localizationService.GetResourceAsync("Pager.PageLinkTitle"), (i + 1)) });
                                links.Append(link.ToString());
                            }
                            links.Append("</li>");
                        }
                    }
                }
                if (model.ShowNext)
                {
                    //next page
                    if ((model.PageIndex + 1) < model.TotalPages)
                    {
                        model.RouteValues.PageNumber = (model.PageIndex + 2);

                        links.Append("<li class=\"next-page\">");
                        if (model.UseRouteLinks)
                        {
                            var link = html.RouteLink(await model.GetNextButtonTextAsync(), model.RouteActionName, model.RouteValues, new { title = await localizationService.GetResourceAsync("Pager.NextPageTitle") });
                            links.Append(link.ToString());
                        }
                        else
                        {
                            var link = html.ActionLink(await model.GetNextButtonTextAsync(), model.RouteActionName, model.RouteValues, new { title = await localizationService.GetResourceAsync("Pager.NextPageTitle") });
                            links.Append(link.ToString());
                        }
                        links.Append("</li>");
                    }
                }
                if (model.ShowLast)
                {
                    //last page
                    if (((model.PageIndex + 3) < model.TotalPages) && (model.TotalPages > model.IndividualPagesDisplayedCount))
                    {
                        model.RouteValues.PageNumber = model.TotalPages;

                        links.Append("<li class=\"last-page\">");
                        if (model.UseRouteLinks)
                        {
                            var link = html.RouteLink(await model.GetLastButtonTextAsync(), model.RouteActionName, model.RouteValues, new { title = await localizationService.GetResourceAsync("Pager.LastPageTitle") });
                            links.Append(link.ToString());
                        }
                        else
                        {
                            var link = html.ActionLink(await model.GetLastButtonTextAsync(), model.RouteActionName, model.RouteValues, new { title = await localizationService.GetResourceAsync("Pager.LastPageTitle") });
                            links.Append(link.ToString());
                        }
                        links.Append("</li>");
                    }
                }
            }
            var result = links.ToString();
            if (!string.IsNullOrEmpty(result))
            {
                result = "<ul>" + result + "</ul>";
            }
            return new HtmlString(result);
        }

        public static PagerForCatalog PagerForCatalog(this IHtmlHelper helper, IPageableModel pagination, int flipbookContentId, int pageSize)
        {
            return new PagerForCatalog(pagination, helper.ViewContext, flipbookContentId, pageSize);
        }
    }
}
