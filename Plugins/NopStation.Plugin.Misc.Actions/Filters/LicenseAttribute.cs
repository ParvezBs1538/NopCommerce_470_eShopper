//using System;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Controllers;
//using Microsoft.AspNetCore.Mvc.Filters;
//using Nop.Data;
//using Nop.Plugin.NopStation.Core.Services;

//namespace Nop.Plugin.NopStation.Core.Filters
//{
//    public partial class LicenseAttribute : TypeFilterAttribute
//    {
//        #region Ctor

//        public LicenseAttribute() : base(typeof(LicenseFilter))
//        {
//        }

//        #endregion

//        #region Nested filter

//        private class LicenseFilter : IAuthorizationFilter
//        {
//            #region Fields

//            private readonly INopStationLicenseService _nopStationLicenseService;

//            #endregion

//            #region Ctor

//            public LicenseFilter(INopStationLicenseService nopStationLicenseService)
//            {
//                _nopStationLicenseService = nopStationLicenseService;
//            }

//            #endregion

//            #region Methods

//            public void OnAuthorization(AuthorizationFilterContext filterContext)
//            {
//                if (filterContext == null)
//                    throw new ArgumentNullException(nameof(filterContext));

//                if (!DataSettingsManager.IsDatabaseInstalledAsync().Result)
//                    return;

//                var descriptor = filterContext?.ActionDescriptor as ControllerActionDescriptor;
//                if (!_nopStationLicenseService.IsLicensed(descriptor.ControllerTypeInfo.Assembly))
//                    filterContext.Result = new RedirectToActionResult("License", "NopStationLicense", null);
//            }

//            #endregion
//        }

//        #endregion
//    }
//}