using System.Threading.Tasks;
using NopStation.Plugin.Misc.SqlManager.Areas.Admin.Models;
using NopStation.Plugin.Misc.SqlManager.Domain;

namespace NopStation.Plugin.Misc.SqlManager.Areas.Admin.Factories
{
    public interface ISqlReportModelFactory
    {
        Task<SqlReportSearchModel> PrepareSqlReportSearchModelAsync(SqlReportSearchModel searchModel);

        Task<SqlReportListModel> PrepareSqlReportListModelAsync(SqlReportSearchModel searchModel);

        Task<SqlReportModel> PrepareSqlReportModelAsync(SqlReportModel model, SqlReport sqlReport, bool excludeProperties = false);

        Task<SqlReportModel> PrepareSqlReportViewModelAsync(SqlReport sqlReport);
    }
}