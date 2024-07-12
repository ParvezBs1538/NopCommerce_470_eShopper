using System;
using System.Threading.Tasks;
using Nop.Services.Logging;
using Nop.Services.ScheduleTasks;
using NopStation.Plugin.Widgets.AbandonedCarts.Domain;
using NopStation.Plugin.Widgets.AbandonedCarts.Models;
using NopStation.Plugin.Widgets.AbandonedCarts.Services;

namespace NopStation.Plugin.Widgets.AbandonedCarts.TaskService
{
    public class DeleteAbandonedCartsTask : IScheduleTask
    {
        #region Fields

        private readonly ILogger _logger;
        private readonly IAbandonedCartService _abandonedCartService;

        #endregion

        #region Ctors

        public DeleteAbandonedCartsTask(IAbandonedCartService abandonedCartService,
            ILogger logger)
        {
            _abandonedCartService = abandonedCartService;
            _logger = logger;
        }

        #endregion

        #region Methods

        public async Task ExecuteAsync()
        {
            var model = new AbandonmentMaintenanceModel()
            {
                StatusId = (int)ClearAbandonedStatus.Deleted,
                LastActivityBefore = DateTime.UtcNow.AddDays(-1),
            };
            var count = await _abandonedCartService.BulkDeleteAbandonedCartsAsync(model);
            await _logger.InformationAsync($"(Abandoned Carts Plugin) Task Scheduler for Deleting Abandoned Carts has deleted {count} carts.");
        }

        #endregion
    }
}
