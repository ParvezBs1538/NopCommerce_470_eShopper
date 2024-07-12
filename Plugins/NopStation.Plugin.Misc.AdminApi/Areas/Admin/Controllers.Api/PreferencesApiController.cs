using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Common;

namespace NopStation.Plugin.Misc.AdminApi.Areas.Admin.Controllers.Api;

[Route("api/a/Preferences/[action]")]
public partial class PreferencesApiController : BaseAdminApiController
{
    #region Fields

    private readonly IGenericAttributeService _genericAttributeService;
    private readonly IWorkContext _workContext;

    #endregion

    #region Ctor

    public PreferencesApiController(IGenericAttributeService genericAttributeService,
        IWorkContext workContext)
    {
        _genericAttributeService = genericAttributeService;
        _workContext = workContext;
    }

    #endregion

    #region Methods

    [HttpPost]
    public virtual async Task<IActionResult> SavePreference(string name, bool value)
    {
        //permission validation is not required here
        if (string.IsNullOrEmpty(name))
            return BadRequest("name is empty!");

        await _genericAttributeService.SaveAttributeAsync(await _workContext.GetCurrentCustomerAsync(), name, value);

        return Ok(defaultMessage: true);
    }

    #endregion
}