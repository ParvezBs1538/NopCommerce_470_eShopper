using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using NopStation.Plugin.Misc.SqlManager.Domain;

namespace NopStation.Plugin.Misc.SqlManager.Services
{
    public class SqlParameterService : ISqlParameterService
    {
        #region Fields

        private readonly IRepository<SqlParameter> _sqlParameterRepository;
        private readonly IRepository<SqlParameterValue> _sqlParameterValueRepository;

        #endregion

        #region Ctor

        public SqlParameterService(IRepository<SqlParameter> sqlParameterRepository,
            IRepository<SqlParameterValue> sqlParameterValueRepository)
        {
            _sqlParameterRepository = sqlParameterRepository;
            _sqlParameterValueRepository = sqlParameterValueRepository;
        }

        #endregion

        #region Methods

        public async Task DeleteSqlParameterAsync(SqlParameter sqlParameter)
        {
            if (sqlParameter == null)
                throw new ArgumentNullException(nameof(sqlParameter));

            await _sqlParameterRepository.DeleteAsync(sqlParameter);
        }

        public async Task InsertSqlParameterAsync(SqlParameter sqlParameter)
        {
            if (sqlParameter == null)
                throw new ArgumentNullException(nameof(sqlParameter));

            await _sqlParameterRepository.InsertAsync(sqlParameter);
        }

        public async Task UpdateSqlParameterAsync(SqlParameter sqlParameter)
        {
            if (sqlParameter == null)
                throw new ArgumentNullException(nameof(sqlParameter));

            await _sqlParameterRepository.UpdateAsync(sqlParameter);
        }

        public async Task<SqlParameter> GetSqlParameterByIdAsync(int sqlParameterId)
        {
            if (sqlParameterId == 0)
                return null;

            return await _sqlParameterRepository.GetByIdAsync(sqlParameterId, cache => default);
        }

        public async Task<SqlParameter> GetSqlParameterBySystemNameAsync(string systemName)
        {
            if (string.IsNullOrWhiteSpace(systemName))
                return null;

            return await _sqlParameterRepository.Table.FirstOrDefaultAsync(x => x.SystemName == systemName);
        }

        public async Task<IPagedList<SqlParameter>> GetAllSqlParametersAsync(int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _sqlParameterRepository.Table;
            query = query.OrderByDescending(e => e.Id);

            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

        public bool IsSystemNameExsistForAnotherSqlParameter(string systemName, int currentParameterId)
        {
            if (string.IsNullOrEmpty(systemName) || currentParameterId < 1)
                return false;

            var query = from spr in _sqlParameterRepository.Table
                        where spr.SystemName == systemName &&
                        spr.Id != currentParameterId
                        select spr;

            return query.Count() > 0;
        }

        public async Task DeleteSqlParameterValueAsync(SqlParameterValue sqlParameterValue)
        {
            if (sqlParameterValue == null)
                throw new ArgumentNullException(nameof(sqlParameterValue));

            await _sqlParameterValueRepository.DeleteAsync(sqlParameterValue);
        }

        public async Task InsertSqlParameterValueAsync(SqlParameterValue sqlParameterValue)
        {
            if (sqlParameterValue == null)
                throw new ArgumentNullException(nameof(sqlParameterValue));

            await _sqlParameterValueRepository.InsertAsync(sqlParameterValue);
        }

        public async Task UpdateSqlParameterValueAsync(SqlParameterValue sqlParameterValue)
        {
            if (sqlParameterValue == null)
                throw new ArgumentNullException(nameof(sqlParameterValue));

            await _sqlParameterValueRepository.UpdateAsync(sqlParameterValue);
        }

        public async Task<SqlParameterValue> GetSqlParameterValueByIdAsync(int sqlParameterValueId)
        {
            if (sqlParameterValueId == 0)
                return null;

            return await _sqlParameterValueRepository.GetByIdAsync(sqlParameterValueId, cache => default);
        }

        public async Task<IList<SqlParameterValue>> GetqlParameterValuesByParameterIdAsync(int parameterId)
        {
            var query = _sqlParameterValueRepository.Table.Where(x => x.SqlParameterId == parameterId);

            return await query.ToListAsync();
        }

        #endregion
    }
}