using System.Collections.Generic;
using Newtonsoft.Json;

namespace NopStation.Plugin.Widgets.SEOExpert.Domains
{
    public class SEOContent
    {
        [JsonProperty(PropertyName = "Id")]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "Sku")]
        public string Sku { get; set; }

        [JsonProperty(PropertyName = "MetaTitle")]
        public string MetaTitle { get; set; }

        [JsonProperty(PropertyName = "MetaDescription")]
        public string MetaDescription { get; set; }

        [JsonProperty(PropertyName = "MetaKeywords")]
        public string MetaKeywords { get; set; }
    }
    public class MessageBody
    {
        [JsonProperty(PropertyName = "role")]
        public string Role { get; set; }

        [JsonProperty(PropertyName = "content")]
        public string Content { get; set; }
    }

    public class ChoicesData
    {
        [JsonProperty(PropertyName = "index")]
        public int Index { get; set; }

        [JsonProperty(PropertyName = "message")]
        public MessageBody Message { get; set; }

        [JsonProperty(PropertyName = "finish_reason")]
        public string FinishReason { get; set; }
    }
    public class UsageData
    {
        [JsonProperty(PropertyName = "prompt_tokens")]
        public int PromptTokens { get; set; }

        [JsonProperty(PropertyName = "completion_tokens")]
        public int CompletionTokens { get; set; }

        [JsonProperty(PropertyName = "total_tokens")]
        public int TotalTokens { get; set; }
    }

    public class AiResponse
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "object")]
        public string Object { get; set; }

        [JsonProperty(PropertyName = "created")]
        public string Created { get; set; }

        [JsonProperty(PropertyName = "model")]
        public string Model { get; set; }

        [JsonProperty(PropertyName = "choices")]
        public List<ChoicesData> Choices { get; set; }

        [JsonProperty(PropertyName = "usage")]
        public UsageData Usage { get; set; }
    }
}
