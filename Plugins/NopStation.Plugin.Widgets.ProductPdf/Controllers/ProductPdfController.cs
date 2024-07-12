using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Widgets.ProductPdf.Services;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Services.Stores;

namespace NopStation.Plugin.Widgets.ProductPdf.Controllers
{
    public class ProductPdfController : NopStationPublicController
    {
        #region Fields

        private readonly IAclService _aclService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IProductService _productService;
        private readonly CatalogSettings _catalogSettings;
        private readonly IStoreMappingService _storeMappingService;
        private readonly ProductPdfSettings _productPdfSettings;
        private readonly IProductPdfService _productPdfService;
        private readonly ILocalizationService _localizationService;

        #endregion

        #region Ctor

        public ProductPdfController(IAclService aclService,
            IPermissionService permissionService,
            ISettingService settingService,
            IStoreContext storeContext,
            IProductService productService,
            CatalogSettings catalogSettings,
            IStoreMappingService storeMappingService,
            ProductPdfSettings productPdfSettings,
            IProductPdfService productPdfService,
            ILocalizationService localizationService)
        {
            _aclService = aclService;
            _permissionService = permissionService;
            _settingService = settingService;
            _storeContext = storeContext;
            _productService = productService;
            _catalogSettings = catalogSettings;
            _storeMappingService = storeMappingService;
            _productPdfSettings = productPdfSettings;
            _productPdfService = productPdfService;
            _localizationService = localizationService;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Pdf(int productId)
        {
            if (!_productPdfSettings.EnablePlugin)
                return RedirectToRoute("Homepage");

            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null || product.Deleted)
                return InvokeHttp404();

            var notAvailable =
                //published?
                (!product.Published && !_catalogSettings.AllowViewUnpublishedProductPage) ||
                //ACL (access control list) 
                !await _aclService.AuthorizeAsync(product) ||
                //Store mapping
                !await _storeMappingService.AuthorizeAsync(product) ||
                //availability dates
                !_productService.ProductIsAvailable(product);
            //Check whether the current user has a "Manage products" permission (usually a store owner)
            //We should allows him (her) to use "Preview" functionality
            var hasAdminAccess = await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel) && await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts);
            if (notAvailable && !hasAdminAccess)
                return InvokeHttp404();

            byte[] bytes;
            await using (var stream = new MemoryStream())
            {
                await _productPdfService.PrintProductToPdfAsync(stream, product);
                bytes = stream.ToArray();
            }

            return File(bytes, MimeTypes.ApplicationPdf, $"product_{product.Id}.pdf");
        }

        #endregion
    }
}