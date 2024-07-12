using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Shipping;

namespace NopStation.Plugin.Widgets.DMS.Services
{
    public interface IDMSPdfService
    {
        Task PrintPackagingSlipsToPdfAsync(Stream stream, IList<Shipment> shipments, int languageId = 0);
        Task PrintPackagingSlipToPdfAsync(Stream stream, Shipment shipment, Language language = null);
    }
}
