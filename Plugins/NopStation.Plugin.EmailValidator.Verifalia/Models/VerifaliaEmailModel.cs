using System;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.EmailValidator.Verifalia.Models
{
    public record VerifaliaEmailModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Admin.NopStation.VerifaliaEmailValidator.VerifaliaEmails.Fields.Email")]
        public string Email { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VerifaliaEmailValidator.VerifaliaEmails.Fields.Status")]
        public int StatusId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VerifaliaEmailValidator.VerifaliaEmails.Fields.Status")]
        public string Status { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VerifaliaEmailValidator.VerifaliaEmails.Fields.Classification")]
        public string Classification { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VerifaliaEmailValidator.VerifaliaEmails.Fields.IsDisposable")]
        public bool IsDisposable { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VerifaliaEmailValidator.VerifaliaEmails.Fields.IsFree")]
        public bool IsFree { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VerifaliaEmailValidator.VerifaliaEmails.Fields.IsRoleAccount")]
        public bool IsRoleAccount { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VerifaliaEmailValidator.VerifaliaEmails.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VerifaliaEmailValidator.VerifaliaEmails.Fields.UpdatedOn")]
        public DateTime UpdatedOn { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VerifaliaEmailValidator.VerifaliaEmails.Fields.ValidationCount")]
        public int ValidationCount { get; set; }
    }
}