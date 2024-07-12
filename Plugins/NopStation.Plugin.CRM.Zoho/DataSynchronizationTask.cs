using System;
using System.Threading.Tasks;
using Nop.Services.ScheduleTasks;
using NopStation.Plugin.CRM.Zoho.Domain;
using NopStation.Plugin.CRM.Zoho.Services;

namespace NopStation.Plugin.CRM.Zoho
{
    public class DataSynchronizationTask : IScheduleTask
    {
        private readonly IZohoService _zohoService;
        private readonly ZohoCRMSettings _zohoCRMSettings;

        public DataSynchronizationTask(IZohoService zohoService,
            ZohoCRMSettings zohoCRMSettings)
        {
            _zohoService = zohoService;
            _zohoCRMSettings = zohoCRMSettings;
        }

        #region Task

        public async Task ExecuteAsync()
        {
            if (_zohoCRMSettings.ExpireOnUtc.AddMinutes(-10) <= DateTime.UtcNow)
                await _zohoService.RefreshAccessTokenAsync();

            await _zohoService.SyncStoresAsync(SyncType.DifferentialSync);
            await _zohoService.SyncCustomersAsync(SyncType.DifferentialSync);
            await _zohoService.SyncVendorsAsync(SyncType.DifferentialSync);
            await _zohoService.SyncProductsAsync(SyncType.DifferentialSync);
            await _zohoService.SyncOrdersAsync(SyncType.DifferentialSync);

            if (_zohoCRMSettings.SyncShipment)
            {
                await _zohoService.SyncShipmentsAsync(SyncType.DifferentialSync);
                if (_zohoCRMSettings.SyncShipmentItem)
                    await _zohoService.SyncShipmentItemsAsync(SyncType.DifferentialSync);
            }
        }

        #endregion
    }
}
