using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.Widgets.ProductQuestionAnswer.Areas.Admin.Models;

namespace NopStation.Plugin.Widgets.ProductQuestionAnswer.Areas.Admin.Validators
{
    public partial class ProductQAConfigurationValidator : BaseNopValidator<ConfigurationModel>
    {
        public ProductQAConfigurationValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.LimitedCustomerRole).NotEmpty()
                .When(x => x.SelectedLimitedCustomerRoleIds.Count <= 0)
                .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.ProductQuestionAnswer.Configuration.WhoAskTheQuestion.Required").Result);
            RuleFor(x => x.AnswerdCustomerRole).NotEmpty()
                .When(x => x.SelectedAnsweredCustomerRoleIds.Count <= 0)
                .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.ProductQuestionAnswer.Configuration.WhoAnswerTheQuestion.Required").Result);
        }
    }
}
