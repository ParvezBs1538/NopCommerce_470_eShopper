using System.Threading.Tasks;
using Nop.Services.ScheduleTasks;
using NopStation.Plugin.Misc.AlgoliaSearch.Factories;

namespace NopStation.Plugin.Misc.AlgoliaSearch
{
    public class AlgoliaUpdateItemTask : IScheduleTask
    {
        private readonly IAlgoliaHelperFactory _algoliaSearchHelperService;

        public AlgoliaUpdateItemTask(IAlgoliaHelperFactory algoliaSearchHelperService)
        {
            _algoliaSearchHelperService = algoliaSearchHelperService;
        }

        public async Task ExecuteAsync()
        {
            await _algoliaSearchHelperService.UpdateAlgoliaItemAsync();
        }
    }
}
