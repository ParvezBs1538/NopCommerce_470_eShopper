using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.Misc.AjaxFilter.Areas.Admin.Models;
using NopStation.Plugin.Misc.AjaxFilter.Areas.Admin.Services;
using NopStation.Plugin.Misc.AjaxFilter.Domains;

namespace NopStation.Plugin.Misc.AjaxFilter.Areas.Admin.Factories
{
    public partial class AjaxFilterSpecificationAttributeModelFactory : IAjaxFilterSpecificationAttributeModelFactory
    {
        #region Fields
        private readonly IAjaxFilterSpecificationAttributeService _ajaxFilterSpecificationAttributeService;
        #endregion

        #region Ctor
        public AjaxFilterSpecificationAttributeModelFactory(IAjaxFilterSpecificationAttributeService ajaxFilterSpecificationAttributeService)
        {
            _ajaxFilterSpecificationAttributeService = ajaxFilterSpecificationAttributeService;
        }
        #endregion

        #region Methods
        public Task<AjaxFilterSpecificationAttributeSearchModel> PrepareAjaxFilterSpecificationAttributeSearchModel(AjaxFilterSpecificationAttributeSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare page parameters
            searchModel.SetGridPageSize();

            return Task.FromResult(searchModel);
        }
        public async Task<AjaxFilterSpecificationAttributeListModel> PrepareAjaxFilterSpecificationAttributeListModelAsync(AjaxFilterSpecificationAttributeSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var specificationAttributes = (await _ajaxFilterSpecificationAttributeService.GetAjaxFilterSpecificationAttributesAsync(searchModel, searchModel.Page - 1, searchModel.PageSize));

            //prepare list model
            var model = await new AjaxFilterSpecificationAttributeListModel().PrepareToGridAsync(searchModel, specificationAttributes, () =>
             {
                 //fill in model values from the entity
                 return specificationAttributes.SelectAwait(async attribute => new AjaxFilterSpecificationAttributeModel()
                 {
                     Id = attribute.Id,
                     SpecificationAttributeName = (await _ajaxFilterSpecificationAttributeService.GetSpecificationAttributeBySpecificationIdAsync(attribute.SpecificationId)).Name
                 });
             });
            return model;
        }

        public async Task<SpecificationAttributeListModel> PrepareSpecificationAttributeListModelAsync(AjaxFilterSpecificationAttributeSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var specificationAttributes = (await _ajaxFilterSpecificationAttributeService.GetSpecificationAttributesAsync(searchModel, searchModel.Page - 1, searchModel.PageSize));

            //prepare list model
            var model = new SpecificationAttributeListModel().PrepareToGrid(searchModel, specificationAttributes, () =>
            {
                //fill in model values from the entity
                return specificationAttributes.Select(attribute => new SpecificationAttributeModel()
                {
                    Id = attribute.Id,
                    Name = attribute.Name
                });
            });

            return model;
        }

        public Task<AjaxFilterSpecificationAttributeModel> PrepareAjaxFilterSpecificationAttributeAsync(AjaxFilterSpecificationAttribute ajaxFilterSpecificationAttribute)
        {
            var model = new AjaxFilterSpecificationAttributeModel();

            model.Id = ajaxFilterSpecificationAttribute.Id;
            model.SpecificationId = ajaxFilterSpecificationAttribute.SpecificationId;
            model.MaxSpecificationAttributesToDisplay = ajaxFilterSpecificationAttribute.MaxSpecificationAttributesToDisplay;
            model.CloseSpecificationAttributeByDefault = ajaxFilterSpecificationAttribute.CloseSpecificationAttributeByDefault;
            model.AlternateName = ajaxFilterSpecificationAttribute.AlternateName;
            model.DisplayOrder = ajaxFilterSpecificationAttribute.DisplayOrder;
            model.HideProductCount = ajaxFilterSpecificationAttribute.HideProductCount;
            return Task.FromResult(model);
        }

        public Task<AjaxFilterSpecificationAttribute> PrepareSpecificationAttributeAsync(AjaxFilterSpecificationAttributeModel ajaxFilterSpecificationAttributeModel)
        {
            var ajaxFilterSpecificationAttribute = new AjaxFilterSpecificationAttribute();

            ajaxFilterSpecificationAttribute.Id = ajaxFilterSpecificationAttributeModel.Id;
            ajaxFilterSpecificationAttribute.SpecificationId = ajaxFilterSpecificationAttributeModel.SpecificationId;
            ajaxFilterSpecificationAttribute.MaxSpecificationAttributesToDisplay = ajaxFilterSpecificationAttributeModel.MaxSpecificationAttributesToDisplay;
            ajaxFilterSpecificationAttribute.CloseSpecificationAttributeByDefault = ajaxFilterSpecificationAttributeModel.CloseSpecificationAttributeByDefault;
            ajaxFilterSpecificationAttribute.AlternateName = ajaxFilterSpecificationAttributeModel.AlternateName;
            ajaxFilterSpecificationAttribute.DisplayOrder = ajaxFilterSpecificationAttributeModel.DisplayOrder;
            ajaxFilterSpecificationAttribute.HideProductCount = ajaxFilterSpecificationAttributeModel.HideProductCount;
            return Task.FromResult(ajaxFilterSpecificationAttribute);
        }
        #endregion
    }
}
