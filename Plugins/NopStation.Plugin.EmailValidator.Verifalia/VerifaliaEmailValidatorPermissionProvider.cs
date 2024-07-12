using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;
using System.Collections.Generic;

namespace NopStation.Plugin.EmailValidator.Verifalia
{
    public class VerifaliaEmailValidatorPermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord ManageVerifalia = new PermissionRecord { Name = "NopStation Verifalia Email Validator. Manage Verifalia", SystemName = "ManageVerifaliaEmailValidator", Category = "NopStation" };

        public virtual HashSet<(string systemRoleName, PermissionRecord[] permissions)> GetDefaultPermissions()
        {
            return new HashSet<(string, PermissionRecord[])>
            {
                (
                    NopCustomerDefaults.AdministratorsRoleName,
                    new[]
                    {
                        ManageVerifalia,
                    }
                )
            };
        }

        public IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                ManageVerifalia,
            };
        }
    }
}
