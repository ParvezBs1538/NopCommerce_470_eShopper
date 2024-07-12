using System;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Cms;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Services.Authentication.External;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Logging;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using Nop.Services.Shipping;
using Nop.Services.Shipping.Pickup;

namespace NopStation.Plugin.Misc.Core.Helpers;

public static class NopPlugin
{
    public static async Task<bool> IsEnabledAsync<TPlugin>(string systemName) where TPlugin : class, IPlugin
    {
        try
        {
            var pluginService = NopInstance.Load<IPluginService>();
            var storeContext = NopInstance.Load<IStoreContext>();
            var workContext = NopInstance.Load<IWorkContext>();

            var pluginDescriptor = await pluginService.GetPluginDescriptorBySystemNameAsync<TPlugin>(systemName,
                LoadPluginsMode.InstalledOnly, await workContext.GetCurrentCustomerAsync(), storeContext.GetCurrentStore().Id);

            if (pluginDescriptor == null)
                return false;

            var pluginInstance = pluginDescriptor.Instance<IPlugin>();

            switch (pluginInstance)
            {
                case IPaymentMethod paymentMethod:
                    var paymentPluginManager = NopInstance.Load<IPaymentPluginManager>();
                    return paymentPluginManager.IsPluginActive(paymentMethod);
                case IShippingRateComputationMethod shippingRateComputationMethod:
                    var shippingPluginManager = NopInstance.Load<IShippingPluginManager>();
                    return shippingPluginManager.IsPluginActive(shippingRateComputationMethod);
                case IPickupPointProvider pickupPointProvider:
                    var pickupPluginManager = NopInstance.Load<IPickupPluginManager>();
                    return pickupPluginManager.IsPluginActive(pickupPointProvider);
                case IExternalAuthenticationMethod externalAuthenticationMethod:
                    var authenticationPluginManager = NopInstance.Load<IAuthenticationPluginManager>();
                    return authenticationPluginManager.IsPluginActive(externalAuthenticationMethod);
                case IWidgetPlugin widgetPlugin:
                    var widgetPluginManager = NopInstance.Load<IWidgetPluginManager>();
                    return widgetPluginManager.IsPluginActive(widgetPlugin);
                default:
                    return false;
            }
        }
        catch (Exception ex)
        {
            await NopInstance.Load<ILogger>().ErrorAsync($"Failed to check {systemName}: {ex.Message}", ex);
            return false;
        }
    }

    public static async Task EnablePlugin(this IPlugin plugin, PluginType pluginType)
    {
        try
        {
            var settingService = NopInstance.Load<ISettingService>();
            switch (pluginType)
            {
                case PluginType.PaymentMethod:
                    var paymentSettings = NopInstance.Load<PaymentSettings>();
                    if (!paymentSettings.ActivePaymentMethodSystemNames.Contains(plugin.PluginDescriptor.SystemName))
                    {
                        paymentSettings.ActivePaymentMethodSystemNames.Add(plugin.PluginDescriptor.SystemName);
                        await settingService.SaveSettingAsync(paymentSettings);
                    }
                    break;
                case PluginType.ShippingRateComputationMethod:
                    var shippingSettings = NopInstance.Load<ShippingSettings>();
                    if (!shippingSettings.ActiveShippingRateComputationMethodSystemNames.Contains(plugin.PluginDescriptor.SystemName))
                    {
                        shippingSettings.ActiveShippingRateComputationMethodSystemNames.Add(plugin.PluginDescriptor.SystemName);
                        await settingService.SaveSettingAsync(shippingSettings);
                    }
                    break;
                case PluginType.PickupPointProvider:
                    var shippingSettings1 = NopInstance.Load<ShippingSettings>();
                    if (!shippingSettings1.ActivePickupPointProviderSystemNames.Contains(plugin.PluginDescriptor.SystemName))
                    {
                        shippingSettings1.ActivePickupPointProviderSystemNames.Add(plugin.PluginDescriptor.SystemName);
                        await settingService.SaveSettingAsync(shippingSettings1);
                    }
                    break;
                case PluginType.ExternalAuthenticationMethod:
                    var externalAuthenticationSettings = NopInstance.Load<ExternalAuthenticationSettings>();
                    if (!externalAuthenticationSettings.ActiveAuthenticationMethodSystemNames.Contains(plugin.PluginDescriptor.SystemName))
                    {
                        externalAuthenticationSettings.ActiveAuthenticationMethodSystemNames.Add(plugin.PluginDescriptor.SystemName);
                        await settingService.SaveSettingAsync(externalAuthenticationSettings);
                    }
                    break;
                case PluginType.TaxProvider:
                    break;
                case PluginType.MultiFactorAuthenticationMethod:
                    var multiFactorAuthenticationSettings = NopInstance.Load<MultiFactorAuthenticationSettings>();
                    if (!multiFactorAuthenticationSettings.ActiveAuthenticationMethodSystemNames.Contains(plugin.PluginDescriptor.SystemName))
                    {
                        multiFactorAuthenticationSettings.ActiveAuthenticationMethodSystemNames.Add(plugin.PluginDescriptor.SystemName);
                        await settingService.SaveSettingAsync(multiFactorAuthenticationSettings);
                    }
                    break;
                case PluginType.WidgetPlugin:
                    var widgetSettings = NopInstance.Load<WidgetSettings>();
                    if (!widgetSettings.ActiveWidgetSystemNames.Contains(plugin.PluginDescriptor.SystemName))
                    {
                        widgetSettings.ActiveWidgetSystemNames.Add(plugin.PluginDescriptor.SystemName);
                        await settingService.SaveSettingAsync(widgetSettings);
                    }
                    break;
                default:
                    break;
            }
        }
        catch (Exception ex)
        {
            await NopInstance.Load<ILogger>()
                  .ErrorAsync($"Failed to enable {plugin.PluginDescriptor.SystemName}: {ex.Message}", ex);
        }
    }

