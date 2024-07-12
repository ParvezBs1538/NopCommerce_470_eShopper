using Nop.Services.ScheduleTasks;
using NopStation.Plugin.Payments.Quickstream.Services;

namespace NopStation.Plugin.Payments.Quickstream
{
    public class UpdatePaymentStatusTask : IScheduleTask
    {
        private readonly IQuickStreamPaymentService _quickStreamPaymentService;

        public UpdatePaymentStatusTask(IQuickStreamPaymentService quickStreamPaymentService)
        {
            _quickStreamPaymentService = quickStreamPaymentService;
        }

        public async System.Threading.Tasks.Task ExecuteAsync()
        {
            await _quickStreamPaymentService.UpdateOrderPaymentStatusAsync();
        }
    }
}
