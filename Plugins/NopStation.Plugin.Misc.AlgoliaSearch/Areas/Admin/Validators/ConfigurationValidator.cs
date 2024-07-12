using System;
using System.Linq;
using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.Misc.AlgoliaSearch.Areas.Admin.Models;

namespace NopStation.Plugin.Misc.AlgoliaSearch.Areas.Admin.Validators
{
    public class ConfigurationValidator : BaseNopValidator<ConfigurationModel>
    {
        public ConfigurationValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.ApplicationId).NotEmpty().WithMessage(localizationService.GetResourceAsync("Admin.NopStation.AlgoliaSearch.Configuration.Fields.ApplicationId.Required").Result);
            RuleFor(x => x.SearchOnlyKey).NotEmpty().WithMessage(localizationService.GetResourceAsync("Admin.NopStation.AlgoliaSearch.Configuration.Fields.SearchOnlyKey.Required").Result);
            RuleFor(x => x.AdminKey).NotEmpty().WithMessage(localizationService.GetResourceAsync("Admin.NopStation.AlgoliaSearch.Configuration.Fields.AdminKey.Required").Result);

            RuleFor(x => x.SearchTermMinimumLength).GreaterThan(0).WithMessage(localizationService.GetResourceAsync("Admin.NopStation.AlgoliaSearch.Configuration.Fields.MinimumQueryLength.GreaterThanZero").Result);
            RuleFor(x => x.AutoCompleteListSize)
                .GreaterThan(0)
                .When(x => x.EnableAutoComplete)
                .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.AlgoliaSearch.Configuration.Fields.AutoCompleteListSize.GreaterThanZero").Result);
            RuleFor(x => x.SearchPagePageSizeOptions)
                .NotEmpty()
                .When(x => x.AllowCustomersToSelectPageSize)
                .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.AlgoliaSearch.Configuration.Fields.SearchPagePageSizeOptions.Required").Result);
            RuleFor(x => x.SearchPagePageSizeOptions).ValidPageSizeOptions().WithMessage(localizationService.GetResourceAsync("Admin.NopStation.AlgoliaSearch.Configuration.Fields.SearchPagePageSizeOptions.InvalidPageSizeOptions").Result);
            RuleFor(x => x.SearchPageProductsPerPage)
                .GreaterThan(0)
                .When(x => !x.AllowCustomersToSelectPageSize)
                .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.AlgoliaSearch.Configuration.Fields.SearchPageProductsPerPage.GreaterThanZero").Result);
            RuleFor(x => x.DefaultViewMode)
                .NotEmpty()
                .When(x => x.AllowProductViewModeChanging)
                .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.AlgoliaSearch.Configuration.Fields.DefaultViewMode.Required").Result);

            RuleFor(x => x.MaximumCategoriesShowInFilter)
                .GreaterThan(0)
                .When(x => x.AllowCategoryFilter)
                .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.AlgoliaSearch.Configuration.Fields.MaximumCategoriesShowInFilter.GreaterThanZero").Result);
            RuleFor(x => x.MaximumVendorsShowInFilter)
                .GreaterThan(0)
                .When(x => x.AllowVendorFilter)
                .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.AlgoliaSearch.Configuration.Fields.MaximumVendorsShowInFilter.GreaterThanZero").Result);
            RuleFor(x => x.MaximumManufacturersShowInFilter)
                .GreaterThan(0)
                .When(x => x.AllowManufacturerFilter)
                .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.AlgoliaSearch.Configuration.Fields.MaximumManufacturersShowInFilter.GreaterThanZero").Result);
            RuleFor(x => x.MaximumSpecificationsShowInFilter)
                .GreaterThan(0)
                .When(x => x.AllowSpecificationFilter)
                .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.AlgoliaSearch.Configuration.Fields.MaximumSpecificationsShowInFilter.GreaterThanZero").Result);
            RuleFor(x => x.AllowedSortingOptions)
                .NotEmpty()
                .When(x => x.AllowProductSorting)
                .WithMessage(localizationService.GetResourceAsync("Admin.NopStation.AlgoliaSearch.Configuration.Fields.AllowedSortOptions.Required").Result);
        }
    }

    public static class CustomValidators
    {
        public static IRuleBuilderOptions<T, string> ValidPageSizeOptions<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder.Must(m => m != null && ValidPageSize(m));
        }

        private static bool ValidPageSize(string m)
        {
            var items = m.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return !items.Any(x => !int.TryParse(x, out _));
        }
    }
}
