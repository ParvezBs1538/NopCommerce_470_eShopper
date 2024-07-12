using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NopStation.Plugin.Misc.Core.Infrastructure;
using NopStation.Plugin.Widgets.Helpdesk.Areas.Admin.Factories;
using NopStation.Plugin.Widgets.Helpdesk.Services;

namespace NopStation.Plugin.Widgets.Helpdesk.Infrastructure
{
    public class PluginNopStartup : INopStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddNopStationServices("NopStation.Plugin.Widgets.Helpdesk");

            services.AddScoped<ITicketService, TicketService>();
            services.AddScoped<IDepartmentService, DepartmentService>();
            services.AddScoped<IStaffService, StaffService>();

            services.AddScoped<Factories.ITicketModelFactory, Factories.TicketModelFactory>();

            services.AddScoped<IDepartmentModelFactory, DepartmentModelFactory>();
            services.AddScoped<IStaffModelFactory, StaffModelFactory>();
            services.AddScoped<ITicketModelFactory, TicketModelFactory>();
            services.AddScoped<IHelpdeskModelFactory, HelpdeskModelFactory>();
        }

        public void Configure(IApplicationBuilder application)
        {
        }

        public int Order => 11;
    }
}