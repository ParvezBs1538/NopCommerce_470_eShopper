using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.ProductQuestionAnswer.Areas.Admin.Models
{
    public record ConfigurationModel : BaseNopModel
    {
        #region Ctor
        public ConfigurationModel()
        {
            SelectedLimitedCustomerRoleIds = new List<int>();
            SelectedAnsweredCustomerRoleIds = new List<int>();
            SelectedLimitedStoreId = new List<int>();

            AvailableLimitedCustomerRoles = new List<SelectListItem>();
            AvailableAnsweredCustomerRoles = new List<SelectListItem>();
            AvailableLimitedStoreId = new List<SelectListItem>();
        }
        #endregion

        #region Properties

        public int ActiveStoreScopeConfiguration { get; set; }

        //enable
        [NopResourceDisplayName("Admin.NopStation.ProductQuestionAnswer.Configuration.Fields.Enable")]
        public bool IsEnable { get; set; }
        public bool IsEnable_OverrideForStore { get; set; }

        //QuestionAnonymous
        [NopResourceDisplayName("Admin.NopStation.ProductQuestionAnswer.Configuration.Fields.QuestionAnonymous")]
        public bool QuestionAnonymous { get; set; }
        public bool QuestionAnonymous_OverrideForStore { get; set; }

        //limited store
        [NopResourceDisplayName("Admin.NopStation.ProductQuestionAnswer.Configuration.Fields..Store")]
        public string LimitedStoreId { get; set; }
        public bool LimitedStoreId_OverrideForStore { get; set; }
        public IList<int> SelectedLimitedStoreId { get; set; }
        public IList<SelectListItem> AvailableLimitedStoreId { get; set; }

        //limited customer role - who ask the questions
        [NopResourceDisplayName("Admin.NopStation.ProductQuestionAnswer.Configuration.Fields..WhoAskTheQuestion")]
        public string LimitedCustomerRole { get; set; }
        public bool LimitedCustomer_OverrideForStore { get; set; }
        public IList<int> SelectedLimitedCustomerRoleIds { get; set; }
        public IList<SelectListItem> AvailableLimitedCustomerRoles { get; set; }

        //answered question - who answer the question
        [NopResourceDisplayName("Admin.NopStation.ProductQuestionAnswer.Configuration.Fields..WhoAnswerTheQuestion")]
        public string AnswerdCustomerRole { get; set; }
        public bool AnswerdCustomerRole_OverrideForStore { get; set; }
        public IList<int> SelectedAnsweredCustomerRoleIds { get; set; }
        public IList<SelectListItem> AvailableAnsweredCustomerRoles { get; set; }
        #endregion
    }
}
