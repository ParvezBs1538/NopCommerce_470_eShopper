namespace NopStation.Plugin.Misc.AlgoliaSearch.Models
{
    public class AlgoliaSearchModel
    {
        public bool AutoCompleteEnabled { get; set; }
        public bool ShowProductImagesInSearchAutoComplete { get; set; }
        public int SearchTermMinimumLength { get; set; }
        public string SearchOnlyKey { get; set; }
        public string ApplicationId { get; set; }
        public int AutoCompleteListSize { get; set; }
        public bool HidePoweredByAlgolia { get; set; }
    }
}
