using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;
using System.Collections.Generic;

namespace NopStation.Plugin.EmailValidator.Abstract
{
    public class AbstractEmailValidatorPermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord ManageAbstract = new PermissionRecord { Name = "NopStation Abstract Email Validator. Manage Abstract", SystemName = "ManageAbstractEmailValidator", Category = "NopStation" };

        public virtual HashSet<(string systemRoleName, PermissionRecord[] permissions)> GetDefaultPermissions()
        {
            return new HashSet<(string, PermissionRecord[])>
            {
                (
                    NopCustomerDefaults.AdministratorsRoleName,
                    new[]
                    {
                        ManageAbstract,
                    }
                )
            };
        }

        public IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                ManageAbstract,
            };
        }
    }
}
