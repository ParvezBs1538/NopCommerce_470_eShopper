using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Customers;
using NopStation.Plugin.Misc.SqlManager.Areas.Admin.Models;
using NopStation.Plugin.Misc.SqlManager.Domain;

namespace NopStation.Plugin.Misc.SqlManager.Services
{
    public interface ISqlReportService
    {
        Task DeleteSqlReportAsync(SqlReport sqlReport);

        Task InsertSqlReportAsync(SqlReport sqlReport);

        Task UpdateSqlReportAsync(SqlReport sqlReport);

        Task<SqlReport> GetSqlReportByIdAsync(int sqlReportId);

        Task<IPagedList<SqlReport>> GetAllSqlReportsAsync(int[] customerRoleIds = null, int pageIndex = 0, int pageSize = int.MaxValue);

        Task<bool> AuthorizeAsync(SqlReport report);

        Task<bool> AuthorizeAsync(SqlReport report, Customer customer);

        Task<IList<Dictionary<string, object>>> RunQueryAsync(string query);

        Task<SqlQueryModel> RunQueryAsync(SqlQueryModel model);

        Task<byte[]> ExportToExcelAsync(string sql);
    }
}