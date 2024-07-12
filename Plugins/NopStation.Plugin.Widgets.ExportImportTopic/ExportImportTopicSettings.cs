using Nop.Core.Configuration;

namespace NopStation.Plugin.Widgets.ExportImportTopic
{
    public class ExportImportTopicSettings : ISettings
    {
        public bool CheckBodyMaximumLength { get; set; }

        public int BodyMaximumLength { get; set; }
    }
}
