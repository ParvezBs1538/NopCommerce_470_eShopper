using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Plugin.Misc.AbandonedCarts.Factories;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework.Mvc;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Widgets.Product360View.Domain;
using NopStation.Plugin.Widgets.Product360View.Infrastructure;
using NopStation.Plugin.Widgets.Product360View.Models;
using NopStation.Plugin.Widgets.Product360View.Services;

namespace NopStation.Plugin.Widgets.Product360View.Controllers
{
    public class WidgetsProduct360ViewController : NopStationAdminController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IProductService _productService;
        private readonly IWorkContext _workContext;
        private readonly IProductPictureMappingService _productPictureMappingService;
        private readonly IProductImageSettingService _productImageSettingService;
        private readonly IPictureService _pictureService;
        private readonly IProduct360ModelFactory _product360ModelFactory;
        private readonly IStaticCacheManager _staticCacheManager;

        #endregion

        #region Ctor

        public WidgetsProduct360ViewController(ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            ISettingService settingService,
            IStoreContext storeContext,
            IProductService productService,
            IWorkContext workContext,
            IProductPictureMappingService productPictureMappingService,
            IPictureService pictureService,
            IProduct360ModelFactory product360ModelFactory,
            IProductImageSettingService productImageSettingService,
            IStaticCacheManager staticCacheManager)
        {
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _settingService = settingService;
            _storeContext = storeContext;
            _productService = productService;
            _workContext = workContext;
            _productPictureMappingService = productPictureMappingService;
            _pictureService = pictureService;
            _product360ModelFactory = product360ModelFactory;
            _productImageSettingService = productImageSettingService;
            _staticCacheManager = staticCacheManager;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var settings = await _settingService.LoadSettingAsync<Product360ViewSettings>(storeScope);

            var model = new ConfigurationModel
            {
                IsEnabled = settings.IsEnabled,
                ActiveStoreScopeConfiguration = storeScope,
            };

            if (storeScope > 0)
            {
                model.IsEnabled_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.IsEnabled, storeScope);
            }

            return View("~/Plugins/NopStation.Plugin.Widgets.Product360View/Views/Configure.cshtml", model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return await Configure();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var settings = await _settingService.LoadSettingAsync<Product360ViewSettings>(storeScope);

            //save settings
            settings.IsEnabled = model.IsEnabled;

            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.IsEnabled, model.IsEnabled_OverrideForStore, storeScope, false);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

            return await Configure();
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public virtual async Task<IActionResult> Product360PictureAdd(int productId, IFormCollection form)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            if (productId == 0)
                throw new ArgumentException();

            //try to get a product with the specified id
            var product = await _productService.GetProductByIdAsync(productId)
                ?? throw new ArgumentException("No product found with the specified id");

