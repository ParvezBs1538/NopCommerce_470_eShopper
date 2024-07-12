namespace NopStation.Plugin.Misc.WidgetManager.Domain.Conditions;

public interface ICustomerConditionSupported
{
    bool HasCustomerConditionApplied { get; set; }
}
