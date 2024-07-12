using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Media;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.Widgets.Product360View.Models;
using NopStation.Plugin.Widgets.Product360View.Services;

namespace Nop.Plugin.Misc.AbandonedCarts.Factories
{
    public class Product360ModelFactory : IProduct360ModelFactory
    {
        #region Fields

        private readonly IProductService _productService;
        private readonly IProductPictureMappingService _productPictureMappingService;
        private readonly IPictureService _pictureService;
        private readonly IProductImageSettingService _productImageSettingService;

        #endregion

        #region Ctor

        public Product360ModelFactory(IProductService productService,
            IProductPictureMappingService productPictureMappingService,
            IPictureService pictureService,
            IProductImageSettingService productImageSettingService)
        {
            _productService = productService;
            _productPictureMappingService = productPictureMappingService;
            _pictureService = pictureService;
            _productImageSettingService = productImageSettingService;
        }

        #endregion

        #region Methods

        public virtual async Task<Product360PictureListModel> PrepareProduct360PictureListModelAsync(Picture360SearchModel searchModel, Product product)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (product == null)
                throw new ArgumentNullException(nameof(product));

            //get product pictures
            var productPictures = (await _productPictureMappingService.GetPictureMappingsByProductIdAsync(product.Id, searchModel.IsPanorama)).ToPagedList(searchModel);

            //prepare grid model
            var model = await new Product360PictureListModel().PrepareToGridAsync(searchModel, productPictures, () =>
            {
                return productPictures.SelectAwait(async productPicture =>
                {
                    //fill in model values from the entity
                    var product360PictureModel = new ProductPicture360Model()
                    {
                        Id = productPicture.Id,
                        ProductId = productPicture.ProductId,
                        PictureId = productPicture.PictureId,
                        DisplayOrder = productPicture.DisplayOrder,
                        IsPanorama = productPicture.IsPanorama,
                    };

                    //fill in additional values (not existing in the entity)
                    var picture = (await _pictureService.GetPictureByIdAsync(productPicture.PictureId))
                        ?? throw new Exception("Picture cannot be loaded");

                    product360PictureModel.PictureUrl = (await _pictureService.GetPictureUrlAsync(picture)).Url;

                    product360PictureModel.OverrideAltAttribute = picture.AltAttribute;
                    product360PictureModel.OverrideTitleAttribute = picture.TitleAttribute;

                    return product360PictureModel;
                });
            });

            return model;
        }

        public async Task<ImageSetting360Model> PrepareImageSetting360ModelAsync(int productId)
        {
            var imageSetting = await _productImageSettingService.GetImageSettingByProductIdAsync(productId);
            if (imageSetting != null)
            {
                var imageSettingModel = imageSetting.ToModel<ImageSetting360Model>();
                return imageSettingModel;
            }
            return null;
        }

        public async Task<Product360Model> PrepareImage360DetailsModelAsync(int productId)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var imageSetting = await _productImageSettingService.GetImageSettingByProductIdAsync(productId);

            //prepare picture models
            var pictures = await _productPictureMappingService.Get360PicturesByProductIdAsync(productId);

            //all pictures Urls
            var pictureUrls = new List<string>();
            for (var i = 0; i < pictures?.Count; i++)
            {
                var picture = pictures[i];

                (var fullSizeImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture);
                pictureUrls.Add(fullSizeImageUrl);
            }

            var panoramaPictures = await _productPictureMappingService.Get360PicturesByProductIdAsync(productId, true);

            //all panorama pictures Urls
            var panoramaPictureUrls = new List<string>();
            for (var i = 0; i < panoramaPictures?.Count; i++)
            {
                var picture = panoramaPictures[i];

                (var fullSizeImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture);
                panoramaPictureUrls.Add(fullSizeImageUrl);
            }

            var product360Model = new Product360Model();

            // Map Image Setting 
            if (imageSetting != null)
                product360Model.ImageSetting360Model = imageSetting.ToModel<ImageSetting360Model>();
            product360Model.PictureUrls = pictureUrls;
            product360Model.PanoramaPictureUrls = panoramaPictureUrls;
            return product360Model;
        }

        #endregion
    }
}