            var files = form.Files.ToList();
            if (!files.Any())
                return Json(new { success = false });

            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null && product.VendorId != currentVendor.Id)
                return RedirectToAction("List");
            try
            {
                var lastDisplayOrder = await _productPictureMappingService.GetLastPictureOrderByProductIdAsync(productId, false);
                foreach (var file in files)
                {
                    //insert picture
                    var picture = await _pictureService.InsertPictureAsync(file);

                    await _pictureService.SetSeoFilenameAsync(picture.Id, await _pictureService.GetPictureSeNameAsync(product.Name));
                    lastDisplayOrder++;
                    await _productPictureMappingService.InsertPictureMappingAsync(new ProductPictureMapping360
                    {
                        PictureId = picture.Id,
                        ProductId = product.Id,
                        DisplayOrder = lastDisplayOrder,
                        IsPanorama = false,
                    });
                }
            }
            catch (Exception exc)
            {
                return Json(new
                {
                    success = false,
                    message = $"{await _localizationService.GetResourceAsync("Admin.Catalog.Products.Multimedia.Pictures.Alert.PictureAdd")} {exc.Message}",
                });
            }

            return Json(new { success = true });
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public virtual async Task<IActionResult> PanoramaPictureAdd(int productId, IFormCollection form)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            if (productId == 0)
                throw new ArgumentException();

            //try to get a product with the specified id
            var product = await _productService.GetProductByIdAsync(productId)
                ?? throw new ArgumentException("No product found with the specified id");

            var files = form.Files.ToList();
            if (!files.Any())
                return Json(new { success = false });

            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null && product.VendorId != currentVendor.Id)
                return RedirectToAction("List");
            try
            {
                var lastDisplayOrder = await _productPictureMappingService.GetLastPictureOrderByProductIdAsync(productId, true);
                foreach (var file in files)
                {
                    //insert picture
                    var picture = await _pictureService.InsertPictureAsync(file);

                    await _pictureService.SetSeoFilenameAsync(picture.Id, await _pictureService.GetPictureSeNameAsync(product.Name));
                    lastDisplayOrder++;
                    await _productPictureMappingService.InsertPictureMappingAsync(new ProductPictureMapping360
                    {
                        PictureId = picture.Id,
                        ProductId = product.Id,
                        DisplayOrder = lastDisplayOrder,
                        IsPanorama = true,
                    });
                }
            }
            catch (Exception exc)
            {
                return Json(new
                {
                    success = false,
                    message = $"{await _localizationService.GetResourceAsync("Admin.Catalog.Products.Multimedia.Pictures.Alert.PictureAdd")} {exc.Message}",
                });
            }

            return Json(new { success = true });
        }

        [HttpPost]
        public virtual async Task<IActionResult> Product360PictureList(Picture360SearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return await AccessDeniedDataTablesJson();

            //try to get a product with the specified id
            var product = await _productService.GetProductByIdAsync(searchModel.ProductId)
                ?? throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null && product.VendorId != currentVendor.Id)
                return Content("This is not your product");

            //prepare model
            var model = await _product360ModelFactory.PrepareProduct360PictureListModelAsync(searchModel, product);

            return Json(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> Product360PictureUpdate(ProductPicture360Model model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //try to get a product picture with the specified id
            var productPicture = await _productPictureMappingService.GetPictureMappingByIdAsync(model.Id)
                ?? throw new ArgumentException("No product 360 picture found with the specified id");

            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null)
            {
                var product = await _productService.GetProductByIdAsync(productPicture.ProductId);
                if (product != null && product.VendorId != currentVendor.Id)
                    return Content("This is not your product");
            }

            //try to get a picture with the specified id
            var picture = await _pictureService.GetPictureByIdAsync(productPicture.PictureId)
                ?? throw new ArgumentException("No picture found with the specified id");

            // Clearing Cached 360 view Pictures of this product
            await _staticCacheManager.RemoveByPrefixAsync(Picture360CacheKeys.Picture360Prefix, productPicture.ProductId);

            await _pictureService.UpdatePictureAsync(picture.Id,
                await _pictureService.LoadPictureBinaryAsync(picture),
                picture.MimeType,
                picture.SeoFilename,
                model.OverrideAltAttribute,
                model.OverrideTitleAttribute);

            productPicture.DisplayOrder = model.DisplayOrder;
            await _productPictureMappingService.UpdatePictureMappingAsync(productPicture);

            return new NullJsonResult();
        }

        [HttpPost]
        public virtual async Task<IActionResult> Product360PictureDelete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //try to get a product picture with the specified id
            var productPicture = await _productPictureMappingService.GetPictureMappingByIdAsync(id)
                ?? throw new ArgumentException("No product 360 picture found with the specified id");

            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null)
            {
                var product = await _productService.GetProductByIdAsync(productPicture.ProductId);
                if (product != null && product.VendorId != currentVendor.Id)
                    return Content("This is not your product");
            }

            var pictureId = productPicture.PictureId;
            await _productPictureMappingService.DeletePictureMappingAsync(productPicture);

            //try to get a picture with the specified id
            var picture = await _pictureService.GetPictureByIdAsync(pictureId)
                ?? throw new ArgumentException("No picture found with the specified id");

            await _pictureService.DeletePictureAsync(picture);

            return new NullJsonResult();
        }

        [HttpPost]
        public virtual async Task<IActionResult> ProductImageSettingAddOrUpdate(ImageSetting360Model model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            if (model == null)
                throw new ArgumentNullException(nameof(model));

            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null)
            {
                var product = await _productService.GetProductByIdAsync(model.ProductId);
                if (product != null && product.VendorId != currentVendor.Id)
                    return Content("This is not your product");
            }

            await _productImageSettingService.AddOrUpdateImageSettingAsync(model);
            return new NullJsonResult();
        }

        #endregion
    }
}