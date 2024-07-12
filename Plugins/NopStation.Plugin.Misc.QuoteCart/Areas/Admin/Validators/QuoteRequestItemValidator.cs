using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.Misc.QuoteCart.Areas.Admin.Models;

namespace NopStation.Plugin.Misc.QuoteCart.Areas.Admin.Validators;

public class QuoteRequestItemValidator : BaseNopValidator<QuoteRequestItemModel>
{
    public QuoteRequestItemValidator(ILocalizationService localizationService)
    {
        RuleFor(x => x.ProductId).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Admin.NopStation.Plugin.Misc.QuoteCart.QuoteRequests.Fields.ProductSku.Required"));
        When(x => x.ProductId > 0, () =>
        {
            RuleFor(x => x.Quantity).GreaterThan(0);
        });
    }
}
