namespace NopStation.Plugin.Widgets.ProgressiveWebApp.Models
{
    public record FooterComponentModel
    {
        public string PublicKey { get; set; }

        public bool CheckPushManagerSubscription { get; set; }
    }
}
