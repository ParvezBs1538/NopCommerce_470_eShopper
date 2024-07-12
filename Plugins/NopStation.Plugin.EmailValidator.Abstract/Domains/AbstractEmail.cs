using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Nop.Core;

namespace NopStation.Plugin.EmailValidator.Abstract.Domains
{
    public class AbstractEmail : BaseEntity
    {
        public string Email { get; set; }

        public string Deliverability { get; set; }

        public bool IsDisposable { get; set; }

        public bool IsFree { get; set; }

        public bool IsRoleAccount { get; set; }

        public DateTime CreatedOnUtc { get; set; }

        public DateTime UpdatedOnUtc { get; set; }

        public int ValidationCount { get; set; }

        public decimal QualityScore { get; set; }
    }
}
