using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.ProductQuestionAnswer.Areas.Admin.Models
{
    public partial record ProductQASearchModel : BaseSearchModel
    {
        #region Ctor
        public ProductQASearchModel()
        {
            AvailableStores = new List<SelectListItem>();
            ApprovalOptions = new List<SelectListItem>();
            AnswerOptions = new List<SelectListItem>();
        }
        #endregion

        #region Properties
        [NopResourceDisplayName("Admin.NopStation.ProductQuestionAnswer.ProductQnA.List.Fields.SearchStoreId")]
        public int SearchStoreId { get; set; }
        public IList<SelectListItem> AvailableStores { get; set; }

        [NopResourceDisplayName("Admin.NopStation.ProductQuestionAnswer.ProductQnA.List.Fields.SearchProductId")]
        public int SearchProductId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.ProductQuestionAnswer.ProductQnA.List.Fields.SearchApproveOptionId")]
        public int SearchApproveOptionId { get; set; }
        public IList<SelectListItem> ApprovalOptions { get; set; }

        [NopResourceDisplayName("Admin.NopStation.ProductQuestionAnswer.ProductQnA.List.Fields.SearchAnswerOptionId")]
        public int SearchAnswerOptionId { get; set; }
        public IList<SelectListItem> AnswerOptions { get; set; }

        [NopResourceDisplayName("Admin.NopStation.ProductQuestionAnswer.ProductQnA.List.Fields.CreatedFrom")]
        [UIHint("DateNullable")]
        public DateTime? CreatedFrom { get; set; }
        [NopResourceDisplayName("Admin.NopStation.ProductQuestionAnswer.ProductQnA.List.Fields.CreatedTo")]

        [UIHint("DateNullable")]
        public DateTime? CreatedTo { get; set; }

        #endregion
    }
}
