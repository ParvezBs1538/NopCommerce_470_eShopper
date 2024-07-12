using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.WebApi.Areas.Admin.Models
{
    public record SliderModel : BaseNopEntityModel
    {
        public SliderModel()
        {
            AvailableSliderTypes = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.NopStation.WebApi.Sliders.Fields.Picture")]
        [UIHint("Picture")]
        public int PictureId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApi.Sliders.Fields.Picture")]
        public string PictureUrl { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApi.Sliders.Fields.ActiveStartDate")]
        [UIHint("DateTimeNullable")]
        public DateTime? ActiveStartDate { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApi.Sliders.Fields.ActiveEndDate")]
        [UIHint("DateTimeNullable")]
        public DateTime? ActiveEndDate { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApi.Sliders.Fields.SliderType")]
        public int SliderTypeId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApi.Sliders.Fields.SliderType")]
        public string SliderTypeStr { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApi.Sliders.Fields.EntityId")]
        public int EntityId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApi.Sliders.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [NopResourceDisplayName("Admin.NopStation.WebApi.Sliders.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }

        public IList<SelectListItem> AvailableSliderTypes { get; set; }
    }
}
