using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Services.Localization;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.Misc.QuoteCart.Areas.Admin.Models;
using NopStation.Plugin.Misc.QuoteCart.Domain;
using NopStation.Plugin.Misc.QuoteCart.Services;

namespace NopStation.Plugin.Misc.QuoteCart.Areas.Admin.Factories;

public class FormAttributeModelFactory : IFormAttributeModelFactory
{
    #region Fields

    private readonly IFormAttributeService _formAttributeService;
    private readonly IQuoteFormService _quoteFormService;
    private readonly ILocalizedModelFactory _localizedModelFactory;
    private readonly ILocalizationService _localizationService;

    #endregion

    #region Ctor

    public FormAttributeModelFactory(
        IFormAttributeService formAttributeService,
        IQuoteFormService quoteFormService,
        ILocalizedModelFactory localizedModelFactory,
        ILocalizationService localizationService)
    {
        _formAttributeService = formAttributeService;
        _quoteFormService = quoteFormService;
        _localizedModelFactory = localizedModelFactory;
        _localizationService = localizationService;
    }

    #endregion

    #region Methods

    public Task<FormAttributeSearchModel> PrepareFormAttributeSearchModelAsync(FormAttributeSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        searchModel.SetGridPageSize();

        return Task.FromResult(searchModel);
    }

    public virtual async Task<FormAttributeListModel> PrepareFormAttributeListModelAsync(FormAttributeSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        //get form attributes
        var formAttributes = await _formAttributeService
            .GetAllFormAttributesAsync(pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

        //prepare list model
        var model = new FormAttributeListModel().PrepareToGrid(searchModel, formAttributes, () =>
        {
            //fill in model values from the entity
            return formAttributes.Select(attribute => attribute.ToModel<FormAttributeModel>());
        });

        return model;
    }

    public virtual async Task<FormAttributeModel> PrepareFormAttributeModelAsync(FormAttributeModel model,
        FormAttribute formAttribute, bool excludeProperties = false)
    {
        Func<FormAttributeLocalizedModel, int, Task> localizedModelConfiguration = null;

        if (formAttribute != null)
        {
            //fill in model values from the entity
            model ??= formAttribute.ToModel<FormAttributeModel>();

            PreparePredefinedFormAttributeValueSearchModel(model.PredefinedFormAttributeValueSearchModel, formAttribute);
            PrepareFormAttributeFormSearchModel(model.FormAttributeFormSearchModel, formAttribute);

            //define localized model configuration action
            localizedModelConfiguration = async (locale, languageId) =>
            {
                locale.Name = await _localizationService.GetLocalizedAsync(formAttribute, entity => entity.Name, languageId, false, false);
                locale.Description = await _localizationService.GetLocalizedAsync(formAttribute, entity => entity.Description, languageId, false, false);
            };
        }

        //prepare localized models
        if (!excludeProperties)
            model.Locales = await _localizedModelFactory.PrepareLocalizedModelsAsync(localizedModelConfiguration);

        return model;
    }

    public virtual async Task<PredefinedFormAttributeValueListModel> PreparePredefinedFormAttributeValueListModelAsync(
       PredefinedFormAttributeValueSearchModel searchModel, FormAttribute formAttribute)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        ArgumentNullException.ThrowIfNull(formAttribute);

        //get predefined form attribute values
        var values = (await _formAttributeService.GetPredefinedFormAttributeValuesAsync(formAttribute.Id)).ToPagedList(searchModel);

        //prepare list model
        var model = new PredefinedFormAttributeValueListModel().PrepareToGrid(searchModel, values, () =>
        {
            return values.Select(value =>
            {
                //fill in model values from the entity
                var predefinedFormAttributeValueModel = value.ToModel<PredefinedFormAttributeValueModel>();

                return predefinedFormAttributeValueModel;
            });
        });

        return model;
    }

    public virtual async Task<PredefinedFormAttributeValueModel> PreparePredefinedFormAttributeValueModelAsync(PredefinedFormAttributeValueModel model,
        FormAttribute formAttribute, PredefinedFormAttributeValue predefinedFormAttributeValue, bool excludeProperties = false)
    {
        ArgumentNullException.ThrowIfNull(formAttribute);

        Func<PredefinedFormAttributeValueLocalizedModel, int, Task> localizedModelConfiguration = null;

        if (predefinedFormAttributeValue != null)
        {
            //fill in model values from the entity
            if (model == null)
            {
                model = predefinedFormAttributeValue.ToModel<PredefinedFormAttributeValueModel>();
            }

            //define localized model configuration action
            localizedModelConfiguration = async (locale, languageId) =>
            {
                locale.Name = await _localizationService.GetLocalizedAsync(predefinedFormAttributeValue, entity => entity.Name, languageId, false, false);
            };
        }

        model.FormAttributeId = formAttribute.Id;

        //prepare localized models
        if (!excludeProperties)
            model.Locales = await _localizedModelFactory.PrepareLocalizedModelsAsync(localizedModelConfiguration);

        return model;
    }

    public virtual async Task<FormAttributeFormListModel> PrepareFormAttributeFormListModelAsync(FormAttributeFormSearchModel searchModel, FormAttribute formAttribute)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        ArgumentNullException.ThrowIfNull(formAttribute);

        //get quote forms
        var quoteForms = await _quoteFormService.GetQuoteFormsByFormAttributeIdAsync(
            formAttributeId: formAttribute.Id,
            includeDeleted: true,
            pageIndex: searchModel.Page - 1,
            pageSize: searchModel.PageSize);

        //prepare list model
        var model = new FormAttributeFormListModel().PrepareToGrid(searchModel, quoteForms, () =>
        {
            //fill in model values from the entity
            return quoteForms.Select(quoteForm =>
            {
                return new FormAttributeFormModel
                {
                    QuoteFormName = quoteForm.Title,
                    Active = quoteForm.Active,
                    Id = quoteForm.Id
                };
            });
        });

        return model;
    }

    #endregion

    #region Utilities

    protected virtual PredefinedFormAttributeValueSearchModel PreparePredefinedFormAttributeValueSearchModel(
        PredefinedFormAttributeValueSearchModel searchModel, FormAttribute formAttribute)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        ArgumentNullException.ThrowIfNull(formAttribute);

        searchModel.FormAttributeId = formAttribute.Id;

        //prepare page parameters
        searchModel.SetGridPageSize();

        return searchModel;
    }

    protected virtual FormAttributeFormSearchModel PrepareFormAttributeFormSearchModel(FormAttributeFormSearchModel searchModel,
       FormAttribute formAttribute)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        ArgumentNullException.ThrowIfNull(formAttribute);

        searchModel.FormAttributeId = formAttribute.Id;

        //prepare page parameters
        searchModel.SetGridPageSize();

        return searchModel;
    }

    #endregion

}
