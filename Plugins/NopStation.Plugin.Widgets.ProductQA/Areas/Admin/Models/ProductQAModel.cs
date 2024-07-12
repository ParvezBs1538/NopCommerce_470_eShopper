using System;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.ProductQuestionAnswer.Areas.Admin.Models
{
    public record ProductQAModel : BaseNopEntityModel
    {
        #region Properties

        public int StoreId { get; set; }
        [NopResourceDisplayName("Admin.NopStation.ProductQuestionAnswer.ProductQnA.Fields.ProductId")]
        public int ProductId { get; set; }
        public int ProductVendorId { get; set; }
        [NopResourceDisplayName("Admin.NopStation.ProductQuestionAnswer.ProductQnA.Fields.ProductInformation")]
        public string ProductInformation { get; set; }
        public int CustomerId { get; set; }
        public string CustomerUserName { get; set; }
        [NopResourceDisplayName("Admin.NopStation.ProductQuestionAnswer.ProductQnA.Fields.AnsweredBy")]
        public string AnsweredByCustomerName { get; set; }
        [NopResourceDisplayName("Admin.NopStation.ProductQuestionAnswer.ProductQnA.Fields.CustomerInformation")]
        public string CustomerInformation { get; set; }
        public int UpdatedByCustomerId { get; set; }
        public int ApprovedByCustomerId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.ProductQuestionAnswer.ProductQnA.Fields.QuestionText")]
        public string QuestionText { get; set; }

        [NopResourceDisplayName("Admin.NopStation.ProductQuestionAnswer.ProductQnA.Fields.AnswerText")]
        public string AnswerText { get; set; }
        public bool Deleted { get; set; }
        [NopResourceDisplayName("Admin.NopStation.ProductQuestionAnswer.ProductQnA.Fields.IsApproved")]
        public bool IsApproved { get; set; }
        public bool IsAnonymous { get; set; }

        [NopResourceDisplayName("Admin.NopStation.ProductQuestionAnswer.ProductQnA.Fields.CreatedOnUtc")]
        public DateTime CreatedOnUtc { get; set; }
        [NopResourceDisplayName("Admin.NopStation.ProductQuestionAnswer.ProductQnA.Fields.UpdatedOnUtc")]
        public DateTime UpdatedOnUtc { get; set; }

        #endregion
    }
}
