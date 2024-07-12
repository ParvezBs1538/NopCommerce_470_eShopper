namespace NopStation.Plugin.Widgets.ProductQuestionAnswer.Models
{
    public class ProductQuestionSaveModel
    {
        #region Properties
        public int ProductId { get; set; }
        public string QuestionText { get; set; }
        public bool IsAnonymous { get; set; }
        #endregion
    }
}
