using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Shipping.Redx.Domains;

namespace NopStation.Plugin.Shipping.Redx.Services
{
    public interface IRedxAreaService
    {
        Task InsertRedxAreaAsync(RedxArea redxArea);

        Task UpdateRedxAreaAsync(RedxArea redxArea);

        Task DeleteRedxAreaAsync(RedxArea redxArea);

        Task<RedxArea> GetRedxAreaByRedxAreaIdAsync(int redxAreaId);

        Task<IList<RedxArea>> GetRedxAreasByPostCodeAsync(string postCode);

        Task<IPagedList<RedxArea>> GetRedxAreasAsync(int? stateProvinceId = null, bool loadUnmapped = true,
            string distName = "", int pageIndex = 0, int pageSize = int.MaxValue);
    }
}