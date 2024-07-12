using Nop.Core;
using NopStation.Plugin.Shipping.DHL.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NopStation.Plugin.Shipping.DHL.Services
{
    public interface IDHLAcceptedServicesService
    {
        #region Service

        Task<IPagedList<DHLService>> GetAllDHLServicesAsync(int pageIndex = 0, int pageSize = int.MaxValue);

        Task<DHLService> GetDHLServiceByIdAsync(int id);

        Task InsertDHLServiceAsync(DHLService dhlService);

        Task UpdateDHLServiceAsync(DHLService dhlService);

        Task DeleteDHLServiceAsync(DHLService dhlService);

        #endregion
    }
}
