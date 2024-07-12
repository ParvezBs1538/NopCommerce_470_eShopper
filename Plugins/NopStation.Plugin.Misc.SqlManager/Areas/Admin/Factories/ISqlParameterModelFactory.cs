using System.Threading.Tasks;
using NopStation.Plugin.Misc.SqlManager.Areas.Admin.Models;
using NopStation.Plugin.Misc.SqlManager.Domain;

namespace NopStation.Plugin.Misc.SqlManager.Areas.Admin.Factories
{
    public interface ISqlParameterModelFactory
    {
        Task<SqlParameterSearchModel> PrepareSqlParameterSearchModelAsync(SqlParameterSearchModel searchModel);

        Task<SqlParameterListModel> PrepareSqlParameterListModelAsync(SqlParameterSearchModel searchModel);

        Task<SqlParameterModel> PrepareSqlParameterModelAsync(SqlParameterModel model, SqlParameter sqlParameter, bool excludeProperties = false);

        Task<SqlParameterValueListModel> PrepareSqlParameterValueListModelAsync(SqlParameterValueSearchModel searchModel, SqlParameter sqlParameter);

        Task<SqlParameterValueModel> PrepareSqlParameterValueModelAsync(SqlParameterValueModel model, SqlParameterValue sqlParameterValue,
            SqlParameter sqlParameter, bool excludeProperties = false);
    }
}