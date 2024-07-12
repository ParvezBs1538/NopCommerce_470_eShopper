using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Data;
using Nop.Services.Security;

namespace NopStation.Plugin.Misc.Core.Filters;

public class EditAccessAttribute : TypeFilterAttribute
{
    #region Fields

    private readonly bool _ignoreFilter;

    #endregion

    #region Ctor

    /// <summary>
    /// Create instance of the filter attribute
    /// </summary>
    /// <param name="ignore">Whether to ignore the execution of filter actions</param>
    public EditAccessAttribute(bool ignore = false) : base(typeof(EditAccessFilter))
    {
        _ignoreFilter = ignore;
        Arguments = new object[] { ignore };
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets a value indicating whether to ignore the execution of filter actions
    /// </summary>
    public bool IgnoreFilter => _ignoreFilter;

    #endregion

    #region Nested filter

    private class EditAccessFilter : IAuthorizationFilter
    {
        #region Fields

        private readonly bool _ignoreFilter;
        private readonly IPermissionService _permissionService;

        #endregion

        #region Ctor

        public EditAccessFilter(bool ignoreFilter, IPermissionService permissionService)
        {
            _ignoreFilter = ignoreFilter;
            _permissionService = permissionService;
        }

        #endregion

        #region Methods

        public void OnAuthorization(AuthorizationFilterContext filterContext)
        {
            if (filterContext == null)
                throw new ArgumentNullException(nameof(filterContext));

            //check whether this filter has been overridden for the Action
            var actionFilter = filterContext.ActionDescriptor.FilterDescriptors
                .Where(filterDescriptor => filterDescriptor.Scope == FilterScope.Action)
                .Select(filterDescriptor => filterDescriptor.Filter).OfType<EditAccessAttribute>().FirstOrDefault();

            //ignore filter (the action is available even if navigation is not allowed)
            if (actionFilter?.IgnoreFilter ?? _ignoreFilter)
                return;

            if (!DataSettingsManager.IsDatabaseInstalled())
                return;

            //check whether current customer has access to a public store
            if (_permissionService.AuthorizeAsync(CorePermissionProvider.ManageNopStationFeatures).Result)
                return;

            var referer = "/";
            if (filterContext.HttpContext?.Request?.Headers?.ContainsKey("Referer") ?? false)
                referer = filterContext.HttpContext.Request.Headers["Referer"].ToString();

            filterContext.Result = new RedirectToActionResult("EditAccessRedirect", "NopStation", new { returnUrl = referer });
            return;
        }

        #endregion
    }

    #endregion
}