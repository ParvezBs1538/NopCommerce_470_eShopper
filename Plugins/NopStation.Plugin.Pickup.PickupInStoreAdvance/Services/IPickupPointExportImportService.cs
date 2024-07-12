using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using NopStation.Plugin.Pickup.PickupInStoreAdvance.Areas.Admin.Models;

namespace NopStation.Plugin.Pickup.PickupInStoreAdvance.Services
{
    public interface IPickupPointExportImportService
    {
        Task ImportFromXlsxAsync(Stream stream);

        Task<byte[]> ExportToXlsxAsync(IList<StorePickupPointsExportImportModel> models);
    }
}