using System.Collections.Generic;

namespace NopStation.Plugin.Misc.WebApi.Models.Catalog
{
    public class CategoryTreeModel
    {
        public CategoryTreeModel()
        {
            SubCategories = new List<CategoryTreeModel>();
        }

        public int CategoryId { get; set; }

        public string SeName { get; set; }

        public string Name { get; set; }

        public string IconUrl { get; set; }

        public IList<CategoryTreeModel> SubCategories { get; set; }
    }
}
