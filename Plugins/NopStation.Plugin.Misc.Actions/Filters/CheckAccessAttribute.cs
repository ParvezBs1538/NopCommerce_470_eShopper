using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Core;
using Nop.Data;
using Nop.Services.Customers;

namespace NopStation.Plugin.Misc.Core.Filters;

public partial class CheckAccessAttribute : TypeFilterAttribute
{
    #region Ctor

    public CheckAccessAttribute() : base(typeof(CheckAccessFilter))
    {
    }

    #endregion

    #region Nested filter

    private class CheckAccessFilter : IAuthorizationFilter
    {
        #region Fields

        private readonly IWorkContext _workContext;
        private readonly NopStationCoreSettings _coreSettings;
        private readonly ICustomerService _customerService;
        private readonly IWebHelper _webHelper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        #endregion

        #region Ctor

        public CheckAccessFilter(IWorkContext workContext,
            NopStationCoreSettings coreSettings,
            ICustomerService customerService,
            IWebHelper webHelper,
            IHttpContextAccessor httpContextAccessor)
        {
            _workContext = workContext;
            _coreSettings = coreSettings;
            _customerService = customerService;
            _webHelper = webHelper;
            _httpContextAccessor = httpContextAccessor;
        }

        #endregion

        #region Methods

        public void OnAuthorization(AuthorizationFilterContext filterContext)
        {
            if (filterContext == null)
                throw new ArgumentNullException(nameof(filterContext));

            if (!DataSettingsManager.IsDatabaseInstalled())
                return;

            if (_coreSettings.RestrictMainMenuByCustomerRoles)
            {
                var crids = _customerService.GetCustomerRoleIdsAsync(_workContext.GetCurrentCustomerAsync().Result).Result;
                foreach (var crid in crids)
                    if (_coreSettings.AllowedCustomerRoleIds.Contains(crid))
                        return;

                filterContext.Result = new RedirectToActionResult("AccessDenied", "Security",
                    new { pageUrl = _webHelper.GetRawUrl(_httpContextAccessor.HttpContext.Request) });
            }
        }

        #endregion
    }

    #endregion
}