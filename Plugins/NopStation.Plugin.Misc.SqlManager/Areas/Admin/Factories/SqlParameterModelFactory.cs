using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Services;
using Nop.Services.Localization;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.Misc.SqlManager.Areas.Admin.Models;
using NopStation.Plugin.Misc.SqlManager.Domain;
using NopStation.Plugin.Misc.SqlManager.Services;

namespace NopStation.Plugin.Misc.SqlManager.Areas.Admin.Factories
{
    public class SqlParameterModelFactory : ISqlParameterModelFactory
    {
        #region Fields

        private readonly ISqlParameterService _sqlParameterService;
        private readonly ILocalizationService _localizationService;

        #endregion

        #region Ctor

        public SqlParameterModelFactory(ISqlParameterService sqlParameterService,
            ILocalizationService localizationService)
        {
            _sqlParameterService = sqlParameterService;
            _localizationService = localizationService;
        }

        #endregion

        #region Utilities

        protected virtual SqlParameterValueSearchModel PrepareSqlParameterValueSearchModel(SqlParameterValueSearchModel searchModel, SqlParameter sqlParameter)
        {
            if (sqlParameter == null)
                throw new ArgumentNullException(nameof(sqlParameter));

            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            searchModel.SetGridPageSize();
            searchModel.SqlParameterId = sqlParameter.Id;

            return searchModel;
        }

        #endregion

        #region Methods

        public virtual Task<SqlParameterSearchModel> PrepareSqlParameterSearchModelAsync(SqlParameterSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            searchModel.SetGridPageSize();

            return Task.FromResult(searchModel);
        }

        public virtual async Task<SqlParameterListModel> PrepareSqlParameterListModelAsync(SqlParameterSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var sqlParameters = await _sqlParameterService.GetAllSqlParametersAsync(pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            var model = await new SqlParameterListModel().PrepareToGridAsync(searchModel, sqlParameters, () =>
            {
                return sqlParameters.SelectAwait(async sqlParameter =>
                {
                    var sqlParameterModel = await PrepareSqlParameterModelAsync(null, sqlParameter, true);

                    return sqlParameterModel;
                });
            });

            return model;
        }

        public virtual async Task<SqlParameterModel> PrepareSqlParameterModelAsync(SqlParameterModel model, SqlParameter sqlParameter, bool excludeProperties = false)
        {
            if (sqlParameter != null)
            {
                if (model == null)
                {
                    model = sqlParameter.ToModel<SqlParameterModel>();
                    model.DataTypeStr = await _localizationService.GetLocalizedEnumAsync(sqlParameter.DataType);
                }

                if (!excludeProperties)
                {
                    model.SqlParameterValueSearchModel = PrepareSqlParameterValueSearchModel(new SqlParameterValueSearchModel(), sqlParameter);
                }

                model.IsDateType = sqlParameter.DataType == DataType.Date;
                model.IsTextInputItem = sqlParameter.DataType == DataType.InputText;
            }

            if (!excludeProperties)
            {
                model.AvailableDataTypes = (await DataType.TextList.ToSelectListAsync()).ToList();
            }

            return model;
        }

        public virtual async Task<SqlParameterValueListModel> PrepareSqlParameterValueListModelAsync(SqlParameterValueSearchModel searchModel, SqlParameter sqlParameter)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (sqlParameter == null)
                throw new ArgumentNullException(nameof(sqlParameter));

            var sqlParameterValues = (await _sqlParameterService.GetqlParameterValuesByParameterIdAsync(searchModel.SqlParameterId)).ToPagedList(searchModel);

            var model = await new SqlParameterValueListModel().PrepareToGridAsync(searchModel, sqlParameterValues, () =>
            {
                return sqlParameterValues.SelectAwait(async sqlParameterValue =>
                {
                    var sqlParameterValueModel = await PrepareSqlParameterValueModelAsync(null, sqlParameterValue, sqlParameter, true);

                    return sqlParameterValueModel;
                });
            });

            return model;
        }

        public virtual Task<SqlParameterValueModel> PrepareSqlParameterValueModelAsync(SqlParameterValueModel model, SqlParameterValue sqlParameterValue,
            SqlParameter sqlParameter, bool excludeProperties = false)
        {
            if (sqlParameter == null)
                throw new ArgumentNullException(nameof(sqlParameter));

            if (sqlParameterValue != null)
            {
                var numberType = sqlParameter.DataType == DataType.Number || sqlParameter.DataType == DataType.NumberList;
                if (model == null)
                {
                    model = new SqlParameterValueModel();
                    model.Id = sqlParameterValue.Id;
                    model.Value = sqlParameterValue.Value;
                    model.SqlParameterName = sqlParameter.Name;
                    model.IsValid = numberType ? decimal.TryParse(sqlParameterValue.Value, out _) : true;
                }

                if (!excludeProperties)
                {
                }
            }

            model.SqlParameterId = sqlParameter.Id;

            if (!excludeProperties)
            {

            }

            return Task.FromResult(model);
        }

        #endregion
    }
}