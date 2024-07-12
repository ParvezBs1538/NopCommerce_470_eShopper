namespace NopStation.Plugin.Misc.AlgoliaSearch
{
    public class AlgoliaDefaults
    {
        public static string ScheduleTaskType = "NopStation.Plugin.Misc.AlgoliaSearch.AlgoliaUpdateItemTask";

        public static string Delimiter = "____";

        public static string DefaultIndexName = "Products";
        public static string MultilingualProductNameFormate => "Product_Name_{0}";

        public static string[] SearchableAttributes = new string[] { "Keywords", "FilterableCategories.Name",
            "FilterableManufacturers.Name", "Name", "FilterableVendor.Name", "FilterableSpecifications.ValueRaw",
            "Sku", "GTIN","ordered(Name)" ,"ProductCombinations.GTIN" ,"ProductCombinations.Sku" };

        public static string[] FacetedAttributes = new string[] { "FilterableSpecifications.OptionIdSpecificationId",
            "Price", "FilterableCategories.Id", "FilterableVendor.Id", "FilterableManufacturers.Id", "Rating",
            "searchable(Sku)", "searchable(SeName)" };

        public static string[] RatingResourceKey = new string[] {
            "NopStation.AlgoliaSearch.Filterings.Ratings.OneStar",
            "NopStation.AlgoliaSearch.Filterings.Ratings.TwoStar",
            "NopStation.AlgoliaSearch.Filterings.Ratings.ThreeStar",
            "NopStation.AlgoliaSearch.Filterings.Ratings.FourStar",
            "NopStation.AlgoliaSearch.Filterings.Ratings.FiveStar"
        };
    }
}
