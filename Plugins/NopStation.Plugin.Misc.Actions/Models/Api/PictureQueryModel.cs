namespace NopStation.Plugin.Misc.Core.Models.Api;

public class PictureQueryModel
{
    public string Base64Image { get; set; }

    public string FileName { get; set; }

    public string ContentType { get; set; }

    public int LengthInBytes { get; set; }
}
