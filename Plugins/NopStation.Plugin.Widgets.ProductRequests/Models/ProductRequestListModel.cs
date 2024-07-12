using System.Collections.Generic;

namespace NopStation.Plugin.Widgets.ProductRequests.Models
{
    public class ProductRequestListModel
    {
        public ProductRequestListModel()
        {
            Requests = new List<ProductRequestModel>();
        }

        public IList<ProductRequestModel> Requests { get; set; }
    }
}
