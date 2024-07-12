using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Nop.Core;
using Nop.Core.Infrastructure;
using NopStation.Plugin.Misc.Core.Helpers;
using Nop.Services.Localization;
using Nop.Web.Framework.Extensions;
using Nop.Web.Framework.UI.Paging;

namespace NopStation.Plugin.Widgets.Flipbooks.Extensions
{
    /// <summary>
    /// Renders a PagerForCatalog component from an IPageableModel datasource.
    /// </summary>
    public class PagerForCatalog : IHtmlContent
    {
        /// <summary>
        /// Model
        /// </summary>
        protected readonly IPageableModel _model;
        /// <summary>
        /// ViewContext
        /// </summary>
        protected readonly ViewContext _viewContext;
        /// <summary>
        /// Page query string prameter name
        /// </summary>
        protected string _pageQueryName = "page";
        /// <summary>
        /// A value indicating whether to show Total summary
        /// </summary>
        protected bool _showTotalSummary;
        /// <summary>
        /// A value indicating whether to show PagerForCatalog items
        /// </summary>
        protected bool _showPagerItems = true;
        /// <summary>
        /// A value indicating whether to show the first item
        /// </summary>
        protected bool _showFirst = true;
        /// <summary>
        /// A value indicating whether to the previous item
        /// </summary>
        protected bool _showPrevious = true;
        /// <summary>
        /// A value indicating whether to show the next item
        /// </summary>
        protected bool _showNext = true;
        /// <summary>
        /// A value indicating whether to show the last item
        /// </summary>
        protected bool _showLast = true;
        /// <summary>
        /// A value indicating whether to show individual page
        /// </summary>
        protected bool _showIndividualPages = true;
        /// <summary>
        /// A value indicating whether to render empty query string parameters (without values)
        /// </summary>
        protected bool _renderEmptyParameters = true;
        /// <summary>
        /// Number of individual page items to display
        /// </summary>
        protected int _individualPagesDisplayedCount = 5;
        /// <summary>
        /// Boolean parameter names
        /// </summary>
        protected IList<string> _booleanParameterNames;
        /// <summary>
        /// First page css class name
        /// </summary>
        protected string _firstPageCssClass = "first-page";
        /// <summary>
        /// Previous page css class name
        /// </summary>
        protected string _previousPageCssClass = "previous-page";
        /// <summary>
		/// Current page css class name
		/// </summary>
        protected string _currentPageCssClass = "current-page";
        /// <summary>
        /// Individual page css class name
        /// </summary>
        protected string _individualPageCssClass = "individual-page";
        /// <summary>
		/// Next page css class name
		/// </summary>
        protected string _nextPageCssClass = "next-page";
        /// <summary>
		/// Last page css class name
		/// </summary>
        protected string _lastPageCssClass = "last-page";
        /// <summary>
		/// Main ul css class name
		/// </summary>
        protected string _mainUlCssClass = "";

        private int _contentId = 0;
        private int _pageSize = 0;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="model">Model</param>
        /// <param name="context">ViewContext</param>
		public PagerForCatalog(IPageableModel model, ViewContext context, int contentId, int pageSize)
        {
            this._model = model;
            _viewContext = context;
            _booleanParameterNames = new List<string>();
            this._contentId = contentId;
            this._pageSize = pageSize;
        }

        /// <summary>
        /// ViewContext
        /// </summary>
		protected ViewContext ViewContext
        {
            get { return _viewContext; }
        }

