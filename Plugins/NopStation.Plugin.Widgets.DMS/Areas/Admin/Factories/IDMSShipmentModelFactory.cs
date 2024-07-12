using System.Threading.Tasks;

namespace NopStation.Plugin.Widgets.DMS.Areas.Admin.Factories
{
    public interface IDMSShipmentModelFactory
    {
        Task<byte[]> GeneratePackagingSlipsToPdfAsync(int shipmentId = 0);
    }
}
