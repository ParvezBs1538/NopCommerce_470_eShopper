using System.Threading.Tasks;
using Nop.Services.ScheduleTasks;
using NopStation.Plugin.Payments.Afterpay.Services;

namespace NopStation.Plugin.Payments.Afterpay
{
    public class AfterpayPaymentStatusUpdateTask : IScheduleTask
    {
        private readonly IAfterpayUpdateService _afterpayUpdateService;

        public AfterpayPaymentStatusUpdateTask(IAfterpayUpdateService afterpayUpdateService)
        {
            _afterpayUpdateService = afterpayUpdateService;
        }

        public async Task ExecuteAsync()
        {
            await _afterpayUpdateService.UpdateOrderPaymentStatusAsync();
        }
    }
}
