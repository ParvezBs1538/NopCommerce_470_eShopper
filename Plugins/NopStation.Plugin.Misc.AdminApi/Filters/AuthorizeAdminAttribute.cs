using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Data;
using Nop.Services.Localization;
using Nop.Services.Security;
using NopStation.Plugin.Misc.Core.Helpers;
using NopStation.Plugin.Misc.Core.Models.Api;

namespace NopStation.Plugin.Misc.AdminApi.Filters;

/// <summary>
/// Represents a filter attribute that confirms access to the admin panel
/// </summary>
public sealed class AuthorizeAdminAttribute : TypeFilterAttribute
{
    #region Ctor

    /// <summary>
    /// Create instance of the filter attribute
    /// </summary>
    /// <param name="ignore">Whether to ignore the execution of filter actions</param>
    public AuthorizeAdminAttribute(bool ignore = false) : base(typeof(AuthorizeAdminFilter))
    {
        IgnoreFilter = ignore;
        Arguments = [ignore];
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets a value indicating whether to ignore the execution of filter actions
    /// </summary>
    public bool IgnoreFilter { get; }

    #endregion

    #region Nested filter

    /// <summary>
    /// Represents a filter that confirms access to the admin panel
    /// </summary>
    private class AuthorizeAdminFilter : IAsyncAuthorizationFilter
    {
        #region Fields

        private readonly bool _ignoreFilter;
        private readonly IPermissionService _permissionService;

        #endregion

        #region Ctor

        public AuthorizeAdminFilter(bool ignoreFilter, IPermissionService permissionService)
        {
            _ignoreFilter = ignoreFilter;
            _permissionService = permissionService;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Called early in the filter pipeline to confirm request is authorized
        /// </summary>
        /// <param name="context">Authorization filter context</param>
        private async Task AuthorizeAdminAsync(AuthorizationFilterContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            if (!DataSettingsManager.IsDatabaseInstalled())
                return;

            //check whether this filter has been overridden for the action
            var actionFilter = context.ActionDescriptor.FilterDescriptors
                .Where(filterDescriptor => filterDescriptor.Scope == FilterScope.Action)
                .Select(filterDescriptor => filterDescriptor.Filter)
                .OfType<AuthorizeAdminAttribute>()
                .FirstOrDefault();

            //ignore filter (the action is available even if a customer hasn't access to the admin area)
            if (actionFilter?.IgnoreFilter ?? _ignoreFilter)
                return;

            //there is AdminAuthorizeFilter, so check access
            if (context.Filters.Any(filter => filter is AuthorizeAdminFilter))
            {
                //authorize permission of access to the admin area
                if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
                {
                    var localizationService = NopInstance.Load<ILocalizationService>();
                    var response = new BaseResponseModel
                    {
                        ErrorList =
                        [
                            await localizationService.GetResourceAsync("NopStation.AdminApi.Response.Unauthorized")
                        ]
                    };

                    context.Result = new UnauthorizedObjectResult(response);
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Called early in the filter pipeline to confirm request is authorized
        /// </summary>
        /// <param name="context">Authorization filter context</param>
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            await AuthorizeAdminAsync(context);
        }

        #endregion
    }

    #endregion
}