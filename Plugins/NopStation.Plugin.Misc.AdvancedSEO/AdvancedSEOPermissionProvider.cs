using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;

namespace NopStation.Plugin.Misc.AdvancedSEO
{
    public class AdvancedSEOPermissionProvider : IPermissionProvider
    {
        #region Fields
        #endregion

        #region Ctor

        public static readonly PermissionRecord ManageAdvancedSEOConfiguration = new PermissionRecord { Name = "NopStation AdvancedSEO. Manage Configuration", SystemName = "NopStationAdvancedSEOManageConfiguration", Category = "NopStation" };

        public static readonly PermissionRecord ManageAdvancedSEOCategoryTemplates = new PermissionRecord { Name = "NopStation AdvancedSEO. Manage Advanced SEO Category Templates", SystemName = "NopStationAdvancedSEOManageCategoryTemplates", Category = "NopStation" };

        public static readonly PermissionRecord ManageAdvancedSEOManufacturerTemplates = new PermissionRecord { Name = "NopStation AdvancedSEO. Manage Advanced SEO Manufacturer Templates", SystemName = "NopStationAdvancedSEOManageManufacturerTemplates", Category = "NopStation" };

        public static readonly PermissionRecord ManageAdvancedSEOProductTemplates = new PermissionRecord { Name = "NopStation AdvancedSEO. Manage Advanced SEO Product Templates", SystemName = "NopStationAdvancedSEOManageProductTemplates", Category = "NopStation" };

        #endregion

        #region Utilites



        #endregion

        #region Methods

        public HashSet<(string systemRoleName, PermissionRecord[] permissions)> GetDefaultPermissions()
        {
            return new HashSet<(string, PermissionRecord[])>
            {
                (
                    NopCustomerDefaults.AdministratorsRoleName,
                    new[]
                    {
                        ManageAdvancedSEOConfiguration,
                        ManageAdvancedSEOCategoryTemplates,
                        ManageAdvancedSEOManufacturerTemplates,
                        ManageAdvancedSEOProductTemplates,
                    }
                )
            };
        }

        public IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                ManageAdvancedSEOConfiguration,
                ManageAdvancedSEOCategoryTemplates,
                ManageAdvancedSEOManufacturerTemplates,
                ManageAdvancedSEOProductTemplates,
            };
        }

        #endregion
    }
}
