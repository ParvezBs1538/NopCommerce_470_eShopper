using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Services.Helpers;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.Misc.SqlManager.Areas.Admin.Models;
using NopStation.Plugin.Misc.SqlManager.Domain;
using NopStation.Plugin.Misc.SqlManager.Services;

namespace NopStation.Plugin.Misc.SqlManager.Areas.Admin.Factories
{
    public class SqlReportModelFactory : ISqlReportModelFactory
    {
        #region Fields

        private readonly ISqlReportService _sqlReportService;
        private readonly IAclSupportedModelFactory _aclSupportedModelFactory;
        private readonly ISqlParameterService _sqlParameterService;
        private readonly IDateTimeHelper _dateTimeHelper;

        #endregion

        #region Ctor

        public SqlReportModelFactory(ISqlReportService sqlReportService,
            IAclSupportedModelFactory aclSupportedModelFactory,
            ISqlParameterService sqlParameterService,
            IDateTimeHelper dateTimeHelper)
        {
            _sqlReportService = sqlReportService;
            _aclSupportedModelFactory = aclSupportedModelFactory;
            _sqlParameterService = sqlParameterService;
            _dateTimeHelper = dateTimeHelper;
        }

        #endregion

        #region Methods

        public virtual Task<SqlReportSearchModel> PrepareSqlReportSearchModelAsync(SqlReportSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            searchModel.SetGridPageSize();

            return Task.FromResult(searchModel);
        }

        public virtual async Task<SqlReportListModel> PrepareSqlReportListModelAsync(SqlReportSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (searchModel.SelectedCustomerRoleIds.Any())
                searchModel.SelectedCustomerRoleIds = searchModel.SelectedCustomerRoleIds.Where(x => x != 0).ToList();

            var sqlReports = await _sqlReportService.GetAllSqlReportsAsync(
                customerRoleIds: searchModel.SelectedCustomerRoleIds.ToArray(),
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.PageSize);

            var model = await new SqlReportListModel().PrepareToGridAsync(searchModel, sqlReports, () =>
            {
                return sqlReports.SelectAwait(async sqlReport =>
                {
                    return await PrepareSqlReportModelAsync(null, sqlReport, true);
                });
            });

            return model;
        }

        public virtual async Task<SqlReportModel> PrepareSqlReportModelAsync(SqlReportModel model, SqlReport sqlReport, bool excludeProperties = false)
        {
            if (sqlReport != null)
            {
                if (model == null)
                {
                    model = sqlReport.ToModel<SqlReportModel>();
                    model.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(sqlReport.CreatedOnUtc, DateTimeKind.Utc);
                    model.UpdatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(sqlReport.UpdatedOnUtc, DateTimeKind.Utc);
                }
            }

            if (!excludeProperties)
            {
                //prepare model customer roles
                await _aclSupportedModelFactory.PrepareModelCustomerRolesAsync(model, sqlReport, excludeProperties);

                var parameters = await _sqlParameterService.GetAllSqlParametersAsync();
                model.AvailableParameters = parameters.Select(x => x.SystemName).ToList();
            }

            return model;
        }

        public virtual async Task<SqlReportModel> PrepareSqlReportViewModelAsync(SqlReport sqlReport)
        {
            if (sqlReport == null)
                throw new ArgumentNullException(nameof(sqlReport));

            var model = sqlReport.ToModel<SqlReportModel>();
            model.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(sqlReport.CreatedOnUtc, DateTimeKind.Utc);
            model.UpdatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(sqlReport.UpdatedOnUtc, DateTimeKind.Utc);

            var pattern = "@P\\((.*?)\\)";
            var matchesConst = Regex.Matches(sqlReport.Query, pattern);
            foreach (Match i in matchesConst)
            {
                var systemName = i.Groups[1].ToString();
                var sqlParameter = await _sqlParameterService.GetSqlParameterBySystemNameAsync(systemName);
                if (sqlParameter != null)
                {
                    var order = model.SqlReportFilterOptions
                        .Count(x => x.SystemName.Equals(systemName, StringComparison.InvariantCultureIgnoreCase)) + 1;

                    var values = (await _sqlParameterService.GetqlParameterValuesByParameterIdAsync(sqlParameter.Id))
                        .Select(x => new SelectListItem()
                        {
                            Text = x.Value,
                            Value = x.Value
                        }).ToList();

                    model.SqlReportFilterOptions.Add(new SqlReportFilterOptionModel()
                    {
                        Name = sqlParameter.Name,
                        SystemName = sqlParameter.SystemName,
                        Order = order,
                        AvailableValues = values,
                        IsListItem = sqlParameter.DataType == DataType.NumberList || sqlParameter.DataType == DataType.TextList,
                        IsDateItem = sqlParameter.DataType == DataType.Date,
                        IsTextInputItem = sqlParameter.DataType == DataType.InputText
                    });
                }
            }
            return model;
        }

        #endregion
    }
}
