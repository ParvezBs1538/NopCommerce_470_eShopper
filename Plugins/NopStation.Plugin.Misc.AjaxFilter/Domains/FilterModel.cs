namespace NopStation.Plugin.Misc.AjaxFilter.Domains
{
    public class FilterModel
    {
        public string FilterType { get; set; }

        public int FilterId { get; set; }

        public int FilterCount { get; set; }

        public string Name { get; set; }

        public string Color { get; set; }

        public int SpecificationAttributeId { get; set; }

        public int ProductAttributeId { get; set; }
    }
}
