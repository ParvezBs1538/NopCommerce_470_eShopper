namespace NopStation.Plugin.Widgets.ProductBadge.Models;

public class CssModel
{
    public CssModel()
    {
        Overview = new DisplayModel();
        Details = new DisplayModel();
    }

    public DisplayModel Overview { get; set; }

    public DisplayModel Details { get; set; }


    public class DisplayModel
    {
        public DisplayModel()
        {
            Small = new SizeModel();
            Medium = new SizeModel();
            Large = new SizeModel();
        }

        public SizeModel Small { get; set; }
        public SizeModel Medium { get; set; }
        public SizeModel Large { get; set; }
    }

    public class SizeModel
    {
        public decimal Width { get; set; }

        public decimal PentagonHeight { get; set; }
        public decimal PentagonSideLength { get; set; }
    }
}