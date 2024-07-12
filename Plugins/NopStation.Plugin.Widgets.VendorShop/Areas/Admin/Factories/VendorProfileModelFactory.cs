using System;
using System.Threading.Tasks;
using Nop.Services.Localization;
using Nop.Web.Framework.Factories;
using NopStation.Plugin.Widgets.VendorShop.Areas.Admin.Models;
using NopStation.Plugin.Widgets.VendorShop.Domains;

namespace NopStation.Plugin.Widgets.VendorShop.Areas.Admin.Factories
{
    public class VendorProfileModelFactory : IVendorProfileModelFactory
    {
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedModelFactory _localizedModelFactory;

        public VendorProfileModelFactory(ILocalizationService localizationService,
            ILocalizedModelFactory localizedModelFactory)
        {
            _localizationService = localizationService;
            _localizedModelFactory = localizedModelFactory;
        }
        public async Task<VendorProfileModel> PrepareVendorProfileModelAsync(
            VendorProfileModel model,
            VendorProfile vendorProfile)
        {
            Func<VendorProfileLocalizedModel, int, Task> localizedModelConfiguration = null;

            if (vendorProfile != null)
            {
                //define localized model configuration action
                localizedModelConfiguration = async (locale, languageId) =>
                {
                    locale.Description = await _localizationService.GetLocalizedAsync(vendorProfile, entity => entity.Description, languageId, false, false);
                };

                model.Id = vendorProfile.Id;
                model.ProfilePictureId = vendorProfile.ProfilePictureId;
                model.BannerPictureId = vendorProfile.BannerPictureId;
                model.MobileBannerPictureId = vendorProfile.MobileBannerPictureId;
                model.Description = vendorProfile.Description;
                model.CustomCss = vendorProfile.CustomCss;
                model.VendorId = vendorProfile.VendorId;
            }

            model.Locales = await _localizedModelFactory.PrepareLocalizedModelsAsync(localizedModelConfiguration);

            return model;
        }
    }
}
