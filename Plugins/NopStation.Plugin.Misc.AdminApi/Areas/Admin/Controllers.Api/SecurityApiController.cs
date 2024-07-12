using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Nop.Core;
using Nop.Core.Domain.Security;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.Security;
using NopStation.Plugin.Misc.Core.Extensions;
using NopStation.Plugin.Misc.Core.Models.Api;

namespace NopStation.Plugin.Misc.AdminApi.Areas.Admin.Controllers.Api;

[Route("api/a/security/[action]")]
public partial class SecurityApiController : BaseAdminApiController
{
    #region Fields

    private readonly ICustomerService _customerService;
    private readonly ILocalizationService _localizationService;
    private readonly IPermissionService _permissionService;
    private readonly ISecurityModelFactory _securityModelFactory;

    #endregion

    #region Ctor

    public SecurityApiController(ICustomerService customerService,
        ILocalizationService localizationService,
        IPermissionService permissionService,
        ISecurityModelFactory securityModelFactory)
    {
        _customerService = customerService;
        _localizationService = localizationService;
        _permissionService = permissionService;
        _securityModelFactory = securityModelFactory;
    }

    #endregion

    #region Methods

    public virtual async Task<IActionResult> Permissions()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAcl))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _securityModelFactory.PreparePermissionMappingModelAsync(new PermissionMappingModel());

        return OkWrap(model);
    }

    public virtual async Task<IActionResult> PermissionsSave([FromBody] BaseQueryModel<PermissionMappingModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAcl))
            return AdminApiAccessDenied();

        var permissionRecords = await _permissionService.GetAllPermissionRecordsAsync();
        var customerRoles = await _customerService.GetAllCustomerRolesAsync(true);

        var form = queryModel.FormValues.ToNameValueCollection();

        foreach (var cr in customerRoles)
        {
            var formKey = "allow_" + cr.Id;
            var permissionRecordSystemNamesToRestrict = !string.IsNullOrEmpty(form[formKey])
                ? form[formKey].ToString().Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                : [];

            foreach (var pr in permissionRecords)
            {
                var allow = permissionRecordSystemNamesToRestrict.Contains(pr.SystemName);

                if (allow == await _permissionService.AuthorizeAsync(pr.SystemName, cr.Id))
                    continue;

                if (allow)
                {
                    await _permissionService.InsertPermissionRecordCustomerRoleMappingAsync(new PermissionRecordCustomerRoleMapping { PermissionRecordId = pr.Id, CustomerRoleId = cr.Id });
                }
                else
                {
                    await _permissionService.DeletePermissionRecordCustomerRoleMappingAsync(pr.Id, cr.Id);
                }

                await _permissionService.UpdatePermissionRecordAsync(pr);
            }
        }

        return Ok(await _localizationService.GetResourceAsync("Admin.Configuration.ACL.Updated"));
    }

    #endregion
}