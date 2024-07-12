using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Configuration;
using NopStation.Plugin.Misc.AjaxFilter.Extensions;
using NopStation.Plugin.Misc.AjaxFilter.Models;
using NopStation.Plugin.Misc.Core.Components;

namespace NopStation.Plugin.Misc.AjaxFilter.Components
{
    public class CatalogFiltersViewComponent : NopStationViewComponent
    {
        #region Fields

        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;

        #endregion

        #region Ctor

        public CatalogFiltersViewComponent(ISettingService settingService, IStoreContext storeContext)
        {
            _settingService = settingService;
            _storeContext = storeContext;
        }

        #endregion

        #region Methods

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            var storeId = (await _storeContext.GetCurrentStoreAsync()).Id;

            var settings = await _settingService.LoadSettingAsync<AjaxFilterSettings>(storeId);

            if (!settings.EnableFilter)
                return Content("");

            int categoryId = 0;

            int manufacturerId = 0;

            if (Url.ActionContext != null)
            {
                var categoryObj = Url.ActionContext.RouteData.Values["categoryId"];
                if (categoryObj != null)
                {
                    int.TryParse(categoryObj.ToString(), out categoryId);
                }

                var manufacturerObj = Url.ActionContext.RouteData.Values["manufacturerId"];

                if (manufacturerObj != null)
                {
                    int.TryParse(manufacturerObj.ToString(), out manufacturerId);
                }
            }

            if (manufacturerId > 0)
                return Content("");

            if (categoryId == 0 && manufacturerId == 0)
            {
                return Content("");
            }

            var list = new List<RequestParams>();
            foreach (var item in Url.ActionContext.HttpContext.Request.Query.ToList())
            {
                list.Add(new RequestParams
                {
                    Key = item.Key,
                    Value = item.Value
                });
            }

            HttpContext.Session.SetComplexData("_routeValue", Url.ActionContext.RouteData.Values);
            HttpContext.Session.SetComplexData("_requestParams", list);
            HttpContext.Session.SetComplexData("_requestPath", Url.ActionContext.HttpContext.Request.Path.Value);

            return View();
        }

        #endregion
    }
}
