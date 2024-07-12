using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Vendors;
using Nop.Core.Http;
using Nop.Core.Http.Extensions;
using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.ExportImport;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Mvc.ModelBinding;
using NopStation.Plugin.Misc.Core.Extensions;
using NopStation.Plugin.Misc.Core.Models.Api;

namespace NopStation.Plugin.Misc.AdminApi.Areas.Admin.Controllers.Api;

[Route("api/a/product/[action]")]
public partial class ProductApiController : BaseAdminApiController
{
    #region Fields

    private readonly IAclService _aclService;
    private readonly IBackInStockSubscriptionService _backInStockSubscriptionService;
    private readonly ICategoryService _categoryService;
    private readonly ICopyProductService _copyProductService;
    private readonly ICustomerActivityService _customerActivityService;
    private readonly ICustomerService _customerService;
    private readonly IDiscountService _discountService;
    private readonly IDownloadService _downloadService;
    private readonly IExportManager _exportManager;
    private readonly IGenericAttributeService _genericAttributeService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IImportManager _importManager;
    private readonly ILanguageService _languageService;
    private readonly ILocalizationService _localizationService;
    private readonly ILocalizedEntityService _localizedEntityService;
    private readonly IManufacturerService _manufacturerService;
    private readonly INopFileProvider _fileProvider;
    private readonly IPdfService _pdfService;
    private readonly IPermissionService _permissionService;
    private readonly IPictureService _pictureService;
    private readonly IProductAttributeParser _productAttributeParser;
    private readonly IProductAttributeService _productAttributeService;
    private readonly IProductAttributeFormatter _productAttributeFormatter;
    private readonly IProductModelFactory _productModelFactory;
    private readonly IProductService _productService;
    private readonly IProductTagService _productTagService;
    private readonly ISettingService _settingService;
    private readonly IShippingService _shippingService;
    private readonly IShoppingCartService _shoppingCartService;
    private readonly ISpecificationAttributeService _specificationAttributeService;
    private readonly IStoreContext _storeContext;
    private readonly IUrlRecordService _urlRecordService;
    private readonly IVideoService _videoService;
    private readonly IWebHelper _webHelper;
    private readonly IWorkContext _workContext;
    private readonly VendorSettings _vendorSettings;

    #endregion

    #region Ctor

    public ProductApiController(IAclService aclService,
        IBackInStockSubscriptionService backInStockSubscriptionService,
        ICategoryService categoryService,
        ICopyProductService copyProductService,
        ICustomerActivityService customerActivityService,
        ICustomerService customerService,
        IDiscountService discountService,
        IDownloadService downloadService,
        IExportManager exportManager,
        IImportManager importManager,
        ILanguageService languageService,
        ILocalizationService localizationService,
        ILocalizedEntityService localizedEntityService,
        IManufacturerService manufacturerService,
        INopFileProvider fileProvider,
        IPdfService pdfService,
        IPermissionService permissionService,
        IPictureService pictureService,
        IProductAttributeParser productAttributeParser,
        IProductAttributeService productAttributeService,
        IProductAttributeFormatter productAttributeFormatter,
        IProductModelFactory productModelFactory,
        IProductService productService,
        IProductTagService productTagService,
        ISettingService settingService,
        IShippingService shippingService,
        IShoppingCartService shoppingCartService,
        ISpecificationAttributeService specificationAttributeService,
        IStoreContext storeContext,
        IUrlRecordService urlRecordService,
        IWorkContext workContext,
        VendorSettings vendorSettings,
        IGenericAttributeService genericAttributeService,
        IHttpClientFactory httpClientFactory,
        IWebHelper webHelper,
        IVideoService videoService)
    {
        _aclService = aclService;
        _backInStockSubscriptionService = backInStockSubscriptionService;
        _categoryService = categoryService;
        _copyProductService = copyProductService;
        _customerActivityService = customerActivityService;
        _customerService = customerService;
        _discountService = discountService;
        _downloadService = downloadService;
        _exportManager = exportManager;
        _importManager = importManager;
        _languageService = languageService;
        _localizationService = localizationService;
        _localizedEntityService = localizedEntityService;
        _manufacturerService = manufacturerService;
        _fileProvider = fileProvider;
        _pdfService = pdfService;
        _permissionService = permissionService;
        _pictureService = pictureService;
        _productAttributeParser = productAttributeParser;
        _productAttributeService = productAttributeService;
        _productAttributeFormatter = productAttributeFormatter;
        _productModelFactory = productModelFactory;
        _productService = productService;
        _productTagService = productTagService;
        _settingService = settingService;
        _shippingService = shippingService;
        _shoppingCartService = shoppingCartService;
        _specificationAttributeService = specificationAttributeService;
        _storeContext = storeContext;
        _urlRecordService = urlRecordService;
        _workContext = workContext;
        _vendorSettings = vendorSettings;
        _genericAttributeService = genericAttributeService;
        _httpClientFactory = httpClientFactory;
        _webHelper = webHelper;
        _videoService = videoService;
    }

    #endregion

    #region Utilities

    protected virtual async Task UpdateLocalesAsync(Product product, ProductModel model)
    {
        foreach (var localized in model.Locales)
        {
            await _localizedEntityService.SaveLocalizedValueAsync(product,
                x => x.Name,
                localized.Name,
                localized.LanguageId);
            await _localizedEntityService.SaveLocalizedValueAsync(product,
                x => x.ShortDescription,
                localized.ShortDescription,
                localized.LanguageId);
            await _localizedEntityService.SaveLocalizedValueAsync(product,
                x => x.FullDescription,
                localized.FullDescription,
                localized.LanguageId);
            await _localizedEntityService.SaveLocalizedValueAsync(product,
                x => x.MetaKeywords,
                localized.MetaKeywords,
                localized.LanguageId);
            await _localizedEntityService.SaveLocalizedValueAsync(product,
                x => x.MetaDescription,
                localized.MetaDescription,
                localized.LanguageId);
            await _localizedEntityService.SaveLocalizedValueAsync(product,
                x => x.MetaTitle,
                localized.MetaTitle,
                localized.LanguageId);

            //search engine name
            var seName = await _urlRecordService.ValidateSeNameAsync(product, localized.SeName, localized.Name, false);
            await _urlRecordService.SaveSlugAsync(product, seName, localized.LanguageId);
        }
    }

    protected virtual async Task UpdateLocalesAsync(ProductTag productTag, ProductTagModel model)
    {
        foreach (var localized in model.Locales)
        {
            await _localizedEntityService.SaveLocalizedValueAsync(productTag,
                x => x.Name,
                localized.Name,
                localized.LanguageId);

            var seName = await _urlRecordService.ValidateSeNameAsync(productTag, string.Empty, localized.Name, false);
            await _urlRecordService.SaveSlugAsync(productTag, seName, localized.LanguageId);
        }
    }

    protected virtual async Task UpdateLocalesAsync(ProductAttributeMapping pam, ProductAttributeMappingModel model)
    {
        foreach (var localized in model.Locales)
        {
            await _localizedEntityService.SaveLocalizedValueAsync(pam,
                x => x.TextPrompt,
                localized.TextPrompt,
                localized.LanguageId);
            await _localizedEntityService.SaveLocalizedValueAsync(pam,
                x => x.DefaultValue,
                localized.DefaultValue,
                localized.LanguageId);
        }
    }

    protected virtual async Task UpdateLocalesAsync(ProductAttributeValue pav, ProductAttributeValueModel model)
    {
        foreach (var localized in model.Locales)
        {
            await _localizedEntityService.SaveLocalizedValueAsync(pav,
                x => x.Name,
                localized.Name,
                localized.LanguageId);
        }
    }

    protected virtual async Task UpdatePictureSeoNamesAsync(Product product)
    {
        foreach (var pp in await _productService.GetProductPicturesByProductIdAsync(product.Id))
            await _pictureService.SetSeoFilenameAsync(pp.PictureId, await _pictureService.GetPictureSeNameAsync(product.Name));
    }

    protected virtual async Task SaveProductAclAsync(Product product, ProductModel model)
    {
        product.SubjectToAcl = model.SelectedCustomerRoleIds.Any();
        await _productService.UpdateProductAsync(product);

        var existingAclRecords = await _aclService.GetAclRecordsAsync(product);
        var allCustomerRoles = await _customerService.GetAllCustomerRolesAsync(true);
        foreach (var customerRole in allCustomerRoles)
        {
            if (model.SelectedCustomerRoleIds.Contains(customerRole.Id))
            {
                //new role
                if (existingAclRecords.Count(acl => acl.CustomerRoleId == customerRole.Id) == 0)
                    await _aclService.InsertAclRecordAsync(product, customerRole.Id);
            }
            else
            {
                //remove role
                var aclRecordToDelete = existingAclRecords.FirstOrDefault(acl => acl.CustomerRoleId == customerRole.Id);
                if (aclRecordToDelete != null)
                    await _aclService.DeleteAclRecordAsync(aclRecordToDelete);
            }
        }
    }

    protected virtual async Task SaveCategoryMappingsAsync(Product product, ProductModel model)
    {
        var existingProductCategories = await _categoryService.GetProductCategoriesByProductIdAsync(product.Id, true);

        //delete categories
        foreach (var existingProductCategory in existingProductCategories)
            if (!model.SelectedCategoryIds.Contains(existingProductCategory.CategoryId))
                await _categoryService.DeleteProductCategoryAsync(existingProductCategory);

        //add categories
        foreach (var categoryId in model.SelectedCategoryIds)
        {
            if (_categoryService.FindProductCategory(existingProductCategories, product.Id, categoryId) == null)
            {
                //find next display order
                var displayOrder = 1;
                var existingCategoryMapping = await _categoryService.GetProductCategoriesByCategoryIdAsync(categoryId, showHidden: true);
                if (existingCategoryMapping.Any())
                    displayOrder = existingCategoryMapping.Max(x => x.DisplayOrder) + 1;
                await _categoryService.InsertProductCategoryAsync(new ProductCategory
                {
                    ProductId = product.Id,
                    CategoryId = categoryId,
                    DisplayOrder = displayOrder
                });
            }
        }
    }

    protected virtual async Task SaveManufacturerMappingsAsync(Product product, ProductModel model)
    {
        var existingProductManufacturers = await _manufacturerService.GetProductManufacturersByProductIdAsync(product.Id, true);

        //delete manufacturers
        foreach (var existingProductManufacturer in existingProductManufacturers)
            if (!model.SelectedManufacturerIds.Contains(existingProductManufacturer.ManufacturerId))
                await _manufacturerService.DeleteProductManufacturerAsync(existingProductManufacturer);

        //add manufacturers
        foreach (var manufacturerId in model.SelectedManufacturerIds)
        {
            if (_manufacturerService.FindProductManufacturer(existingProductManufacturers, product.Id, manufacturerId) == null)
            {
                //find next display order
                var displayOrder = 1;
                var existingManufacturerMapping = await _manufacturerService.GetProductManufacturersByManufacturerIdAsync(manufacturerId, showHidden: true);
                if (existingManufacturerMapping.Any())
                    displayOrder = existingManufacturerMapping.Max(x => x.DisplayOrder) + 1;
                await _manufacturerService.InsertProductManufacturerAsync(new ProductManufacturer
                {
                    ProductId = product.Id,
                    ManufacturerId = manufacturerId,
                    DisplayOrder = displayOrder
                });
            }
        }
    }

    protected virtual async Task SaveDiscountMappingsAsync(Product product, ProductModel model)
    {
        var allDiscounts = await _discountService.GetAllDiscountsAsync(DiscountType.AssignedToSkus, showHidden: true, isActive: null);

        foreach (var discount in allDiscounts)
        {
            if (model.SelectedDiscountIds != null && model.SelectedDiscountIds.Contains(discount.Id))
            {
                //new discount
                if (await _productService.GetDiscountAppliedToProductAsync(product.Id, discount.Id) is null)
                    await _productService.InsertDiscountProductMappingAsync(new DiscountProductMapping { EntityId = product.Id, DiscountId = discount.Id });
            }
            else
            {
                //remove discount
                if (await _productService.GetDiscountAppliedToProductAsync(product.Id, discount.Id) is DiscountProductMapping discountProductMapping)
                    await _productService.DeleteDiscountProductMappingAsync(discountProductMapping);
            }
        }

        await _productService.UpdateProductAsync(product);
        await _productService.UpdateHasDiscountsAppliedAsync(product);
    }

    protected virtual async Task<string> GetAttributesXmlForProductAttributeCombinationAsync(NameValueCollection form, List<string> warnings, int productId)
    {
        var attributesXml = string.Empty;

        //get product attribute mappings (exclude non-combinable attributes)
        var attributes = (await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(productId))
            .Where(productAttributeMapping => !productAttributeMapping.IsNonCombinable()).ToList();

        foreach (var attribute in attributes)
        {
            var controlId = $"{NopCatalogDefaults.ProductAttributePrefix}{attribute.Id}";
            StringValues ctrlAttributes;

            switch (attribute.AttributeControlType)
            {
                case AttributeControlType.DropdownList:
                case AttributeControlType.RadioList:
                case AttributeControlType.ColorSquares:
                case AttributeControlType.ImageSquares:
                    ctrlAttributes = form[controlId];
                    if (!string.IsNullOrEmpty(ctrlAttributes))
                    {
                        var selectedAttributeId = int.Parse(ctrlAttributes);
                        if (selectedAttributeId > 0)
                            attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                attribute, selectedAttributeId.ToString());
                    }

                    break;
                case AttributeControlType.Checkboxes:
                    var cblAttributes = form[controlId].ToString();
                    if (!string.IsNullOrEmpty(cblAttributes))
                    {
                        foreach (var item in cblAttributes.Split(new[] { ',' },
                            StringSplitOptions.RemoveEmptyEntries))
                        {
                            var selectedAttributeId = int.Parse(item);
                            if (selectedAttributeId > 0)
                                attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                    attribute, selectedAttributeId.ToString());
                        }
                    }

                    break;
                case AttributeControlType.ReadonlyCheckboxes:
                    //load read-only (already server-side selected) values
                    var attributeValues = await _productAttributeService.GetProductAttributeValuesAsync(attribute.Id);
                    foreach (var selectedAttributeId in attributeValues
                        .Where(v => v.IsPreSelected)
                        .Select(v => v.Id)
                        .ToList())
                    {
                        attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                            attribute, selectedAttributeId.ToString());
                    }

