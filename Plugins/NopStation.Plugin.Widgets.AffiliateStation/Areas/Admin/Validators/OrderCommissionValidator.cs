using FluentValidation;
using NopStation.Plugin.Widgets.AffiliateStation.Areas.Admin.Models;
using NopStation.Plugin.Widgets.AffiliateStation.Domains;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Web.Areas.Admin.Validators.Blogs
{
    public partial class OrderCommissionValidator : BaseNopValidator<OrderCommissionModel>
    {
        public OrderCommissionValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.CommissionPaidOn)
                .NotNull()
                .When(x => x.CommissionStatusId != (int)CommissionStatus.Pending);
            RuleFor(x => x.PartialPaidAmount)
                .GreaterThan(0)
                .When(x => x.CommissionStatusId == (int)CommissionStatus.PartiallyPaid);
        }
    }
}