using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.ProductRequests.Areas.Admin.Models
{
    public record ProductRequestModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Admin.NopStation.ProductRequests.ProductRequests.Fields.Customer")]
        public int CustomerId { get; set; }
        public string Customer { get; set; }

        [NopResourceDisplayName("Admin.NopStation.ProductRequests.ProductRequests.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.NopStation.ProductRequests.ProductRequests.Fields.Link")]
        public string Link { get; set; }

        [NopResourceDisplayName("Admin.NopStation.ProductRequests.ProductRequests.Fields.Description")]
        public string Description { get; set; }

        [NopResourceDisplayName("Admin.NopStation.ProductRequests.ProductRequests.Fields.AdminComment")]
        public string AdminComment { get; set; }

        [NopResourceDisplayName("Admin.NopStation.ProductRequests.ProductRequests.Fields.Store")]
        public int StoreId { get; set; }
        public string Store { get; set; }

        [NopResourceDisplayName("Admin.NopStation.ProductRequests.ProductRequests.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }
    }
}