using System;
using Nop.Core;
using ValidationEntryStatus = Verifalia.Api.EmailValidations.Models.ValidationEntryStatus;

namespace NopStation.Plugin.EmailValidator.Verifalia.Domains
{
    public class VerifaliaEmail : BaseEntity
    {
        public string Email { get; set; }

        public int StatusId { get; set; }

        public string Classification { get; set; }

        public bool IsDisposable { get; set; }

        public bool IsFree { get; set; }

        public bool IsRoleAccount { get; set; }

        public DateTime CreatedOnUtc { get; set; }

        public DateTime UpdatedOnUtc { get; set; }

        public int ValidationCount { get; set; }


        public ValidationEntryStatus Status
        {
            get => (ValidationEntryStatus)StatusId;
            set => StatusId = (int)value;
        }
    }
}
