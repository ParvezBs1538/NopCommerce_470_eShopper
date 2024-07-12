using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using NopStation.Plugin.Misc.WidgetManager.Areas.Admin.Models;

namespace NopStation.Plugin.Widgets.DynamicSurvey.Areas.Admin.Models
{
    public record SurveyModel : BaseNopEntityModel, IAclSupportedModel, ILocalizedModel<SurveyLocalizedModel>,
        IStoreMappingSupportedModel, IWidgetZoneSupportedModel, IScheduleSupportedModel, ICustomerConditionSupportedModel
    {
        #region Ctor

        public SurveyModel()
        {
            Locales = new List<SurveyLocalizedModel>();
            SelectedStoreIds = new List<int>();
            AvailableStores = new List<SelectListItem>();

            SelectedCustomerRoleIds = new List<int>();
            AvailableCustomerRoles = new List<SelectListItem>();

            SurveySubmissionSearchModel = new SurveySubmissionSearchModel();
            SurveyAttributeMappingSearchModel = new SurveyAttributeMappingSearchModel();
            CopySurveyModel = new CopySurveyModel();

            AddWidgetZoneModel = new WidgetZoneModel();
            WidgetZoneSearchModel = new WidgetZoneSearchModel();
            AvailableEmailAccounts = new List<SelectListItem>();

            Schedule = new ScheduleModel();
            CustomerConditionSearchModel = new CustomerConditionSearchModel();
        }

        #endregion

        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.Surveys.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.Surveys.Fields.SystemName")]
        public string SystemName { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.Surveys.Fields.Description")]
        public string Description { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.Surveys.Fields.AllowMultipleSubmissions")]
        public bool AllowMultipleSubmissions { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.Surveys.Fields.IncludeInTopMenu")]
        public bool IncludeInTopMenu { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.Surveys.Fields.EnableEmailing")]
        public bool EnableEmailing { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.Surveys.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.Surveys.Fields.EmailAccountId")]
        public int EmailAccountId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.Surveys.Fields.ToEmailAddresses")]
        public string ToEmailAddresses { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.Surveys.Fields.BccEmailAddresses")]
        public string BccEmailAddresses { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.Surveys.Fields.DelayBeforeSend")]
        public int? DelayBeforeSend { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.Surveys.Fields.DelayPeriodId")]
        public int DelayPeriodId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.Surveys.Fields.Published")]
        public bool Published { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.Surveys.Fields.StartDateUtc")]
        [UIHint("DateTimeNullable")]
        public DateTime? StartDateUtc { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.Surveys.Fields.EndDateUtc")]
        [UIHint("DateTimeNullable")]
        public DateTime? EndDateUtc { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.Surveys.Fields.Name")]
        public DateTime CreatedOnUtc { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.Surveys.Fields.MetaKeywords")]
        public string MetaKeywords { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.Surveys.Fields.MetaDescription")]
        public string MetaDescription { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.Surveys.Fields.MetaTitle")]
        public string MetaTitle { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.Surveys.Fields.SeName")]
        public string SeName { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.Surveys.Fields.SendImmediately")]
        public bool SendImmediately { get; set; }

        public IList<SurveyLocalizedModel> Locales { get; set; }

        //ACL (customer roles)
        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.Surveys.Fields.AclCustomerRoles")]
        public IList<int> SelectedCustomerRoleIds { get; set; }
        public IList<SelectListItem> AvailableCustomerRoles { get; set; }

        //store mapping
        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.Surveys.Fields.LimitedToStores")]
        public IList<int> SelectedStoreIds { get; set; }
        public IList<SelectListItem> AvailableStores { get; set; }

        public ScheduleModel Schedule { get; set; }

        public SurveySubmissionSearchModel SurveySubmissionSearchModel { get; set; }
        public SurveyAttributeMappingSearchModel SurveyAttributeMappingSearchModel { get; set; }
        public CopySurveyModel CopySurveyModel { get; set; }

        public bool SurveyAttributesExist { get; set; }

        public IList<SelectListItem> AvailableEmailAccounts { get; set; }

        public WidgetZoneModel AddWidgetZoneModel { get; set; }
        public WidgetZoneSearchModel WidgetZoneSearchModel { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WidgetManager.CustomerConditionMappings.Fields.EnableCustomerCondition")]
        public bool EnableCustomerCondition { get; set; }

        public CustomerConditionSearchModel CustomerConditionSearchModel { get; set; }
    }

    public partial record SurveyLocalizedModel : ILocalizedLocaleModel
    {
        public SurveyLocalizedModel()
        {
            AvailableEmailAccounts = new List<SelectListItem>();
        }

        public int LanguageId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.Surveys.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.Surveys.Fields.Description")]
        public string Description { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.Surveys.Fields.MetaKeywords")]
        public string MetaKeywords { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.Surveys.Fields.MetaDescription")]
        public string MetaDescription { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.Surveys.Fields.MetaTitle")]
        public string MetaTitle { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.Surveys.Fields.SeName")]
        public string SeName { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DynamicSurvey.Surveys.Fields.EmailAccountId")]
        public int EmailAccountId { get; set; }

        public IList<SelectListItem> AvailableEmailAccounts { get; set; }
    }
}
