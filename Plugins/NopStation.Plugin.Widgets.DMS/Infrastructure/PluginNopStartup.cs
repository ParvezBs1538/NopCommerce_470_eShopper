using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NopStation.Plugin.Misc.Core.Infrastructure;
using NopStation.Plugin.Widgets.DMS.Areas.Admin.Factories;
using NopStation.Plugin.Widgets.DMS.Factories;
using NopStation.Plugin.Widgets.DMS.Services;

namespace NopStation.Plugin.Widgets.DMS.Infrastructure
{
    public class PluginNopStartup : INopStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddNopStationServices("NopStation.Plugin.Widgets.DMS");
            services.AddCors(option =>
            {
                option.AddPolicy("AllowAll", options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            });

            // services
            services.AddScoped<ICourierShipmentService, CourierShipmentService>();
            services.AddScoped<IDeliverFailedRecordService, DeliverFailedRecordService>();
            services.AddScoped<IDMSOtpService, DMSOtpService>();
            services.AddScoped<IDMSPdfService, DMSPdfService>();
            services.AddScoped<IDMSService, DMSService>();
            services.AddScoped<IOTPRecordService, OTPRecordService>();
            services.AddScoped<IProofOfDeliveryDataService, ProofOfDeliveryDataService>();
            services.AddScoped<IQrCodeService, QrCodeService>();
            services.AddScoped<IShipperService, ShipperService>();
            services.AddScoped<IShipmentNoteService, ShipmentNoteService>();
            services.AddScoped<IShipmentPickupPointService, ShipmentPickupPointService>();
            services.AddScoped<IShipperDeviceService, ShipperDeviceService>();

            //Factories
            services.AddScoped<Factories.ICourierShipmentModelFactory, Factories.CourierShipmentModelFactory>();

            services.AddScoped<IShipperModelFactory, ShipperModelFactory>();
            services.AddScoped<Areas.Admin.Factories.ICourierShipmentModelFactory, Areas.Admin.Factories.CourierShipmentModelFactory>();
            services.AddScoped<IDMSShipmentModelFactory, DMSShipmentModelFactory>();
            services.AddScoped<IShipmentPickupPointModelFactory, ShipmentPickupPointModelFactory>();
            services.AddScoped<IDeviceModelFactory, DeviceModelFactory>();
            services.AddScoped<IShipmentNoteModelFactory, ShipmentNoteModelFactory>();
            services.AddFluentValidationAutoValidation(config =>
            {
                config.ImplicitlyValidateChildProperties = true;
            });
        }

        public void Configure(IApplicationBuilder application)
        {
            application.UseCors("AllowAll");
            application.UseMiddleware<JwtAuthMiddleware>();
        }

        public int Order => 1;
    }
}