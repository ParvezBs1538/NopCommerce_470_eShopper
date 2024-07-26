using Nop.Plugin.Payments.AfterpayPayment.Areas.Admin.Services;
using Nop.Services.Tasks;

namespace Nop.Plugin.Payments.AfterpayPayment.Areas.Admin.Extensions
{
    public class UpdateAfterpayPaymentStatus : IScheduleTask
    {
        private readonly IAfterpayUpdateService _afterpayPaymentStatusUpdateService;
        public UpdateAfterpayPaymentStatus(IAfterpayUpdateService afterpayPaymentStatusUpdateService)
        {
            _afterpayPaymentStatusUpdateService = afterpayPaymentStatusUpdateService;
        }
        public void Execute()
        {
            _afterpayPaymentStatusUpdateService.UpdateOrderPaymentStatus();
        }
    }
}
