using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.DynamicSurvey.Areas.Admin.Models
{
    public partial record SurveySubmissionSearchModel : BaseSearchModel
    {

        #region Ctor

        public SurveySubmissionSearchModel()
        {
            AvailableSurveys = new List<SelectListItem>();
        }

        #endregion

        #region Properties

        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.SurveySubmissions.Fields.SurveyId")]
        public int SurveyId { get; set; }

        public int CustomerId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.SurveySubmissions.Fields.CustomerEmail")]
        public string SearchCustomerEmail { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.SurveySubmissions.Fields.CreatedOnFrom")]
        public DateTime? DateStart { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.SurveySubmissions.Fields.CreatedOnTo")]
        public DateTime? DateEnd { get; set; }

        public IList<SelectListItem> AvailableSurveys { get; set; }

        #endregion
    }
}