        /// <summary>
        /// Set 
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>PagerForCatalog</returns>
        public PagerForCatalog QueryParam(string value)
        {
            _pageQueryName = value;
            return this;
        }
        /// <summary>
        /// Set a value indicating whether to show Total summary
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>PagerForCatalog</returns>
        public PagerForCatalog ShowTotalSummary(bool value)
        {
            _showTotalSummary = value;
            return this;
        }
        /// <summary>
        /// Set a value indicating whether to show PagerForCatalog items
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>PagerForCatalog</returns>
        public PagerForCatalog ShowPagerItems(bool value)
        {
            _showPagerItems = value;
            return this;
        }
        /// <summary>
        /// Set a value indicating whether to show the first item
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>PagerForCatalog</returns>
        public PagerForCatalog ShowFirst(bool value)
        {
            _showFirst = value;
            return this;
        }
        /// <summary>
        /// Set a value indicating whether to the previous item
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>PagerForCatalog</returns>
        public PagerForCatalog ShowPrevious(bool value)
        {
            _showPrevious = value;
            return this;
        }
        /// <summary>
        /// Set a  value indicating whether to show the next item
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>PagerForCatalog</returns>
        public PagerForCatalog ShowNext(bool value)
        {
            _showNext = value;
            return this;
        }
        /// <summary>
        /// Set a value indicating whether to show the last item
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>PagerForCatalog</returns>
        public PagerForCatalog ShowLast(bool value)
        {
            _showLast = value;
            return this;
        }
        /// <summary>
        /// Set number of individual page items to display
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>PagerForCatalog</returns>
        public PagerForCatalog ShowIndividualPages(bool value)
        {
            _showIndividualPages = value;
            return this;
        }
        /// <summary>
        /// Set a value indicating whether to render empty query string parameters (without values)
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>PagerForCatalog</returns>
        public PagerForCatalog RenderEmptyParameters(bool value)
        {
            _renderEmptyParameters = value;
            return this;
        }
        /// <summary>
        /// Set number of individual page items to display
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>PagerForCatalog</returns>
        public PagerForCatalog IndividualPagesDisplayedCount(int value)
        {
            _individualPagesDisplayedCount = value;
            return this;
        }
        /// <summary>
        /// little hack here due to ugly MVC implementation
        /// find more info here: http://www.mindstorminteractive.com/topics/jquery-fix-asp-net-mvc-checkbox-truefalse-value/
        /// </summary>
        /// <param name="paramName">Parameter name</param>
        /// <returns>PagerForCatalog</returns>
        public PagerForCatalog BooleanParameterName(string paramName)
        {
            _booleanParameterNames.Add(paramName);
            return this;
        }
        /// <summary>
        /// Set first page PagerForCatalog css class name
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>PagerForCatalog</returns>
        public PagerForCatalog FirstPageCssClass(string value)
        {
            _firstPageCssClass = value;
            return this;
        }
        /// <summary>
        /// Set previous page PagerForCatalog css class name
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>PagerForCatalog</returns>
        public PagerForCatalog PreviousPageCssClass(string value)
        {
            _previousPageCssClass = value;
            return this;
        }
        /// <summary>
        /// Set current page PagerForCatalog css class name
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>PagerForCatalog</returns>
        public PagerForCatalog CurrentPageCssClass(string value)
        {
            _currentPageCssClass = value;
            return this;
        }
        /// <summary>
        /// Set individual page PagerForCatalog css class name
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>PagerForCatalog</returns>
        public PagerForCatalog IndividualPageCssClass(string value)
        {
            _individualPageCssClass = value;
            return this;
        }
        /// <summary>
        /// Set next page PagerForCatalog css class name
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>PagerForCatalog</returns>
        public PagerForCatalog NextPageCssClass(string value)
        {
            _nextPageCssClass = value;
            return this;
        }
        /// <summary>
        /// Set last page PagerForCatalog css class name
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>PagerForCatalog</returns>
        public PagerForCatalog LastPageCssClass(string value)
        {
            _lastPageCssClass = value;
            return this;
        }
        /// <summary>
        /// Set main ul css class name
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>PagerForCatalog</returns>
        public PagerForCatalog MainUlCssClass(string value)
        {
            _mainUlCssClass = value;
            return this;
        }

