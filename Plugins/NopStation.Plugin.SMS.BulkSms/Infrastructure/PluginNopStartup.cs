using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NopStation.Plugin.Misc.Core.Infrastructure;
using NopStation.Plugin.SMS.BulkSms.Areas.Admin.Factories;
using NopStation.Plugin.SMS.BulkSms.Services;

namespace NopStation.Plugin.SMS.BulkSms.Infrastructure
{
    public class PluginNopStartup : INopStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddNopStationServices("NopStation.Plugin.SMS.BulkSms", excludepublicView: true);

            services.AddScoped<ISmsTemplateService, SmsTemplateService>();
            services.AddScoped<IQueuedSmsService, QueuedSmsService>();
            services.AddScoped<IWorkflowNotificationService, WorkflowSmsService>();
            services.AddScoped<ISmsTokenProvider, SmsTokenProvider>();
            services.AddScoped<ISmsSender, SmsSender>();

            services.AddScoped<IBulkSmsModelFactory, BulkSmsModelFactory>();
            services.AddScoped<ISmsTemplateModelFactory, SmsTemplateModelFactory>();
            services.AddScoped<IQueuedSmsModelFactory, QueuedSmsModelFactory>();
        }

        public void Configure(IApplicationBuilder application)
        {
        }

        public int Order => 11;
    }
}