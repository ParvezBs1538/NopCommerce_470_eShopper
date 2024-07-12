namespace NopStation.Plugin.Misc.SqlManager
{
    public class SqlManagerDefaults
    {
        public static string ParamerFormat = "@P({0})";

        public static string GetSqlParameterByIdCacheKey => "NopStation.SqlManager.SqlParameter.SqlParameterId-{0}";

        public static string GetSqlParameterBySystemName => "NopStation.SqlManager.SqlParameter.SystemName-{0}";

        public static string GetSqlParameterValueById => "NopStation.SqlManager.SqlParameter.SqlParameterValueById-{0}";

        public static string IsSystemNameExsistForAnotherSqlParameter => "NopStation.SqlManager.SqlParameter.IsSystemNameExsistForAnotherSqlParameter-{0}";

        public static string GetSqlReportById => "NopStation.SqlManager.SqlReport.SqlReportById-{0}";

        public static string NopStationSqlManagerSqlParameterPrefixCacheKey => "NopStation.SqlManager.SqlParameter";

        public static string NopStationSqlManagerSqlReportPrefixCacheKey => "NopStation.SqlManager.SqlParameter";
    }
}
