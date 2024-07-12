using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Nop.Core.Domain.Logging;
using Nop.Services.Logging;
using NopStation.Plugin.Misc.AmazonS3.Models;

namespace NopStation.Plugin.Misc.AmazonS3.Services
{
    public class AmazonS3Helper : IAmazonS3Helper
    {
        #region Field

        private readonly AmazonS3Settings _amazonS3Settings;
        private readonly ILogger _logger;
        #endregion

        #region Ctor
        public AmazonS3Helper(AmazonS3Settings amazonS3Settings, ILogger logger)
        {
            _amazonS3Settings = amazonS3Settings;
            _logger = logger;
        }

        #endregion

        public static string FavExistsCacheKey => "Nop.aws.fav.existsCache";

        #region Utilities
        private RegionEndpoint GetRegionEndpoint(RegionEndpointEnum regionEnum)
        {
            switch (regionEnum)
            {
                case RegionEndpointEnum.USEast1:
                    return RegionEndpoint.USEast1;

                case RegionEndpointEnum.CACentral1:
                    return RegionEndpoint.CACentral1;

                case RegionEndpointEnum.CNNorthWest1:
                    return RegionEndpoint.CNNorthWest1;

                case RegionEndpointEnum.CNNorth1:
                    return RegionEndpoint.CNNorth1;

                case RegionEndpointEnum.USGovCloudWest1:
                    return RegionEndpoint.USGovCloudWest1;

                case RegionEndpointEnum.SAEast1:
                    return RegionEndpoint.SAEast1;

                case RegionEndpointEnum.APSoutheast1:
                    return RegionEndpoint.APSoutheast1;

                case RegionEndpointEnum.APSouth1:
                    return RegionEndpoint.APSouth1;

                case RegionEndpointEnum.APNortheast2:
                    return RegionEndpoint.APNortheast2;

                case RegionEndpointEnum.APSoutheast2:
                    return RegionEndpoint.APSoutheast2;

                case RegionEndpointEnum.EUCentral1:
                    return RegionEndpoint.EUCentral1;

                case RegionEndpointEnum.EUWest3:
                    return RegionEndpoint.EUWest3;

                case RegionEndpointEnum.EUWest2:
                    return RegionEndpoint.EUWest2;

                case RegionEndpointEnum.EUWest1:
                    return RegionEndpoint.EUWest1;

                case RegionEndpointEnum.USWest2:
                    return RegionEndpoint.USWest2;

                case RegionEndpointEnum.USWest1:
                    return RegionEndpoint.USWest1;

                case RegionEndpointEnum.USEast2:
                    return RegionEndpoint.USEast2;

                case RegionEndpointEnum.APNortheast1:
                    return RegionEndpoint.APNortheast1;
            }
            return RegionEndpoint.CACentral1;
        }

        #endregion

        #region  AWS Helper

        /// <summary>
        /// Get File Cache Path
        /// </summary>
        /// <returns></returns>
        public string GetS3CachePath()
        {
            return $"{_amazonS3Settings.AWSS3BucketName}/caches";
        }

        /// <summary>
        /// Get S3 image thumb path
        /// </summary>
        /// <returns></returns>
        public string GetS3ImageThumbPath()
        {
            return $"{_amazonS3Settings.AWSS3BucketName}/images/thumbs";
        }

        /// <summary>
        /// Get S3 image path (local)
        /// </summary>
        /// <returns></returns>
        public string GetS3ImageLocalPath()
        {
            return $"{_amazonS3Settings.AWSS3BucketName}/images";
        }

        /// <summary>
        /// Read image form thumbs
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public async Task<byte[]> S3ReadAllBytesFromThumbs(int pictureId, string fileName, string lastPart)
        {
            try
            {
                if (string.IsNullOrEmpty(fileName))
                    fileName = $"{pictureId:0000000}.{lastPart}";
                else
                    fileName = $"{pictureId:0000000}_{fileName}.{lastPart}";

                var prefix = $"images/thumbs/{fileName}";

                using (var client = GetS3Client())
                {
                    var getObjectRequest = new GetObjectRequest()
                    {
                        BucketName = GetS3ImageThumbPath(),
                        Key = fileName
                    };

                    using (var getObjectResponse = await client.GetObjectAsync(getObjectRequest))
                    {
                        if (getObjectResponse.HttpStatusCode == HttpStatusCode.OK)
                        {
                            using (var responseStream = getObjectResponse.ResponseStream)
                            {
                                var bytes = S3ReadStream(responseStream);
                                return bytes;
                            }
                        }
                        else
                        {
                            return new byte[0];
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync(ex.Message, ex);
                return new byte[0];
            }
        }

        public async Task<byte[]> S3ReadAllBytesFromImages(int pictureId, string fileName, string lastPart)
        {
            try
            {
                fileName = $"{pictureId:0000000}_0.{lastPart}";
                var prefix = $"images/{fileName}";

                using (var client = GetS3Client())
                {
                    var getObjectRequest = new GetObjectRequest()
                    {
                        BucketName = GetS3ImageLocalPath(),
                        Key = fileName
                    };

                    using (var getObjectResponse = await client.GetObjectAsync(getObjectRequest))
                    {
                        if (getObjectResponse.HttpStatusCode == HttpStatusCode.OK)
                        {
                            using (Stream responseStream = getObjectResponse.ResponseStream)
                            {
                                var bytes = S3ReadStream(responseStream);
                                return bytes;
                            }
                        }
                        else
                        {
                            return Array.Empty<byte>();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync(ex.Message, ex);
                return Array.Empty<byte>();
            }
        }

        /// <summary>
        /// Read Stream From S3
        /// </summary>
        /// <param name="responseStream"></param>
        /// <returns></returns>
        public byte[] S3ReadStream(Stream responseStream)
        {
            var buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = responseStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.WriteAsync(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Get AWS S3 Client with credentials
        /// </summary>
        /// <returns></returns>
        private IAmazonS3 GetS3Client()
        {
            return new AmazonS3Client(_amazonS3Settings.AWSS3AccessKeyId, _amazonS3Settings.AWSS3SecretKey, GetRegionEndpoint((RegionEndpointEnum)_amazonS3Settings.RegionEndpointId));
        }

        #endregion
    }
}
