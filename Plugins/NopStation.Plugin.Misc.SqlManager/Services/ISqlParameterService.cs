using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Misc.SqlManager.Domain;

namespace NopStation.Plugin.Misc.SqlManager.Services
{
    public interface ISqlParameterService
    {
        Task DeleteSqlParameterAsync(SqlParameter sqlParameter);

        Task InsertSqlParameterAsync(SqlParameter sqlParameter);

        Task UpdateSqlParameterAsync(SqlParameter sqlParameter);

        Task<SqlParameter> GetSqlParameterByIdAsync(int sqlParameterId);

        Task<SqlParameter> GetSqlParameterBySystemNameAsync(string systemName);

        Task<IPagedList<SqlParameter>> GetAllSqlParametersAsync(int pageIndex = 0, int pageSize = int.MaxValue);

        Task DeleteSqlParameterValueAsync(SqlParameterValue sqlParameterValue);

        Task InsertSqlParameterValueAsync(SqlParameterValue sqlParameterValue);

        Task UpdateSqlParameterValueAsync(SqlParameterValue sqlParameterValue);

        Task<SqlParameterValue> GetSqlParameterValueByIdAsync(int sqlParameterValueId);

        Task<IList<SqlParameterValue>> GetqlParameterValuesByParameterIdAsync(int parameterId);

        bool IsSystemNameExsistForAnotherSqlParameter(string systemName, int currentParameterId);
    }
}