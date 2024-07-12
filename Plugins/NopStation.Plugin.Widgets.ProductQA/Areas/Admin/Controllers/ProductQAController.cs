using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Widgets.ProductQuestionAnswer.Areas.Admin.Factories;
using NopStation.Plugin.Widgets.ProductQuestionAnswer.Areas.Admin.Models;
using NopStation.Plugin.Widgets.ProductQuestionAnswer.Services;

namespace NopStation.Plugin.Widgets.ProductQuestionAnswer.Areas.Admin.Controller
{
    public class ProductQAController : NopStationAdminController
    {
        #region Fields

        private readonly IPermissionService _permissionService;
        private readonly IProductQAModelFactory _productQAModelFactory;
        private readonly IWorkContext _workContext;
        private readonly ILocalizationService _localizationService;
        private readonly IProductQAService _productQAService;
        private readonly INotificationService _notificationService;

        #endregion

        #region Ctor

        public ProductQAController(IPermissionService permissionService,
            IProductQAModelFactory productQAModelFactory,
            IWorkContext workContext,
            ILocalizationService localizationService,
            IProductQAService productQAService,
            INotificationService notificationService)
        {
            _permissionService = permissionService;
            _productQAModelFactory = productQAModelFactory;
            _workContext = workContext;
            _localizationService = localizationService;
            _productQAService = productQAService;
            _notificationService = notificationService;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(ProductQAPermissionProvider.ManageProductQA))
                return AccessDeniedView();

            var model = await _productQAModelFactory.PrepareProductQASearchModelAsync(new ProductQASearchModel());

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> List(ProductQASearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(ProductQAPermissionProvider.ManageProductQA))
                return AccessDeniedView();

            var model = await _productQAModelFactory.PrepareProdouctQAListModelAsync(searchModel);

            return Json(model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(ProductQAPermissionProvider.ManageProductQA))
                return AccessDeniedView();

            var productQnA = await _productQAService.GetProductQnAByIdAsync(id);
            if (productQnA == null)
                throw new ArgumentException("The question is not find with the specified id", nameof(id));

            await _productQAService.DeleteProductQnAAsync(productQnA);
            return new NullJsonResult();
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(ProductQAPermissionProvider.ManageProductQA))
                return AccessDeniedView();
            var accessToEdit = await _productQAModelFactory.IsAccessToEditAsync();

            if(!accessToEdit)
                return AccessDeniedView();

            var model = await _productQAModelFactory.PrepareModelForEditPageAsync(id);
            if (model == null || model.Deleted)
                return RedirectToAction("List");

            return View(model);
        }

        [EditAccess, HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> Edit(ProductQAModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(ProductQAPermissionProvider.ManageProductQA))
                return AccessDeniedView();

            

            var productQA = await _productQAService.GetProductQnAByIdAsync(model.Id);
            if (productQA == null || productQA.Deleted)
                return RedirectToAction("List");

            productQA.AnswerText = model.AnswerText;
            productQA.QuestionText = model.QuestionText;
            productQA.UpdatedOnUtc = DateTime.UtcNow;
            productQA.IsApproved = model.IsApproved;
            productQA.UpdatedByCustomerId = (await _workContext.GetCurrentCustomerAsync()).Id;

            await _productQAService.UpdateProductQnAAsync(productQA);
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.ProductQuestionAnswer.ProductQnA.Updated"));

            if (!continueEditing)
                return RedirectToAction("List");
            
            return RedirectToAction("Edit", new { id = model.Id });
        }

        #endregion
    }
}
