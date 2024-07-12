using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NopStation.Plugin.Misc.Core.Infrastructure;
using NopStation.Plugin.SMS.Messente.Areas.Admin.Factories;
using NopStation.Plugin.SMS.Messente.Services;

namespace NopStation.Plugin.SMS.Messente.Infrastructure
{
    public class PluginNopStartup : INopStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddNopStationServices("NopStation.Plugin.SMS.Messente", excludepublicView: true);

            services.AddScoped<ISmsTemplateService, SmsTemplateService>();
            services.AddScoped<IQueuedSmsService, QueuedSmsService>();
            services.AddScoped<IWorkflowNotificationService, WorkflowSmsService>();
            services.AddScoped<ISmsTokenProvider, SmsTokenProvider>();
            services.AddScoped<ISmsSender, SmsSender>();

            services.AddScoped<IMessenteSmsModelFactory, MessenteSmsModelFactory>();
            services.AddScoped<ISmsTemplateModelFactory, SmsTemplateModelFactory>();
            services.AddScoped<IQueuedSmsModelFactory, QueuedSmsModelFactory>();
        }

        public void Configure(IApplicationBuilder application)
        {
        }

        public int Order => 11;
    }
}