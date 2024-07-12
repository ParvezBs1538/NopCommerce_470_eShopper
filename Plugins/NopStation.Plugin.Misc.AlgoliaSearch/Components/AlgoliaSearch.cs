using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Misc.AlgoliaSearch.Models;
using NopStation.Plugin.Misc.Core.Components;

namespace NopStation.Plugin.Misc.AlgoliaSearch.Components
{
    public class AlgoliaSearchViewComponent : NopStationViewComponent
    {
        private readonly AlgoliaSearchSettings _algoliaSearchSettings;

        public AlgoliaSearchViewComponent(AlgoliaSearchSettings algoliaSearchSettings)
        {
            _algoliaSearchSettings = algoliaSearchSettings;
        }

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            var model = new AlgoliaSearchModel()
            {
                ApplicationId = _algoliaSearchSettings.ApplicationId,
                AutoCompleteEnabled = _algoliaSearchSettings.EnableAutoComplete,
                SearchOnlyKey = _algoliaSearchSettings.SearchOnlyKey,
                SearchTermMinimumLength = _algoliaSearchSettings.SearchTermMinimumLength,
                ShowProductImagesInSearchAutoComplete = _algoliaSearchSettings.ShowProductImagesInSearchAutoComplete,
                AutoCompleteListSize = _algoliaSearchSettings.AutoCompleteListSize,
                HidePoweredByAlgolia = _algoliaSearchSettings.HidePoweredByAlgolia
            };

            return View(model);
        }
    }
}