                    break;
                case AttributeControlType.TextBox:
                case AttributeControlType.MultilineTextbox:
                    ctrlAttributes = form[controlId];
                    if (!string.IsNullOrEmpty(ctrlAttributes))
                    {
                        var enteredText = ctrlAttributes.ToString().Trim();
                        attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                            attribute, enteredText);
                    }

                    break;
                case AttributeControlType.Datepicker:
                    var date = form[controlId + "_day"];
                    var month = form[controlId + "_month"];
                    var year = form[controlId + "_year"];
                    DateTime? selectedDate = null;
                    try
                    {
                        selectedDate = new DateTime(int.Parse(year), int.Parse(month), int.Parse(date));
                    }
                    catch
                    {
                        //ignore any exception
                    }

                    if (selectedDate.HasValue)
                    {
                        attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                            attribute, selectedDate.Value.ToString("D"));
                    }

                    break;
                case AttributeControlType.FileUpload:
                    var httpPostedFile = Request.Form.Files[controlId];
                    if (!string.IsNullOrEmpty(httpPostedFile?.FileName))
                    {
                        var fileSizeOk = true;
                        if (attribute.ValidationFileMaximumSize.HasValue)
                        {
                            //compare in bytes
                            var maxFileSizeBytes = attribute.ValidationFileMaximumSize.Value * 1024;
                            if (httpPostedFile.Length > maxFileSizeBytes)
                            {
                                warnings.Add(string.Format(
                                    await _localizationService.GetResourceAsync("ShoppingCart.MaximumUploadedFileSize"),
                                    attribute.ValidationFileMaximumSize.Value));
                                fileSizeOk = false;
                            }
                        }

                        if (fileSizeOk)
                        {
                            //save an uploaded file
                            var download = new Download
                            {
                                DownloadGuid = Guid.NewGuid(),
                                UseDownloadUrl = false,
                                DownloadUrl = string.Empty,
                                DownloadBinary = await _downloadService.GetDownloadBitsAsync(httpPostedFile),
                                ContentType = httpPostedFile.ContentType,
                                Filename = _fileProvider.GetFileNameWithoutExtension(httpPostedFile.FileName),
                                Extension = _fileProvider.GetFileExtension(httpPostedFile.FileName),
                                IsNew = true
                            };
                            await _downloadService.InsertDownloadAsync(download);

                            //save attribute
                            attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                attribute, download.DownloadGuid.ToString());
                        }
                    }

                    break;
                default:
                    break;
            }
        }

        //validate conditional attributes (if specified)
        foreach (var attribute in attributes)
        {
            var conditionMet = await _productAttributeParser.IsConditionMetAsync(attribute, attributesXml);
            if (conditionMet.HasValue && !conditionMet.Value)
            {
                attributesXml = _productAttributeParser.RemoveProductAttribute(attributesXml, attribute);
            }
        }

        return attributesXml;
    }

    protected virtual string[] ParseProductTags(string productTags)
    {
        var result = new List<string>();
        if (string.IsNullOrWhiteSpace(productTags))
            return result.ToArray();

        var values = productTags.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var val in values)
            if (!string.IsNullOrEmpty(val.Trim()))
                result.Add(val.Trim());

        return result.ToArray();
    }

    protected virtual async Task SaveProductWarehouseInventoryAsync(Product product, ProductModel model, NameValueCollection formData)
    {
        if (product == null)
            throw new ArgumentNullException(nameof(product));

        if (model.ManageInventoryMethodId != (int)ManageInventoryMethod.ManageStock)
            return;

        if (!model.UseMultipleWarehouses)
            return;

        var warehouses = await _shippingService.GetAllWarehousesAsync();

        foreach (var warehouse in warehouses)
        {
            //parse stock quantity
            var stockQuantity = 0;
            foreach (string formKey in formData.Keys)
            {
                if (!formKey.Equals($"warehouse_qty_{warehouse.Id}", StringComparison.InvariantCultureIgnoreCase))
                    continue;

                _ = int.TryParse(formData[formKey], out stockQuantity);
                break;
            }

            //parse reserved quantity
            var reservedQuantity = 0;
            foreach (string formKey in formData.Keys)
                if (formKey.Equals($"warehouse_reserved_{warehouse.Id}", StringComparison.InvariantCultureIgnoreCase))
                {
                    _ = int.TryParse(formData[formKey], out reservedQuantity);
                    break;
                }

            //parse "used" field
            var used = false;
            foreach (string formKey in formData.Keys)
                if (formKey.Equals($"warehouse_used_{warehouse.Id}", StringComparison.InvariantCultureIgnoreCase))
                {
                    _ = int.TryParse(formData[formKey], out var tmp);
                    used = tmp == warehouse.Id;
                    break;
                }

            //quantity change history message
            var message = $"{await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.MultipleWarehouses")} {await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.Edit")}";

            var existingPwI = (await _productService.GetAllProductWarehouseInventoryRecordsAsync(product.Id)).FirstOrDefault(x => x.WarehouseId == warehouse.Id);
            if (existingPwI != null)
            {
                if (used)
                {
                    var previousStockQuantity = existingPwI.StockQuantity;

                    //update existing record
                    existingPwI.StockQuantity = stockQuantity;
                    existingPwI.ReservedQuantity = reservedQuantity;
                    await _productService.UpdateProductWarehouseInventoryAsync(existingPwI);

                    //quantity change history
                    await _productService.AddStockQuantityHistoryEntryAsync(product, existingPwI.StockQuantity - previousStockQuantity, existingPwI.StockQuantity,
                        existingPwI.WarehouseId, message);
                }
                else
                {
                    //delete. no need to store record for qty 0
                    await _productService.DeleteProductWarehouseInventoryAsync(existingPwI);

                    //quantity change history
                    await _productService.AddStockQuantityHistoryEntryAsync(product, -existingPwI.StockQuantity, 0, existingPwI.WarehouseId, message);
                }
            }
            else
            {
                if (!used)
                    continue;

                //no need to insert a record for qty 0
                existingPwI = new ProductWarehouseInventory
                {
                    WarehouseId = warehouse.Id,
                    ProductId = product.Id,
                    StockQuantity = stockQuantity,
                    ReservedQuantity = reservedQuantity
                };

                await _productService.InsertProductWarehouseInventoryAsync(existingPwI);

                //quantity change history
                await _productService.AddStockQuantityHistoryEntryAsync(product, existingPwI.StockQuantity, existingPwI.StockQuantity,
                    existingPwI.WarehouseId, message);
            }
        }
    }

    protected virtual async Task SaveConditionAttributesAsync(ProductAttributeMapping productAttributeMapping,
        ProductAttributeConditionModel model, NameValueCollection form)
    {
        string attributesXml = null;
        if (model.EnableCondition)
        {
            var attribute = await _productAttributeService.GetProductAttributeMappingByIdAsync(model.SelectedProductAttributeId);
            if (attribute != null)
            {
                var controlId = $"{NopCatalogDefaults.ProductAttributePrefix}{attribute.Id}";
                switch (attribute.AttributeControlType)
                {
                    case AttributeControlType.DropdownList:
                    case AttributeControlType.RadioList:
                    case AttributeControlType.ColorSquares:
                    case AttributeControlType.ImageSquares:
                        var ctrlAttributes = form[controlId];
                        if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                        {
                            var selectedAttributeId = int.Parse(ctrlAttributes);
                            //for conditions we should empty values save even when nothing is selected
                            //otherwise "attributesXml" will be empty
                            //hence we won't be able to find a selected attribute
                            attributesXml = _productAttributeParser.AddProductAttribute(null, attribute,
                                selectedAttributeId > 0 ? selectedAttributeId.ToString() : string.Empty);
                        }
                        else
                        {
                            //for conditions we should empty values save even when nothing is selected
                            //otherwise "attributesXml" will be empty
                            //hence we won't be able to find a selected attribute
                            attributesXml = _productAttributeParser.AddProductAttribute(null,
                                attribute, string.Empty);
                        }

                        break;
                    case AttributeControlType.Checkboxes:
                        var cblAttributes = form[controlId];
                        if (!StringValues.IsNullOrEmpty(cblAttributes))
                        {
                            var anyValueSelected = false;
                            foreach (var item in cblAttributes.ToString()
                                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                            {
                                var selectedAttributeId = int.Parse(item);
                                if (selectedAttributeId <= 0)
                                    continue;

                                attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                    attribute, selectedAttributeId.ToString());
                                anyValueSelected = true;
                            }

                            if (!anyValueSelected)
                            {
                                //for conditions we should save empty values even when nothing is selected
                                //otherwise "attributesXml" will be empty
                                //hence we won't be able to find a selected attribute
                                attributesXml = _productAttributeParser.AddProductAttribute(null,
                                    attribute, string.Empty);
                            }
                        }
                        else
                        {
                            //for conditions we should save empty values even when nothing is selected
                            //otherwise "attributesXml" will be empty
                            //hence we won't be able to find a selected attribute
                            attributesXml = _productAttributeParser.AddProductAttribute(null,
                                attribute, string.Empty);
                        }

                        break;
                    case AttributeControlType.ReadonlyCheckboxes:
                    case AttributeControlType.TextBox:
                    case AttributeControlType.MultilineTextbox:
                    case AttributeControlType.Datepicker:
                    case AttributeControlType.FileUpload:
                    default:
                        //these attribute types are supported as conditions
                        break;
                }
            }
        }

        productAttributeMapping.ConditionAttributeXml = attributesXml;
        await _productAttributeService.UpdateProductAttributeMappingAsync(productAttributeMapping);
    }

    protected virtual async Task GenerateAttributeCombinationsAsync(Product product, IList<int> allowedAttributeIds = null)
    {
        var allAttributesXml = await _productAttributeParser.GenerateAllCombinationsAsync(product, true, allowedAttributeIds);
        foreach (var attributesXml in allAttributesXml)
        {
            var existingCombination = await _productAttributeParser.FindProductAttributeCombinationAsync(product, attributesXml);

            //already exists?
            if (existingCombination != null)
                continue;

            //new one
            var warnings = new List<string>();
            warnings.AddRange(await _shoppingCartService.GetShoppingCartItemAttributeWarningsAsync(await _workContext.GetCurrentCustomerAsync(),
                ShoppingCartType.ShoppingCart, product, 1, attributesXml, true, true));
            if (warnings.Count != 0)
                continue;

            //save combination
            var combination = new ProductAttributeCombination
            {
                ProductId = product.Id,
                AttributesXml = attributesXml,
                StockQuantity = 0,
                AllowOutOfStockOrders = false,
                Sku = null,
                ManufacturerPartNumber = null,
                Gtin = null,
                OverriddenPrice = null,
                NotifyAdminForQuantityBelow = 1
            };
            await _productAttributeService.InsertProductAttributeCombinationAsync(combination);
        }
    }

    protected virtual async Task PingVideoUrlAsync(string videoUrl)
    {
        var path = videoUrl.StartsWith("/") ? $"{_webHelper.GetStoreLocation()}{videoUrl.TrimStart('/')}" : videoUrl;

        var client = _httpClientFactory.CreateClient(NopHttpDefaults.DefaultHttpClient);
        await client.GetStringAsync(path);
    }

    #endregion

    #region Methods

    #region Product list / create / edit / delete

    public virtual async Task<IActionResult> List()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _productModelFactory.PrepareProductSearchModelAsync(new ProductSearchModel());

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ProductList([FromBody] BaseQueryModel<ProductSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _productModelFactory.PrepareProductListModelAsync(searchModel);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> GoToSku([FromBody] BaseQueryModel<ProductSearchModel> queryModel)
    {
        var searchModel = queryModel.Data;
        //try to load a product entity, if not found, then try to load a product attribute combination
        var productId = (await _productService.GetProductBySkuAsync(searchModel.GoDirectlyToSku))?.Id
            ?? (await _productAttributeService.GetProductAttributeCombinationBySkuAsync(searchModel.GoDirectlyToSku))?.ProductId;

        if (productId != null)
            return await Edit(productId.Value);

        //not found
        return BadRequest();
    }

    public virtual async Task<IActionResult> Create()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AdminApiAccessDenied();

        //validate maximum number of products per vendor
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (_vendorSettings.MaximumProductNumber > 0 && currentVendor != null
            && await _productService.GetNumberOfProductsByVendorIdAsync(currentVendor.Id) >= _vendorSettings.MaximumProductNumber)
        {
            return BadRequest(string.Format(await _localizationService.GetResourceAsync("Admin.Catalog.Products.ExceededMaximumNumber"),
                _vendorSettings.MaximumProductNumber));
        }

        //prepare model
        var model = await _productModelFactory.PrepareProductModelAsync(new ProductModel(), null);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Create([FromBody] BaseQueryModel<ProductModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //validate maximum number of products per vendor
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (_vendorSettings.MaximumProductNumber > 0 && currentVendor != null
            && await _productService.GetNumberOfProductsByVendorIdAsync(currentVendor.Id) >= _vendorSettings.MaximumProductNumber)
        {
            return BadRequest(string.Format(await _localizationService.GetResourceAsync("Admin.Catalog.Products.ExceededMaximumNumber"),
                _vendorSettings.MaximumProductNumber));
        }

        if (ModelState.IsValid)
        {
            //validate maximum number of products per vendor
            if (currentVendor != null)
                model.VendorId = currentVendor.Id;

            //vendors cannot edit "Show on home page" property
            if (currentVendor != null && model.ShowOnHomepage)
                model.ShowOnHomepage = false;

            //product
            var product = model.ToEntity<Product>();
            product.CreatedOnUtc = DateTime.UtcNow;
            product.UpdatedOnUtc = DateTime.UtcNow;
            await _productService.InsertProductAsync(product);

            //search engine name
            model.SeName = await _urlRecordService.ValidateSeNameAsync(product, model.SeName, product.Name, true);
            await _urlRecordService.SaveSlugAsync(product, model.SeName, 0);

            //locales
            await UpdateLocalesAsync(product, model);

            //categories
            await SaveCategoryMappingsAsync(product, model);

            //manufacturers
            await SaveManufacturerMappingsAsync(product, model);

            //ACL (customer roles)
            await SaveProductAclAsync(product, model);

            //stores
            await _productService.UpdateProductStoreMappingsAsync(product, model.SelectedStoreIds);

            //discounts
            await SaveDiscountMappingsAsync(product, model);

            //tags
            await _productTagService.UpdateProductTagsAsync(product, model.SelectedProductTags.ToArray());

            var form = queryModel.FormValues.ToNameValueCollection();

            //warehouses
            await SaveProductWarehouseInventoryAsync(product, model, form);

            //quantity change history
            await _productService.AddStockQuantityHistoryEntryAsync(product, product.StockQuantity, product.StockQuantity, product.WarehouseId,
                await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.Edit"));

            //activity log
            await _customerActivityService.InsertActivityAsync("AddNewProduct",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.AddNewProduct"), product.Name), product);

            return Created(product.Id, await _localizationService.GetResourceAsync("Admin.Catalog.Products.Added"));
        }

        //prepare model
        model = await _productModelFactory.PrepareProductModelAsync(model, null, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    [HttpGet("{id}")]
    public virtual async Task<IActionResult> Edit(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AdminApiAccessDenied();

        //try to get a product with the specified id
        var product = await _productService.GetProductByIdAsync(id);
        if (product == null || product.Deleted)
            return NotFound("No product found with the specified id");

        //validate maximum number of products per vendor
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null && product.VendorId != currentVendor.Id)
            return AdminApiAccessDenied();

        //prepare model
        var model = await _productModelFactory.PrepareProductModelAsync(null, product);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Edit([FromBody] BaseQueryModel<ProductModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get a product with the specified id
        var product = await _productService.GetProductByIdAsync(model.Id);
        if (product == null || product.Deleted)
            return NotFound("No product found with the specified id");

        //validate maximum number of products per vendor
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null && product.VendorId != currentVendor.Id)
            return AdminApiAccessDenied();

        //check if the product quantity has been changed while we were editing the product
        //and if it has been changed then we show error notification
        //and redirect on the editing page without data saving
        if (product.StockQuantity != model.LastStockQuantity)
        {
            return BadRequest(await _localizationService.GetResourceAsync("Admin.Catalog.Products.Fields.StockQuantity.ChangedWarning"));
        }

        if (ModelState.IsValid)
        {
            //validate maximum number of products per vendor
            if (currentVendor != null)
                model.VendorId = currentVendor.Id;

            //we do not validate maximum number of products per vendor when editing existing products (only during creation of new products)
            //vendors cannot edit "Show on home page" property
            if (currentVendor != null && model.ShowOnHomepage != product.ShowOnHomepage)
                model.ShowOnHomepage = product.ShowOnHomepage;

            //some previously used values
            var prevTotalStockQuantity = await _productService.GetTotalStockQuantityAsync(product);
            var prevDownloadId = product.DownloadId;
            var prevSampleDownloadId = product.SampleDownloadId;
            var previousStockQuantity = product.StockQuantity;
            var previousWarehouseId = product.WarehouseId;
            var previousProductType = product.ProductType;

            //product
            product = model.ToEntity(product);

            product.UpdatedOnUtc = DateTime.UtcNow;
            await _productService.UpdateProductAsync(product);

            //remove associated products
            if (previousProductType == ProductType.GroupedProduct && product.ProductType == ProductType.SimpleProduct)
            {
                var store = await _storeContext.GetCurrentStoreAsync();
                var storeId = store?.Id ?? 0;
                var vendorId = currentVendor?.Id ?? 0;

                var associatedProducts = await _productService.GetAssociatedProductsAsync(product.Id, storeId, vendorId);
                foreach (var associatedProduct in associatedProducts)
                {
                    associatedProduct.ParentGroupedProductId = 0;
                    await _productService.UpdateProductAsync(associatedProduct);
                }
            }

            //search engine name
            model.SeName = await _urlRecordService.ValidateSeNameAsync(product, model.SeName, product.Name, true);
            await _urlRecordService.SaveSlugAsync(product, model.SeName, 0);

            //locales
            await UpdateLocalesAsync(product, model);

            //tags
            await _productTagService.UpdateProductTagsAsync(product,model.SelectedProductTags.ToArray());

            var form = queryModel.FormValues.ToNameValueCollection();

            //warehouses
            await SaveProductWarehouseInventoryAsync(product, model, form);

            //categories
            await SaveCategoryMappingsAsync(product, model);

            //manufacturers
            await SaveManufacturerMappingsAsync(product, model);

            //ACL (customer roles)
            await SaveProductAclAsync(product, model);

            //stores
            await _productService.UpdateProductStoreMappingsAsync(product, model.SelectedStoreIds);

            //discounts
            await SaveDiscountMappingsAsync(product, model);

            //picture seo names
            await UpdatePictureSeoNamesAsync(product);

            //back in stock notifications
            if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock &&
                product.BackorderMode == BackorderMode.NoBackorders &&
                product.AllowBackInStockSubscriptions &&
                await _productService.GetTotalStockQuantityAsync(product) > 0 &&
                prevTotalStockQuantity <= 0 &&
                product.Published &&
                !product.Deleted)
            {
                await _backInStockSubscriptionService.SendNotificationsToSubscribersAsync(product);
            }

            //delete an old "download" file (if deleted or updated)
            if (prevDownloadId > 0 && prevDownloadId != product.DownloadId)
            {
                var prevDownload = await _downloadService.GetDownloadByIdAsync(prevDownloadId);
                if (prevDownload != null)
                    await _downloadService.DeleteDownloadAsync(prevDownload);
            }

            //delete an old "sample download" file (if deleted or updated)
            if (prevSampleDownloadId > 0 && prevSampleDownloadId != product.SampleDownloadId)
            {
                var prevSampleDownload = await _downloadService.GetDownloadByIdAsync(prevSampleDownloadId);
                if (prevSampleDownload != null)
                    await _downloadService.DeleteDownloadAsync(prevSampleDownload);
            }

            //quantity change history
            if (previousWarehouseId != product.WarehouseId)
            {
                //warehouse is changed 
                //compose a message
                var oldWarehouseMessage = string.Empty;
                if (previousWarehouseId > 0)
                {
                    var oldWarehouse = await _shippingService.GetWarehouseByIdAsync(previousWarehouseId);
                    if (oldWarehouse != null)
                        oldWarehouseMessage = string.Format(await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.EditWarehouse.Old"), oldWarehouse.Name);
                }

                var newWarehouseMessage = string.Empty;
                if (product.WarehouseId > 0)
                {
                    var newWarehouse = await _shippingService.GetWarehouseByIdAsync(product.WarehouseId);
                    if (newWarehouse != null)
                        newWarehouseMessage = string.Format(await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.EditWarehouse.New"), newWarehouse.Name);
                }

                var message = string.Format(await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.EditWarehouse"), oldWarehouseMessage, newWarehouseMessage);

                //record history
                await _productService.AddStockQuantityHistoryEntryAsync(product, -previousStockQuantity, 0, previousWarehouseId, message);
                await _productService.AddStockQuantityHistoryEntryAsync(product, product.StockQuantity, product.StockQuantity, product.WarehouseId, message);
            }
            else
            {
                await _productService.AddStockQuantityHistoryEntryAsync(product, product.StockQuantity - previousStockQuantity, product.StockQuantity,
                    product.WarehouseId, await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.Edit"));
            }

            //activity log
            await _customerActivityService.InsertActivityAsync("EditProduct",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditProduct"), product.Name), product);

            return Ok(await _localizationService.GetResourceAsync("Admin.Catalog.Products.Updated"));
        }

        //prepare model
        model = await _productModelFactory.PrepareProductModelAsync(model, product, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> Delete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AdminApiAccessDenied();

        //try to get a product with the specified id
        var product = await _productService.GetProductByIdAsync(id);
        if (product == null)
            return NotFound("No product found with the specified id");

        //validate maximum number of products per vendor
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null && product.VendorId != currentVendor.Id)
            return AdminApiAccessDenied();

        await _productService.DeleteProductAsync(product);

        //activity log
        await _customerActivityService.InsertActivityAsync("DeleteProduct",
            string.Format(await _localizationService.GetResourceAsync("ActivityLog.DeleteProduct"), product.Name), product);

        return Ok(await _localizationService.GetResourceAsync("Admin.Catalog.Products.Deleted"));
    }

    [HttpPost]
    public virtual async Task<IActionResult> DeleteSelected([FromBody] BaseQueryModel<ICollection<int>> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AdminApiAccessDenied();

        var selectedIds = queryModel.Data;
        if (selectedIds == null || selectedIds.Count == 0)
            return NotFound();

        var currentVendor = await _workContext.GetCurrentVendorAsync();
        await _productService.DeleteProductsAsync((await _productService.GetProductsByIdsAsync(selectedIds.ToArray()))
            .Where(p => currentVendor == null || p.VendorId == currentVendor.Id).ToList());

        return Ok(defaultMessage: true);
    }

    [HttpPost]
    public virtual async Task<IActionResult> CopyProduct([FromBody] BaseQueryModel<ProductModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        var copyModel = model.CopyProductModel;
        try
        {
            var originalProduct = await _productService.GetProductByIdAsync(copyModel.Id);

            //validate maximum number of products per vendor
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null && originalProduct.VendorId != currentVendor.Id)
                return AdminApiAccessDenied();

            var newProduct = await _copyProductService.CopyProductAsync(originalProduct, copyModel.Name, copyModel.Published, copyModel.CopyMultimedia);

            return Ok(await _localizationService.GetResourceAsync("Admin.Catalog.Products.Copied"));
        }
        catch (Exception exc)
        {
            return InternalServerError(exc.Message);
        }
    }

    [HttpGet("{productId}/{sku}")]
    //action displaying notification (warning) to a store owner that entered SKU already exists
    public virtual async Task<IActionResult> SkuReservedWarning(int productId, string sku)
    {
        string message;

        //check whether product with passed SKU already exists
        var productBySku = await _productService.GetProductBySkuAsync(sku);
        if (productBySku != null)
        {
            if (productBySku.Id == productId)
                return Ok(defaultMessage: true);

            return Ok(string.Format(await _localizationService.GetResourceAsync("Admin.Catalog.Products.Fields.Sku.Reserved"), productBySku.Name));
        }

        //check whether combination with passed SKU already exists
        var combinationBySku = await _productAttributeService.GetProductAttributeCombinationBySkuAsync(sku);
        if (combinationBySku == null)
            return Ok(defaultMessage: true);

        message = string.Format(await _localizationService.GetResourceAsync("Admin.Catalog.Products.ProductAttributes.AttributeCombinations.Fields.Sku.Reserved"),
            (await _productService.GetProductByIdAsync(combinationBySku.ProductId))?.Name);

        return Ok(message);
    }

    #endregion

    #region Required products

    [HttpPost("{productIds}")]
    public virtual async Task<IActionResult> LoadProductFriendlyNames(string productIds)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AdminApiAccessDenied();

        if (string.IsNullOrWhiteSpace(productIds))
            return Ok(defaultMessage: true);

        var ids = new List<int>();
        var rangeArray = productIds
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .ToList();

        foreach (var str1 in rangeArray)
        {
            if (int.TryParse(str1, out var tmp1))
                ids.Add(tmp1);
        }

        var products = await _productService.GetProductsByIdsAsync(ids.ToArray());
        var result = string.Join(", ", products.Select(p => p.Name));

        return Ok(result);
    }

    public virtual async Task<IActionResult> RequiredProductAddPopup()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _productModelFactory.PrepareAddRequiredProductSearchModelAsync(new AddRequiredProductSearchModel());

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> RequiredProductAddPopupList([FromBody] BaseQueryModel<AddRequiredProductSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _productModelFactory.PrepareAddRequiredProductListModelAsync(searchModel);

        return OkWrap(model);
    }

    #endregion

    #region Related products

    [HttpPost]
    public virtual async Task<IActionResult> RelatedProductList([FromBody] BaseQueryModel<RelatedProductSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //try to get a product with the specified id
        var product = await _productService.GetProductByIdAsync(searchModel.ProductId);
        if (product == null || product.Deleted)
            return NotFound("No product found with the specified id");

        //validate maximum number of products per vendor
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null && product.VendorId != currentVendor.Id)
            return AdminApiAccessDenied();

        //prepare model
        var model = await _productModelFactory.PrepareRelatedProductListModelAsync(searchModel, product);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> RelatedProductUpdate([FromBody] BaseQueryModel<RelatedProductModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get a related product with the specified id
        var relatedProduct = await _productService.GetRelatedProductByIdAsync(model.Id);
        if (relatedProduct == null)
            return NotFound("No related product found with the specified id");

        //validate maximum number of products per vendor
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null)
        {
            var product = await _productService.GetProductByIdAsync(relatedProduct.ProductId1);
            if (product != null && product.VendorId != currentVendor.Id)
                return AdminApiAccessDenied();
        }

        relatedProduct.DisplayOrder = model.DisplayOrder;
        await _productService.UpdateRelatedProductAsync(relatedProduct);

        return Ok(defaultMessage: true);
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> RelatedProductDelete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AdminApiAccessDenied();

        //try to get a related product with the specified id
        var relatedProduct = await _productService.GetRelatedProductByIdAsync(id);
        if (relatedProduct == null)
            return NotFound("No related product found with the specified id");

        var productId = relatedProduct.ProductId1;

        //validate maximum number of products per vendor
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product != null && product.VendorId != currentVendor.Id)
                return AdminApiAccessDenied();
        }

        await _productService.DeleteRelatedProductAsync(relatedProduct);

        return Ok(defaultMessage: true);
    }

    [HttpGet("{productId}")]
    public virtual async Task<IActionResult> RelatedProductAddPopup(int productId)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _productModelFactory.PrepareAddRelatedProductSearchModelAsync(new AddRelatedProductSearchModel());

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> RelatedProductAddPopupList([FromBody] BaseQueryModel<AddRelatedProductSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _productModelFactory.PrepareAddRelatedProductListModelAsync(searchModel);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> RelatedProductAddPopup([FromBody] BaseQueryModel<AddRelatedProductModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        var selectedProducts = await _productService.GetProductsByIdsAsync(model.SelectedProductIds.ToArray());
        if (selectedProducts.Any())
        {
            var existingRelatedProducts = await _productService.GetRelatedProductsByProductId1Async(model.ProductId, showHidden: true);
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            foreach (var product in selectedProducts)
            {
                //validate maximum number of products per vendor
                if (currentVendor != null && product.VendorId != currentVendor.Id)
                    continue;

                if (_productService.FindRelatedProduct(existingRelatedProducts, model.ProductId, product.Id) != null)
                    continue;

                await _productService.InsertRelatedProductAsync(new RelatedProduct
                {
                    ProductId1 = model.ProductId,
                    ProductId2 = product.Id,
                    DisplayOrder = 1
                });
            }
        }

        return Ok(defaultMessage: true);
    }

    #endregion

    #region Cross-sell products

    [HttpPost]
    public virtual async Task<IActionResult> CrossSellProductList([FromBody] BaseQueryModel<CrossSellProductSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //try to get a product with the specified id
        var product = await _productService.GetProductByIdAsync(searchModel.ProductId);
        if (product == null)
            return NotFound("No product found with the specified id");

        //validate maximum number of products per vendor
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null && product.VendorId != currentVendor.Id)
            return AdminApiAccessDenied();

        //prepare model
        var model = await _productModelFactory.PrepareCrossSellProductListModelAsync(searchModel, product);

        return OkWrap(model);
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> CrossSellProductDelete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AdminApiAccessDenied();

        //try to get a cross-sell product with the specified id
        var crossSellProduct = await _productService.GetCrossSellProductByIdAsync(id);
        if (crossSellProduct == null)
            return NotFound("No cross-sell product found with the specified id");

        //validate maximum number of products per vendor
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null)
        {
            var product = await _productService.GetProductByIdAsync(crossSellProduct.ProductId1);
            if (product != null && product.VendorId != currentVendor.Id)
                return AdminApiAccessDenied();
        }

        await _productService.DeleteCrossSellProductAsync(crossSellProduct);

        return Ok(defaultMessage: true);
    }

    [HttpGet("{productId}")]
    public virtual async Task<IActionResult> CrossSellProductAddPopup(int productId)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _productModelFactory.PrepareAddCrossSellProductSearchModelAsync(new AddCrossSellProductSearchModel());

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> CrossSellProductAddPopupList([FromBody] BaseQueryModel<AddCrossSellProductSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _productModelFactory.PrepareAddCrossSellProductListModelAsync(searchModel);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> CrossSellProductAddPopup([FromBody] BaseQueryModel<AddCrossSellProductModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        var selectedProducts = await _productService.GetProductsByIdsAsync(model.SelectedProductIds.ToArray());
        if (selectedProducts.Any())
        {
            var existingCrossSellProducts = await _productService.GetCrossSellProductsByProductId1Async(model.ProductId, showHidden: true);
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            foreach (var product in selectedProducts)
            {
                //validate maximum number of products per vendor
                if (currentVendor != null && product.VendorId != currentVendor.Id)
                    continue;

                if (_productService.FindCrossSellProduct(existingCrossSellProducts, model.ProductId, product.Id) != null)
                    continue;

                await _productService.InsertCrossSellProductAsync(new CrossSellProduct
                {
                    ProductId1 = model.ProductId,
                    ProductId2 = product.Id
                });
            }
        }

        return Ok(defaultMessage: true);
    }

    #endregion

    #region Associated products

    [HttpPost]
    public virtual async Task<IActionResult> AssociatedProductList([FromBody] BaseQueryModel<AssociatedProductSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //try to get a product with the specified id
        var product = await _productService.GetProductByIdAsync(searchModel.ProductId);
        if (product == null)
            return NotFound("No related product found with the specified id");

        //validate maximum number of products per vendor
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null && product.VendorId != currentVendor.Id)
            return AdminApiAccessDenied();

        //prepare model
        var model = await _productModelFactory.PrepareAssociatedProductListModelAsync(searchModel, product);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> AssociatedProductUpdate([FromBody] BaseQueryModel<AssociatedProductModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get an associated product with the specified id
        var associatedProduct = await _productService.GetProductByIdAsync(model.Id);
        if (associatedProduct == null)
            return NotFound("No associated product found with the specified id");

        //validate maximum number of products per vendor
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null && associatedProduct.VendorId != currentVendor.Id)
            return AdminApiAccessDenied();

        associatedProduct.DisplayOrder = model.DisplayOrder;
        await _productService.UpdateProductAsync(associatedProduct);

        return Ok(defaultMessage: true);
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> AssociatedProductDelete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AdminApiAccessDenied();

        //try to get an associated product with the specified id
        var product = await _productService.GetProductByIdAsync(id);
        if (product == null)
            return NotFound("No associated product found with the specified id");

        //validate maximum number of products per vendor
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null && product.VendorId != currentVendor.Id)
            return AdminApiAccessDenied();

        product.ParentGroupedProductId = 0;
        await _productService.UpdateProductAsync(product);

        return Ok(defaultMessage: true);
    }

    [HttpGet("{productId}")]
    public virtual async Task<IActionResult> AssociatedProductAddPopup(int productId)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _productModelFactory.PrepareAddAssociatedProductSearchModelAsync(new AddAssociatedProductSearchModel());

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> AssociatedProductAddPopupList([FromBody] BaseQueryModel<AddAssociatedProductSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _productModelFactory.PrepareAddAssociatedProductListModelAsync(searchModel);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> AssociatedProductAddPopup([FromBody] BaseQueryModel<AddAssociatedProductModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        var selectedProducts = await _productService.GetProductsByIdsAsync(model.SelectedProductIds.ToArray());

        var tryToAddSelfGroupedProduct = selectedProducts
            .Select(p => p.Id)
            .Contains(model.ProductId);

        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (selectedProducts.Any())
        {
            foreach (var product in selectedProducts)
            {
                if (product.Id == model.ProductId)
                    continue;

                //validate maximum number of products per vendor
                if (currentVendor != null && product.VendorId != currentVendor.Id)
                    continue;

                product.ParentGroupedProductId = model.ProductId;
                await _productService.UpdateProductAsync(product);
            }
        }

        if (tryToAddSelfGroupedProduct)
        {
            var addAssociatedProductSearchModel = await _productModelFactory.PrepareAddAssociatedProductSearchModelAsync(new AddAssociatedProductSearchModel());
            //set current product id
            addAssociatedProductSearchModel.ProductId = model.ProductId;

            return OkWrap(addAssociatedProductSearchModel, await _localizationService.GetResourceAsync("Admin.Catalog.Products.AssociatedProducts.TryToAddSelfGroupedProduct"));
        }

        return Ok(defaultMessage: true);
    }

    #endregion

    #region Product pictures

    [HttpPost]
    public virtual async Task<IActionResult> ProductPictureAdd(int productId)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AdminApiAccessDenied();

        if (productId == 0)
            return BadRequest();

        //try to get a product with the specified id
        var product = await _productService.GetProductByIdAsync(productId);
        if (product == null || product.Deleted)
            return BadRequest("No product found with the specified id");

        var files = Request.Form.Files.ToList();
        if (files.Count == 0)
            return BadRequest("No file found");

        //a vendor should have access only to his products
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null && product.VendorId != currentVendor.Id)
            return BadRequest();
        try
        {
            foreach (var file in files)
            {
                //insert picture
                var picture = await _pictureService.InsertPictureAsync(file);

                await _pictureService.SetSeoFilenameAsync(picture.Id, await _pictureService.GetPictureSeNameAsync(product.Name));

                await _productService.InsertProductPictureAsync(new ProductPicture
                {
                    PictureId = picture.Id,
                    ProductId = product.Id,
                    DisplayOrder = 0
                });
            }
        }
        catch (Exception exc)
        {
            return BadRequest($"{await _localizationService.GetResourceAsync("Admin.Catalog.Products.Multimedia.Pictures.Alert.PictureAdd")} {exc.Message}");
        }

        return Ok(defaultMessage: true);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ProductPictureList([FromBody] BaseQueryModel<ProductPictureSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //try to get a product with the specified id
        var product = await _productService.GetProductByIdAsync(searchModel.ProductId);
        if (product == null || product.Deleted)
            return NotFound("No product found with the specified id");

        //validate maximum number of products per vendor
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null && product.VendorId != currentVendor.Id)
            return AdminApiAccessDenied();

        //prepare model
        var model = await _productModelFactory.PrepareProductPictureListModelAsync(searchModel, product);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ProductPictureUpdate([FromBody] BaseQueryModel<ProductPictureModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get a product picture with the specified id
        var productPicture = await _productService.GetProductPictureByIdAsync(model.Id);
        if (productPicture == null)
            return NotFound("No product picture found with the specified id");

        //validate maximum number of products per vendor
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null)
        {
            var product = await _productService.GetProductByIdAsync(productPicture.ProductId);
            if (product != null && product.VendorId != currentVendor.Id)
                return AdminApiAccessDenied();
        }

        //try to get a picture with the specified id
        var picture = await _pictureService.GetPictureByIdAsync(productPicture.PictureId);
        if (picture == null)
            return NotFound("No picture found with the specified id");

        await _pictureService.UpdatePictureAsync(picture.Id,
            await _pictureService.LoadPictureBinaryAsync(picture),
            picture.MimeType,
            picture.SeoFilename,
            model.OverrideAltAttribute,
            model.OverrideTitleAttribute);

        productPicture.DisplayOrder = model.DisplayOrder;
        await _productService.UpdateProductPictureAsync(productPicture);

        return Ok(defaultMessage: true);
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> ProductPictureDelete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AdminApiAccessDenied();

        //try to get a product picture with the specified id
        var productPicture = await _productService.GetProductPictureByIdAsync(id);
        if (productPicture == null)
            return NotFound("No product picture found with the specified id");

        //validate maximum number of products per vendor
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null)
        {
            var product = await _productService.GetProductByIdAsync(productPicture.ProductId);
            if (product != null && product.VendorId != currentVendor.Id)
                return AdminApiAccessDenied();
        }

        var pictureId = productPicture.PictureId;
        await _productService.DeleteProductPictureAsync(productPicture);

        //try to get a picture with the specified id
        var picture = await _pictureService.GetPictureByIdAsync(pictureId);
        if (picture == null)
            return NotFound("No picture found with the specified id");

        await _pictureService.DeletePictureAsync(picture);

        return Ok(defaultMessage: true);
    }

    #endregion

    #region Product videos

    [HttpPost]
    public virtual async Task<IActionResult> ProductVideoAdd([FromBody] BaseQueryModel<ProductVideoModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        if (model.ProductId == 0)
            throw new ArgumentException(nameof(model.ProductId));

        //try to get a product with the specified id
        var product = await _productService.GetProductByIdAsync(model.ProductId);
        if (product == null)
            return NotFound("No product found with the specified id");

        var videoUrl = model.VideoUrl.TrimStart('~');

        try
        {
            await PingVideoUrlAsync(videoUrl);
        }
        catch (Exception exc)
        {
            return BadRequest($"{await _localizationService.GetResourceAsync("Admin.Catalog.Products.Multimedia.Videos.Alert.VideoAdd")} {exc.Message}");
        }

        if (!ModelState.IsValid)
            return BadRequest(ModelState.SerializeErrors());

        //a vendor should have access only to his products
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null && product.VendorId != currentVendor.Id)
            return NotFound("This Vendor don't have access to it");
        try
        {
            var video = new Video
            {
                VideoUrl = videoUrl
            };

            //insert video
            await _videoService.InsertVideoAsync(video);

            await _productService.InsertProductVideoAsync(new ProductVideo
            {
                VideoId = video.Id,
                ProductId = product.Id,
                DisplayOrder = model.DisplayOrder
            });
        }
        catch (Exception exc)
        {
            return BadRequest($"{await _localizationService.GetResourceAsync("Admin.Catalog.Products.Multimedia.Videos.Alert.VideoAdd")} {exc.Message}");
        }

        return Ok(defaultMessage: true);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ProductVideoList([FromBody] BaseQueryModel<ProductVideoSearchModel> querySearchModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AdminApiAccessDenied();

        //try to get a product with the specified id
        var searchModel = querySearchModel.Data;
        var product = await _productService.GetProductByIdAsync(searchModel.ProductId);
        if (product == null)
            return NotFound("No product found with the specified id");

        //a vendor should have access only to his products
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null && product.VendorId != currentVendor.Id)
            return BadRequest("This is not your product");

        //prepare model
        var model = await _productModelFactory.PrepareProductVideoListModelAsync(searchModel, product);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ProductVideoUpdate([FromBody] BaseQueryModel<ProductVideoModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get a product picture with the specified id
        var productVideo = await _productService.GetProductVideoByIdAsync(model.Id);
        if (productVideo == null)
            return NotFound("No product video found with the specified id");

        //a vendor should have access only to his products
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null)
        {
            var product = await _productService.GetProductByIdAsync(productVideo.ProductId);
            if (product != null && product.VendorId != currentVendor.Id)
                return BadRequest("This is not your product");
        }

        //try to get a video with the specified id
        var video = await _videoService.GetVideoByIdAsync(productVideo.VideoId);
        if (video == null)
            return NotFound("No video found with the specified id");

        var videoUrl = model.VideoUrl.TrimStart('~');

        try
        {
            await PingVideoUrlAsync(videoUrl);
        }
        catch (Exception exc)
        {
            return BadRequest($"{await _localizationService.GetResourceAsync("Admin.Catalog.Products.Multimedia.Videos.Alert.VideoUpdate")} {exc.Message}");
        }

        video.VideoUrl = videoUrl;

        await _videoService.UpdateVideoAsync(video);

        productVideo.DisplayOrder = model.DisplayOrder;
        await _productService.UpdateProductVideoAsync(productVideo);

        return Ok(defaultMessage: true);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ProductVideoDelete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AdminApiAccessDenied();

        //try to get a product video with the specified id
        var productVideo = await _productService.GetProductVideoByIdAsync(id);
        if (productVideo == null)
            return NotFound("No product video found with the specified id");

        //a vendor should have access only to his products
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null)
        {
            var product = await _productService.GetProductByIdAsync(productVideo.ProductId);
            if (product != null && product.VendorId != currentVendor.Id)
                return BadRequest("This is not your product");
        }

        var videoId = productVideo.VideoId;
        await _productService.DeleteProductVideoAsync(productVideo);

        //try to get a video with the specified id
        var video = await _videoService.GetVideoByIdAsync(videoId);
        if (video == null)
            return NotFound("No video found with the specified id");

        await _videoService.DeleteVideoAsync(video);

        return Ok(defaultMessage: true);
    }

    #endregion

    #region Product specification attributes

    [HttpPost]
    public virtual async Task<IActionResult> ProductSpecificationAttributeAdd([FromBody] BaseQueryModel<AddSpecificationAttributeModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        var product = await _productService.GetProductByIdAsync(model.ProductId);
        if (product == null)
            return NotFound("No product found with the specified id");

        //validate maximum number of products per vendor
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null && product.VendorId != currentVendor.Id)
            return AdminApiAccessDenied();

        //we allow filtering only for "Option" attribute type
        if (model.AttributeTypeId != (int)SpecificationAttributeType.Option)
            model.AllowFiltering = false;

        //we don't allow CustomValue for "Option" attribute type
        if (model.AttributeTypeId == (int)SpecificationAttributeType.Option)
            model.ValueRaw = null;

        //store raw html if field allow this
        if (model.AttributeTypeId == (int)SpecificationAttributeType.CustomText
            || model.AttributeTypeId == (int)SpecificationAttributeType.Hyperlink)
            model.ValueRaw = model.Value;

        var psa = model.ToEntity<ProductSpecificationAttribute>();
        psa.CustomValue = model.ValueRaw;
        await _specificationAttributeService.InsertProductSpecificationAttributeAsync(psa);

        switch (psa.AttributeType)
        {
            case SpecificationAttributeType.CustomText:
                foreach (var localized in model.Locales)
                {
                    await _localizedEntityService.SaveLocalizedValueAsync(psa,
                        x => x.CustomValue,
                        localized.Value,
                        localized.LanguageId);
                }

                break;
            case SpecificationAttributeType.CustomHtmlText:
                foreach (var localized in model.Locales)
                {
                    await _localizedEntityService.SaveLocalizedValueAsync(psa,
                        x => x.CustomValue,
                        localized.ValueRaw,
                        localized.LanguageId);
                }

                break;
            case SpecificationAttributeType.Option:
                break;
            case SpecificationAttributeType.Hyperlink:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(queryModel));
        }

        return Created(model.ProductId);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ProductSpecAttrList([FromBody] BaseQueryModel<ProductSpecificationAttributeSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //try to get a product with the specified id
        var product = await _productService.GetProductByIdAsync(searchModel.ProductId);
        if (product == null)
            return NotFound("No product found with the specified id");

        //validate maximum number of products per vendor
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null && product.VendorId != currentVendor.Id)
            return AdminApiAccessDenied();

        //prepare model
        var model = await _productModelFactory.PrepareProductSpecificationAttributeListModelAsync(searchModel, product);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ProductSpecAttrUpdate([FromBody] BaseQueryModel<AddSpecificationAttributeModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get a product specification attribute with the specified id
        var psa = await _specificationAttributeService.GetProductSpecificationAttributeByIdAsync(model.SpecificationId);
        if (psa == null)
        {
            //select an appropriate card
            return NotFound("No product specification attribute found with the specified id");
        }

        //validate maximum number of products per vendor
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null
            && (await _productService.GetProductByIdAsync(psa.ProductId)).VendorId != currentVendor.Id)
        {
            return AdminApiAccessDenied();
        }

        //we allow filtering and change option only for "Option" attribute type
        //save localized values for CustomHtmlText and CustomText
        switch (model.AttributeTypeId)
        {
            case (int)SpecificationAttributeType.Option:
                psa.AllowFiltering = model.AllowFiltering;
                psa.SpecificationAttributeOptionId = model.SpecificationAttributeOptionId;

                break;
            case (int)SpecificationAttributeType.CustomHtmlText:
                psa.CustomValue = model.ValueRaw;
                foreach (var localized in model.Locales)
                {
                    await _localizedEntityService.SaveLocalizedValueAsync(psa,
                        x => x.CustomValue,
                        localized.ValueRaw,
                        localized.LanguageId);
                }

                break;
            case (int)SpecificationAttributeType.CustomText:
                psa.CustomValue = model.Value;
                foreach (var localized in model.Locales)
                {
                    await _localizedEntityService.SaveLocalizedValueAsync(psa,
                        x => x.CustomValue,
                        localized.Value,
                        localized.LanguageId);
                }

                break;
            default:
                psa.CustomValue = model.Value;

                break;
        }

        psa.ShowOnProductPage = model.ShowOnProductPage;
        psa.DisplayOrder = model.DisplayOrder;
        await _specificationAttributeService.UpdateProductSpecificationAttributeAsync(psa);

        return Ok(defaultMessage: true);
    }

    [HttpGet("{productId}/{specificationId?}")]
    public virtual async Task<IActionResult> ProductSpecAttributeAddOrEdit(int productId, int? specificationId)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AdminApiAccessDenied();

        if (await _productService.GetProductByIdAsync(productId) == null)
            return NotFound("No product found with the specified id");

        //try to get a product specification attribute with the specified id
        try
        {
            var model = await _productModelFactory.PrepareAddSpecificationAttributeModelAsync(productId, specificationId);
            return OkWrap(model);
        }
        catch (Exception ex)
        {
            return InternalServerError(ex.Message);
        }
    }

    [HttpPost]
    public virtual async Task<IActionResult> ProductSpecAttrDelete([FromBody] BaseQueryModel<AddSpecificationAttributeModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get a product specification attribute with the specified id
        var psa = await _specificationAttributeService.GetProductSpecificationAttributeByIdAsync(model.SpecificationId);
        if (psa == null)
        {
            //select an appropriate card
            return NotFound("No product specification attribute found with the specified id");
        }

        //validate maximum number of products per vendor
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null && (await _productService.GetProductByIdAsync(psa.ProductId)).VendorId != currentVendor.Id)
        {
            return AdminApiAccessDenied();
        }

        await _specificationAttributeService.DeleteProductSpecificationAttributeAsync(psa);

        return Ok(defaultMessage: true);
    }

    #endregion

    #region Product tags

    public virtual async Task<IActionResult> ProductTags()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProductTags))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _productModelFactory.PrepareProductTagSearchModelAsync(new ProductTagSearchModel());

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ProductTags([FromBody] BaseQueryModel<ProductTagSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProductTags))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _productModelFactory.PrepareProductTagListModelAsync(searchModel);

        return OkWrap(model);
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> ProductTagDelete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProductTags))
            return AdminApiAccessDenied();

        //try to get a product tag with the specified id
        var tag = await _productTagService.GetProductTagByIdAsync(id);
        if (tag == null)
            return NotFound("No product tag found with the specified id");

        await _productTagService.DeleteProductTagAsync(tag);

        return Ok(await _localizationService.GetResourceAsync("Admin.Catalog.ProductTags.Deleted"));
    }

    [HttpPost]
    public virtual async Task<IActionResult> ProductTagsDelete([FromBody] BaseQueryModel<ICollection<int>> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProductTags))
            return AdminApiAccessDenied();

        var selectedIds = queryModel.Data;
        if (selectedIds != null)
        {
            var tags = await _productTagService.GetProductTagsByIdsAsync(selectedIds.ToArray());
            await _productTagService.DeleteProductTagsAsync(tags);
        }

        return Ok(defaultMessage: true);
    }

    [HttpGet("{id}")]
    public virtual async Task<IActionResult> EditProductTag(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProductTags))
            return AdminApiAccessDenied();

        //try to get a product tag with the specified id
        var productTag = await _productTagService.GetProductTagByIdAsync(id);
        if (productTag == null)
            return NotFound("No product tag found with the specified id");

        //prepare tag model
        var model = await _productModelFactory.PrepareProductTagModelAsync(null, productTag);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> EditProductTag([FromBody] BaseQueryModel<ProductTagModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProductTags))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get a product tag with the specified id
        var productTag = await _productTagService.GetProductTagByIdAsync(model.Id);
        if (productTag == null)
            return NotFound("No product tag found with the specified id");

        if (ModelState.IsValid)
        {
            productTag.Name = model.Name;
            await _productTagService.UpdateProductTagAsync(productTag);

            //locales
            await UpdateLocalesAsync(productTag, model);

            return Ok(await _localizationService.GetResourceAsync("Admin.Catalog.ProductTags.Updated"));
        }

        //prepare model
        model = await _productModelFactory.PrepareProductTagModelAsync(model, productTag, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    #endregion

    #region Purchased with order

    [HttpPost]
    public virtual async Task<IActionResult> PurchasedWithOrders([FromBody] BaseQueryModel<ProductOrderSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //try to get a product with the specified id
        var product = await _productService.GetProductByIdAsync(searchModel.ProductId);
        if (product == null)
            return NotFound("No product found with the specified id");

        //validate maximum number of products per vendor
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null && product.VendorId != currentVendor.Id)
            return AdminApiAccessDenied();

        //prepare model
        var model = await _productModelFactory.PrepareProductOrderListModelAsync(searchModel, product);

        return OkWrap(model);
    }

    #endregion

    #region Export / Import

    [HttpPost]
    public virtual async Task<IActionResult> DownloadCatalogAsPdf([FromBody] BaseQueryModel<ProductSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //validate maximum number of products per vendor
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null)
        {
            model.SearchVendorId = currentVendor.Id;
        }

        var categoryIds = new List<int> { model.SearchCategoryId };
        //include subcategories
        if (model.SearchIncludeSubCategories && model.SearchCategoryId > 0)
            categoryIds.AddRange(await _categoryService.GetChildCategoryIdsAsync(parentCategoryId: model.SearchCategoryId, showHidden: true));

        //0 - all (according to "ShowHidden" parameter)
        //1 - published only
        //2 - unpublished only
        bool? overridePublished = null;
        if (model.SearchPublishedId == 1)
            overridePublished = true;
        else if (model.SearchPublishedId == 2)
            overridePublished = false;

        var products = await _productService.SearchProductsAsync(0,
            categoryIds: categoryIds,
            manufacturerIds: new List<int> { model.SearchManufacturerId },
            storeId: model.SearchStoreId,
            vendorId: model.SearchVendorId,
            warehouseId: model.SearchWarehouseId,
            productType: model.SearchProductTypeId > 0 ? (ProductType?)model.SearchProductTypeId : null,
            keywords: model.SearchProductName,
            showHidden: true,
            overridePublished: overridePublished);

        try
        {
            byte[] bytes;
            await using (var stream = new MemoryStream())
            {
                await _pdfService.PrintProductsToPdfAsync(stream, products);
                bytes = stream.ToArray();
            }

            return File(bytes, MimeTypes.ApplicationPdf, "pdfcatalog.pdf");
        }
        catch (Exception exc)
        {
            return InternalServerError(exc.Message);
        }
    }

    [HttpPost]
    public virtual async Task<IActionResult> ExportXmlAll([FromBody] BaseQueryModel<ProductSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //validate maximum number of products per vendor
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null)
        {
            model.SearchVendorId = currentVendor.Id;
        }

        var categoryIds = new List<int> { model.SearchCategoryId };
        //include subcategories
        if (model.SearchIncludeSubCategories && model.SearchCategoryId > 0)
            categoryIds.AddRange(await _categoryService.GetChildCategoryIdsAsync(parentCategoryId: model.SearchCategoryId, showHidden: true));

        //0 - all (according to "ShowHidden" parameter)
        //1 - published only
        //2 - unpublished only
        bool? overridePublished = null;
        if (model.SearchPublishedId == 1)
            overridePublished = true;
        else if (model.SearchPublishedId == 2)
            overridePublished = false;

        var products = await _productService.SearchProductsAsync(0,
            categoryIds: categoryIds,
            manufacturerIds: [model.SearchManufacturerId],
            storeId: model.SearchStoreId,
            vendorId: model.SearchVendorId,
            warehouseId: model.SearchWarehouseId,
            productType: model.SearchProductTypeId > 0 ? (ProductType?)model.SearchProductTypeId : null,
            keywords: model.SearchProductName,
            showHidden: true,
            overridePublished: overridePublished);

        try
        {
            var xml = await _exportManager.ExportProductsToXmlAsync(products);

            return File(Encoding.UTF8.GetBytes(xml), MimeTypes.ApplicationXml, "products.xml");
        }
        catch (Exception exc)
        {
            return InternalServerError(exc.Message);
        }
    }

    public virtual async Task<IActionResult> ExportXmlSelected([FromBody] BaseQueryModel<string> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AdminApiAccessDenied();

        var selectedIds = queryModel.Data;
        if (selectedIds == null)
            return NotFound();

        var products = new List<Product>();
        if (selectedIds != null)
        {
            var ids = selectedIds
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => Convert.ToInt32(x))
                .ToArray();
            products.AddRange(await _productService.GetProductsByIdsAsync(ids));
        }
        //a vendor should have access only to his products
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null)
        {
            products = products.Where(p => p.VendorId == currentVendor.Id).ToList();
        }

        try
        {
            var xml = await _exportManager.ExportProductsToXmlAsync(products);
            return File(Encoding.UTF8.GetBytes(xml), MimeTypes.ApplicationXml, "products.xml");
        }
        catch (Exception exc)
        {
            return InternalServerError(exc.Message);
        }
    }

    [HttpPost]
    public virtual async Task<IActionResult> ExportExcelAll([FromBody] BaseQueryModel<ProductSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //validate maximum number of products per vendor
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null)
        {
            model.SearchVendorId = currentVendor.Id;
        }

        List<int> categoryIds = [model.SearchCategoryId];
        //include subcategories
        if (model.SearchIncludeSubCategories && model.SearchCategoryId > 0)
            categoryIds.AddRange(await _categoryService.GetChildCategoryIdsAsync(parentCategoryId: model.SearchCategoryId, showHidden: true));

        //0 - all (according to "ShowHidden" parameter)
        //1 - published only
        //2 - unpublished only
        bool? overridePublished = null;
        if (model.SearchPublishedId == 1)
            overridePublished = true;
        else if (model.SearchPublishedId == 2)
            overridePublished = false;

        var products = await _productService.SearchProductsAsync(0,
            categoryIds: categoryIds,
            manufacturerIds: new List<int> { model.SearchManufacturerId },
            storeId: model.SearchStoreId,
            vendorId: model.SearchVendorId,
            warehouseId: model.SearchWarehouseId,
            productType: model.SearchProductTypeId > 0 ? (ProductType?)model.SearchProductTypeId : null,
            keywords: model.SearchProductName,
            showHidden: true,
            overridePublished: overridePublished);

        try
        {
            var bytes = await _exportManager.ExportProductsToXlsxAsync(products);

            return File(bytes, MimeTypes.TextXlsx, "products.xlsx");
        }
        catch (Exception exc)
        {
            return InternalServerError(exc.Message);
        }
    }

    [HttpPost]
    public virtual async Task<IActionResult> ExportExcelSelected([FromBody] BaseQueryModel<string> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AdminApiAccessDenied();

        var selectedIds = queryModel.Data;
        if (selectedIds == null)
            return NotFound();

        var products = new List<Product>();
        if (selectedIds != null)
        {
            var ids = selectedIds
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => Convert.ToInt32(x))
                .ToArray();
            products.AddRange(await _productService.GetProductsByIdsAsync(ids));
        }
        //a vendor should have access only to his products
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null)
        {
            products = products.Where(p => p.VendorId == currentVendor.Id).ToList();
        }

        try
        {
            var bytes = await _exportManager.ExportProductsToXlsxAsync(products);

            return File(bytes, MimeTypes.TextXlsx, "products.xlsx");
        }
        catch (Exception exc)
        {
            return InternalServerError(exc.Message);
        }
    }

    [HttpPost]
    public virtual async Task<IActionResult> ImportExcel()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AdminApiAccessDenied();

        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null && !_vendorSettings.AllowVendorsToImportProducts)
            //a vendor can not import products
            return AdminApiAccessDenied();

        try
        {
            var importExcelFile = await Request.GetFirstOrDefaultFileAsync();
            if (importExcelFile != null && importExcelFile.Length > 0)
            {
                await _importManager.ImportProductsFromXlsxAsync(importExcelFile.OpenReadStream());
            }
            else
            {
                return BadRequest(await _localizationService.GetResourceAsync("Admin.Common.UploadFile"));
            }

            return Ok(await _localizationService.GetResourceAsync("Admin.Catalog.Products.Imported"));
        }
        catch (Exception exc)
        {
            return InternalServerError(exc.Message);
        }
    }

    #endregion

    #region Tier prices

    [HttpPost]
    public virtual async Task<IActionResult> TierPriceList([FromBody] BaseQueryModel<TierPriceSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //try to get a product with the specified id
        var product = await _productService.GetProductByIdAsync(searchModel.ProductId);
        if (product == null)
            return NotFound("No product found with the specified id");

        //validate maximum number of products per vendor
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null && product.VendorId != currentVendor.Id)
            return AdminApiAccessDenied();

        //prepare model
        var model = await _productModelFactory.PrepareTierPriceListModelAsync(searchModel, product);

        return OkWrap(model);
    }

    [HttpGet("{productId}")]
    public virtual async Task<IActionResult> TierPriceCreatePopup(int productId)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AdminApiAccessDenied();

        //try to get a product with the specified id
        var product = await _productService.GetProductByIdAsync(productId);
        if (product == null)
            return NotFound("No product found with the specified id");

        //prepare model
        var model = await _productModelFactory.PrepareTierPriceModelAsync(new TierPriceModel(), product, null);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> TierPriceCreatePopup([FromBody] BaseQueryModel<TierPriceModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get a product with the specified id
        var product = await _productService.GetProductByIdAsync(model.ProductId);
        if (product == null)
            return NotFound("No product found with the specified id");

        //validate maximum number of products per vendor
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null && product.VendorId != currentVendor.Id)
            return AdminApiAccessDenied();

        if (ModelState.IsValid)
        {
            //fill entity from model
            var tierPrice = model.ToEntity<TierPrice>();
            tierPrice.ProductId = product.Id;
            tierPrice.CustomerRoleId = model.CustomerRoleId > 0 ? model.CustomerRoleId : (int?)null;

            await _productService.InsertTierPriceAsync(tierPrice);

            //update "HasTierPrices" property
            await _productService.UpdateHasTierPricesPropertyAsync(product);

            return OkWrap(model);
        }

        //prepare model
        model = await _productModelFactory.PrepareTierPriceModelAsync(model, product, null, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    [HttpGet("{id}")]
    public virtual async Task<IActionResult> TierPriceEditPopup(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AdminApiAccessDenied();

        //try to get a tier price with the specified id
        var tierPrice = await _productService.GetTierPriceByIdAsync(id);
        if (tierPrice == null)
            return NotFound("No tier price found with the specified id");

        //try to get a product with the specified id
        var product = await _productService.GetProductByIdAsync(tierPrice.ProductId);
        if (product == null)
            return NotFound("No product found with the specified id");

        //validate maximum number of products per vendor
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null && product.VendorId != currentVendor.Id)
            return AdminApiAccessDenied();

        //prepare model
        var model = await _productModelFactory.PrepareTierPriceModelAsync(null, product, tierPrice);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> TierPriceEditPopup([FromBody] BaseQueryModel<TierPriceModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get a tier price with the specified id
        var tierPrice = await _productService.GetTierPriceByIdAsync(model.Id);
        if (tierPrice == null)
            return NotFound("No tier price found with the specified id");

        //try to get a product with the specified id
        var product = await _productService.GetProductByIdAsync(tierPrice.ProductId);
        if (product == null)
            return NotFound("No product found with the specified id");

        //validate maximum number of products per vendor
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null && product.VendorId != currentVendor.Id)
            return AdminApiAccessDenied();

        if (ModelState.IsValid)
        {
            //fill entity from model
            tierPrice = model.ToEntity(tierPrice);
            tierPrice.CustomerRoleId = model.CustomerRoleId > 0 ? model.CustomerRoleId : (int?)null;
            await _productService.UpdateTierPriceAsync(tierPrice);

            return OkWrap(model);
        }

        //prepare model
        model = await _productModelFactory.PrepareTierPriceModelAsync(model, product, tierPrice, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> TierPriceDelete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AdminApiAccessDenied();

        //try to get a tier price with the specified id
        var tierPrice = await _productService.GetTierPriceByIdAsync(id);
        if (tierPrice == null)
            return NotFound("No tier price found with the specified id");

        //try to get a product with the specified id
        var product = await _productService.GetProductByIdAsync(tierPrice.ProductId);
        if (product == null)
            return NotFound("No product found with the specified id");

        //validate maximum number of products per vendor
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null && product.VendorId != currentVendor.Id)
            return AdminApiAccessDenied();

        await _productService.DeleteTierPriceAsync(tierPrice);

        //update "HasTierPrices" property
        await _productService.UpdateHasTierPricesPropertyAsync(product);

        return Ok(defaultMessage: true);
    }

    #endregion

    #region Product attributes

    [HttpPost]
    public virtual async Task<IActionResult> ProductAttributeMappingList([FromBody] BaseQueryModel<ProductAttributeMappingSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //try to get a product with the specified id
        var product = await _productService.GetProductByIdAsync(searchModel.ProductId);
        if (product == null)
            return NotFound("No product found with the specified id");

        //validate maximum number of products per vendor
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null && product.VendorId != currentVendor.Id)
            return AdminApiAccessDenied();

        //prepare model
        var model = await _productModelFactory.PrepareProductAttributeMappingListModelAsync(searchModel, product);

        return OkWrap(model);
    }

    [HttpGet("{productId}")]
    public virtual async Task<IActionResult> ProductAttributeMappingCreate(int productId)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AdminApiAccessDenied();

        //try to get a product with the specified id
        var product = await _productService.GetProductByIdAsync(productId);
        if (product == null)
            return NotFound("No product found with the specified id");

        //validate maximum number of products per vendor
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null && product.VendorId != currentVendor.Id)
            return AdminApiAccessDenied();

        //prepare model
        var model = await _productModelFactory.PrepareProductAttributeMappingModelAsync(new ProductAttributeMappingModel(), product, null);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ProductAttributeMappingCreate([FromBody] BaseQueryModel<ProductAttributeMappingModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get a product with the specified id
        var product = await _productService.GetProductByIdAsync(model.ProductId);
        if (product == null)
            return NotFound("No product found with the specified id");

        //validate maximum number of products per vendor
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null && product.VendorId != currentVendor.Id)
            return AdminApiAccessDenied();

        //ensure this attribute is not mapped yet
        if ((await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(product.Id))
            .Any(x => x.ProductAttributeId == model.ProductAttributeId))
        {
            model = await _productModelFactory.PrepareProductAttributeMappingModelAsync(model, product, null, true);

            return BadRequestWrap(model, errors: new List<string> { await _localizationService.GetResourceAsync("Admin.Catalog.Products.ProductAttributes.Attributes.AlreadyExists") });
        }

        //insert mapping
        var productAttributeMapping = model.ToEntity<ProductAttributeMapping>();

        await _productAttributeService.InsertProductAttributeMappingAsync(productAttributeMapping);
        await UpdateLocalesAsync(productAttributeMapping, model);

        //predefined values
        var predefinedValues = await _productAttributeService.GetPredefinedProductAttributeValuesAsync(model.ProductAttributeId);
        foreach (var predefinedValue in predefinedValues)
        {
            var pav = new ProductAttributeValue
            {
                ProductAttributeMappingId = productAttributeMapping.Id,
                AttributeValueType = AttributeValueType.Simple,
                Name = predefinedValue.Name,
                PriceAdjustment = predefinedValue.PriceAdjustment,
                PriceAdjustmentUsePercentage = predefinedValue.PriceAdjustmentUsePercentage,
                WeightAdjustment = predefinedValue.WeightAdjustment,
                Cost = predefinedValue.Cost,
                IsPreSelected = predefinedValue.IsPreSelected,
                DisplayOrder = predefinedValue.DisplayOrder
            };
            await _productAttributeService.InsertProductAttributeValueAsync(pav);

            //locales
            var languages = await _languageService.GetAllLanguagesAsync(true);

            //localization
            foreach (var lang in languages)
            {
                var name = await _localizationService.GetLocalizedAsync(predefinedValue, x => x.Name, lang.Id, false, false);
                if (!string.IsNullOrEmpty(name))
                    await _localizedEntityService.SaveLocalizedValueAsync(pav, x => x.Name, name, lang.Id);
            }
        }

        return Created(productAttributeMapping.Id, await _localizationService.GetResourceAsync("Admin.Catalog.Products.ProductAttributes.Attributes.Added"));
    }

    [HttpGet("{id}")]
    public virtual async Task<IActionResult> ProductAttributeMappingEdit(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AdminApiAccessDenied();

        //try to get a product attribute mapping with the specified id
        var productAttributeMapping = await _productAttributeService.GetProductAttributeMappingByIdAsync(id);
        if (productAttributeMapping == null)
            return NotFound("No product attribute mapping found with the specified id");

        //try to get a product with the specified id
        var product = await _productService.GetProductByIdAsync(productAttributeMapping.ProductId);
        if (product == null)
            return NotFound("No product found with the specified id");

        //validate maximum number of products per vendor
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null && product.VendorId != currentVendor.Id)
            return AdminApiAccessDenied();

        //prepare model
        var model = await _productModelFactory.PrepareProductAttributeMappingModelAsync(null, product, productAttributeMapping);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ProductAttributeMappingEdit([FromBody] BaseQueryModel<ProductAttributeMappingModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        var form = queryModel.FormValues.ToNameValueCollection();
        //try to get a product attribute mapping with the specified id
        var productAttributeMapping = await _productAttributeService.GetProductAttributeMappingByIdAsync(model.Id);
        if (productAttributeMapping == null)
            return NotFound("No product attribute mapping found with the specified id");

        //try to get a product with the specified id
        var product = await _productService.GetProductByIdAsync(productAttributeMapping.ProductId);
        if (product == null)
            return NotFound("No product found with the specified id");

        //validate maximum number of products per vendor
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null && product.VendorId != currentVendor.Id)
            return AdminApiAccessDenied();

        //ensure this attribute is not mapped yet
        if ((await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(product.Id))
            .Any(x => x.ProductAttributeId == model.ProductAttributeId && x.Id != productAttributeMapping.Id))
        {
            model = await _productModelFactory.PrepareProductAttributeMappingModelAsync(model, product, productAttributeMapping, true);

            return BadRequestWrap(model, errors: [ await _localizationService.GetResourceAsync("Admin.Catalog.Products.ProductAttributes.Attributes.AlreadyExists") ]);
        }

        //fill entity from model
        productAttributeMapping = model.ToEntity(productAttributeMapping);
        await _productAttributeService.UpdateProductAttributeMappingAsync(productAttributeMapping);

        await UpdateLocalesAsync(productAttributeMapping, model);

        await SaveConditionAttributesAsync(productAttributeMapping, model.ConditionModel, form);

        return Ok(await _localizationService.GetResourceAsync("Admin.Catalog.Products.ProductAttributes.Attributes.Updated"));
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> ProductAttributeMappingDelete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AdminApiAccessDenied();

        //try to get a product attribute mapping with the specified id
        var productAttributeMapping = await _productAttributeService.GetProductAttributeMappingByIdAsync(id);
        if (productAttributeMapping == null)
            return NotFound("No product attribute mapping found with the specified id");

        //try to get a product with the specified id
        var product = await _productService.GetProductByIdAsync(productAttributeMapping.ProductId);
        if (product == null)
            return NotFound("No product found with the specified id");

        //validate maximum number of products per vendor
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null && product.VendorId != currentVendor.Id)
            return AdminApiAccessDenied();

        //check if existed combinations contains the specified attribute
        var existedCombinations = await _productAttributeService.GetAllProductAttributeCombinationsAsync(product.Id);
        if (existedCombinations?.Any() == true)
        {
            foreach (var combination in existedCombinations)
            {
                var mappings = await _productAttributeParser
                    .ParseProductAttributeMappingsAsync(combination.AttributesXml);

                if (mappings?.Any(m => m.Id == productAttributeMapping.Id) == true)
                {
                    return BadRequest(
                        string.Format(await _localizationService.GetResourceAsync("Admin.Catalog.Products.ProductAttributes.Attributes.AlreadyExistsInCombination"),
                            await _productAttributeFormatter.FormatAttributesAsync(product, combination.AttributesXml, await _workContext.GetCurrentCustomerAsync(), await _storeContext.GetCurrentStoreAsync(), ", ")));
                }
            }
        }

        await _productAttributeService.DeleteProductAttributeMappingAsync(productAttributeMapping);

        return Ok(await _localizationService.GetResourceAsync("Admin.Catalog.Products.ProductAttributes.Attributes.Deleted"));
    }

    [HttpPost]
    public virtual async Task<IActionResult> ProductAttributeValueList([FromBody] BaseQueryModel<ProductAttributeValueSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //try to get a product attribute mapping with the specified id
        var productAttributeMapping = await _productAttributeService.GetProductAttributeMappingByIdAsync(searchModel.ProductAttributeMappingId);
        if (productAttributeMapping == null)
            return NotFound("No product attribute mapping found with the specified id");

        //try to get a product with the specified id
        var product = await _productService.GetProductByIdAsync(productAttributeMapping.ProductId);
        if (product == null)
            return NotFound("No product found with the specified id");

        //validate maximum number of products per vendor
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null && product.VendorId != currentVendor.Id)
            return AdminApiAccessDenied();

        //prepare model
        var model = await _productModelFactory.PrepareProductAttributeValueListModelAsync(searchModel, productAttributeMapping);

        return OkWrap(model);
    }

    [HttpGet("{productAttributeMappingId}")]
    public virtual async Task<IActionResult> ProductAttributeValueCreatePopup(int productAttributeMappingId)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AdminApiAccessDenied();

        //try to get a product attribute mapping with the specified id
        var productAttributeMapping = await _productAttributeService.GetProductAttributeMappingByIdAsync(productAttributeMappingId);
        if (productAttributeMapping == null)
            return NotFound("No product attribute mapping found with the specified id");

        //try to get a product with the specified id
        var product = await _productService.GetProductByIdAsync(productAttributeMapping.ProductId);
        if (product == null)
            return NotFound("No product found with the specified id");

        //validate maximum number of products per vendor
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null && product.VendorId != currentVendor.Id)
            return AdminApiAccessDenied();

        //prepare model
        var model = await _productModelFactory.PrepareProductAttributeValueModelAsync(new ProductAttributeValueModel(), productAttributeMapping, null);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ProductAttributeValueCreatePopup([FromBody] BaseQueryModel<ProductAttributeValueModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get a product attribute mapping with the specified id
        var productAttributeMapping = await _productAttributeService.GetProductAttributeMappingByIdAsync(model.ProductAttributeMappingId);
        if (productAttributeMapping == null)
            return NotFound("No product attribute mapping found with the specified id");

        //try to get a product with the specified id
        var product = await _productService.GetProductByIdAsync(productAttributeMapping.ProductId);
        if (product == null)
            return NotFound("No product found with the specified id");

        //validate maximum number of products per vendor
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null && product.VendorId != currentVendor.Id)
            return AdminApiAccessDenied();

        if (productAttributeMapping.AttributeControlType == AttributeControlType.ColorSquares)
        {
            //ensure valid color is chosen/entered
            if (string.IsNullOrEmpty(model.ColorSquaresRgb))
                ModelState.AddModelError(string.Empty, "Color is required");
            try
            {
                //ensure color is valid (can be instantiated)
                System.Drawing.ColorTranslator.FromHtml(model.ColorSquaresRgb);
            }
            catch (Exception exc)
            {
                ModelState.AddModelError(string.Empty, exc.Message);
            }
        }

        //ensure a picture is uploaded
        if (productAttributeMapping.AttributeControlType == AttributeControlType.ImageSquares && model.ImageSquaresPictureId == 0)
        {
            ModelState.AddModelError(string.Empty, "Image is required");
        }

        if (ModelState.IsValid)
        {
            //fill entity from model
            var pav = model.ToEntity<ProductAttributeValue>();

            pav.Quantity = model.CustomerEntersQty ? 1 : model.Quantity;

            await _productAttributeService.InsertProductAttributeValueAsync(pav);
            await UpdateLocalesAsync(pav, model);

            return OkWrap(model);
        }

        //prepare model
        model = await _productModelFactory.PrepareProductAttributeValueModelAsync(model, productAttributeMapping, null, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    [HttpGet("{id}")]
    public virtual async Task<IActionResult> ProductAttributeValueEditPopup(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AdminApiAccessDenied();

        //try to get a product attribute value with the specified id
        var productAttributeValue = await _productAttributeService.GetProductAttributeValueByIdAsync(id);
        if (productAttributeValue == null)
            return NotFound("No product attribute value found with the specified id");

        //try to get a product attribute mapping with the specified id
        var productAttributeMapping = await _productAttributeService.GetProductAttributeMappingByIdAsync(productAttributeValue.ProductAttributeMappingId);
        if (productAttributeMapping == null)
            return NotFound("No product attribute mapping found with the specified id");

        //try to get a product with the specified id
        var product = await _productService.GetProductByIdAsync(productAttributeMapping.ProductId);
        if (product == null)
            return NotFound("No product found with the specified id");

        //validate maximum number of products per vendor
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null && product.VendorId != currentVendor.Id)
            return AdminApiAccessDenied();

        //prepare model
        var model = await _productModelFactory.PrepareProductAttributeValueModelAsync(null, productAttributeMapping, productAttributeValue);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ProductAttributeValueEditPopup([FromBody] BaseQueryModel<ProductAttributeValueModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get a product attribute value with the specified id
        var productAttributeValue = await _productAttributeService.GetProductAttributeValueByIdAsync(model.Id);
        if (productAttributeValue == null)
            return NotFound("No product attribute value found with the specified id");

        //try to get a product attribute mapping with the specified id
        var productAttributeMapping = await _productAttributeService.GetProductAttributeMappingByIdAsync(productAttributeValue.ProductAttributeMappingId);
        if (productAttributeMapping == null)
            return NotFound("No product attribute mapping found with the specified id");

        //try to get a product with the specified id
        var product = await _productService.GetProductByIdAsync(productAttributeMapping.ProductId);
        if (product == null)
            return NotFound("No product found with the specified id");

        //validate maximum number of products per vendor
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null && product.VendorId != currentVendor.Id)
            return AdminApiAccessDenied();

        if (productAttributeMapping.AttributeControlType == AttributeControlType.ColorSquares)
        {
            //ensure valid color is chosen/entered
            if (string.IsNullOrEmpty(model.ColorSquaresRgb))
                ModelState.AddModelError(string.Empty, "Color is required");
            try
            {
                //ensure color is valid (can be instantiated)
                System.Drawing.ColorTranslator.FromHtml(model.ColorSquaresRgb);
            }
            catch (Exception exc)
            {
                ModelState.AddModelError(string.Empty, exc.Message);
            }
        }

        //ensure a picture is uploaded
        if (productAttributeMapping.AttributeControlType == AttributeControlType.ImageSquares && model.ImageSquaresPictureId == 0)
        {
            ModelState.AddModelError(string.Empty, "Image is required");
        }

        if (ModelState.IsValid)
        {
            //fill entity from model
            productAttributeValue = model.ToEntity(productAttributeValue);
            productAttributeValue.Quantity = model.CustomerEntersQty ? 1 : model.Quantity;
            await _productAttributeService.UpdateProductAttributeValueAsync(productAttributeValue);

            await UpdateLocalesAsync(productAttributeValue, model);

            return OkWrap(model);
        }

        //prepare model
        model = await _productModelFactory.PrepareProductAttributeValueModelAsync(model, productAttributeMapping, productAttributeValue, true);

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState);
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> ProductAttributeValueDelete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AdminApiAccessDenied();

        //try to get a product attribute value with the specified id
        var productAttributeValue = await _productAttributeService.GetProductAttributeValueByIdAsync(id);
        if (productAttributeValue == null)
            return NotFound("No product attribute value found with the specified id");

        //try to get a product attribute mapping with the specified id
        var productAttributeMapping = await _productAttributeService.GetProductAttributeMappingByIdAsync(productAttributeValue.ProductAttributeMappingId);
        if (productAttributeMapping == null)
            return NotFound("No product attribute mapping found with the specified id");

        //try to get a product with the specified id
        var product = await _productService.GetProductByIdAsync(productAttributeMapping.ProductId);
        if (product == null)
            return NotFound("No product found with the specified id");

        //validate maximum number of products per vendor
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null && product.VendorId != currentVendor.Id)
            return AdminApiAccessDenied();

        //check if existed combinations contains the specified attribute value
        var existedCombinations = await _productAttributeService.GetAllProductAttributeCombinationsAsync(product.Id);
        if (existedCombinations?.Any() == true)
        {
            foreach (var combination in existedCombinations)
            {
                var attributeValues = await _productAttributeParser.ParseProductAttributeValuesAsync(combination.AttributesXml);

                if (attributeValues.Where(attribute => attribute.Id == id).Any())
                {
                    return BadRequest(string.Format(await _localizationService.GetResourceAsync("Admin.Catalog.Products.ProductAttributes.Attributes.Values.AlreadyExistsInCombination"),
                        await _productAttributeFormatter.FormatAttributesAsync(product, combination.AttributesXml, await _workContext.GetCurrentCustomerAsync(), await _storeContext.GetCurrentStoreAsync(), ", ")));
                }
            }
        }

        await _productAttributeService.DeleteProductAttributeValueAsync(productAttributeValue);

        return Ok(defaultMessage: true);
    }

    public virtual async Task<IActionResult> AssociateProductToAttributeValuePopup()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AdminApiAccessDenied();

        //prepare model
        var model = await _productModelFactory.PrepareAssociateProductToAttributeValueSearchModelAsync(new AssociateProductToAttributeValueSearchModel());

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> AssociateProductToAttributeValuePopupList([FromBody] BaseQueryModel<AssociateProductToAttributeValueSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //prepare model
        var model = await _productModelFactory.PrepareAssociateProductToAttributeValueListModelAsync(searchModel);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> AssociateProductToAttributeValuePopup([FromBody] BaseQueryModel<AssociateProductToAttributeValueModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //try to get a product with the specified id
        var associatedProduct = await _productService.GetProductByIdAsync(model.AssociatedToProductId);
        if (associatedProduct == null)
            return NotFound("No product found with the specified id");

        //validate maximum number of products per vendor
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null && associatedProduct.VendorId != currentVendor.Id)
            return AdminApiAccessDenied();

        return Ok(defaultMessage: true);
    }

    [HttpGet("{productId}")]
    //action displaying notification (warning) to a store owner when associating some product
    public virtual async Task<IActionResult> AssociatedProductGetWarnings(int productId)
    {
        var associatedProduct = await _productService.GetProductByIdAsync(productId);
        if (associatedProduct == null)
            return Ok(defaultMessage: true);

        //attributes
        if (await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(associatedProduct.Id) is IList<ProductAttributeMapping> mapping && mapping.Any())
        {
            if (mapping.Any(attribute => attribute.IsRequired))
                return BadRequest(await _localizationService.GetResourceAsync("Admin.Catalog.Products.ProductAttributes.Attributes.Values.Fields.AssociatedProduct.HasRequiredAttributes"));

            return BadRequest(await _localizationService.GetResourceAsync("Admin.Catalog.Products.ProductAttributes.Attributes.Values.Fields.AssociatedProduct.HasAttributes"));
        }

        //gift card
        if (associatedProduct.IsGiftCard)
        {
            return BadRequest(await _localizationService.GetResourceAsync("Admin.Catalog.Products.ProductAttributes.Attributes.Values.Fields.AssociatedProduct.GiftCard"));
        }

        //downloadable product
        if (associatedProduct.IsDownload)
        {
            return BadRequest(await _localizationService.GetResourceAsync("Admin.Catalog.Products.ProductAttributes.Attributes.Values.Fields.AssociatedProduct.Downloadable"));
        }

        return Ok(defaultMessage: true);
    }

    #endregion

    #region Product attribute combinations

    [HttpPost]
    public virtual async Task<IActionResult> ProductAttributeCombinationList([FromBody] BaseQueryModel<ProductAttributeCombinationSearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        //try to get a product with the specified id
        var product = await _productService.GetProductByIdAsync(searchModel.ProductId);
        if (product == null)
            return NotFound("No product found with the specified id");

        //validate maximum number of products per vendor
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null && product.VendorId != currentVendor.Id)
            return AdminApiAccessDenied();

        //prepare model
        var model = await _productModelFactory.PrepareProductAttributeCombinationListModelAsync(searchModel, product);

        return OkWrap(model);
    }

    [HttpPost("{id}")]
    public virtual async Task<IActionResult> ProductAttributeCombinationDelete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AdminApiAccessDenied();

        //try to get a combination with the specified id
        var combination = await _productAttributeService.GetProductAttributeCombinationByIdAsync(id);
        if (combination == null)
            return NotFound("No product attribute combination found with the specified id");

        //try to get a product with the specified id
        var product = await _productService.GetProductByIdAsync(combination.ProductId);
        if (product == null)
            return NotFound("No product found with the specified id");

        //validate maximum number of products per vendorz
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null && product.VendorId != currentVendor.Id)
            return AdminApiAccessDenied();

        await _productAttributeService.DeleteProductAttributeCombinationAsync(combination);

        return Ok(defaultMessage: true);
    }

    [HttpGet("{productId}")]
    public virtual async Task<IActionResult> ProductAttributeCombinationCreatePopup(int productId)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AdminApiAccessDenied();

        //try to get a product with the specified id
        var product = await _productService.GetProductByIdAsync(productId);
        if (product == null)
            return NotFound("No product found with the specified id");

        //validate maximum number of products per vendor
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null && product.VendorId != currentVendor.Id)
            return AdminApiAccessDenied();

        //prepare model
        var model = await _productModelFactory.PrepareProductAttributeCombinationModelAsync(new ProductAttributeCombinationModel(), product, null);

        return OkWrap(model);
    }

    [HttpPost("{productId}")]
    public virtual async Task<IActionResult> ProductAttributeCombinationCreatePopup(int productId, [FromBody] BaseQueryModel<ProductAttributeCombinationModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        var form = queryModel.FormValues.ToNameValueCollection();
        //try to get a product with the specified id
        var product = await _productService.GetProductByIdAsync(productId);
        if (product == null)
            return NotFound("No product found with the specified id");

        //validate maximum number of products per vendor
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null && product.VendorId != currentVendor.Id)
            return AdminApiAccessDenied();

        //attributes
        var warnings = new List<string>();
        var attributesXml = await GetAttributesXmlForProductAttributeCombinationAsync(form, warnings, product.Id);

        //check whether the attribute value is specified
        if (string.IsNullOrEmpty(attributesXml))
            warnings.Add(await _localizationService.GetResourceAsync("Admin.Catalog.Products.ProductAttributes.AttributeCombinations.Alert.FailedValue"));

        warnings.AddRange(await _shoppingCartService.GetShoppingCartItemAttributeWarningsAsync(await _workContext.GetCurrentCustomerAsync(),
            ShoppingCartType.ShoppingCart, product, 1, attributesXml, true));

        //check whether the same attribute combination already exists
        var existingCombination = await _productAttributeParser.FindProductAttributeCombinationAsync(product, attributesXml);
        if (existingCombination != null)
            warnings.Add(await _localizationService.GetResourceAsync("Admin.Catalog.Products.ProductAttributes.AttributeCombinations.AlreadyExists"));

        if (!warnings.Any())
        {
            //save combination
            var combination = model.ToEntity<ProductAttributeCombination>();

            //fill attributes
            combination.AttributesXml = attributesXml;

            await _productAttributeService.InsertProductAttributeCombinationAsync(combination);

            //quantity change history
            await _productService.AddStockQuantityHistoryEntryAsync(product, combination.StockQuantity, combination.StockQuantity,
                message: await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.Combination.Edit"), combinationId: combination.Id);

            return OkWrap(model);
        }

        //prepare model
        model = await _productModelFactory.PrepareProductAttributeCombinationModelAsync(model, product, null, true);
        model.Warnings = warnings;

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState, warnings);
    }

    [HttpGet("{productId}")]
    public virtual async Task<IActionResult> ProductAttributeCombinationGeneratePopup(int productId)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AdminApiAccessDenied();

        //try to get a product with the specified id
        var product = await _productService.GetProductByIdAsync(productId);
        if (product == null)
            return NotFound("No product found with the specified id");

        //validate maximum number of products per vendor
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null && product.VendorId != currentVendor.Id)
            return AdminApiAccessDenied();

        //prepare model
        var model = await _productModelFactory.PrepareProductAttributeCombinationModelAsync(new ProductAttributeCombinationModel(), product, null);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ProductAttributeCombinationGeneratePopup([FromBody] BaseQueryModel<ProductAttributeCombinationModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        var form = queryModel.FormValues.ToNameValueCollection();
        //try to get a product with the specified id
        var product = await _productService.GetProductByIdAsync(model.ProductId);
        if (product == null)
            return NotFound("No product found with the specified id");

        var allowedAttributeIds = new List<int>();
        foreach (string key in form.Keys)
        {
            if (key.Contains("attribute_value_") && int.TryParse(form[key], out var id))
                allowedAttributeIds.Add(id);
        }

        var requiredAttributeNames = await (await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(product.Id))
            .Where(pam => pam.IsRequired)
            .Where(pam => !pam.IsNonCombinable())
            .WhereAwait(async pam => !(await _productAttributeService.GetProductAttributeValuesAsync(pam.Id)).Any(v => allowedAttributeIds.Any(id => id == v.Id)))
            .SelectAwait(async pam => (await _productAttributeService.GetProductAttributeByIdAsync(pam.ProductAttributeId)).Name).ToListAsync();

        if (requiredAttributeNames.Count != 0)
        {
            model = await _productModelFactory.PrepareProductAttributeCombinationModelAsync(model, product, null, true);
            model.ProductAttributes.SelectMany(pa => pa.Values)
                .Where(v => allowedAttributeIds.Any(id => id == v.Id))
                .ToList().ForEach(v => v.Checked = "checked");

            return OkWrap(model, errors: [ string.Format(await _localizationService.GetResourceAsync("Admin.Catalog.Products.ProductAttributes.AttributeCombinations.SelectRequiredAttributes"), string.Join(", ", requiredAttributeNames)) ]);
        }

        await GenerateAttributeCombinationsAsync(product, allowedAttributeIds);

        return Ok(defaultMessage: true);
    }

    [HttpGet("{id}")]
    public virtual async Task<IActionResult> ProductAttributeCombinationEditPopup(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AdminApiAccessDenied();

        //try to get a combination with the specified id
        var combination = await _productAttributeService.GetProductAttributeCombinationByIdAsync(id);
        if (combination == null)
            return NotFound("No product attribute combination found with the specified id");

        //try to get a product with the specified id
        var product = await _productService.GetProductByIdAsync(combination.ProductId);
        if (product == null)
            return NotFound("No product found with the specified id");

        //validate maximum number of products per vendor
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null && product.VendorId != currentVendor.Id)
            return AdminApiAccessDenied();

        //prepare model
        var model = await _productModelFactory.PrepareProductAttributeCombinationModelAsync(null, product, combination);

        return OkWrap(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ProductAttributeCombinationEditPopup([FromBody] BaseQueryModel<ProductAttributeCombinationModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        var form = queryModel.FormValues.ToNameValueCollection();
        //try to get a combination with the specified id
        var combination = await _productAttributeService.GetProductAttributeCombinationByIdAsync(model.Id);
        if (combination == null)
            return NotFound("No product attribute combination found with the specified id");

        //try to get a product with the specified id
        var product = await _productService.GetProductByIdAsync(combination.ProductId);
        if (product == null)
            return NotFound("No product found with the specified id");

        //validate maximum number of products per vendor
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null && product.VendorId != currentVendor.Id)
            return AdminApiAccessDenied();

        //attributes
        var warnings = new List<string>();
        var attributesXml = await GetAttributesXmlForProductAttributeCombinationAsync(form, warnings, product.Id);

        //check whether the attribute value is specified
        if (string.IsNullOrEmpty(attributesXml))
            warnings.Add(await _localizationService.GetResourceAsync("Admin.Catalog.Products.ProductAttributes.AttributeCombinations.Alert.FailedValue"));

        warnings.AddRange(await _shoppingCartService.GetShoppingCartItemAttributeWarningsAsync(await _workContext.GetCurrentCustomerAsync(),
            ShoppingCartType.ShoppingCart, product, 1, attributesXml, true));

        //check whether the same attribute combination already exists
        var existingCombination = await _productAttributeParser.FindProductAttributeCombinationAsync(product, attributesXml);
        if (existingCombination != null && existingCombination.Id != model.Id && existingCombination.AttributesXml.Equals(attributesXml))
            warnings.Add(await _localizationService.GetResourceAsync("Admin.Catalog.Products.ProductAttributes.AttributeCombinations.AlreadyExists"));

        if (!warnings.Any() && ModelState.IsValid)
        {
            var previousStockQuantity = combination.StockQuantity;

            //save combination
            //fill entity from model
            combination = model.ToEntity(combination);
            combination.AttributesXml = attributesXml;

            await _productAttributeService.UpdateProductAttributeCombinationAsync(combination);

            //quantity change history
            await _productService.AddStockQuantityHistoryEntryAsync(product, combination.StockQuantity - previousStockQuantity, combination.StockQuantity,
                message: await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.Combination.Edit"), combinationId: combination.Id);

            return OkWrap(model);
        }

        //prepare model
        model = await _productModelFactory.PrepareProductAttributeCombinationModelAsync(model, product, combination, true);
        model.Warnings = warnings;

        //if we got this far, something failed, redisplay form
        return BadRequestWrap(model, ModelState, warnings);
    }

    [HttpPost("{productId}")]
    public virtual async Task<IActionResult> GenerateAllAttributeCombinations(int productId)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AdminApiAccessDenied();

        //try to get a product with the specified id
        var product = await _productService.GetProductByIdAsync(productId);
        if (product == null)
            return NotFound("No product found with the specified id");

        //validate maximum number of products per vendor
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null && product.VendorId != currentVendor.Id)
            return AdminApiAccessDenied();

        await GenerateAttributeCombinationsAsync(product);

        return Ok(defaultMessage: true);
    }

    #endregion

    #region Product editor settings

    [HttpPost]
    public virtual async Task<IActionResult> SaveProductEditorSettings([FromBody] BaseQueryModel<ProductModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AdminApiAccessDenied();

        var model = queryModel.Data;
        //vendors cannot manage these settings
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null)
            return AdminApiAccessDenied();

        var productEditorSettings = await _settingService.LoadSettingAsync<ProductEditorSettings>();
        productEditorSettings = model.ProductEditorSettingsModel.ToSettings(productEditorSettings);
        await _settingService.SaveSettingAsync(productEditorSettings);

        return Ok(defaultMessage: true);
    }

    #endregion

    #region Stock quantity history

    [HttpPost]
    public virtual async Task<IActionResult> StockQuantityHistory([FromBody] BaseQueryModel<StockQuantityHistorySearchModel> queryModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AdminApiAccessDenied();

        var searchModel = queryModel.Data;
        var product = await _productService.GetProductByIdAsync(searchModel.ProductId);
        if (product == null)
            return NotFound("No product found with the specified id");

        //validate maximum number of products per vendor
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null && product.VendorId != currentVendor.Id)
            return AdminApiAccessDenied();

        //prepare model
        var model = await _productModelFactory.PrepareStockQuantityHistoryListModelAsync(searchModel, product);

        return OkWrap(model);
    }

    #endregion

    #endregion
}