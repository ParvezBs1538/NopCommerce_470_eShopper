using System;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Services.Localization;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.Widgets.FAQ.Areas.Admin.Models;
using NopStation.Plugin.Widgets.FAQ.Domains;
using NopStation.Plugin.Widgets.FAQ.Services;

namespace NopStation.Plugin.Widgets.FAQ.Areas.Admin.Factories
{
    public partial class FAQItemModelFactory : IFAQItemModelFactory
    {
        #region Fields

        private readonly IFAQItemService _itemService;
        private readonly IStoreMappingSupportedModelFactory _storeMappingSupportedModelFactory;
        private readonly ILocalizedModelFactory _localizedModelFactory;
        private readonly ILocalizationService _localizationService;
        private readonly IFAQTagService _tagService;
        private readonly IFAQCategoryService _fAQCategoryService;
        private readonly IStoreContext _storeContext;

        #endregion

        #region Ctor

        public FAQItemModelFactory(IFAQItemService itemService,
            IStoreMappingSupportedModelFactory storeMappingSupportedModelFactory,
            ILocalizedModelFactory localizedModelFactory,
            ILocalizationService localizationService,
            IFAQTagService tagService,
            IFAQCategoryService fAQCategoryService,
            IStoreContext storeContext)
        {
            _itemService = itemService;
            _storeMappingSupportedModelFactory = storeMappingSupportedModelFactory;
            _localizedModelFactory = localizedModelFactory;
            _localizationService = localizationService;
            _tagService = tagService;
            _fAQCategoryService = fAQCategoryService;
            _storeContext = storeContext;
        }

        #endregion

        #region Methods

        public virtual async Task<FAQItemSearchModel> PrepareFAQItemSearchModelAsync(FAQItemSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            searchModel.SetGridPageSize();

            var categories = await _fAQCategoryService.SearchFAQCategoriesAsync();

            searchModel.AvailableFAQCategories = categories.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Id.ToString()
            }).ToList();

            searchModel.AvailableFAQCategories.Insert(0, new SelectListItem()
            {
                Text = await _localizationService.GetResourceAsync("Admin.NopStation.FAQ.FAQItems.List.SearchCategory.All"),
                Value = "0"
            });

            return searchModel;
        }

        public async virtual Task<FAQItemListModel> PrepareFAQItemListModelAsync(FAQItemSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get carousels
            var items = await _itemService.SearchFAQItemsAsync(
                searchModel.SearchKeyword,
                searchModel.SearchCategoryId,
                0, null, 0,
                searchModel.Page - 1, searchModel.PageSize);

            //prepare list model
            var model = new FAQItemListModel().PrepareToGridAsync(searchModel, items, () =>
            {
                return items.SelectAwait(async cat =>
                {
                    return await PrepareFAQItemModelAsync(null, cat, true);
                });
            });

            return await model;
        }

        public async Task<FAQItemModel> PrepareFAQItemModelAsync(FAQItemModel model, FAQItem item, bool excludeProperties = false)
        {
            Func<FAQItemLocalizedModel, int, Task> localizedModelConfiguration = null;

            if (item != null)
            {
                if (model == null)
                {
                    model = item.ToModel<FAQItemModel>();
                    model.FAQTags = string.Join(", ", (await _tagService.GetAllFAQTagsByFAQItemIdAsync(item.Id)).Select(tag => tag.Name));
                    model.SelectedCategoryIds = _fAQCategoryService.GetFAQItemCategoryIds(item);
                }

                if (!excludeProperties)
                {
                    localizedModelConfiguration = async (locale, languageId) =>
                    {
                        locale.Answer = await _localizationService.GetLocalizedAsync(item, entity => entity.Answer, languageId, false, false);
                        locale.Question = await _localizationService.GetLocalizedAsync(item, entity => entity.Question, languageId, false, false);
                    };
                }
            }

            if (!excludeProperties)
            {
                var tags = await _tagService.SearchFAQTagsAsync();
                var tagsSb = new StringBuilder();
                tagsSb.Append("var initialTags = [");
                for (var i = 0; i < tags.Count; i++)
                {
                    var tag = tags[i];
                    tagsSb.Append("'");
                    tagsSb.Append(JavaScriptEncoder.Default.Encode(tag.Name));
                    tagsSb.Append("'");
                    if (i != tags.Count - 1)
                        tagsSb.Append(",");
                }
                tagsSb.Append("]");
                model.InitialFAQTags = tagsSb.ToString();

                model.AvailableFAQCategories = (await _fAQCategoryService.SearchFAQCategoriesAsync(storeId: (await _storeContext.GetCurrentStoreAsync()).Id, published: true))
                   .Select(s => new SelectListItem()
                   {
                       Text = s.Name,
                       Value = s.Id.ToString(),
                       Selected = model.SelectedCategoryIds.Contains(s.Id)
                   }).ToList();

                model.Locales = await _localizedModelFactory.PrepareLocalizedModelsAsync(localizedModelConfiguration);
                await _storeMappingSupportedModelFactory.PrepareModelStoresAsync(model, item, excludeProperties);
            }

            return model;
        }

        #endregion
    }
}
