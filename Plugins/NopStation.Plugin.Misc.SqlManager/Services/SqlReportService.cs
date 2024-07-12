using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ClosedXML.Excel;
using LinqToDB;
using Microsoft.Data.SqlClient;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Data;
using Nop.Services.Customers;
using Nop.Services.Logging;
using Nop.Services.Security;
using NopStation.Plugin.Misc.SqlManager.Areas.Admin.Models;
using NopStation.Plugin.Misc.SqlManager.Domain;

namespace NopStation.Plugin.Misc.SqlManager.Services
{
    public class SqlReportService : ISqlReportService
    {
        #region Fields

        private readonly IRepository<SqlReport> _sqlReportRepository;
        private readonly IRepository<AclRecord> _aclRepository;
        private readonly ICustomerService _customerService;
        private readonly IStaticCacheManager _cacheManager;
        private readonly IWorkContext _workContext;
        private readonly ILogger _logger;

        #endregion

        #region Ctor

        public SqlReportService(IRepository<SqlReport> sqlReportRepository,
            IRepository<AclRecord> aclRepository,
            ICustomerService customerService,
            IStaticCacheManager cacheManager,
            IWorkContext workContext,
            ILogger logger)
        {
            _sqlReportRepository = sqlReportRepository;
            _aclRepository = aclRepository;
            _customerService = customerService;
            _cacheManager = cacheManager;
            _workContext = workContext;
            _logger = logger;
        }

        #endregion

        #region Utilities

        protected virtual async Task<int[]> GetCustomerRoleIdsWithAccessAsync(SqlReport report)
        {
            if (report == null)
                throw new ArgumentNullException(nameof(report));

            var entityId = report.Id;
            var entityName = report.GetType().Name;

            var key = _cacheManager.PrepareKeyForDefaultCache(NopSecurityDefaults.AclRecordCacheKey, entityId, entityName);

            var query = from ur in _aclRepository.Table
                        where ur.EntityId == entityId &&
                              ur.EntityName == entityName
                        select ur.CustomerRoleId;

            return await query.ToArrayAsync();
        }

        private List<Dictionary<string, object>> Serialize(SqlDataReader reader)
        {
            var results = new List<Dictionary<string, object>>();
            var cols = new List<string>();

            for (int i = 0; i < reader.FieldCount; i++)
            {
                cols.Add(reader.GetName(i));
            }

            while (reader.Read())
            {
                results.Add(SerializeRow(cols, reader));
            }
            return results;
        }

        private Dictionary<string, object> SerializeRow(List<string> cols, SqlDataReader reader)
        {
            Dictionary<string, object> row = new Dictionary<string, object>();
            foreach (string col in cols)
            {
                // Replacing anything that's not in a-z, A-Z, 0-9.
                Regex rgx = new Regex("[^a-zA-Z0-9]");
                string colName = rgx.Replace(col, "_");
                if (colName.Substring(0, 1).Any(char.IsDigit))
                {
                    colName = "_" + colName;
                }
                if (!row.ContainsKey(colName))
                    row.Add(colName, reader[col]);
            }
            return row;
        }

        /// <summary>
        /// Set caption style to excel cell
        /// </summary>
        /// <param name="cell">Excel cell</param>
        public void SetCaptionStyle(IXLCell cell)
        {
            cell.Style.Fill.PatternType = XLFillPatternValues.Solid;
            cell.Style.Fill.BackgroundColor = XLColor.FromColor(Color.FromArgb(13, 81, 163));
            cell.Style.Font.Bold = true;
            cell.Style.Font.FontColor = XLColor.FromColor(Color.FromArgb(255, 255, 255));
        }

        #endregion

        #region Methods

        public async Task DeleteSqlReportAsync(SqlReport sqlReport)
        {
            if (sqlReport == null)
                throw new ArgumentNullException(nameof(sqlReport));

            await _sqlReportRepository.DeleteAsync(sqlReport);
        }

        public async Task InsertSqlReportAsync(SqlReport sqlReport)
        {
            if (sqlReport == null)
                throw new ArgumentNullException(nameof(sqlReport));

            await _sqlReportRepository.InsertAsync(sqlReport);
        }

        public async Task UpdateSqlReportAsync(SqlReport sqlReport)
        {
            if (sqlReport == null)
                throw new ArgumentNullException(nameof(sqlReport));

            await _sqlReportRepository.UpdateAsync(sqlReport);
        }

        public async Task<SqlReport> GetSqlReportByIdAsync(int sqlReportId)
        {
            if (sqlReportId == 0)
                return null;

            return await _sqlReportRepository.GetByIdAsync(sqlReportId, cache => default);
        }

