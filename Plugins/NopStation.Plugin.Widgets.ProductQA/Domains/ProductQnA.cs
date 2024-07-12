using System;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Localization;

namespace NopStation.Plugin.Widgets.ProductQuestionAnswer.Domains
{
    public class ProductQnA : BaseEntity, ILocalizedEntity, ISoftDeletedEntity
    {
        public int ProductId { get; set; }
        public string QuestionText { get; set; }
        public string AnswerText { get; set; }
        public bool IsApproved { get; set; }
        public bool IsAnonymous { get; set; }
        public int StoreId { get; set; }
        public int CustomerId { get; set; }
        public int UpdatedByCustomerId { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public bool Deleted { get; set; }
    }
}
