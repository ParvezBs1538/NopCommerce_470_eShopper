using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Topics;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Services.Topics;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Templates;
using NopStation.Plugin.Misc.Core.Models.Api;

namespace NopStation.Plugin.Misc.AdminApi.Areas.Admin.Controllers.Api;

[Route("api/a/template/[action]")]
public partial class TemplateApiController : BaseAdminApiController
{
    #region Fields

    private readonly ICategoryTemplateService _categoryTemplateService;
    private readonly ILocalizationService _localizationService;
    private readonly IManufacturerTemplateService _manufacturerTemplateService;
    private readonly IPermissionService _permissionService;
    private readonly IProductTemplateService _productTemplateService;
    private readonly ITemplateModelFactory _templateModelFactory;
    private readonly ITopicTemplateService _topicTemplateService;

    #endregion

    #region Ctor

    public TemplateApiController(ICategoryTemplateService categoryTemplateService,
        IManufacturerTemplateService manufacturerTemplateService,
        IPermissionService permissionService,
        IProductTemplateService productTemplateService,
        ITemplateModelFactory templateModelFactory,
        ITopicTemplateService topicTemplateService,
        ILocalizationService localizationService)
    {
        _categoryTemplateService = categoryTemplateService;
        _manufacturerTemplateService = manufacturerTemplateService;
        _permissionService = permissionService;
        _productTemplateService = productTemplateService;
        _templateModelFactory = templateModelFactory;
        _topicTemplateService = topicTemplateService;
        _localizationService = localizationService;
    }

    #endregion

    #region Methods

