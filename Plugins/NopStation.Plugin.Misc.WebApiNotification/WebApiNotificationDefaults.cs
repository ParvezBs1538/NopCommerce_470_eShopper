using Nop.Core.Caching;

namespace NopStation.Plugin.Misc.WebApiNotification
{
    public static partial class WebApiNotificationDefaults
    {
        public static CacheKey MessageTemplatesAllCacheKey => new CacheKey("Nopstation.apppushnotificationtemplate.all-{0}-{1}-{2}-{3}-{4}", MessageTemplatesPrefixCacheKey);
        public static CacheKey MessageTemplatesByNameCacheKey => new CacheKey("Nopstation.apppushnotificationtemplate.name-{0}-{1}", MessageTemplatesPrefixCacheKey);
        public static string MessageTemplatesPrefixCacheKey => "Nopstation.apppushnotificationtemplate.";

        public static string QueuedSendTaskType => "NopStation.Plugin.Misc.WebApiNotification.QueuedWebApiNotificationSendTask";
        public static string CampaignSendTaskType => "NopStation.Plugin.Misc.WebApiNotification.WebApiNotificationCampaignSendTask";

        public static string HuaweiOauthRequestUri => "https://oauth-login.cloud.huawei.com/oauth2/v3/token";

        public static string HuaweiPushAPIUri => "https://push-api.cloud.huawei.com/v2";

        public static string HUAWEI_PUSH_SUCCESS_CODE => "80000000";

        public static string HuaweiRequestMediaTypeHeader => "application/x-www-form-urlencoded";
    }
}