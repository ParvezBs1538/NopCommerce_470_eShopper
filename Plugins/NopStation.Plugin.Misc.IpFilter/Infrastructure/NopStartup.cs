using Firewall;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core;
using Nop.Core.Infrastructure;
using NopStation.Plugin.Misc.Core.Helpers;
using NopStation.Plugin.Misc.IpFilter.Helper;
using NopStation.Plugin.Misc.IpFilter.Services;
using Nop.Services.Configuration;
using Nop.Services.Directory;

namespace NopStation.Plugin.Misc.IpFilter.Infrastructure
{
    public class NopStartup : INopStartup
    {
        public int Order => 100;

        public void Configure(IApplicationBuilder application)
        {
            var storeContext = NopInstance.Load<IStoreContext>();
            var settingService = NopInstance.Load<ISettingService>();
            var storeId = storeContext.GetActiveStoreScopeConfigurationAsync().Result;
            var settings = settingService.LoadSettingAsync<IpFilterSettings>(storeId).Result;

            if (settings.IsEnabled)
            {
                var webHelper = NopInstance.Load<IWebHelper>();
                var geoLookUpService = NopInstance.Load<IGeoLookupService>();
                var fileProvider = NopInstance.Load<INopFileProvider>();

                var countryBlockRuleService = NopInstance.Load<ICountryBlockRuleService>();
                var ipBlockRuleService = NopInstance.Load<IIpBlockRuleService>();
                var ipRangeBlockRuleService = NopInstance.Load<IIpRangeBlockRuleService>();

                // get block IP Address Lists
                var blockIPList = ipBlockRuleService.GetAllBlockedIPAddressesAsync().Result;

                // get Block Country List
                var countryCodes = countryBlockRuleService.GetAllBlockedCountryCodesAsync().Result;

                // get block CIDRNotion (IP Range)
                var cidrNotation = ipRangeBlockRuleService.GetAllCIDRNotationsAsync().Result;

                // html path for custom view
                var viewPath = fileProvider.MapPath("~/Plugins/NopStation.Plugin.Misc.IpFilter/Html/");
                var file = fileProvider.ReadAllBytesAsync(fileProvider.Combine(viewPath, "AccessDenied.html")).Result;

                // Add firewall middleware 
                application.UseFirewall(
                    FirewallRulesEngine
                    .DenyAllAccess()                                            //  Default rule for firewall
                    .AccessAllRules()                                           //  Custom rule to make access all user
                    .DenyIpAddresses(blockIPList, webHelper)                    //  Block list IP Address added
                    .DenyCountries(countryCodes, webHelper, geoLookUpService)   //  Block country List added
                    .DenyIPAddressRange(cidrNotation, webHelper),               //  Block CIRDNotation(IP Range) added
                        accessDeniedDelegate:                                   // custom view
                            ctx =>
                            {
                                ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                                ctx.Response.ContentType = "text/html";
                                ctx.Response.ContentLength = file.Length;
                                ctx.Response.Body.WriteAsync(file, 0, file.Length).Wait();
                                ctx.Response.Body.FlushAsync().Wait();
                                return null;
                            });
            }
        }

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {

        }
    }
}
