using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Caching;
using Nop.Core.Domain.Media;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Media;
using NopStation.Plugin.Misc.AjaxFilter.Infrastructure.Cache;
using NopStation.Plugin.Misc.AjaxFilter.Models;

namespace NopStation.Plugin.Misc.AjaxFilter.Factories
{
    public class CategoryImageFactory : ICategoryImageFactory
    {
        private readonly MediaSettings _mediaSettings;
        private readonly IStaticCacheManager _cacheManager;
        private readonly IProductService _productService;
        private readonly IPictureService _pictureService;
        private readonly ILocalizationService _localizationService;

        public CategoryImageFactory(MediaSettings mediaSettings,
            IStaticCacheManager cacheManager,
            IProductService productService,
            IPictureService pictureService,
            ILocalizationService localizationService)
        {
            _mediaSettings = mediaSettings;
            _cacheManager = cacheManager;
            _productService = productService;
            _pictureService = pictureService;
            _localizationService = localizationService;
        }

        public async Task<IList<CategoryPictureModel>> GetProductImages(IList<int> productIds)
        {
            var pictureModels = new List<CategoryPictureModel>();
            var defaultPictureSize = _mediaSettings.ProductDetailsPictureSize;
            foreach (var productId in productIds)
            {
                var productPicturesCacheKey = _cacheManager.PrepareKeyForDefaultCache(CategoryImagesModelCache.CategoryProductImagesCacheKey, productId);
                pictureModels = await _cacheManager.GetAsync(productPicturesCacheKey, async () =>
                {
                    var product = await _productService.GetProductByIdAsync(productId);
                    var pictures = await _pictureService.GetPicturesByProductIdAsync(productId);//product.ProductPictures;
                    var productName = product.Name;
                    foreach (var picture in pictures)
                    {
                        var pictureModel = new CategoryPictureModel
                        {
                            ProductId = productId,
                            ImageUrl = (await _pictureService.GetPictureUrlAsync(picture, defaultPictureSize)).Url,
                            ThumbImageUrl = (await _pictureService.GetPictureUrlAsync(picture, _mediaSettings.ProductThumbPictureSizeOnProductDetailsPage)).Url,
                            FullSizeImageUrl = (await _pictureService.GetPictureUrlAsync(picture)).Url,
                            Title = string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageLinkTitleFormat.Details"), productName),
                            AlternateText = string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageAlternateTextFormat.Details"), productName),
                        };
                        //"title" attribute
                        pictureModel.Title = !string.IsNullOrEmpty(picture.TitleAttribute) ?
                            picture.TitleAttribute :
                            string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageLinkTitleFormat.Details"), productName);
                        //"alt" attribute
                        pictureModel.AlternateText = !string.IsNullOrEmpty(picture.AltAttribute) ?
                            picture.AltAttribute :
                            string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageAlternateTextFormat.Details"), productName);

                        pictureModels.Add(pictureModel);
                    }
                    return pictureModels;
                });
            }

            return pictureModels;
        }
    }
}
