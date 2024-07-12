using System;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.EmailValidator.Abstract.Models
{
    public record AbstractEmailModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Admin.NopStation.AbstractEmailValidator.AbstractEmails.Fields.Email")]
        public string Email { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AbstractEmailValidator.AbstractEmails.Fields.QualityScore")]
        public decimal QualityScore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AbstractEmailValidator.AbstractEmails.Fields.Deliverability")]
        public string Deliverability { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AbstractEmailValidator.AbstractEmails.Fields.IsDisposable")]
        public bool IsDisposable { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AbstractEmailValidator.AbstractEmails.Fields.IsFree")]
        public bool IsFree { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AbstractEmailValidator.AbstractEmails.Fields.IsRoleAccount")]
        public bool IsRoleAccount { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AbstractEmailValidator.AbstractEmails.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AbstractEmailValidator.AbstractEmails.Fields.UpdatedOn")]
        public DateTime UpdatedOn { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AbstractEmailValidator.AbstractEmails.Fields.ValidationCount")]
        public int ValidationCount { get; set; }
    }
}