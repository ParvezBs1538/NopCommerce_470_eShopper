using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Core;
using Nop.Data;
using Nop.Services.Customers;
using Nop.Services.Localization;
using NopStation.Plugin.Misc.Core.Helpers;
using NopStation.Plugin.Misc.Core.Models.Api;

namespace NopStation.Plugin.Misc.AdminApi.Filters;

/// <summary>
/// Represents a filter attribute confirming that user with "Vendor" customer role has appropriate vendor account associated (and active)
/// </summary>
public sealed class ValidateVendorAttribute : TypeFilterAttribute
{
    #region Ctor

    /// <summary>
    /// Create instance of the filter attribute
    /// </summary>
    /// <param name="ignore">Whether to ignore the execution of filter actions</param>
    public ValidateVendorAttribute(bool ignore = false) : base(typeof(ValidateVendorFilter))
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
    /// Represents a filter confirming that user with "Vendor" customer role has appropriate vendor account associated (and active)
    /// </summary>
    private class ValidateVendorFilter : IAsyncAuthorizationFilter
    {
        #region Fields

        private readonly bool _ignoreFilter;
        private readonly ICustomerService _customerService;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public ValidateVendorFilter(bool ignoreFilter, IWorkContext workContext, ICustomerService customerService)
        {
            _ignoreFilter = ignoreFilter;
            _customerService = customerService;
            _workContext = workContext;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Called early in the filter pipeline to confirm request is authorized
        /// </summary>
        /// <param name="context">Authorization filter context</param>
        private async Task ValidateVendorAsync(AuthorizationFilterContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            if (!DataSettingsManager.IsDatabaseInstalled())
                return;

            //check whether this filter has been overridden for the Action
            var actionFilter = context.ActionDescriptor.FilterDescriptors
                .Where(filterDescriptor => filterDescriptor.Scope == FilterScope.Action)
                .Select(filterDescriptor => filterDescriptor.Filter)
                .OfType<ValidateVendorAttribute>()
                .FirstOrDefault();

            //ignore filter (the action is available even if the current customer isn't a vendor)
            if (actionFilter?.IgnoreFilter ?? _ignoreFilter)
                return;

            //whether current customer is vendor
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsVendorAsync(customer))
                return;

            //ensure that this user has active vendor record associated
            var vendor = await _workContext.GetCurrentVendorAsync();
            if (vendor != null)
                return;

            var localizationService = NopInstance.Load<ILocalizationService>();
            var response = new BaseResponseModel
            {
                ErrorList = [
                    await localizationService.GetResourceAsync("NopStation.AdminApi.Response.InvalidVendor")
                ]
            };
            context.Result = new UnauthorizedObjectResult(response);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Called early in the filter pipeline to confirm request is authorized
        /// </summary>
        /// <param name="context">Authorization filter context</param>
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            await ValidateVendorAsync(context);
        }

        #endregion
    }

    #endregion
}