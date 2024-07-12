using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NopStation.Plugin.Misc.Core.Infrastructure;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Areas.Admin.Factories;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Services;

namespace NopStation.Plugin.Widgets.ProgressiveWebApp.Infrastructure
{
    public class PluginNopStartup : INopStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddNopStationServices("NopStation.Plugin.Widgets.ProgressiveWebApp");

            services.AddScoped<IWebAppDeviceService, WebAppDeviceService>();
            services.AddScoped<IWebPushService, WebPushService>();
            services.AddScoped<IAbandonedCartTrackingService, AbandonedCartTrackingService>();
            services.AddScoped<IPushNotificationTemplateService, PushNotificationTemplateService>();
            services.AddScoped<IQueuedPushNotificationService, QueuedPushNotificationService>();
            services.AddScoped<IWorkflowNotificationService, WorkflowNotificationService>();
            services.AddScoped<IPushNotificationTokenProvider, PushNotificationTokenProvider>();
            services.AddScoped<IPushNotificationSender, PushNotificationSender>();
            services.AddScoped<IPushNotificationCustomerService, PushNotificationCustomerService>();
            services.AddScoped<IPushNotificationAnnouncementService, PushNotificationAnnouncementService>();

            services.AddScoped<Factories.IProgressiveWebAppModelFactory, Factories.ProgressiveWebAppModelFactory>();

            services.AddScoped<IWebAppModelFactory, WebAppModelFactory>();
            services.AddScoped<IPushNotificationTemplateModelFactory, PushNotificationTemplateModelFactory>();
            services.AddScoped<IPushNotificationAnnouncementModelFactory, PushNotificationAnnouncementModelFactory>();
            services.AddScoped<IWebAppDeviceModelFactory, WebAppDeviceModelFactory>();
            services.AddScoped<IQueuedPushNotificationModelFactory, QueuedPushNotificationModelFactory>();
        }

        public void Configure(IApplicationBuilder application)
        {
        }

        public int Order => 11;
    }
}