﻿using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NopStation.Plugin.Misc.Core.Infrastructure;
using NopStation.Plugin.Widgets.Announcement.Areas.Admin.Factories;
using NopStation.Plugin.Widgets.Announcement.Services;

namespace NopStation.Plugin.Widgets.Announcement.Infrastructure;

public class PluginNopStartup : INopStartup
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddNopStationServices("NopStation.Plugin.Widgets.Announcement");
        services.AddScoped<IAnnouncementItemService, AnnouncementItemService>();

        services.AddScoped<IAnnouncementItemModelFactory, AnnouncementItemModelFactory>();
    }

    public void Configure(IApplicationBuilder application)
    {
    }

    public int Order => 11;
}