    public virtual async Task<IActionResult> List()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageMaintenance))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _templateModelFactory.PrepareTemplatesModelAsync(new TemplatesModel());

        return OkWrap(model);
    }

    #region Category templates        

    [HttpPost]
    public virtual async Task<IActionResult> CategoryTemplates([FromBody] BaseQueryModel<CategoryTemplateSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageMaintenance))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _templateModelFactory.PrepareCategoryTemplateListModelAsync(searchModel);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> CategoryTemplateUpdate([FromBody] BaseQueryModel<CategoryTemplateModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageMaintenance))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        if (!ModelState.IsValid)
            return BadRequestWrap(model, ModelState);

        //try to get a category template with the specified id
        var template = await _categoryTemplateService.GetCategoryTemplateByIdAsync(model.Id);
        if (template == null)
            return NotFound("No template found with the specified id");

        template = model.ToEntity(template);
        await _categoryTemplateService.UpdateCategoryTemplateAsync(template);

        return Ok(defaultMessage: true);
    }

    [HttpPost]
    public virtual async Task<IActionResult> CategoryTemplateAdd([FromBody] BaseQueryModel<CategoryTemplateModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageMaintenance))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        if (!ModelState.IsValid)
            return BadRequestWrap(model, ModelState);

        var template = new CategoryTemplate();
        template = model.ToEntity(template);
        await _categoryTemplateService.InsertCategoryTemplateAsync(template);

        return Created(template.Id);
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> CategoryTemplateDelete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageMaintenance))
            return AdminApiAccessDenied();

        if ((await _categoryTemplateService.GetAllCategoryTemplatesAsync()).Count == 1)
            return BadRequest(await _localizationService.GetResourceAsync("Admin.System.Templates.NotDeleteOnlyOne"));

        //try to get a category template with the specified id
        var template = await _categoryTemplateService.GetCategoryTemplateByIdAsync(id);
        if (template == null)
            return NotFound("No template found with the specified id");

        await _categoryTemplateService.DeleteCategoryTemplateAsync(template);

        return Ok(defaultMessage: true);
    }

    #endregion

    #region Manufacturer templates        

    [HttpPost]
    public virtual async Task<IActionResult> ManufacturerTemplates([FromBody] BaseQueryModel<ManufacturerTemplateSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageMaintenance))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _templateModelFactory.PrepareManufacturerTemplateListModelAsync(searchModel);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ManufacturerTemplateUpdate([FromBody] BaseQueryModel<ManufacturerTemplateModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageMaintenance))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        if (!ModelState.IsValid)
            return BadRequestWrap(model, ModelState);

        //try to get a manufacturer template with the specified id
        var template = await _manufacturerTemplateService.GetManufacturerTemplateByIdAsync(model.Id);
        if (template == null)
            return NotFound("No template found with the specified id");

        template = model.ToEntity(template);
        await _manufacturerTemplateService.UpdateManufacturerTemplateAsync(template);

        return Ok(defaultMessage: true);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ManufacturerTemplateAdd([FromBody] BaseQueryModel<ManufacturerTemplateModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageMaintenance))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        if (!ModelState.IsValid)
            return BadRequestWrap(model, ModelState);

        var template = new ManufacturerTemplate();
        template = model.ToEntity(template);
        await _manufacturerTemplateService.InsertManufacturerTemplateAsync(template);

        return Created(template.Id);
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> ManufacturerTemplateDelete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageMaintenance))
            return AdminApiAccessDenied();

        if ((await _manufacturerTemplateService.GetAllManufacturerTemplatesAsync()).Count == 1)
            return BadRequest(await _localizationService.GetResourceAsync("Admin.System.Templates.NotDeleteOnlyOne"));

        //try to get a manufacturer template with the specified id
        var template = await _manufacturerTemplateService.GetManufacturerTemplateByIdAsync(id);
        if (template == null)
            return NotFound("No template found with the specified id");

        await _manufacturerTemplateService.DeleteManufacturerTemplateAsync(template);

        return Ok(defaultMessage: true);
    }

    #endregion

    #region Product templates

    [HttpPost]
    public virtual async Task<IActionResult> ProductTemplates([FromBody] BaseQueryModel<ProductTemplateSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageMaintenance))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _templateModelFactory.PrepareProductTemplateListModelAsync(searchModel);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ProductTemplateUpdate([FromBody] BaseQueryModel<ProductTemplateModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageMaintenance))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        if (!ModelState.IsValid)
            return BadRequestWrap(model, ModelState);
        ;

        //try to get a product template with the specified id
        var template = await _productTemplateService.GetProductTemplateByIdAsync(model.Id);
        if (template == null)
            return NotFound("No template found with the specified id");

        template = model.ToEntity(template);
        await _productTemplateService.UpdateProductTemplateAsync(template);

        return Ok(defaultMessage: true);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ProductTemplateAdd([FromBody] BaseQueryModel<ProductTemplateModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageMaintenance))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        if (!ModelState.IsValid)
            return BadRequestWrap(model, ModelState);
        ;

        var template = new ProductTemplate();
        template = model.ToEntity(template);
        await _productTemplateService.InsertProductTemplateAsync(template);

        return Created(template.Id);
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> ProductTemplateDelete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageMaintenance))
            return AdminApiAccessDenied();

        if ((await _productTemplateService.GetAllProductTemplatesAsync()).Count == 1)
            return BadRequest(await _localizationService.GetResourceAsync("Admin.System.Templates.NotDeleteOnlyOne"));

        //try to get a product template with the specified id
        var template = await _productTemplateService.GetProductTemplateByIdAsync(id);
        if (template == null)
            return NotFound();

        await _productTemplateService.DeleteProductTemplateAsync(template);

        return Ok(defaultMessage: true);
    }

    #endregion

    #region Topic templates

    [HttpPost]
    public virtual async Task<IActionResult> TopicTemplates([FromBody] BaseQueryModel<TopicTemplateSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageMaintenance))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _templateModelFactory.PrepareTopicTemplateListModelAsync(searchModel);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> TopicTemplateUpdate([FromBody] BaseQueryModel<TopicTemplateModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageMaintenance))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        if (!ModelState.IsValid)
            return BadRequestWrap(model, ModelState);
        ;

        //try to get a topic template with the specified id
        var template = await _topicTemplateService.GetTopicTemplateByIdAsync(model.Id);
        if (template == null)
            return NotFound("No template found with the specified id");

        template = model.ToEntity(template);
        await _topicTemplateService.UpdateTopicTemplateAsync(template);

        return Ok(defaultMessage: true);
    }

    [HttpPost]
    public virtual async Task<IActionResult> TopicTemplateAdd([FromBody] BaseQueryModel<TopicTemplateModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageMaintenance))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        if (!ModelState.IsValid)
            return BadRequestWrap(model, ModelState);
        ;

        var template = new TopicTemplate();
        template = model.ToEntity(template);
        await _topicTemplateService.InsertTopicTemplateAsync(template);

        return Created(template.Id);
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> TopicTemplateDelete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageMaintenance))
            return AdminApiAccessDenied();

        if ((await _topicTemplateService.GetAllTopicTemplatesAsync()).Count == 1)
            return BadRequest(await _localizationService.GetResourceAsync("Admin.System.Templates.NotDeleteOnlyOne"));

        //try to get a topic template with the specified id
        var template = await _topicTemplateService.GetTopicTemplateByIdAsync(id);
        if (template == null)
            return NotFound("No template found with the specified id");

        await _topicTemplateService.DeleteTopicTemplateAsync(template);

        return Ok(defaultMessage: true);
    }

    #endregion

    #endregion
}