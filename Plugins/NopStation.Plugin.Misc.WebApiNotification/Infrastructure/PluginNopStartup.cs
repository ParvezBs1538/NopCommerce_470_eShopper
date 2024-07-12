using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NopStation.Plugin.Misc.Core.Infrastructure;
using NopStation.Plugin.Misc.WebApiNotification.Areas.Admin.Factories;
using NopStation.Plugin.Misc.WebApiNotification.Factories;
using NopStation.Plugin.Misc.WebApiNotification.Services;

namespace NopStation.Plugin.Misc.WebApiNotification.Infrastructure
{
    public class PluginNopStartup : INopStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddNopStationServices("NopStation.Plugin.Misc.WebApiNotification", excludepublicView: true);

            services.AddScoped<IPushNotificationTemplateService, WebApiNotificationTemplateService>();
            services.AddScoped<IQueuedPushNotificationService, QueuedPushNotificationService>();
            services.AddScoped<IWorkflowNotificationService, WorkflowNotificationService>();
            services.AddScoped<IPushNotificationTokenProvider, WebApiNotificationTokenProvider>();
            services.AddScoped<IPushNotificationSender, WebApiNotificationSender>();

            services.AddScoped<IPushNotificationCampaignService, WebApiNotificationCampaignService>();

            services.AddScoped<IWebApiNotificationModelFactory, WebApiNotificationModelFactory>();
            services.AddScoped<IPushNotificationTemplateModelFactory, PushNotificationTemplateModelFactory>();
            services.AddScoped<IQueuedPushNotificationModelFactory, QueuedPushNotificationModelFactory>();
            services.AddScoped<IPushNotificationCampaignModelFactory, PushNotificationCampaignModelFactory>();

            services.AddScoped<IPushKitHelperFactory, PushKitHelperFactory>();
        }

        public void Configure(IApplicationBuilder application)
        {
        }

        public int Order => 11;
    }
}