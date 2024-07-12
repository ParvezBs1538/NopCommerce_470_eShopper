using Nop.Services.Plugins;
using Nop.Services.Cms;
using System.Collections.Generic;
using Nop.Web.Framework.Infrastructure;
using System.Threading.Tasks;
using Nop.Services.Localization;
using NopStation.Plugin.Misc.Core.Services;
using NopStation.Plugin.Widgets.BulkDeleteCustomer.Components;
using System;

namespace NopStation.Plugin.Widgets.BulkDeleteCustomer
{
    public class BulkDeleteCustomerPlugin : BasePlugin, IWidgetPlugin, INopStationPlugin
    {
        #region Fields
        
        public bool HideInWidgetList => false;

        #endregion

        #region Methods

        public Type GetWidgetViewComponent(string widgetZone)
        {
            return typeof(BulkDeleteCustomerViewComponent);
        }

        public override async Task InstallAsync()
        {
            await this.InstallPluginAsync(null);
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await this.UninstallPluginAsync(null);
            await base.UninstallAsync();
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            var list = new List<KeyValuePair<string, string>>
            {
                new("Admin.NopStation.BulkDeleteCustomer.CustomersDeleted", "{0} customer(s) deleted successfully."),
                new("Admin.NopStation.BulkDeleteCustomer.DeletSelectedConfirmation", "Are you sure you want to delete these customers?"),
                new("Admin.NopStation.BulkDeleteCustomer.SelectCustomer", "Select at least 1 customer.")
            };

            return list;
        }

        public Task<IList<string>> GetWidgetZonesAsync()
        {
            return Task.FromResult<IList<string>>(new List<string>
            {
                AdminWidgetZones.CustomerListButtons
            });
        }

        #endregion
    }
}
