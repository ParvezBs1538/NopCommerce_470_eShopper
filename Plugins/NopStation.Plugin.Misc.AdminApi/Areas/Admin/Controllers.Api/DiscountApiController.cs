using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Discounts;
using Nop.Services.Catalog;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Discounts;
using NopStation.Plugin.Misc.Core.Models.Api;

namespace NopStation.Plugin.Misc.AdminApi.Areas.Admin.Controllers.Api;

[Route("api/a/discount/[action]")]
public partial class DiscountApiController : BaseAdminApiController
{
    #region Fields

    private readonly CatalogSettings _catalogSettings;
    private readonly ICategoryService _categoryService;
    private readonly ICustomerActivityService _customerActivityService;
    private readonly IDiscountModelFactory _discountModelFactory;
    private readonly IDiscountService _discountService;
    private readonly ILocalizationService _localizationService;
    private readonly IManufacturerService _manufacturerService;
    private readonly IPermissionService _permissionService;
    private readonly IProductService _productService;

    #endregion

    #region Ctor

    public DiscountApiController(CatalogSettings catalogSettings,
        ICategoryService categoryService,
        ICustomerActivityService customerActivityService,
        IDiscountModelFactory discountModelFactory,
        IDiscountService discountService,
        ILocalizationService localizationService,
        IManufacturerService manufacturerService,
        IPermissionService permissionService,
        IProductService productService)
    {
        _catalogSettings = catalogSettings;
        _categoryService = categoryService;
        _customerActivityService = customerActivityService;
        _discountModelFactory = discountModelFactory;
        _discountService = discountService;
        _localizationService = localizationService;
        _manufacturerService = manufacturerService;
        _permissionService = permissionService;
        _productService = productService;
    }

    #endregion

    #region Methods

    #region Discounts

