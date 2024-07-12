using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.EmailValidator.Abstract.Models
{
    public record AbstractEmailSearchModel : BaseSearchModel
    {
        public AbstractEmailSearchModel()
        {
            AvailableDeliverabilityItems = new List<SelectListItem>();
            AvailableBooleanItems = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.NopStation.AbstractEmailValidator.AbstractEmails.List.SearchEmail")]
        public string SearchEmail { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AbstractEmailValidator.AbstractEmails.List.SearchDeliverability")]
        public string SearchDeliverability { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AbstractEmailValidator.AbstractEmails.List.SearchDisposable")]
        public int SearchDisposableId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AbstractEmailValidator.AbstractEmails.List.SearchFree")]
        public int SearchFreeId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AbstractEmailValidator.AbstractEmails.List.SearchRoleAccount")]
        public int SearchRoleAccountId { get; set; }

        [UIHint("DateTimeNullable")]
        [NopResourceDisplayName("Admin.NopStation.AbstractEmailValidator.AbstractEmails.List.SearchCreatedFrom")]
        public DateTime? SearchCreatedFrom { get; set; }

        [UIHint("DateTimeNullable")]
        [NopResourceDisplayName("Admin.NopStation.AbstractEmailValidator.AbstractEmails.List.SearchCreatedTo")]
        public DateTime? SearchCreatedTo { get; set; }

        public IList<SelectListItem> AvailableDeliverabilityItems { get; set; }
        public IList<SelectListItem> AvailableBooleanItems { get; set; }
    }
}