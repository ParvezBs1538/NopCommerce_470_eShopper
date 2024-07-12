using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Data;
using Nop.Services.Localization;
using NopStation.Plugin.Misc.Core.Helpers;
using NopStation.Plugin.Misc.Core.Services;

namespace NopStation.Plugin.Misc.Core.Filters;

public partial class NopStationApiLicenseAttribute : TypeFilterAttribute
{
    #region Ctor

    public NopStationApiLicenseAttribute() : base(typeof(NopStationApiLicenseFilter))
    {
    }

    #endregion

    #region Nested filter

    private class NopStationApiLicenseFilter : IAuthorizationFilter
    {
        #region Fields

        private readonly ILicenseService _licenseService;

        #endregion

        #region Ctor

        public NopStationApiLicenseFilter(ILicenseService licenseService)
        {
            _licenseService = licenseService;
        }

        #endregion

        #region Methods

        public void OnAuthorization(AuthorizationFilterContext filterContext)
        {
            if (filterContext == null)
                throw new ArgumentNullException(nameof(filterContext));

            if (!DataSettingsManager.IsDatabaseInstalled())
                return;

            var descriptor = filterContext?.ActionDescriptor as ControllerActionDescriptor;
            if (!_licenseService.IsLicensedAsync(descriptor.ControllerTypeInfo.Assembly).Result)
                CreateNstAccessResponceMessage(filterContext);
        }

        private void CreateNstAccessResponceMessage(AuthorizationFilterContext filterContext)
        {
            var localizationService = NopInstance.Load<ILocalizationService>();
            var response = new BaseResponseModel
            {
                ErrorList = new List<string>
                {
                    localizationService.GetResourceAsync("NopStation.WebApi.Response.InvalidLicense").Result
                }
            };

            filterContext.Result = new BadRequestObjectResult(response);
            return;
        }

        #endregion
    }

    #endregion

    public class BaseResponseModel
    {
        public BaseResponseModel()
        {
            ErrorList = new List<string>();
        }

        public List<string> ErrorList { get; set; }
    }
}