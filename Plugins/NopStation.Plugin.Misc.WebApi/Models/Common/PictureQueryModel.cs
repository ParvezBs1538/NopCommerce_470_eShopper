namespace Nop.Plugin.NopStation.WebApi.Models.Common
{
    public class PictureQueryModel
    {
        public string Base64Image { get; set; }

        public string FileName { get; set; }

        public string ContentType { get; set; }

        public int LengthInBytes { get; set; }
    }
}
