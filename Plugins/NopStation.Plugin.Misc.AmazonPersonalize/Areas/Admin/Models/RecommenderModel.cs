using System;
using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Misc.AmazonPersonalize.Areas.Admin.Models
{
    public record RecommenderModel : BaseNopEntityModel
    {
        public DateTime CreationDateTime { get; set; }
        public DateTime LastUpdatedDateTime { get; set; }
        public string DatasetGroupArn { get; set; }
        public string Name { get; set; }
        public string RecommenderArn { get; set; }
        public string RecipeArn { get; set; }
        public string Status { get; set; }
    }
}