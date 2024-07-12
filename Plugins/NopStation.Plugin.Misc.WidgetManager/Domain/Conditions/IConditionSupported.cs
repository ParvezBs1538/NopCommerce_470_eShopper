namespace NopStation.Plugin.Misc.WidgetManager.Domain.Conditions;

public interface IConditionSupported
{
    bool HasConditionApplied { get; set; }

    bool EnableCondition { get; set; }
}
