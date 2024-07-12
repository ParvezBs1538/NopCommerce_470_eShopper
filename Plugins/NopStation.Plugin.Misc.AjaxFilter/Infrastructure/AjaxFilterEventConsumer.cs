using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Core.Events;
using Nop.Services.Events;
using NopStation.Plugin.Misc.AjaxFilter.Areas.Admin.Services;
using NopStation.Plugin.Misc.AjaxFilter.Domains;

namespace NopStation.Plugin.Misc.AjaxFilter.Infrastructure
{
    public class AjaxFilterEventConsumer : IConsumer<EntityDeletedEvent<Category>>,
        IConsumer<EntityDeletedEvent<SpecificationAttribute>>,
        IConsumer<EntityInsertedEvent<Category>>,
        IConsumer<EntityInsertedEvent<SpecificationAttribute>>
    {
        private readonly IAjaxFilterParentCategoryService _ajaxFilterParentCategoryService;
        private readonly IAjaxFilterSpecificationAttributeService _ajaxFilterSpecificationAttributeService;
        public AjaxFilterEventConsumer(IAjaxFilterParentCategoryService ajaxFilterParentCategoryService,
                                       IAjaxFilterSpecificationAttributeService ajaxFilterSpecificationAttributeService)
        {
            _ajaxFilterParentCategoryService = ajaxFilterParentCategoryService;
            _ajaxFilterSpecificationAttributeService = ajaxFilterSpecificationAttributeService;
        }
        public async Task HandleEventAsync(EntityDeletedEvent<SpecificationAttribute> eventMessage)
        {
            var specId = eventMessage.Entity.Id;
            var ajaxFilterSpecificationAttribute = await _ajaxFilterSpecificationAttributeService.GetAjaxFilterSpecificationAttributeBySpecificationAttributeId(specId);
            await _ajaxFilterSpecificationAttributeService.DeleteAjaxFilterSpecificationAttribute(ajaxFilterSpecificationAttribute);
        }

        public async Task HandleEventAsync(EntityDeletedEvent<Category> eventMessage)
        {
            var categoryId = eventMessage.Entity.Id;
            var ajaxFilterParentCategory = await _ajaxFilterParentCategoryService.GetParentCategoryByCategoryIdAsync(categoryId);
            await _ajaxFilterParentCategoryService.DeleteParentCategoryAsync(ajaxFilterParentCategory);
        }

        public async Task HandleEventAsync(EntityInsertedEvent<SpecificationAttribute> eventMessage)
        {
            var specId = eventMessage.Entity.Id;
            var ajaxFilterSpecificationAttribute = new AjaxFilterSpecificationAttribute()
            {
                SpecificationId = specId,
                CloseSpecificationAttributeByDefault = false,
                AlternateName = "",
                MaxSpecificationAttributesToDisplay = 10
            };
            await _ajaxFilterSpecificationAttributeService.InsertAjaxFilterSpecificationAttributeAsync(ajaxFilterSpecificationAttribute);
        }

        public async Task HandleEventAsync(EntityInsertedEvent<Category> eventMessage)
        {
            var categoryId = eventMessage.Entity.Id;
            var ajaxFilterCategory = new AjaxFilterParentCategory()
            {
                CategoryId = categoryId,
                EnableManufactureFiltering = true,
                EnablePriceRangeFiltering = true,
                EnableSearchForManufacturers = true,
                EnableSearchForSpecifications = true,
                EnableSpecificationAttributeFiltering = true,
                EnableVendorFiltering = false
            };
            await _ajaxFilterParentCategoryService.InsertParentCategoryAsync(ajaxFilterCategory);
        }
    }
}
