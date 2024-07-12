namespace NopStation.Plugin.Payments.BlueSnapHosted
{
    public class BlueSnapDefaults
    {
        public const string PAYMENT_INFO_VIEW_COMPONENT_NAME = "BlueSnap";

        public static string SystemName => "NopStation.Plugin.Payments.BlueSnapHosted";

        public static string SynchronizationTask => "NopStation.Plugin.Payments.BlueSnapHosted.Services.ChargeSynchronizationTask";

        public static int DefaultSynchronizationPeriod => 12;

        public static string SynchronizationTaskName => "Synchronization (BlueSnap plugin)";

        public static string PfTokenAttribute => "PfTokenAttribute";
    }
}
