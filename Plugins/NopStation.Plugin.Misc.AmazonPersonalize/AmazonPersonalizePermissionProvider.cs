using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;

namespace NopStation.Plugin.Misc.AmazonPersonalize
{
    public class AmazonPersonalizePermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord ManageConfiguration = new PermissionRecord { Name = "NopStation Amazon Personalize. Manage Configuration", SystemName = "ManageNopStationAmazonPersonalizeConfiguration", Category = "NopStation" };
       
        public static readonly PermissionRecord ManageRecommenders = new PermissionRecord { Name = "NopStation Amazon Personalize. Manage Recommenders", SystemName = "ManageNopStationAmazonPersonalizeRecommenders", Category = "NopStation" };
        public static readonly PermissionRecord ManageUploadInteractions = new PermissionRecord { Name = "NopStation Amazon Personalize. Manage Upload Interactions ", SystemName = "ManageNopStationAmazonPersonalizeUploadInteractions", Category = "NopStation" };

        public virtual IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                ManageConfiguration,
                ManageRecommenders,
                ManageUploadInteractions
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
                        ManageConfiguration,
                        ManageRecommenders,
                        ManageUploadInteractions
                    }
                )
            };
        }
    }
}