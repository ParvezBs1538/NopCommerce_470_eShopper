using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;

namespace NopStation.Plugin.Widgets.ProductQuestionAnswer
{
    public class ProductQAPermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord ManageProductQA = new PermissionRecord { Name = "NopStation product Q&A. Manage product Q&A", SystemName = "ManageNopStationProductQA", Category = "NopStation" };
        public static readonly PermissionRecord ManageConfiguration = new PermissionRecord { Name = "NopStation product Q&A. Manage Configuration", SystemName = "ManageNopStationProductQAConfiguration", Category = "NopStation" };
        public virtual IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                ManageProductQA,
                ManageConfiguration
            };
        }
        public virtual HashSet<(string systemRoleName, PermissionRecord[] permissions)> GetDefaultPermissions()
        {

            return new HashSet<(string, PermissionRecord[])>
            {
                (
                    NopCustomerDefaults.AdministratorsRoleName,
                    new[]
                    {
                      ManageProductQA,
                      ManageConfiguration
                    }
                )
            };
        }
    }
}
