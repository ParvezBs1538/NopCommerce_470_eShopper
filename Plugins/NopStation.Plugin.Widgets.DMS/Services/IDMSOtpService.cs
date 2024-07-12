using System.Threading.Tasks;
using NopStation.Plugin.Widgets.DMS.Domain;

namespace NopStation.Plugin.Widgets.DMS.Services
{
    public interface IDMSOtpService
    {
        Task<string> GetOtpAsync(int otpLength = 4);

        Task<OTPRecord> GenerateCourierVerificationOtpByCourierShipmentAsync(CourierShipment courierShipment, Shipper shipper);

        Task<bool> CheckAndVerifyProofOfDeliveryOtpAsync(Shipper shipper, int shipmentId, string otp);
    }
}
