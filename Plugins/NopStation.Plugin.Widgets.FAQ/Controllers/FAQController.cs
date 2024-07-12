using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Localization;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Widgets.FAQ.Models;
using NopStation.Plugin.Widgets.FAQ.Services;

namespace NopStation.Plugin.Widgets.FAQ.Controllers
{
    public class FAQController : NopStationPublicController
    {
        private readonly IFAQItemService _itemService;
        private readonly IFAQCategoryService _categoryService;
        private readonly IStoreContext _storeContext;
        private readonly ILocalizationService _localizationService;
        private readonly IFAQTagService _tagService;
        private readonly FAQSettings _faqSettings;

        public FAQController(IFAQItemService itemService,
            IFAQCategoryService categoryService,
            IStoreContext storeContext,
            ILocalizationService localizationService,
            IFAQTagService tagService,
            FAQSettings faqSettings)
        {
            _itemService = itemService;
            _categoryService = categoryService;
            _storeContext = storeContext;
            _localizationService = localizationService;
            _tagService = tagService;
            _faqSettings = faqSettings;
        }

        public async Task<IActionResult> FindFAQ(int categoryId = 0, int tagId = 0)
        {
            if (!_faqSettings.EnablePlugin)
                return RedirectToRoute("Homepage");

            var categories = await _categoryService.SearchFAQCategoriesAsync(published: true, storeId: _storeContext.GetCurrentStore().Id);
            var faqItems = await _itemService.SearchFAQItemsAsync(categoryId: categoryId, tagId: tagId, published: true);

            var model = new FAQModel();
            foreach (var category in categories.Where(c => categoryId == 0 || c.Id == categoryId))
            {
                var categoryModel = new FAQCategoryModel()
                {
                    Description = await _localizationService.GetLocalizedAsync(category, x => x.Description),
                    Name = await _localizationService.GetLocalizedAsync(category, x => x.Name),
                    Id = category.Id
                };
                model.Categories.Add(categoryModel);
            }

            foreach (var item in faqItems)
            {
                var itemModel = new FAQItemModel()
                {
                    Question = await _localizationService.GetLocalizedAsync(item, x => x.Question),
                    Answer = await _localizationService.GetLocalizedAsync(item, x => x.Answer),
                    Id = item.Id,
                    Permalink = item.Permalink
                };

                var itemTags = await _tagService.GetAllFAQTagsByFAQItemIdAsync(item.Id);
                foreach (var tag in itemTags)
                {
                    itemModel.Tags.Add(new FAQItemModel.TagModel()
                    {
                        Name = tag.Name,
                        Id = tag.Id
                    });
                }

                var mappedCategories = await _categoryService.GetFAQItemCategoriesByItemIdAsync(item.Id);
                itemModel.MappedCategoryIds = mappedCategories.Select(x => x.FAQCategoryId).ToList();

                model.Items.Add(itemModel);
            }

            return View(model);
        }
    }
}
