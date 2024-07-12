using System;
using Nop.Core;
using Nop.Core.Domain.Security;

namespace NopStation.Plugin.Misc.SqlManager.Domain
{
    public class SqlReport : BaseEntity, IAclSupported
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public string Query { get; set; }

        public bool SubjectToAcl { get; set; }

        public DateTime CreatedOnUtc { get; set; }

        public DateTime UpdatedOnUtc { get; set; }
    }
}
