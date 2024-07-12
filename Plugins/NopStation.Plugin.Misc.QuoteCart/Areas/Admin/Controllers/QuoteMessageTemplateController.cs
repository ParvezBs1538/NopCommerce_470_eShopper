using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.Messages;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.QuoteCart.Areas.Admin.Factories;

namespace NopStation.Plugin.Misc.QuoteCart.Areas.Admin.Controllers;

public class QuoteMessageTemplateController : NopStationAdminController
{
    #region Fields

    private readonly IMessageTemplateModelFactory _messageTemplateModelFactory;
    private readonly IPermissionService _permissionService;
    private readonly IQuoteRequestModelFactory _quoteRequestModelFactory;

    #endregion

    #region Ctor

    public QuoteMessageTemplateController(
        IMessageTemplateModelFactory messageTemplateModelFactory,
        IPermissionService permissionService,
        IQuoteRequestModelFactory quoteRequestModelFactory)
    {
        _messageTemplateModelFactory = messageTemplateModelFactory;
        _permissionService = permissionService;
        _quoteRequestModelFactory = quoteRequestModelFactory;
    }

    #endregion

    #region Methods

    public async Task<IActionResult> List()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageMessageTemplates))
            return AccessDeniedView();

        //prepare model
        var model = await _messageTemplateModelFactory.PrepareMessageTemplateSearchModelAsync(new MessageTemplateSearchModel());

        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> List(MessageTemplateSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageMessageTemplates))
            return await AccessDeniedDataTablesJson();

        //prepare model
        var model = await _quoteRequestModelFactory.PrepareMessageTemplateListModelAsync(searchModel);

        return Json(model);
    }

    #endregion
}
