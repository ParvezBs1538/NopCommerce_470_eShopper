using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;
using NopStation.Plugin.Misc.Core.Models.Api;

namespace NopStation.Plugin.Misc.AdminApi.Areas.Admin.Controllers.Api;

[Route("api/a/productattribute/[action]")]
public partial class ProductAttributeApiController : BaseAdminApiController
{
    #region Fields

    private readonly ICustomerActivityService _customerActivityService;
    private readonly ILocalizationService _localizationService;
    private readonly ILocalizedEntityService _localizedEntityService;
    private readonly IPermissionService _permissionService;
    private readonly IProductAttributeModelFactory _productAttributeModelFactory;
    private readonly IProductAttributeService _productAttributeService;

    #endregion Fields

    #region Ctor

    public ProductAttributeApiController(ICustomerActivityService customerActivityService,
        ILocalizationService localizationService,
        ILocalizedEntityService localizedEntityService,
        IPermissionService permissionService,
        IProductAttributeModelFactory productAttributeModelFactory,
        IProductAttributeService productAttributeService)
    {
        _customerActivityService = customerActivityService;
        _localizationService = localizationService;
        _localizedEntityService = localizedEntityService;
        _permissionService = permissionService;
        _productAttributeModelFactory = productAttributeModelFactory;
        _productAttributeService = productAttributeService;
    }

    #endregion

    #region Utilities

    protected virtual async Task UpdateLocalesAsync(ProductAttribute productAttribute, ProductAttributeModel model)
    {
        foreach (var localized in model.Locales)
        {
            await _localizedEntityService.SaveLocalizedValueAsync(productAttribute,
                x => x.Name,
                localized.Name,
                localized.LanguageId);

            await _localizedEntityService.SaveLocalizedValueAsync(productAttribute,
                x => x.Description,
                localized.Description,
                localized.LanguageId);
        }
    }

    protected virtual async Task UpdateLocalesAsync(PredefinedProductAttributeValue ppav, PredefinedProductAttributeValueModel model)
    {
        foreach (var localized in model.Locales)
        {
            await _localizedEntityService.SaveLocalizedValueAsync(ppav,
                x => x.Name,
                localized.Name,
                localized.LanguageId);
        }
    }

    #endregion

    #region Methods

    #region Attribute list / create / edit / delete

