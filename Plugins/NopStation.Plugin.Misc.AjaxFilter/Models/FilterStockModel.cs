namespace NopStation.Plugin.Misc.AjaxFilter.Models
{
    public class FilterStockModel
    {
        public bool OnlyInStock { get; set; }
        public bool NotInStock { get; set; }
        public int OnlyInStockQuantity { get; set; }
    }
}
