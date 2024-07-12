using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using NopStation.Plugin.Shipping.DHL.Domain;
using Nop.Services.Caching;

namespace NopStation.Plugin.Shipping.DHL.Services
{
    public class DHLAcceptedServicesService : IDHLAcceptedServicesService
    {
        #region Fields

        private readonly IRepository<DHLService> _dhlAcceptedService;

        #endregion

        #region ctor

        public DHLAcceptedServicesService(IRepository<DHLService> dhlAcceptedService)
        {
            _dhlAcceptedService = dhlAcceptedService;
        }

        #endregion

        public async Task DeleteDHLServiceAsync(DHLService dhlService)
        {
            await _dhlAcceptedService.DeleteAsync(dhlService);
        }

        public Task<IPagedList<DHLService>> GetAllDHLServicesAsync(int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _dhlAcceptedService.Table;
            query = query.OrderBy(ds => ds.Id);

            return  query.ToPagedListAsync( pageIndex, pageSize);
        }

        public async Task<DHLService> GetDHLServiceByIdAsync(int id)
        {
            if (id == 0)
                return null;

            return await _dhlAcceptedService.GetByIdAsync(id, cache => default);
        }

        public async Task InsertDHLServiceAsync(DHLService dhlService)
        {
            await _dhlAcceptedService.InsertAsync(dhlService);
        }

        public async Task UpdateDHLServiceAsync(DHLService dhlService)
        {
            await _dhlAcceptedService.UpdateAsync(dhlService);
        }
    }
}
