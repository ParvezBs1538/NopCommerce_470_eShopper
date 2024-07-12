using System.Threading.Tasks;

namespace NopStation.Plugin.Misc.AmazonS3.Services
{
    public interface IAmazonS3Helper
    {
        string GetS3ImageLocalPath();

        string GetS3CachePath();

        Task<byte[]> S3ReadAllBytesFromThumbs(int pictureId, string fileName, string lastPart);

        Task<byte[]> S3ReadAllBytesFromImages(int pictureId, string fileName, string lastPart);
    }
}