    public virtual async Task<IActionResult> List()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAttributes))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _productAttributeModelFactory.PrepareProductAttributeSearchModelAsync(new ProductAttributeSearchModel());

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> List([FromBody] BaseQueryModel<ProductAttributeSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAttributes))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _productAttributeModelFactory.PrepareProductAttributeListModelAsync(searchModel);

        return OkWrap(model);
    }

    public virtual async Task<IActionResult> Create()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAttributes))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _productAttributeModelFactory.PrepareProductAttributeModelAsync(new ProductAttributeModel(), null);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Create([FromBody] BaseQueryModel<ProductAttributeModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAttributes))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        if (ModelState.IsValid)
        {
            var productAttribute = model.ToEntity<ProductAttribute>();
            await _productAttributeService.InsertProductAttributeAsync(productAttribute);
            await UpdateLocalesAsync(productAttribute, model);

            //activity log
            await _customerActivityService.InsertActivityAsync("AddNewProductAttribute",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.AddNewProductAttribute"), productAttribute.Name), productAttribute);

            return Created(productAttribute.Id, await _localizationService.GetResourceAsync("Admin.Catalog.Attributes.ProductAttributes.Added"));
        }

        //prepare model
        model = await _productAttributeModelFactory.PrepareProductAttributeModelAsync(model, null, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    [HttpGet("{id}")]
    public virtual async Task<IActionResult> Edit(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAttributes))
            return AdminApiAccessDenied();

        //try to get a product attribute with the specified id
        var productAttribute = await _productAttributeService.GetProductAttributeByIdAsync(id);
        if (productAttribute == null)
            return NotFound("No product attribute found with the specified id");

        //prepare model
        var model = await _productAttributeModelFactory.PrepareProductAttributeModelAsync(null, productAttribute);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Edit([FromBody] BaseQueryModel<ProductAttributeModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAttributes))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get a product attribute with the specified id
        var productAttribute = await _productAttributeService.GetProductAttributeByIdAsync(model.Id);
        if (productAttribute == null)
            return NotFound("No product attribute found with the specified id");

        if (ModelState.IsValid)
        {
            productAttribute = model.ToEntity(productAttribute);
            await _productAttributeService.UpdateProductAttributeAsync(productAttribute);

            await UpdateLocalesAsync(productAttribute, model);

            //activity log
            await _customerActivityService.InsertActivityAsync("EditProductAttribute",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditProductAttribute"), productAttribute.Name), productAttribute);

            return Ok(await _localizationService.GetResourceAsync("Admin.Catalog.Attributes.ProductAttributes.Updated"));
        }

        //prepare model
        model = await _productAttributeModelFactory.PrepareProductAttributeModelAsync(model, productAttribute, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> Delete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAttributes))
            return AdminApiAccessDenied();

        //try to get a product attribute with the specified id
        var productAttribute = await _productAttributeService.GetProductAttributeByIdAsync(id);
        if (productAttribute == null)
            return NotFound("No product attribute found with the specified id");

        await _productAttributeService.DeleteProductAttributeAsync(productAttribute);

        //activity log
        await _customerActivityService.InsertActivityAsync("DeleteProductAttribute",
            string.Format(await _localizationService.GetResourceAsync("ActivityLog.DeleteProductAttribute"), productAttribute.Name), productAttribute);

        return Ok(await _localizationService.GetResourceAsync("Admin.Catalog.Attributes.ProductAttributes.Deleted"));
    }

    [HttpPost]
    public virtual async Task<IActionResult> DeleteSelected([FromBody] BaseQueryModel<ICollection<int>> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAttributes))
            return AdminApiAccessDenied();

        var selectedIds = queryModel.Data;
        if (selectedIds != null || selectedIds.Count == 0)
            return NotFound("Not found with specified Id");

        var productAttributes = await _productAttributeService.GetProductAttributeByIdsAsync(selectedIds.ToArray());
        await _productAttributeService.DeleteProductAttributesAsync(productAttributes);

        foreach (var productAttribute in productAttributes)
        {
            await _customerActivityService.InsertActivityAsync("DeleteProductAttribute",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.DeleteProductAttribute"), productAttribute.Name), productAttribute);
        }

        return Ok(defaultMessage: true);
    }

    #endregion

    #region Used by products

    [HttpPost]
    public virtual async Task<IActionResult> UsedByProducts([FromBody] BaseQueryModel<ProductAttributeProductSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAttributes))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //try to get a product attribute with the specified id
        var productAttribute = await _productAttributeService.GetProductAttributeByIdAsync(searchModel.ProductAttributeId);
        if (productAttribute == null)
            return NotFound("No product attribute found with the specified id");

        //prepare model
        var model = await _productAttributeModelFactory.PrepareProductAttributeProductListModelAsync(searchModel, productAttribute);

        return OkWrap(model);
    }

    #endregion

    #region values

    [HttpPost]
    public virtual async Task<IActionResult> PredefinedProductAttributeValueList([FromBody] BaseQueryModel<PredefinedProductAttributeValueSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAttributes))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //try to get a product attribute with the specified id
        var productAttribute = await _productAttributeService.GetProductAttributeByIdAsync(searchModel.ProductAttributeId);
        if (productAttribute == null)
            return NotFound("No product attribute found with the specified id");

        //prepare model
        var model = await _productAttributeModelFactory.PreparePredefinedProductAttributeValueListModelAsync(searchModel, productAttribute);

        return OkWrap(model);
    }

    [HttpGet("{productAttributeId}")]
    public virtual async Task<IActionResult> PredefinedProductAttributeValueCreatePopup(int productAttributeId)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAttributes))
            return AdminApiAccessDenied();

        //try to get a product attribute with the specified id
        var productAttribute = await _productAttributeService.GetProductAttributeByIdAsync(productAttributeId);
        if (productAttribute == null)
            return NotFound("No product attribute found with the specified id");

        //prepare model
        var model = await _productAttributeModelFactory
            .PreparePredefinedProductAttributeValueModelAsync(new PredefinedProductAttributeValueModel(), productAttribute, null);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> PredefinedProductAttributeValueCreatePopup([FromBody] BaseQueryModel<PredefinedProductAttributeValueModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAttributes))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get a product attribute with the specified id
        var productAttribute = await _productAttributeService.GetProductAttributeByIdAsync(model.ProductAttributeId);
        if (productAttribute == null)
            return NotFound("No product attribute found with the specified id");

        if (ModelState.IsValid)
        {
            //fill entity from model
            var ppav = model.ToEntity<PredefinedProductAttributeValue>();

            await _productAttributeService.InsertPredefinedProductAttributeValueAsync(ppav);
            await UpdateLocalesAsync(ppav, model);

            return OkWrap(model);
        }

        //prepare model
        model = await _productAttributeModelFactory.PreparePredefinedProductAttributeValueModelAsync(model, productAttribute, null, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    [HttpGet("{id}")]
    public virtual async Task<IActionResult> PredefinedProductAttributeValueEditPopup(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAttributes))
            return AdminApiAccessDenied();

        //try to get a product attribute value with the specified id
        var productAttributeValue = await _productAttributeService.GetPredefinedProductAttributeValueByIdAsync(id);
        if (productAttributeValue == null)
            return NotFound("No product attribute value found with the specified id");

        //try to get a product attribute with the specified id
        var productAttribute = await _productAttributeService.GetProductAttributeByIdAsync(productAttributeValue.ProductAttributeId);
        if (productAttribute == null)
            return NotFound("No product attribute found with the specified id");

        //prepare model
        var model = await _productAttributeModelFactory.PreparePredefinedProductAttributeValueModelAsync(null, productAttribute, productAttributeValue);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> PredefinedProductAttributeValueEditPopup([FromBody] BaseQueryModel<PredefinedProductAttributeValueModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAttributes))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get a product attribute value with the specified id
        var productAttributeValue = await _productAttributeService.GetPredefinedProductAttributeValueByIdAsync(model.Id);
        if (productAttributeValue == null)
            return NotFound("No product attribute value found with the specified id");

        //try to get a product attribute with the specified id
        var productAttribute = await _productAttributeService.GetProductAttributeByIdAsync(productAttributeValue.ProductAttributeId);
        if (productAttribute == null)
            return NotFound("No product attribute found with the specified id");

        if (ModelState.IsValid)
        {
            productAttributeValue = model.ToEntity(productAttributeValue);
            await _productAttributeService.UpdatePredefinedProductAttributeValueAsync(productAttributeValue);

            await UpdateLocalesAsync(productAttributeValue, model);

            return OkWrap(model);
        }

        //prepare model
        model = await _productAttributeModelFactory.PreparePredefinedProductAttributeValueModelAsync(model, productAttribute, productAttributeValue, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> PredefinedProductAttributeValueDelete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAttributes))
            return AdminApiAccessDenied();

        //try to get a product attribute value with the specified id
        var productAttributeValue = await _productAttributeService.GetPredefinedProductAttributeValueByIdAsync(id);
        if (productAttributeValue == null)
            return NotFound("No product attribute value found with the specified id");

        await _productAttributeService.DeletePredefinedProductAttributeValueAsync(productAttributeValue);

        return Ok(defaultMessage: true);
    }

    #endregion

    #endregion
}