        /// <summary>
        /// Write control
        /// </summary>
        /// <param name="writer">Writer</param>
        /// <param name="encoder">Encoder</param>
	    public void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
            var htmlString = GenerateHtmlStringAsync();
            writer.Write(htmlString);
        }
        /// <summary>
        /// Generate HTML control
        /// </summary>
        /// <returns>HTML control</returns>
	    public override string ToString()
        {
            return GenerateHtmlStringAsync().Result;
        }
        /// <summary>
        /// Generate HTML control
        /// </summary>
        /// <returns>HTML control</returns>
        public virtual async Task<string> GenerateHtmlStringAsync()
        {
            if (_model.TotalItems == 0)
                return null;
            var localizationService = NopInstance.Load<ILocalizationService>();

            var links = new StringBuilder();
            if (_showTotalSummary && (_model.TotalPages > 0))
            {
                links.Append("<li class=\"total-summary\">");
                links.Append(string.Format(await localizationService.GetResourceAsync("PagerForCatalog.CurrentPage"), _model.PageIndex + 1, _model.TotalPages, _model.TotalItems));
                links.Append("</li>");
            }
            if (_showPagerItems && (_model.TotalPages > 1))
            {
                if (_showFirst)
                {
                    //first page
                    if ((_model.PageIndex >= 3) && (_model.TotalPages > _individualPagesDisplayedCount))
                    {
                        links.Append(CreatePageLinkAsync(1, await localizationService.GetResourceAsync("PagerForCatalog.First"), _firstPageCssClass));
                    }
                }
                if (_showPrevious)
                {
                    //previous page
                    if (_model.PageIndex > 0)
                    {
                        links.Append(CreatePageLinkAsync(_model.PageIndex, await localizationService.GetResourceAsync("PagerForCatalog.Previous"), _previousPageCssClass));
                    }
                }
                if (_showIndividualPages)
                {
                    //individual pages
                    var firstIndividualPageIndex = GetFirstIndividualPageIndex();
                    var lastIndividualPageIndex = GetLastIndividualPageIndex();
                    for (var i = firstIndividualPageIndex; i <= lastIndividualPageIndex; i++)
                    {
                        if (_model.PageIndex == i)
                        {
                            links.AppendFormat("<li class=\"" + _currentPageCssClass + "\"><span>{0}</span></li>", (i + 1));
                        }
                        else
                        {
                            links.Append(CreatePageLinkAsync(i + 1, (i + 1).ToString(), _individualPageCssClass));
                        }
                    }
                }
                if (_showNext)
                {
                    //next page
                    if ((_model.PageIndex + 1) < _model.TotalPages)
                    {
                        links.Append(CreatePageLinkAsync(_model.PageIndex + 2, await localizationService.GetResourceAsync("PagerForCatalog.Next"), _nextPageCssClass));
                    }
                }
                if (_showLast)
                {
                    //last page
                    if (((_model.PageIndex + 3) < _model.TotalPages) && (_model.TotalPages > _individualPagesDisplayedCount))
                    {
                        links.Append(CreatePageLinkAsync(_model.TotalPages, await localizationService.GetResourceAsync("PagerForCatalog.Last"), _lastPageCssClass));
                    }
                }
            }

            var result = links.ToString();
            if (!string.IsNullOrEmpty(result))
            {

                result = string.Format("<ul{0}>", string.IsNullOrEmpty(_mainUlCssClass) ? "" : " class=\"" + _mainUlCssClass + "\"") + result + "</ul>";
            }
            return result;
        }
        /// <summary>
        /// Is PagerForCatalog empty (only one page)?
        /// </summary>
        /// <returns>Result</returns>
	    public virtual async Task<bool> IsEmptyAsync()
        {
            var html = await GenerateHtmlStringAsync();
            return string.IsNullOrEmpty(html);
        }

