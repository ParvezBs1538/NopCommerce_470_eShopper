using FluentValidation;
using NopStation.Plugin.Widgets.ProductRequests.Areas.Admin.Models;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace NopStation.Plugin.Widgets.ProductRequests.Areas.Admin.Validators
{
    public class ProductRequestValidator : BaseNopValidator<ProductRequestModel>
    {
        public ProductRequestValidator(ILocalizationService localizationService, ProductRequestSettings productRequestSettings)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResourceAsync("Admin.NopStation.ProductRequests.ProductRequests.Fields.Name.Required").Result);
            if (productRequestSettings.DescriptionRequired)
                RuleFor(x => x.Description).NotEmpty().WithMessage(localizationService.GetResourceAsync("Admin.NopStation.ProductRequests.ProductRequests.Fields.Description.Required").Result);
            if (productRequestSettings.LinkRequired)
                RuleFor(x => x.Link).NotEmpty().WithMessage(localizationService.GetResourceAsync("Admin.NopStation.ProductRequests.ProductRequests.Fields.Link.Required").Result);

            RuleFor(x => x.Name).MaximumLength(200).WithMessage(localizationService.GetResourceAsync("Admin.NopStation.ProductRequests.ProductRequests.Fields.Name.MaxLength").Result);
            RuleFor(x => x.Link).MaximumLength(400).WithMessage(localizationService.GetResourceAsync("Admin.NopStation.ProductRequests.ProductRequests.Fields.Link.MaxLength").Result);
            RuleFor(x => x.Description).MaximumLength(500).WithMessage(localizationService.GetResourceAsync("Admin.NopStation.ProductRequests.ProductRequests.Fields.Description.MaxLength").Result);
        }
    }
}
