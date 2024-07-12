using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Services.Common;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.Common;
using Nop.Web.Areas.Admin.Models.Home;

namespace NopStation.Plugin.Misc.AdminApi.Areas.Admin.Controllers.Api;

[Route("api/a/home/[action]")]
public partial class HomeApiController : BaseAdminApiController
{
    #region Fields

    private readonly ICommonModelFactory _commonModelFactory;
    private readonly IHomeModelFactory _homeModelFactory;
    private readonly ILocalizationService _localizationService;
    private readonly INotificationService _notificationService;
    private readonly IPermissionService _permissionService;
    private readonly IGenericAttributeService _genericAttributeService;
    private readonly IWorkContext _workContext;

    #endregion

    #region Ctor

    public HomeApiController(ICommonModelFactory commonModelFactory,
        IHomeModelFactory homeModelFactory,
        ILocalizationService localizationService,
        INotificationService notificationService,
        IPermissionService permissionService,
        IGenericAttributeService genericAttributeService,
        IWorkContext workContext)
    {
        _commonModelFactory = commonModelFactory;
        _homeModelFactory = homeModelFactory;
        _localizationService = localizationService;
        _notificationService = notificationService;
        _permissionService = permissionService;
        _workContext = workContext;
        _genericAttributeService = genericAttributeService;
    }

    #endregion

    #region Methods

    public virtual async Task<IActionResult> Index()
    {
        //display a warning to a store owner if there are some error
        var customer = await _workContext.GetCurrentCustomerAsync();
        var hideCard = await _genericAttributeService.GetAttributeAsync<bool>(customer, NopCustomerDefaults.HideConfigurationStepsAttribute);
        var closeCard = await _genericAttributeService.GetAttributeAsync<bool>(customer, NopCustomerDefaults.CloseConfigurationStepsAttribute);

        //prepare model
        var model = await _homeModelFactory.PrepareDashboardModelAsync(new DashboardModel());

        if ((hideCard || closeCard) && await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageMaintenance))
        {
            var warnings = await _commonModelFactory.PrepareSystemWarningModelsAsync();
            if (warnings.Any(warning => warning.Level == SystemWarningLevel.Fail ||
                                        warning.Level == SystemWarningLevel.Warning))
            
            model.CustomProperties.Add("Warnings", string.Format(await _localizationService.GetResourceAsync("Admin.System.Warnings.Errors"),
                    Url.Action("Warnings", "Common")));
        }

        string message = null;
        //progress of localization 
        var currentLanguage = await _workContext.GetWorkingLanguageAsync();
        var progress = await _genericAttributeService.GetAttributeAsync<string>(currentLanguage, NopCommonDefaults.LanguagePackProgressAttribute);
        if (!string.IsNullOrEmpty(progress))
        {
            var locale = await _localizationService.GetResourceAsync("Admin.Configuration.LanguagePackProgressMessage");
            message = string.Format(locale, progress, NopLinksDefaults.OfficialSite.Translations);
            await _genericAttributeService.SaveAttributeAsync(currentLanguage, NopCommonDefaults.LanguagePackProgressAttribute, string.Empty);
        }

        model.CustomProperties.Add("CanManageOrders", (await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders)).ToString());
        model.CustomProperties.Add("CanManageCustomers", (await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers)).ToString());
        model.CustomProperties.Add("CanManageProducts", (await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts)).ToString());
        model.CustomProperties.Add("CanManageReturnRequests", (await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageReturnRequests)).ToString());

        return OkWrap(model, message);
    }

    public virtual async Task<IActionResult> NopCommerceNews()
    {
        var model = await _homeModelFactory.PrepareNopCommerceNewsModelAsync();

        return OkWrap(model);
    }

    #endregion
}