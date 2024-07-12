using Nop.Core.Configuration;

namespace NopStation.Plugin.Widgets.SEOExpert
{
    public class SEOExpertSettings : ISettings
    {
        public string OpenAIApiKey { get; set; }
        public string Endpoint { get; set; }
        public string ModelName { get; set; }
        public bool RequireAdminApproval { get; set; }
        public string AdditionalInfoWithName { get; set; }
        public string AdditionalInfoWithShortDescription { get; set; }
        public string AdditionalInfoWithFullDescription { get; set; }
        public string RegenerateConditionIds { get; set; }
        public decimal Temperature { get; set; }
        public bool EnableListGeneration { get; set; }
    }
}
