using System;
using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.AmazonPersonalize.Areas.Admin.Models
{
    public record UploadInteractionsDataModel
    {
        [NopResourceDisplayName("Admin.NopStation.AmazonPersonalize.UploadInteractionsData.StartDate")]
        [UIHint("DateNullable")]
        public DateTime? StartDate { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AmazonPersonalize.UploadInteractionsData.EndDate")]
        [UIHint("DateNullable")]
        public DateTime? EndDate { get; set; }
    }
}