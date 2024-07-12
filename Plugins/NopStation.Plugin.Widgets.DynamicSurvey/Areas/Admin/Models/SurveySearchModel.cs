using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.DynamicSurvey.Areas.Admin.Models
{
    public record SurveySearchModel : BaseSearchModel
    {
        #region Ctor

        public SurveySearchModel()
        {
            AvailableStores = new List<SelectListItem>();
            AvailablePublishedOptions = new List<SelectListItem>();
        }

        #endregion

        #region Properties

        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.Surveys.List.SearchSurveyName")]
        public string SearchSurveyName { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.Surveys.List.SearchStore")]
        public int SearchStoreId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.Surveys.List.SearchPublished")]
        public int SearchPublishedId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.Surveys.List.StartDate")]
        public DateTime? StartDate { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.Surveys.List.EndDate")]
        public DateTime? EndDate { get; set; }

        public bool HideStoresList { get; set; }

        public IList<SelectListItem> AvailableStores { get; set; }

        public IList<SelectListItem> AvailablePublishedOptions { get; set; }

        #endregion
    }
}
