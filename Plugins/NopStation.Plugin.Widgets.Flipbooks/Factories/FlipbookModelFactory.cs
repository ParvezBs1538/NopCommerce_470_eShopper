using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NopStation.Plugin.Widgets.Flipbooks.Domains;
using NopStation.Plugin.Widgets.Flipbooks.Models;
using NopStation.Plugin.Widgets.Flipbooks.Services;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Seo;
using Nop.Web.Factories;
using Nop.Web.Models.Catalog;

namespace NopStation.Plugin.Widgets.Flipbooks.Factories
{
    public class FlipbookModelFactory : IFlipbookModelFactory
    {
        private readonly IProductService _productService;
        private readonly IProductModelFactory _productModelFactory;
        private readonly FlipbookSettings _catalogSettings;
        private readonly IPictureService _pictureService;
        private readonly IFlipbookService _flipbookService;
        private readonly ILocalizationService _localizationService;
        private readonly IUrlRecordService _urlRecordService;

        public FlipbookModelFactory(IProductService productService,
            IProductModelFactory productModelFactory,
            FlipbookSettings catalogSettings,
            IPictureService pictureService,
            IFlipbookService flipbookContentService,
            ILocalizationService localizationService,
            IUrlRecordService urlRecordService)
        {
            _productService = productService;
            _productModelFactory = productModelFactory;
            _catalogSettings = catalogSettings;
            _pictureService = pictureService;
            _flipbookService = flipbookContentService;
            _localizationService = localizationService;
            _urlRecordService = urlRecordService;
        }

        public async Task<FlipbookDetailsModel> PrepareFlipbookDetailsModelAsync(Flipbook flipbook)
        {
            var model = new FlipbookDetailsModel
            {
                Id = flipbook.Id,
                Name = await _localizationService.GetLocalizedAsync(flipbook, x => x.Name),
                MetaKeywords = await _localizationService.GetLocalizedAsync(flipbook, x => x.MetaKeywords),
                MetaDescription = await _localizationService.GetLocalizedAsync(flipbook, x => x.MetaDescription),
                MetaTitle = await _localizationService.GetLocalizedAsync(flipbook, x => x.MetaTitle),
                SeName = await _urlRecordService.GetSeNameAsync(flipbook),
            };

            var flipbookContents = await _flipbookService.GetFlipbookContentsByFlipbookIdAsync(flipbook.Id);
            var i = 1;

            foreach (var content in flipbookContents)
            {
                var flipbookContentModel = new FlipbookContentModel
                {
                    Id = content.Id,
                    PageNumber = i++,
                    IsImage = content.IsImage
                };

                if (content.IsImage)
                {
                    flipbookContentModel.ImageUrl = await _pictureService.GetPictureUrlAsync(content.ImageId);
                    flipbookContentModel.RedirectUrl = content.RedirectUrl;
                }
                else
                    flipbookContentModel = await PrepareFlipbookContentProductsAsync(content.Id, 0);

                model.Contents.Add(flipbookContentModel);
            }
            return model;
        }

        public async Task<FlipbookContentModel> PrepareFlipbookContentProductsAsync(int flipbookContentId, int pageIndex = 0)
        {
            var model = new FlipbookContentModel();
            var pageSize =_catalogSettings.DefaultPageSize > 0 ? _catalogSettings.DefaultPageSize : 9;
            var products = await _flipbookService.GetProductsByFlipbookContentIdAsync(flipbookContentId, pageIndex, pageSize);
            model.PagingFilteringContext.LoadPagedList(products);

            model.ProductOverviewModelList = await (await _productModelFactory.PrepareProductOverviewModelsAsync(products)).ToListAsync();
            return model;
        }
    }
}
