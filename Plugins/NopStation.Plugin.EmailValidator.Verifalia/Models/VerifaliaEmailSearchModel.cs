using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.EmailValidator.Verifalia.Models
{
    public record VerifaliaEmailSearchModel : BaseSearchModel
    {
        public VerifaliaEmailSearchModel()
        {
            AvailableStatusItems = new List<SelectListItem>();
            AvailableClassificationItems = new List<SelectListItem>();
            AvailableBooleanItems = new List<SelectListItem>();
            SearchStatusIds = new List<int>();
        }

        [NopResourceDisplayName("Admin.NopStation.VerifaliaEmailValidator.VerifaliaEmails.List.SearchEmail")]
        public string SearchEmail { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VerifaliaEmailValidator.VerifaliaEmails.List.SearchStatus")]
        public IList<int> SearchStatusIds { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VerifaliaEmailValidator.VerifaliaEmails.List.SearchClassification")]
        public string SearchClassification { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VerifaliaEmailValidator.VerifaliaEmails.List.SearchDisposable")]
        public int SearchDisposableId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VerifaliaEmailValidator.VerifaliaEmails.List.SearchFree")]
        public int SearchFreeId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VerifaliaEmailValidator.VerifaliaEmails.List.SearchRoleAccount")]
        public int SearchRoleAccountId { get; set; }

        [UIHint("DateTimeNullable")]
        [NopResourceDisplayName("Admin.NopStation.VerifaliaEmailValidator.VerifaliaEmails.List.SearchCreatedFrom")]
        public DateTime? SearchCreatedFrom { get; set; }

        [UIHint("DateTimeNullable")]
        [NopResourceDisplayName("Admin.NopStation.VerifaliaEmailValidator.VerifaliaEmails.List.SearchCreatedTo")]
        public DateTime? SearchCreatedTo { get; set; }

        public IList<SelectListItem> AvailableStatusItems { get; set; }
        public IList<SelectListItem> AvailableClassificationItems { get; set; }
        public IList<SelectListItem> AvailableBooleanItems { get; set; }
    }
}