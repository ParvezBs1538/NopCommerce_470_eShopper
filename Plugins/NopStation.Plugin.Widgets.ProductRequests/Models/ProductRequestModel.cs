using System;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.ProductRequests.Models
{
    public record ProductRequestModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("NopStation.ProductRequests.ProductRequests.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("NopStation.ProductRequests.ProductRequests.Fields.Link")]
        public string Link { get; set; }

        [NopResourceDisplayName("NopStation.ProductRequests.ProductRequests.Fields.Description")]
        public string Description { get; set; }

        [NopResourceDisplayName("NopStation.ProductRequests.ProductRequests.Fields.AdminComment")]
        public string AdminComment { get; set; }

        [NopResourceDisplayName("NopStation.ProductRequests.ProductRequests.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }

        public bool LinkRequired { get; set; }

        public bool DescriptionRequired { get; set; }
    }
}