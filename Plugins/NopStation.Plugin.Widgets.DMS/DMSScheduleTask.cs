using System;
using Nop.Services.ScheduleTasks;
using NopStation.Plugin.Widgets.DMS.Services;

namespace NopStation.Plugin.Widgets.DMS
{
    public partial class DMSScheduleTask : IScheduleTask
    {
        #region Fields
        private readonly IShipperDeviceService _shipperDeviceService;
        private readonly DMSSettings _dMSSettings;

        #endregion

        #region Ctor

        public DMSScheduleTask(IShipperDeviceService shipperDeviceService,
            DMSSettings dMSSettings)
        {
            _shipperDeviceService = shipperDeviceService;
            _dMSSettings = dMSSettings;
        }

        #endregion

        #region Methods
        public virtual async System.Threading.Tasks.Task ExecuteAsync()
        {
            int pageIndex = 0;
            int pageSize = 1000;
            while (true)
            {
                var shipperDevices = await _shipperDeviceService.GetAllShipperDeviceAsync(pageIndex: pageIndex++, pageSize: pageSize);
                foreach (var shipperDevice in shipperDevices)
                {
                    if (shipperDevice.LocationUpdatedOnUtc.HasValue)
                    {
                        var interval = (DateTime.UtcNow - shipperDevice.LocationUpdatedOnUtc.Value);
                        if (interval.TotalSeconds > (_dMSSettings.LocationUpdateIntervalInSeconds * 5))
                        {
                            shipperDevice.Online = false;
                            await _shipperDeviceService.UpdateShipperDeviceAsync(shipperDevice);
                        }
                    }
                }
            }
        }

        #endregion
    }
}