using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Data;
using Nop.Services.Localization;
using Nop.Services.Security;

namespace NopStation.Plugin.Misc.Core.Filters;

public class EditAccessAjaxAttribute : TypeFilterAttribute
{
    #region Fields

    private readonly bool _ignoreFilter;

    #endregion

    #region Ctor

    /// <summary>
    /// Create instance of the filter attribute
    /// </summary>
    /// <param name="ignore">Whether to ignore the execution of filter actions</param>
    public EditAccessAjaxAttribute(bool ignore = false) : base(typeof(EditAccessAjaxFilter))
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

    private class EditAccessAjaxFilter : IAuthorizationFilter
    {
        #region Fields

        private readonly bool _ignoreFilter;
        private readonly IPermissionService _permissionService;
        private readonly ILocalizationService _localizationService;

        #endregion

        #region Ctor

        public EditAccessAjaxFilter(bool ignoreFilter,
            IPermissionService permissionService,
            ILocalizationService localizationService)
        {
            _ignoreFilter = ignoreFilter;
            _permissionService = permissionService;
            _localizationService = localizationService;
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
                .Select(filterDescriptor => filterDescriptor.Filter).OfType<EditAccessAjaxAttribute>().FirstOrDefault();

            //ignore filter (the action is available even if navigation is not allowed)
            if (actionFilter?.IgnoreFilter ?? _ignoreFilter)
                return;

            if (!DataSettingsManager.IsDatabaseInstalled())
                return;

            //check whether current customer has access to a public store
            if (_permissionService.AuthorizeAsync(CorePermissionProvider.ManageNopStationFeatures).Result)
                return;

            var errorMessage = _localizationService.GetResourceAsync("Admin.NopStation.Core.Resources.EditAccessDenied").Result;

            filterContext.Result = new JsonResult(new
            {
                error = errorMessage,
                Error = errorMessage,
                Message = errorMessage,
                Result = false
            });
            return;
        }

        #endregion
    }

    #endregion
}