        public async Task<IPagedList<SqlReport>> GetAllSqlReportsAsync(int[] customerRoleIds = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _sqlReportRepository.Table;

            if (customerRoleIds != null && customerRoleIds.Any())
            {
                query = from m in query
                        join acl in _aclRepository.Table
                            on new { c1 = m.Id, c2 = nameof(SqlReport) } equals new { c1 = acl.EntityId, c2 = acl.EntityName } into m_acl
                        from acl in m_acl.DefaultIfEmpty()
                        where !m.SubjectToAcl || customerRoleIds.Contains(acl.CustomerRoleId)
                        select m;
            }

            query = query.Distinct().OrderByDescending(e => e.Id);

            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

        public virtual async Task<bool> AuthorizeAsync(SqlReport report)
        {
            return await AuthorizeAsync(report, await _workContext.GetCurrentCustomerAsync());
        }

        public virtual async Task<bool> AuthorizeAsync(SqlReport report, Customer customer)
        {
            if (report == null)
                return false;

            if (customer == null)
                return false;

            if (!report.SubjectToAcl)
                return true;

            foreach (var role1 in await _customerService.GetCustomerRolesAsync(customer))
                foreach (var role2Id in await GetCustomerRoleIdsWithAccessAsync(report))
                    if (role1.Id == role2Id)
                        //yes, we have such permission
                        return true;

            //no permission found
            return false;
        }

        public async Task<IList<Dictionary<string, object>>> RunQueryAsync(string query)
        {
            #region With transaction

            var connectionStringBuilder = new SqlConnectionStringBuilder(DataSettingsManager.LoadSettings().ConnectionString);
            var connection = new SqlConnection(connectionStringBuilder.ConnectionString);

            var jsonQuery = new List<Dictionary<string, object>>();

            try
            {
                connection.Open();
            }
            catch (SqlException ex)
            {
                await _logger.ErrorAsync(ex.Message, ex);
            }
            try
            {
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (var cmd = new SqlCommand(query, connection))
                        {
                            cmd.Transaction = transaction;
                            cmd.CommandType = System.Data.CommandType.Text;

                            var reader = cmd.ExecuteReader();
                            jsonQuery = Serialize(reader);
                            //model.Results = Serialize(reader);
                            reader.Close();
                            //model.ReturnedRow = reader.RecordsAffected;

                        }
                        transaction.Commit();
                        connection.Close();

                    }
                    catch (SqlException ex)
                    {
                        transaction.Rollback();
                        connection.Close();
                        await _logger.ErrorAsync(ex.Message, ex);
                    }
                }
            }
            catch (SqlException ex)
            {
                connection.Close();
                await _logger.ErrorAsync(ex.Message, ex);
            }

            return jsonQuery;

            #endregion
        }

        public async Task<SqlQueryModel> RunQueryAsync(SqlQueryModel model)
        {
            var builder = new SqlConnectionStringBuilder(DataSettingsManager.LoadSettings().ConnectionString);

            #region With transaction

            var connection = new SqlConnection(builder.ConnectionString);

            try
            {
                connection.Open();
            }
            catch (SqlException ex)
            {
                model.Message = ex.Message;
                await _logger.ErrorAsync(ex.Message, ex);

                return model;
            }
            try
            {
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (var cmd = new SqlCommand(model.SQLQuery, connection))
                        {
                            cmd.Transaction = transaction;
                            cmd.CommandType = System.Data.CommandType.Text;
                            var reader = cmd.ExecuteReader();

                            model.Results = Serialize(reader);
                            reader.Close();
                            model.ReturnedRow = reader.RecordsAffected;

                        }
                        transaction.Commit();
                        connection.Close();

                        return model;
                    }
                    catch (SqlException ex)
                    {
                        transaction.Rollback();
                        connection.Close();
                        model.Message = ex.Message;
                        await _logger.ErrorAsync(ex.Message, ex);
                    }
                }
            }
            catch (SqlException ex)
            {
                connection.Close();
                await _logger.ErrorAsync(ex.Message, ex);
            }

            return model;

            #endregion
        }

        public async Task<byte[]> ExportToExcelAsync(string sql)
        {
            var connectionStringBuilder = new SqlConnectionStringBuilder(DataSettingsManager.LoadSettings().ConnectionString);
            var connection = new SqlConnection(connectionStringBuilder.ConnectionString);

            try
            {
                connection.Open();
            }
            catch (Exception ex)
            {
                throw new NopException(ex.Message, ex.InnerException);
            }

            try
            {
                var result = new SqlCommand(sql, connection).ExecuteReader();

                await using var stream = new MemoryStream();

                using (var workbook = new XLWorkbook())
                {
                    // uncomment this line if you want the XML written out to the outputDir
                    //xlPackage.DebugMode = true; 

                    // get handles to the worksheets
                    var worksheet = workbook.Worksheets.Add("Report");
                    var row = 1;
                    var columnNames = new List<string>();

                    //create Headers and format them 
                    //WriteCaption(worksheet);
                    for (var i = 0; i < result.FieldCount; i++)
                    {
                        var cell = worksheet.Row(row).Cell(i + 1);
                        cell.Value = result.GetName(i);
                        columnNames.Add(result.GetName(i));

                        SetCaptionStyle(cell);
                    }

                    while (result.Read())
                    {
                        row++;
                        var xlRrow = worksheet.Row(row);
                        xlRrow.Style.Alignment.WrapText = false;
                        var column = 1;

                        foreach (var columnName in columnNames)
                        {
                            var cell = xlRrow.Cell(column++);
                            cell.Value = result[columnName] as string;
                            cell.Style.Alignment.WrapText = false;
                        }
                    }

                    workbook.SaveAs(stream);
                }

                return stream.ToArray();
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync(ex.Message, ex);
                return null;
            }
        }

        #endregion
    }
}
