using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using NopStation.Plugin.Shipping.Redx.Domains;

namespace NopStation.Plugin.Shipping.Redx.Services
{
    public class RedxAreaService : IRedxAreaService
    {
        private readonly IRepository<RedxArea> _redxAreaRepository;

        #region Ctor

        public RedxAreaService(IRepository<RedxArea> redxAreaRepository)
        {
            _redxAreaRepository = redxAreaRepository;
        }

        #endregion

        #region Methods

        public async Task InsertRedxAreaAsync(RedxArea redxArea)
        {
            await _redxAreaRepository.InsertAsync(redxArea);
        }

        public async Task UpdateRedxAreaAsync(RedxArea redxArea)
        {
            await _redxAreaRepository.UpdateAsync(redxArea);
        }

        public async Task DeleteRedxAreaAsync(RedxArea redxArea)
        {
            await _redxAreaRepository.DeleteAsync(redxArea);
        }

        public async Task<IList<RedxArea>> GetRedxAreasByPostCodeAsync(string postCode)
        {
            var query = from ra in _redxAreaRepository.Table
                        where !ra.Deleted && ra.PostCode == postCode
                        select ra;

            return await query.ToListAsync();
        }

        public async Task<RedxArea> GetRedxAreaByRedxAreaIdAsync(int redxAreaId)
        {
            if (redxAreaId == 0)
                return null;

            var query = from ra in _redxAreaRepository.Table
                        where ra.RedxAreaId == redxAreaId
                        select ra;

            return await query.FirstOrDefaultAsync();
        }

        public async Task<IPagedList<RedxArea>> GetRedxAreasAsync(int? stateProvinceId = null, bool loadUnmapped = true, 
            string distName = "", int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = from ra in _redxAreaRepository.Table
                        where !ra.Deleted && 
                            (string.IsNullOrWhiteSpace(distName) || ra.DistrictName.Contains(distName))
                        select ra;

            if (stateProvinceId > 0)
            {
                if (loadUnmapped)
                    query = query.Where(ra => ra.StateProvinceId == stateProvinceId || !ra.StateProvinceId.HasValue);
                else
                    query = query.Where(ra => ra.StateProvinceId == stateProvinceId);
            }

            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

        #endregion
    }
}