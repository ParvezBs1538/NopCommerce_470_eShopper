using System;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Plugins;
using Nop.Services.ScheduleTasks;

namespace Nopstation.Plugin.Widgets.DooFinder.Services
{
    public class GenerateFeedTask : IScheduleTask
    {
        private readonly ILocalizationService _localizationService;
        private readonly DooFinderSettings _doofinderSettings;
        private readonly IPluginService _pluginService;
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly ILogger _logger;

        public GenerateFeedTask(ILocalizationService localizationService,
            DooFinderSettings doofinderSettings,
            IPluginService pluginService,
            IStoreContext storeContext,
            ISettingService settingService,
            ILogger logger)
        {
            _localizationService = localizationService;
            _doofinderSettings = doofinderSettings;
            _pluginService = pluginService;
            _storeContext = storeContext;
            _logger = logger;
            _settingService = settingService;
        }

        public async Task ExecuteAsync()
        {
            int hour = _doofinderSettings.ScheduleFeedGeneratingHour, minute = _doofinderSettings.ScheduleFeedGeneratingMinute;
            var executionTime = DateTime.Now.Date.AddHours(hour).AddMinutes(minute);

            if (DateTime.Now > executionTime && !_doofinderSettings.ScheduleFeedIsExecutedForToday)
            {
                try
                {
                    //plugin
                    var pluginDescriptor = await _pluginService.GetPluginDescriptorBySystemNameAsync<IPlugin>(DooFinderDefaults.PLUGIN_SYSTEM_NAME);
                    if (pluginDescriptor == null || pluginDescriptor.Instance<IPlugin>() is not DooFinderPlugin plugin)
                        throw new Exception(await _localizationService.GetResourceAsync("Nopstation.Plugin.Widgets.DooFinder.ExceptionLoadPlugin"));

                    // Update googleShoppingSettings.ScheduleFeedLastExecutionStartTime, set datetime
                    _doofinderSettings.ScheduleFeedLastExecutionStartTime = DateTime.Now;

                    // Update googleShoppingSettings.IsFeedGenerating, set true
                    _doofinderSettings.IsFeedGenerating = true;

                    //now clear settings cache
                    await _settingService.SaveSettingAsync(_doofinderSettings);
                    await _settingService.ClearCacheAsync();

                    // generate file
                    await plugin.GenerateStaticFileAsync(_storeContext.GetCurrentStore());

                    // Update googleShoppingSettings.ScheduleFeedLastExecutionStartTime, set datetime
                    _doofinderSettings.ScheduleFeedLastExecutionEndTime = DateTime.Now;

                    // Update googleShoppingSettings.ScheduleFeedIsExecutedForToday, set true
                    _doofinderSettings.ScheduleFeedIsExecutedForToday = true;

                    //now clear settings cache
                    await _settingService.SaveSettingAsync(_doofinderSettings);
                    await _settingService.ClearCacheAsync();

                    _logger.Information(await _localizationService.GetResourceAsync("Nopstation.Plugin.Widgets.DooFinder.SuccessResult"));
                }
                catch (Exception exc)
                {
                    _logger.Error(exc.Message, exc);
                }
                finally
                {
                    // Update googleShoppingSettings.IsFeedGenerating, set false
                    _doofinderSettings.IsFeedGenerating = false;

                    //now clear settings cache
                    await _settingService.SaveSettingAsync(_doofinderSettings);
                    await _settingService.ClearCacheAsync();
                }
            }
            else if (_doofinderSettings.ScheduleFeedLastExecutionStartTime.HasValue &&
                _doofinderSettings.ScheduleFeedLastExecutionStartTime.Value.Date != DateTime.Now.Date)
            {
                // Update googleShoppingSettings.ScheduleFeedIsExecutedForToday, set false
                _doofinderSettings.ScheduleFeedIsExecutedForToday = false;

                //now clear settings cache
                await _settingService.SaveSettingAsync(_doofinderSettings);
                await _settingService.ClearCacheAsync();
            }
        }
    }
}