    public virtual async Task<IActionResult> List()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageDiscounts))
            return AdminApiAccessDenied();

        var errors = new List<string>();
        //whether discounts are ignored
        if (_catalogSettings.IgnoreDiscounts)
            errors.Add(await _localizationService.GetResourceAsync("Admin.Promotions.Discounts.IgnoreDiscounts.Warning"));

        //prepare model
        var model = await _discountModelFactory.PrepareDiscountSearchModelAsync(new DiscountSearchModel());

        return OkWrap(model, errors: errors);
    }

    [HttpPost]
    public virtual async Task<IActionResult> List([FromBody] BaseQueryModel<DiscountSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageDiscounts))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _discountModelFactory.PrepareDiscountListModelAsync(searchModel);

        return OkWrap(model);
    }

    public virtual async Task<IActionResult> Create()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageDiscounts))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _discountModelFactory.PrepareDiscountModelAsync(new DiscountModel(), null);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Create([FromBody] BaseQueryModel<DiscountModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageDiscounts))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        if (ModelState.IsValid)
        {
            var discount = model.ToEntity<Discount>();
            await _discountService.InsertDiscountAsync(discount);

            //activity log
            await _customerActivityService.InsertActivityAsync("AddNewDiscount",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.AddNewDiscount"), discount.Name), discount);

            return Created(discount.Id, await _localizationService.GetResourceAsync("Admin.Promotions.Discounts.Added"));
        }

        //prepare model
        model = await _discountModelFactory.PrepareDiscountModelAsync(model, null, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    [HttpGet("{id}")]
    public virtual async Task<IActionResult> Edit(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageDiscounts))
            return AdminApiAccessDenied();

        //try to get a discount with the specified id
        var discount = await _discountService.GetDiscountByIdAsync(id);
        if (discount == null)
            return NotFound("No discount found with the specified id");

        //prepare model
        var model = await _discountModelFactory.PrepareDiscountModelAsync(null, discount);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Edit([FromBody] BaseQueryModel<DiscountModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageDiscounts))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get a discount with the specified id
        var discount = await _discountService.GetDiscountByIdAsync(model.Id);
        if (discount == null)
            return NotFound("No discount found with the specified id");

        if (ModelState.IsValid)
        {
            var prevDiscountType = discount.DiscountType;
            discount = model.ToEntity(discount);
            await _discountService.UpdateDiscountAsync(discount);

            //clean up old references (if changed) 
            if (prevDiscountType != discount.DiscountType)
            {
                switch (prevDiscountType)
                {
                    case DiscountType.AssignedToSkus:
                        await _productService.ClearDiscountProductMappingAsync(discount);
                        break;
                    case DiscountType.AssignedToCategories:
                        await _categoryService.ClearDiscountCategoryMappingAsync(discount);
                        break;
                    case DiscountType.AssignedToManufacturers:
                        await _manufacturerService.ClearDiscountManufacturerMappingAsync(discount);
                        break;
                    default:
                        break;
                }
            }

            //activity log
            await _customerActivityService.InsertActivityAsync("EditDiscount",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditDiscount"), discount.Name), discount);

            return Ok(await _localizationService.GetResourceAsync("Admin.Promotions.Discounts.Updated"));
        }

        //prepare model
        model = await _discountModelFactory.PrepareDiscountModelAsync(model, discount, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> Delete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageDiscounts))
            return AdminApiAccessDenied();

        //try to get a discount with the specified id
        var discount = await _discountService.GetDiscountByIdAsync(id);
        if (discount == null)
            return NotFound("No discount found with the specified id");

        //applied to products
        var products = await _productService.GetProductsWithAppliedDiscountAsync(discount.Id, true);

        await _discountService.DeleteDiscountAsync(discount);

        //update "HasDiscountsApplied" properties
        foreach (var p in products)
            await _productService.UpdateHasDiscountsAppliedAsync(p);

        //activity log
        await _customerActivityService.InsertActivityAsync("DeleteDiscount",
            string.Format(await _localizationService.GetResourceAsync("ActivityLog.DeleteDiscount"), discount.Name), discount);

        return Ok(await _localizationService.GetResourceAsync("Admin.Promotions.Discounts.Deleted"));
    }

    #endregion

    #region Discount requirements

    //public virtual async Task<IActionResult> GetDiscountRequirementConfigurationUrl(string systemName, int discountId, int? discountRequirementId)
    //{
    //    if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageDiscounts))
    //        return AdminApiAccessDenied();

    //    if (string.IsNullOrEmpty(systemName))
    //        throw new ArgumentNullException(nameof(systemName));

    //    var discountRequirementRule = await _discountPluginManager.LoadPluginBySystemNameAsync(systemName)
    //        ?? throw new ArgumentException("Discount requirement rule could not be loaded");

    //    var discount = await _discountService.GetDiscountByIdAsync(discountId)
    //        ?? throw new ArgumentException("Discount could not be loaded");

    //    var url = discountRequirementRule.GetConfigurationUrl(discount.Id, discountRequirementId);

    //    return Json(new { url });
    //}

    //public virtual async Task<IActionResult> GetDiscountRequirements(int discountId, int discountRequirementId,
    //    int? parentId, int? interactionTypeId, bool deleteRequirement)
    //{
    //    if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageDiscounts))
    //        return AdminApiAccessDenied();

    //    var requirements = new List<DiscountRequirementRuleModel>();

    //    var discount = await _discountService.GetDiscountByIdAsync(discountId);
    //    if (discount == null)
    //        return NotFound();

    //    var discountRequirement = await _discountService.GetDiscountRequirementByIdAsync(discountRequirementId);
    //    if (discountRequirement != null)
    //    {
    //        //delete
    //        if (deleteRequirement)
    //        {
    //            await _discountService.DeleteDiscountRequirementAsync(discountRequirement, true);

    //            var discountRequirements = await _discountService.GetAllDiscountRequirementsAsync(discount.Id);

    //            //delete default group if there are no any requirements
    //            if (!discountRequirements.Any(requirement => requirement.ParentId.HasValue))
    //            {
    //                foreach (var dr in discountRequirements)
    //                    await _discountService.DeleteDiscountRequirementAsync(dr, true);
    //            }
    //        }
    //        //or update the requirement
    //        else
    //        {
    //            var defaultGroupId = (await _discountService.GetAllDiscountRequirementsAsync(discount.Id, true)).FirstOrDefault(requirement => requirement.IsGroup)?.Id ?? 0;
    //            if (defaultGroupId == 0)
    //            {
    //                //add default requirement group
    //                var defaultGroup = new DiscountRequirement
    //                {
    //                    IsGroup = true,
    //                    DiscountId = discount.Id,
    //                    InteractionType = RequirementGroupInteractionType.And,
    //                    DiscountRequirementRuleSystemName = await _localizationService
    //                        .GetResourceAsync("Admin.Promotions.Discounts.Requirements.DefaultRequirementGroup")
    //                };

    //                await _discountService.InsertDiscountRequirementAsync(defaultGroup);

    //                defaultGroupId = defaultGroup.Id;
    //            }

    //            //set parent identifier if specified
    //            if (parentId.HasValue)
    //                discountRequirement.ParentId = parentId.Value;
    //            else
    //            {
    //                //or default group identifier
    //                if (defaultGroupId != discountRequirement.Id)
    //                    discountRequirement.ParentId = defaultGroupId;
    //            }

    //            //set interaction type
    //            if (interactionTypeId.HasValue)
    //                discountRequirement.InteractionTypeId = interactionTypeId;

    //            await _discountService.UpdateDiscountRequirementAsync(discountRequirement);
    //        }
    //    }

    //    //get current requirements
    //    var topLevelRequirements = (await _discountService.GetAllDiscountRequirementsAsync(discount.Id, true)).Where(requirement => requirement.IsGroup).ToList();

    //    //get interaction type of top-level group
    //    var interactionType = topLevelRequirements.FirstOrDefault()?.InteractionType;

    //    if (interactionType.HasValue)
    //    {
    //        requirements = (await _discountModelFactory
    //            .PrepareDiscountRequirementRuleModelsAsync(topLevelRequirements, discount, interactionType.Value)).ToList();
    //    }

    //    //get available groups
    //    var requirementGroups = (await _discountService.GetAllDiscountRequirementsAsync(discount.Id)).Where(requirement => requirement.IsGroup);

    //    var availableRequirementGroups = requirementGroups.Select(requirement =>
    //        new SelectListItem { Value = requirement.Id.ToString(), Text = requirement.DiscountRequirementRuleSystemName }).ToList();

    //    return Json(new { Requirements = requirements, AvailableGroups = availableRequirementGroups });
    //}

    //public virtual async Task<IActionResult> AddNewGroup(int discountId, string name)
    //{
    //    if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageDiscounts))
    //        return AdminApiAccessDenied();

    //    var discount = await _discountService.GetDiscountByIdAsync(discountId);
    //    if (discount == null)
    //        throw new ArgumentException("Discount could not be loaded");

    //    var defaultGroup = (await _discountService.GetAllDiscountRequirementsAsync(discount.Id, true)).FirstOrDefault(requirement => requirement.IsGroup);
    //    if (defaultGroup == null)
    //    {
    //        //add default requirement group
    //        await _discountService.InsertDiscountRequirementAsync(new DiscountRequirement
    //        {
    //            DiscountId = discount.Id,
    //            IsGroup = true,
    //            InteractionType = RequirementGroupInteractionType.And,
    //            DiscountRequirementRuleSystemName = await _localizationService
    //                .GetResourceAsync("Admin.Promotions.Discounts.Requirements.DefaultRequirementGroup")
    //        });
    //    }

    //    //save new requirement group
    //    var discountRequirementGroup = new DiscountRequirement
    //    {
    //        DiscountId = discount.Id,
    //        IsGroup = true,
    //        DiscountRequirementRuleSystemName = name,
    //        InteractionType = RequirementGroupInteractionType.And
    //    };

    //    await _discountService.InsertDiscountRequirementAsync(discountRequirementGroup);

    //    if (!string.IsNullOrEmpty(name))
    //        return Json(new { Result = true, NewRequirementId = discountRequirementGroup.Id });

    //    //set identifier as group name (if not specified)
    //    discountRequirementGroup.DiscountRequirementRuleSystemName = $"#{discountRequirementGroup.Id}";
    //    await _discountService.UpdateDiscountRequirementAsync(discountRequirementGroup);

    //    await _discountService.UpdateDiscountAsync(discount);

    //    return Json(new { Result = true, NewRequirementId = discountRequirementGroup.Id });
    //}

    #endregion

    #region Applied to products

    [HttpPost]
    public virtual async Task<IActionResult> ProductList([FromBody] BaseQueryModel<DiscountProductSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageDiscounts))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //try to get a discount with the specified id
        var discount = await _discountService.GetDiscountByIdAsync(searchModel.DiscountId);
        if (discount == null)
            return NotFound("No discount found with the specified id");

        //prepare model
        var model = await _discountModelFactory.PrepareDiscountProductListModelAsync(searchModel, discount);

        return OkWrap(model);
    }

    [HttpGet("{discountId}/{productId}")]
    public virtual async Task<IActionResult> ProductDelete(int discountId, int productId)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageDiscounts))
            return AdminApiAccessDenied();

        //try to get a discount with the specified id
        var discount = await _discountService.GetDiscountByIdAsync(discountId);
        if (discount == null)
            return NotFound("No discount found with the specified id");

        //try to get a product with the specified id
        var product = await _productService.GetProductByIdAsync(productId);
        if (product == null)
            return NotFound("No product found with the specified id");

        //remove discount
        if (await _productService.GetDiscountAppliedToProductAsync(product.Id, discount.Id) is DiscountProductMapping discountProductMapping)
            await _productService.DeleteDiscountProductMappingAsync(discountProductMapping);

        await _productService.UpdateProductAsync(product);
        await _productService.UpdateHasDiscountsAppliedAsync(product);

        return Ok(defaultMessage: true);
    }

    [HttpGet("{discountId}")]
    public virtual async Task<IActionResult> ProductAddPopup(int discountId)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageDiscounts))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _discountModelFactory.PrepareAddProductToDiscountSearchModelAsync(new AddProductToDiscountSearchModel());

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ProductAddPopupList([FromBody] BaseQueryModel<AddProductToDiscountSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageDiscounts))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _discountModelFactory.PrepareAddProductToDiscountListModelAsync(searchModel);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ProductAddPopup([FromBody] BaseQueryModel<AddProductToDiscountModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageDiscounts))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get a discount with the specified id
        var discount = await _discountService.GetDiscountByIdAsync(model.DiscountId);
        if (discount == null)
            return NotFound("No discount found with the specified id");

        var selectedProducts = await _productService.GetProductsByIdsAsync(model.SelectedProductIds.ToArray());
        if (selectedProducts.Any())
        {
            foreach (var product in selectedProducts)
            {
                if (await _productService.GetDiscountAppliedToProductAsync(product.Id, discount.Id) is null)
                    await _productService.InsertDiscountProductMappingAsync(new DiscountProductMapping { EntityId = product.Id, DiscountId = discount.Id });

                await _productService.UpdateProductAsync(product);
                await _productService.UpdateHasDiscountsAppliedAsync(product);
            }
        }

        return Ok(defaultMessage: true);
    }

    #endregion

    #region Applied to categories

    [HttpPost]
    public virtual async Task<IActionResult> CategoryList([FromBody] BaseQueryModel<DiscountCategorySearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageDiscounts))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //try to get a discount with the specified id
        var discount = await _discountService.GetDiscountByIdAsync(searchModel.DiscountId);
        if (discount == null)
            return NotFound("No discount found with the specified id");

        //prepare model
        var model = await _discountModelFactory.PrepareDiscountCategoryListModelAsync(searchModel, discount);

        return OkWrap(model);
    }

    [HttpGet("{discountId}/{categoryId}")]
    public virtual async Task<IActionResult> CategoryDelete(int discountId, int categoryId)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageDiscounts))
            return AdminApiAccessDenied();

        //try to get a discount with the specified id
        var discount = await _discountService.GetDiscountByIdAsync(discountId);
        if (discount == null)
            return NotFound("No discount found with the specified id");

        //try to get a category with the specified id
        var category = await _categoryService.GetCategoryByIdAsync(categoryId);
        if (category == null)
            return NotFound("No category found with the specified id");

        //remove discount
        if (await _categoryService.GetDiscountAppliedToCategoryAsync(category.Id, discount.Id) is DiscountCategoryMapping mapping)
            await _categoryService.DeleteDiscountCategoryMappingAsync(mapping);

        await _categoryService.UpdateCategoryAsync(category);

        return Ok(defaultMessage: true);
    }

    [HttpGet("{discountId}")]
    public virtual async Task<IActionResult> CategoryAddPopup(int discountId)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageDiscounts))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _discountModelFactory.PrepareAddCategoryToDiscountSearchModelAsync(new AddCategoryToDiscountSearchModel());

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> CategoryAddPopupList([FromBody] BaseQueryModel<AddCategoryToDiscountSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageDiscounts))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _discountModelFactory.PrepareAddCategoryToDiscountListModelAsync(searchModel);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> CategoryAddPopup([FromBody] BaseQueryModel<AddCategoryToDiscountModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageDiscounts))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get a discount with the specified id
        var discount = await _discountService.GetDiscountByIdAsync(model.DiscountId);
        if (discount == null)
            return NotFound("No discount found with the specified id");

        foreach (var id in model.SelectedCategoryIds)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null)
                continue;

            if (await _categoryService.GetDiscountAppliedToCategoryAsync(category.Id, discount.Id) is null)
                await _categoryService.InsertDiscountCategoryMappingAsync(new DiscountCategoryMapping { DiscountId = discount.Id, EntityId = category.Id });

            await _categoryService.UpdateCategoryAsync(category);
        }

        return Ok(defaultMessage: true);
    }

    #endregion

    #region Applied to manufacturers

    [HttpPost]
    public virtual async Task<IActionResult> ManufacturerList([FromBody] BaseQueryModel<DiscountManufacturerSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageDiscounts))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //try to get a discount with the specified id
        var discount = await _discountService.GetDiscountByIdAsync(searchModel.DiscountId);
        if (discount == null)
            return NotFound("No discount found with the specified id");

        //prepare model
        var model = await _discountModelFactory.PrepareDiscountManufacturerListModelAsync(searchModel, discount);

        return OkWrap(model);
    }

    [HttpGet("{discountId}/{manufacturerId}")]
    public virtual async Task<IActionResult> ManufacturerDelete(int discountId, int manufacturerId)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageDiscounts))
            return AdminApiAccessDenied();

        //try to get a discount with the specified id
        var discount = await _discountService.GetDiscountByIdAsync(discountId);
        if (discount == null)
            return NotFound("No discount found with the specified id");

        //try to get a manufacturer with the specified id
        var manufacturer = await _manufacturerService.GetManufacturerByIdAsync(manufacturerId);
        if (manufacturer == null)
            return NotFound("No manufacturer found with the specified id");

        //remove discount
        if (await _manufacturerService.GetDiscountAppliedToManufacturerAsync(manufacturer.Id, discount.Id) is DiscountManufacturerMapping discountManufacturerMapping)
            await _manufacturerService.DeleteDiscountManufacturerMappingAsync(discountManufacturerMapping);

        await _manufacturerService.UpdateManufacturerAsync(manufacturer);

        return Ok(defaultMessage: true);
    }

    [HttpGet("{discountId}")]
    public virtual async Task<IActionResult> ManufacturerAddPopup(int discountId)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageDiscounts))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _discountModelFactory.PrepareAddManufacturerToDiscountSearchModelAsync(new AddManufacturerToDiscountSearchModel());

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ManufacturerAddPopupList([FromBody] BaseQueryModel<AddManufacturerToDiscountSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageDiscounts))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _discountModelFactory.PrepareAddManufacturerToDiscountListModelAsync(searchModel);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ManufacturerAddPopup([FromBody] BaseQueryModel<AddManufacturerToDiscountModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageDiscounts))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get a discount with the specified id
        var discount = await _discountService.GetDiscountByIdAsync(model.DiscountId);
        if (discount == null)
            return NotFound("No discount found with the specified id");

        foreach (var id in model.SelectedManufacturerIds)
        {
            var manufacturer = await _manufacturerService.GetManufacturerByIdAsync(id);
            if (manufacturer == null)
                continue;

            if (await _manufacturerService.GetDiscountAppliedToManufacturerAsync(manufacturer.Id, discount.Id) is null)
                await _manufacturerService.InsertDiscountManufacturerMappingAsync(new DiscountManufacturerMapping { EntityId = manufacturer.Id, DiscountId = discount.Id });

            await _manufacturerService.UpdateManufacturerAsync(manufacturer);
        }

        return Ok(defaultMessage: true);
    }

    #endregion

    #region Discount usage history

    [HttpPost]
    public virtual async Task<IActionResult> UsageHistoryList([FromBody] BaseQueryModel<DiscountUsageHistorySearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageDiscounts))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //try to get a discount with the specified id
        var discount = await _discountService.GetDiscountByIdAsync(searchModel.DiscountId);
        if (discount == null)
            return NotFound("No discount found with the specified id");

        //prepare model
        var model = await _discountModelFactory.PrepareDiscountUsageHistoryListModelAsync(searchModel, discount);

        return OkWrap(model);
    }

    [HttpPost("{discountId}/{id}")]
    public virtual async Task<IActionResult> UsageHistoryDelete(int discountId, int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageDiscounts))
            return AdminApiAccessDenied();

        //try to get a discount with the specified id
        var discount = await _discountService.GetDiscountByIdAsync(discountId);
        if (discount == null)
            return NotFound("No discount found with the specified id");

        //try to get a discount usage history entry with the specified id
        var discountUsageHistoryEntry = await _discountService.GetDiscountUsageHistoryByIdAsync(id);
        if (discountUsageHistoryEntry == null)
            return NotFound("No discount usage history found with the specified id");

        await _discountService.DeleteDiscountUsageHistoryAsync(discountUsageHistoryEntry);

        return Ok(defaultMessage: true);
    }

    #endregion

    #endregion
}