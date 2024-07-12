using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NopStation.Plugin.Misc.AmazonPersonalize.Domains;

namespace NopStation.Plugin.Misc.AmazonPersonalize.Services
{
    public interface IPersonalizeExportManager
    {
        Task<string> ExportInteractionsTxtAsync(IList<EventReportLine> subscriptions);
    }
}
