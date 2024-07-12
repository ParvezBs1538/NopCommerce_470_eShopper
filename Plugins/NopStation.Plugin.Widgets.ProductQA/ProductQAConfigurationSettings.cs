using Nop.Core.Configuration;

namespace NopStation.Plugin.Widgets.ProductQuestionAnswer
{
    public class ProductQAConfigurationSettings : ISettings
    {
        public bool IsEnable { get; set; }
        public bool QuestionAnonymous { get; set; }
        public string LimitedCustomerRole { get; set; }
        public string AnswerdCustomerRole { get; set; }
        public string LimitedStoreId { get; set; }
    }
}
