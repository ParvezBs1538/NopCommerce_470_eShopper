using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Nop.Core.Domain.Topics;

namespace NopStation.Plugin.Widgets.ExportImportTopic.Services
{
    public partial interface IExImManager
    {
        Task<byte[]> ExportToXlsxAsync(IList<Topic> topics);

        Task ImportFromXlsxAsync(Stream stream);
    }
}