    public static async Task DisablePlugin(this IPlugin plugin, PluginType pluginType)
    {
        try
        {
            var settingService = NopInstance.Load<ISettingService>();
            switch (pluginType)
            {
                case PluginType.PaymentMethod:
                    var paymentSettings = NopInstance.Load<PaymentSettings>();
                    if (paymentSettings.ActivePaymentMethodSystemNames.Contains(plugin.PluginDescriptor.SystemName))
                    {
                        paymentSettings.ActivePaymentMethodSystemNames.Remove(plugin.PluginDescriptor.SystemName);
                        await settingService.SaveSettingAsync(paymentSettings);
                    }
                    break;
                case PluginType.ShippingRateComputationMethod:
                    var shippingSettings = NopInstance.Load<ShippingSettings>();
                    if (shippingSettings.ActiveShippingRateComputationMethodSystemNames.Contains(plugin.PluginDescriptor.SystemName))
                    {
                        shippingSettings.ActiveShippingRateComputationMethodSystemNames.Remove(plugin.PluginDescriptor.SystemName);
                        await settingService.SaveSettingAsync(shippingSettings);
                    }
                    break;
                case PluginType.PickupPointProvider:
                    var shippingSettings1 = NopInstance.Load<ShippingSettings>();
                    if (shippingSettings1.ActivePickupPointProviderSystemNames.Contains(plugin.PluginDescriptor.SystemName))
                    {
                        shippingSettings1.ActivePickupPointProviderSystemNames.Remove(plugin.PluginDescriptor.SystemName);
                        await settingService.SaveSettingAsync(shippingSettings1);
                    }
                    break;
                case PluginType.ExternalAuthenticationMethod:
                    var externalAuthenticationSettings = NopInstance.Load<ExternalAuthenticationSettings>();
                    if (externalAuthenticationSettings.ActiveAuthenticationMethodSystemNames.Contains(plugin.PluginDescriptor.SystemName))
                    {
                        externalAuthenticationSettings.ActiveAuthenticationMethodSystemNames.Remove(plugin.PluginDescriptor.SystemName);
                        await settingService.SaveSettingAsync(externalAuthenticationSettings);
                    }
                    break;
                case PluginType.TaxProvider:
                    break;
                case PluginType.MultiFactorAuthenticationMethod:
                    var multiFactorAuthenticationSettings = NopInstance.Load<MultiFactorAuthenticationSettings>();
                    if (multiFactorAuthenticationSettings.ActiveAuthenticationMethodSystemNames.Contains(plugin.PluginDescriptor.SystemName))
                    {
                        multiFactorAuthenticationSettings.ActiveAuthenticationMethodSystemNames.Remove(plugin.PluginDescriptor.SystemName);
                        await settingService.SaveSettingAsync(multiFactorAuthenticationSettings);
                    }
                    break;
                case PluginType.WidgetPlugin:
                    var widgetSettings = NopInstance.Load<WidgetSettings>();
                    if (widgetSettings.ActiveWidgetSystemNames.Contains(plugin.PluginDescriptor.SystemName))
                    {
                        widgetSettings.ActiveWidgetSystemNames.Remove(plugin.PluginDescriptor.SystemName);
                        await settingService.SaveSettingAsync(widgetSettings);
                    }
                    break;
                default:
                    break;
            }
        }
        catch (Exception ex)
        {
            await NopInstance.Load<ILogger>()
                  .ErrorAsync($"Failed to enable {plugin.PluginDescriptor.SystemName}: {ex.Message}", ex);
        }
    }
}