        /// <summary>
        /// Get first individual page index
        /// </summary>
        /// <returns>Page index</returns>
        protected virtual int GetFirstIndividualPageIndex()
        {
            if ((_model.TotalPages < _individualPagesDisplayedCount) ||
                ((_model.PageIndex - (_individualPagesDisplayedCount / 2)) < 0))
            {
                return 0;
            }
            if ((_model.PageIndex + (_individualPagesDisplayedCount / 2)) >= _model.TotalPages)
            {
                return (_model.TotalPages - _individualPagesDisplayedCount);
            }
            return (_model.PageIndex - (_individualPagesDisplayedCount / 2));
        }
        /// <summary>
        /// Get last individual page index
        /// </summary>
        /// <returns>Page index</returns>
        protected virtual int GetLastIndividualPageIndex()
        {
            var num = _individualPagesDisplayedCount / 2;
            if ((_individualPagesDisplayedCount % 2) == 0)
            {
                num--;
            }
            if ((_model.TotalPages < _individualPagesDisplayedCount) ||
                ((_model.PageIndex + num) >= _model.TotalPages))
            {
                return (_model.TotalPages - 1);
            }
            if ((_model.PageIndex - (_individualPagesDisplayedCount / 2)) < 0)
            {
                return (_individualPagesDisplayedCount - 1);
            }
            return (_model.PageIndex + num);
        }
        /// <summary>
        /// Create page link
        /// </summary>
        /// <param name="pageNumber">Page number</param>
        /// <param name="text">Text</param>
        /// <param name="cssClass">CSS class</param>
        /// <returns>Link</returns>
		protected virtual async Task<string> CreatePageLinkAsync(int pageNumber, string text, string cssClass)
        {
            var liBuilder = new TagBuilder("li");
            if (!string.IsNullOrWhiteSpace(cssClass))
                liBuilder.AddCssClass(cssClass);

            var aBuilder = new TagBuilder("a");
            aBuilder.InnerHtml.AppendHtml(text);
            //aBuilder.MergeAttribute("href", CreateDefaultUrl(pageNumber));
            aBuilder.MergeAttribute("onclick='MagazinePager.loadProductsByFlipbookContentId(" + _contentId + ", " + pageNumber + ", " + _pageSize + ")'", "");

            liBuilder.InnerHtml.AppendHtml(aBuilder);
            return await liBuilder.RenderHtmlContentAsync();
        }
        /// <summary>
        /// Create default URL
        /// </summary>
        /// <param name="pageNumber">Page number</param>
        /// <returns>URL</returns>
        protected virtual string CreateDefaultUrl(int pageNumber)
        {
            var routeValues = new RouteValueDictionary();

            var parametersWithEmptyValues = new List<string>();
            foreach (var key in _viewContext.HttpContext.Request.Query.Keys.Where(key => key != null))
            {
                //TODO test new implementation (QueryString, keys). And ensure no null exception is thrown when invoking ToString(). Is "StringValues.IsNullOrEmpty" required?
                var value = _viewContext.HttpContext.Request.Query[key].ToString();
                if (_renderEmptyParameters && string.IsNullOrEmpty(value))
                {
                    //we store query string parameters with empty values separately
                    //we need to do it because they are not properly processed in the UrlHelper.GenerateUrl method (dropped for some reasons)
                    parametersWithEmptyValues.Add(key);
                }
                else
                {
                    if (_booleanParameterNames.Contains(key, StringComparer.InvariantCultureIgnoreCase))
                    {
                        //little hack here due to ugly MVC implementation
                        //find more info here: http://www.mindstorminteractive.com/topics/jquery-fix-asp-net-mvc-checkbox-truefalse-value/
                        if (!string.IsNullOrEmpty(value) && value.Equals("true,false", StringComparison.InvariantCultureIgnoreCase))
                        {
                            value = "true";
                        }
                    }
                    routeValues[key] = value;
                }
            }

            if (pageNumber > 1)
            {
                routeValues[_pageQueryName] = pageNumber;
            }
            else
            {
                //SEO. we do not render pageindex query string parameter for the first page
                if (routeValues.ContainsKey(_pageQueryName))
                {
                    routeValues.Remove(_pageQueryName);
                }
            }

            var webHelper = NopInstance.Load<IWebHelper>();
            var url = webHelper.GetThisPageUrl(false);
            foreach (var routeValue in routeValues)
            {
                url = webHelper.ModifyQueryString(url, routeValue.Key, routeValue.Value?.ToString());
            }
            if (_renderEmptyParameters && parametersWithEmptyValues.Any())
            {
                foreach (var key in parametersWithEmptyValues)
                {
                    url = webHelper.ModifyQueryString(url, key);
                }
            }
            return url;
        }
    }
}
