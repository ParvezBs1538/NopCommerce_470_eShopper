using System;
using System.Collections.Generic;
using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.ProductQuestionAnswer.Models
{
    public record ProductQuestionAnswerModel : BaseNopEntityModel
    {
        #region Ctor

        public ProductQuestionAnswerModel()
        {
            ProductQuestionAnswerPublicInfoModels = new List<ProductQuestionAnswerPublicInfoModel>();
        }

        #endregion

        #region Properties

        public bool NoResults { get; set; }
        public int CurrentPageNumber { get; set; }
        public int TotalPages { get; set; }
        public List<ProductQuestionAnswerPublicInfoModel> ProductQuestionAnswerPublicInfoModels { get; set; }

        #endregion

        #region NestedClass

        public record ProductQuestionAnswerPublicInfoModel : BaseNopEntityModel
        {
            public string QuestionText { get; set; }
            public string QuestionByCustomerName { get; set; }
            public DateTime QuestionAskedDate { get; set; }
            public string AnswerText { get; set; }
            public string AnswerByCustomerName { get; set; }
            public DateTime AnswerGivenDate { get; set; }
        }
        #endregion
    }
}
