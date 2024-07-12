using System.Runtime.ExceptionServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Configuration;
using Nop.Data;
using Nop.Services.Localization;
using Nop.Services.Logging;
using NopStation.Plugin.Misc.Core.Helpers;
using NopStation.Plugin.Misc.Core.Models.Api;

namespace NopStation.Plugin.Misc.WebApi.Infrastructure.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static void UseApiNotFound(this IApplicationBuilder application)
        {
            application.UseStatusCodePages(async context =>
            {
                //handle 404 Not Found
                if (context.HttpContext.Response.StatusCode == StatusCodes.Status404NotFound)
                {
                    if (context.HttpContext.Request.Headers.ContainsKey(WebApiCustomerDefaults.DeviceId))
                    {
                        var res = new BaseResponseModel();
                        res.Message = NopInstance.Load<ILocalizationService>().GetResourceAsync("NopStation.WebApi.Response.PageNotFound").Result;
                        var json = JsonConvert.SerializeObject(res);
                        context.HttpContext.Response.ContentType = "application/json";
                        await context.HttpContext.Response.WriteAsync(json);
                    }
                }
            });
        }

        public static void UseApiExceptionHandler(this IApplicationBuilder application)
        {
            var appSettings = NopInstance.Load<AppSettings>();
            var webHostEnvironment = NopInstance.Load<IWebHostEnvironment>();
            var useDetailedExceptionPage = appSettings.Get<CommonConfig>().DisplayFullErrorStack || webHostEnvironment.IsDevelopment();
            if (useDetailedExceptionPage)
            {
                //get detailed exceptions for developing and testing purposes
                application.UseDeveloperExceptionPage();
            }
            else
            {
                //or use special exception handler
                application.UseExceptionHandler("/Error/Error");
            }

            //log errors
            application.UseExceptionHandler(handler =>
            {
                handler.Run(async context =>
                {
                    var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
                    if (exception == null)
                        return;

                    try
                    {
                        //check whether database is installed
                        if (DataSettingsManager.IsDatabaseInstalled())
                        {
                            //get current customer
                            var currentCustomer = NopInstance.Load<IWorkContext>().GetCurrentCustomerAsync().Result;

                            //log error
                            await NopInstance.Load<ILogger>().ErrorAsync(exception.Message, exception, currentCustomer);
                        }
                    }
                    finally
                    {
                        if (context.Request.Headers.ContainsKey(WebApiCustomerDefaults.DeviceId))
                        {
                            var baseResponse = new BaseResponseModel();
                            baseResponse.ErrorList.Add(exception.Message);
                            context.Response.ContentType = "application/json";
                            await context.Response.WriteAsync(JsonConvert.SerializeObject(baseResponse));
                        }
                        //rethrow the exception to show the error page
                        ExceptionDispatchInfo.Throw(exception);
                    }
                });
            });
        }
    }
}
