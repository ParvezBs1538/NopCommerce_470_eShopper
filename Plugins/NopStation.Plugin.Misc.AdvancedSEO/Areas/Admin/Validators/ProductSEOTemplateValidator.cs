using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Nop.Data.Mapping;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.Misc.AdvancedSEO.Areas.Admin.Models.ProductSEOTemplate;

namespace NopStation.Plugin.Misc.AdvancedSEO.Areas.Admin.Validators
{
    public partial class ProductSEOTemplateValidator : BaseNopValidator<ProductSEOTemplateModel>
    {
        public ProductSEOTemplateValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.TemplateName).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Admin.Configure.SEOTemplate.Fields.TemplateName.Required"));
            RuleFor(x => x.SEOTitleTemplate).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Admin.Configure.SEOTemplate.Fields.SEOTitleTemplate.Required"))
                .When(x => x.IsActive);
            RuleFor(x => x.SEODescriptionTemplate).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Admin.Configure.SEOTemplate.Fields.SEODescriptionTemplate.Required"))
                .When(x => x.IsActive);
            RuleFor(x => x.SEOKeywordsTemplate).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Admin.Configure.SEOTemplate.Fields.SEOKeywordsTemplate.Required"))
                .When(x => x.IsActive);
        }
    }
}
