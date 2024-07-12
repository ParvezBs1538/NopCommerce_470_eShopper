using System.Collections.Generic;
using Newtonsoft.Json;

namespace NopStation.Plugin.EmailValidator.Abstract.Domains
{
    public class ApiResponse
    {
        [JsonProperty("error")]
        public ErrorData Error { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("autocorrect")]
        public string Autocorrect { get; set; }

        [JsonProperty("deliverability")]
        public string Deliverability { get; set; }

        [JsonProperty("quality_score")]
        public string QualityScore { get; set; }

        [JsonProperty("is_valid_format")]
        public NameValue IsValidFormat { get; set; }

        [JsonProperty("is_free_email")]
        public NameValue IsFreeEmail { get; set; }

        [JsonProperty("is_disposable_email")]
        public NameValue IsDisposableEmail { get; set; }

        [JsonProperty("is_role_email")]
        public NameValue IsRoleEmail { get; set; }

        [JsonProperty("is_catchall_email")]
        public NameValue IsCatchallEmail { get; set; }

        [JsonProperty("is_mx_found")]
        public NameValue IsMxFound { get; set; }

        [JsonProperty("is_smtp_valid")]
        public NameValue IsSmtpValid { get; set; }

        public class ErrorDetails
        {
            [JsonProperty("target")]
            public List<string> Target { get; set; }
        }

        public class ErrorData
        {
            [JsonProperty("message")]
            public string Message { get; set; }

            [JsonProperty("code")]
            public string Code { get; set; }

            [JsonProperty("details")]
            public ErrorDetails Details { get; set; }
        }

        public class NameValue
        {
            [JsonProperty("value")]
            public bool? Value { get; set; }

            [JsonProperty("text")]
            public string Text { get; set; }
        }
    }
}
