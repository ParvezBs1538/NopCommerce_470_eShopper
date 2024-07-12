using System.Linq;
using FluentValidation;
using Nop.Core.Domain.Customers;
using NopStation.Plugin.Widgets.AffiliateStation.Models;
using NopStation.Plugin.Misc.Core.Helpers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace NopStation.Plugin.Widgets.AffiliateStation.Validators
{
    public class AddressValidator : BaseNopValidator<AffiliateInfoModel.AffiliateAddressModel>
    {
        public AddressValidator(ILocalizationService localizationService,
            IStateProvinceService stateProvinceService,
            CustomerSettings customerSettings)
        {
            RuleFor(x => x.FirstName)
                .NotEmpty()
                .WithMessage(localizationService.GetResourceAsync("Address.Fields.FirstName.Required").Result);
            RuleFor(x => x.LastName)
                .NotEmpty()
                .WithMessage(localizationService.GetResourceAsync("Address.Fields.LastName.Required").Result);
            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage(localizationService.GetResourceAsync("Address.Fields.Email.Required").Result);
            RuleFor(x => x.Email)
                .EmailAddress()
                .WithMessage(localizationService.GetResourceAsync("Common.WrongEmail").Result);
            RuleFor(x => x.CountryId)
                .NotNull()
                .WithMessage(localizationService.GetResourceAsync("Address.Fields.Country.Required").Result);
            RuleFor(x => x.CountryId)
                .NotEqual(0)
                .WithMessage(localizationService.GetResourceAsync("Address.Fields.Country.Required").Result);
            RuleFor(x => x.StateProvinceId)
                .Must((x, context) =>
                {
                    //does selected country has states?
                    var countryId = x.CountryId.HasValue ? x.CountryId.Value : 0;
                    var hasStates = stateProvinceService.GetStateProvincesByCountryIdAsync(countryId).Result.Any();

                    if (hasStates)
                    {
                        //if yes, then ensure that state is selected
                        if (!x.StateProvinceId.HasValue || x.StateProvinceId.Value == 0)
                            return false;
                    }
                    return true;
                })
                .WithMessage(localizationService.GetResourceAsync("Address.Fields.StateProvince.Required").Result);
            RuleFor(x => x.Address1)
                .NotEmpty()
                .WithMessage(localizationService.GetResourceAsync("Account.Fields.StreetAddress.Required").Result);
            RuleFor(x => x.ZipPostalCode)
                .NotEmpty()
                .WithMessage(localizationService.GetResourceAsync("Account.Fields.ZipPostalCode.Required").Result);
            if (customerSettings.CountyRequired && customerSettings.CountyEnabled)
            {
                RuleFor(x => x.County).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Account.Fields.County.Required"));
            }
            RuleFor(x => x.City)
                .NotEmpty()
                .WithMessage(localizationService.GetResourceAsync("Account.Fields.City.Required").Result);
            RuleFor(x => x.PhoneNumber)
                .NotEmpty()
                .WithMessage(localizationService.GetResourceAsync("Account.Fields.Phone.Required").Result);
        }
    }
}
