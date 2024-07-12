using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using NopStation.Plugin.Widgets.CategoryBanners.Areas.Admin.Models;
using NopStation.Plugin.Widgets.CategoryBanners.Services;
using Nop.Services.Media;
using Nop.Web.Framework.Models.Extensions;

namespace NopStation.Plugin.Widgets.CategoryBanners.Areas.Admin.Factories
{
    public class CategoryBannerModelFactory : ICategoryBannerModelFactory
    {
        private readonly ICategoryBannerService _categoryBannerService;
        private readonly IPictureService _pictureService;

        public CategoryBannerModelFactory(ICategoryBannerService categoryBannerService,
            IPictureService pictureService)
        {
            _categoryBannerService = categoryBannerService;
            _pictureService = pictureService;
        }

        public virtual async Task<CategoryBannerListModel> PrepareProductPictureListModelAsync(CategoryBannerSearchModel searchModel, Category category)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (category == null)
                throw new ArgumentNullException(nameof(category));

            //get product pictures
            var productPictures = _categoryBannerService.GetCategoryBannersByCategoryId(category.Id).ToPagedList(searchModel);

            //prepare grid model
            var model = await new CategoryBannerListModel().PrepareToGridAsync(searchModel, productPictures, () =>
            {
                return productPictures.SelectAwait(async banner =>
                {
                    //fill in model values from the entity
                    var categoryBannerModel = new CategoryBannerModel()
                    {
                        DisplayOrder = banner.DisplayOrder,
                        CategoryId = banner.CategoryId,
                        ForMobile = banner.ForMobile,
                        PictureId = banner.PictureId,
                        Id = banner.Id
                    };

                    //fill in additional values (not existing in the entity)
                    var picture =  await _pictureService.GetPictureByIdAsync(banner.PictureId)
                                  ?? throw new Exception("Picture cannot be loaded");

                    categoryBannerModel.PictureUrl = await _pictureService.GetPictureUrlAsync(picture.Id);
                    categoryBannerModel.OverrideAltAttribute = picture.AltAttribute;
                    categoryBannerModel.OverrideTitleAttribute = picture.TitleAttribute;

                    return categoryBannerModel;
                });
            });

            return model;
        }
    }
}
