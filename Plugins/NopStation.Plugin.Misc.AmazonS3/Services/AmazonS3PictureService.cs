using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Media;
using Nop.Core.Events;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Seo;
using NopStation.Plugin.Misc.AmazonS3.Models;
using SkiaSharp;

namespace NopStation.Plugin.Misc.AmazonS3.Services
{
    public class AmazonS3PictureService : PictureService
    {
        #region Fields

        private readonly IEventPublisher _eventPublisher;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly AmazonS3Settings _amazonS3Settings;
        private readonly IStaticCacheManager _cacheManager;
        private readonly IAmazonS3Helper _amazonS3Helper;

        #endregion

        #region Ctor

        public AmazonS3PictureService(IDownloadService downloadService,
            IStaticCacheManager staticCacheManager,
            IEventPublisher eventPublisher,
            IHttpContextAccessor httpContextAccessor,
            INopFileProvider fileProvider,
            IProductAttributeParser productAttributeParser,
            IRepository<Picture> pictureRepository,
            IRepository<PictureBinary> pictureBinaryRepository,
            IRepository<ProductPicture> productPictureRepository,
            ISettingService settingService,
            IUrlRecordService urlRecordService,
            IWebHelper webHelper,
            MediaSettings mediaSettings,
            AmazonS3Settings amazonS3Settings,
            IStaticCacheManager cacheManager,
            IAmazonS3Helper amazonS3Helper,
            ILogger logger,
            IProductAttributeService productAttributeService) : base(
                downloadService,
                httpContextAccessor,
                logger,
                fileProvider,
                productAttributeParser,
                productAttributeService,
                pictureRepository,
                pictureBinaryRepository,
                productPictureRepository,
                settingService,
                urlRecordService,
                webHelper,
                mediaSettings)
        {
            _staticCacheManager = staticCacheManager;
            _eventPublisher = eventPublisher;
            _amazonS3Settings = amazonS3Settings;
            _cacheManager = cacheManager;
            _amazonS3Helper = amazonS3Helper;
        }

        #endregion

        #region Methods

        public override async Task<Picture> InsertPictureAsync(byte[] pictureBinary, string mimeType, string seoFilename,
            string altAttribute = null, string titleAttribute = null, bool isNew = true, bool validateBinary = true)
        {
            mimeType = CommonHelper.EnsureNotNull(mimeType);
            mimeType = CommonHelper.EnsureMaximumLength(mimeType, 20);

            seoFilename = CommonHelper.EnsureMaximumLength(seoFilename, 100);

            if (validateBinary)
                pictureBinary = await ValidatePictureAsync(pictureBinary, mimeType, seoFilename);

            var picture = new Picture
            {
                MimeType = mimeType,
                SeoFilename = seoFilename,
                AltAttribute = altAttribute,
                TitleAttribute = titleAttribute,
                IsNew = isNew
            };
            await _pictureRepository.InsertAsync(picture);
            await UpdatePictureBinaryAsync(picture, new byte[0]);

            if (!await IsStoreInDbAsync())
                await SavePictureInFileAsync(picture.Id, pictureBinary, mimeType);

            //event notification
            await _eventPublisher.EntityInsertedAsync(picture);

            return picture;
        }

        public override async Task<string> GetDefaultPictureUrlAsync(int targetSize = 0,
            PictureType defaultPictureType = PictureType.Entity, string storeLocation = null)
        {
            string defaultImageFileName;
            switch (defaultPictureType)
            {
                case PictureType.Avatar:
                    defaultImageFileName = await _settingService.GetSettingByKeyAsync("Media.Customer.DefaultAvatarImageName", NopMediaDefaults.DefaultAvatarFileName);
                    break;

                case PictureType.Entity:
                default:
                    defaultImageFileName = await _settingService.GetSettingByKeyAsync("Media.DefaultImageName", NopMediaDefaults.DefaultImageFileName);
                    break;
            }

            if (targetSize == 0)
            {
                var url = (_amazonS3Settings.EnableCdn && !String.IsNullOrEmpty(_amazonS3Settings.CdnBaseUrl)) ? $"{_amazonS3Settings.CdnBaseUrl}/images/default-image.png" : $"{_amazonS3Settings.AWSS3RootUrl}{_amazonS3Settings.AWSS3BucketName}/images/default-image.png";
                return url;
            }
            else
            {
                var thumbFilePath = (_amazonS3Settings.EnableCdn && !String.IsNullOrEmpty(_amazonS3Settings.CdnBaseUrl)) ? $"{_amazonS3Settings.CdnBaseUrl}/images/thumbs/" : $"{_amazonS3Settings.AWSS3RootUrl}{_amazonS3Settings.AWSS3BucketName}/images/thumbs/";
                var thumbFileName = $"{thumbFilePath}default-image_{targetSize}.png";
                var url = thumbFileName;

                return url;
            }
        }

        public override async Task<string> GetPictureUrlAsync(int pictureId, int targetSize = 0, bool showDefaultPicture = true,
            string storeLocation = null, PictureType defaultPictureType = PictureType.Entity)
        {
            var picture = await GetPictureByIdAsync(pictureId);
            return (await GetPictureUrlAsync(picture, targetSize, showDefaultPicture, storeLocation, defaultPictureType)).Url;
        }

        public override async Task<(string Url, Picture Picture)> GetPictureUrlAsync(Picture picture, int targetSize = 0, bool showDefaultPicture = true,
            string storeLocation = null, PictureType defaultPictureType = PictureType.Entity)
        {
            var url = string.Empty;
            byte[] pictureBinary = null;
            if (picture != null)
            {
                var key = _cacheManager.PrepareKeyForDefaultCache(NopAWSMediaDefaults.PictureCacheRawCacheKey, picture.Id);
                /* Need to check this as it shows error cannot use ref out or in parameter inside an anonymous method  */
                var tempPicture = picture;
                pictureBinary = await _cacheManager.GetAsync(key, async () =>
                {
                    return await LoadPictureBinaryAsync(tempPicture);
                });
            }

            if (picture == null || pictureBinary == null || pictureBinary.Length == 0)
            {
                return (await GetDefaultPictureUrlAsync(targetSize, defaultPictureType, storeLocation), picture);
            }

            if (picture.IsNew)
            {
                await DeletePictureThumbsAsync(picture);

                //we do not validate picture binary here to ensure that no exception ("Parameter is not valid") will be thrown
                picture = await UpdatePictureAsync(picture.Id,
                    pictureBinary,
                    picture.MimeType,
                    picture.SeoFilename,
                    picture.AltAttribute,
                    picture.TitleAttribute,
                    false,
                    false);
            }

            var seoFileName = picture.SeoFilename;

            var lastPart = await GetFileExtensionFromMimeTypeAsync(picture.MimeType);
            string thumbFileName;
            if (targetSize == 0)
            {
                thumbFileName = !string.IsNullOrEmpty(seoFileName)
                    ? $"{picture.Id:0000000}_{seoFileName}.{lastPart}"
                    : $"{picture.Id:0000000}.{lastPart}";
            }
            else
            {
                thumbFileName = !string.IsNullOrEmpty(seoFileName)
                    ? $"{picture.Id:0000000}_{seoFileName}_{targetSize}.{lastPart}"
                    : $"{picture.Id:0000000}_{targetSize}.{lastPart}";
            }

            var thumbFilePath = $"{_amazonS3Settings.AWSS3RootUrl}{_amazonS3Settings.AWSS3BucketName}/images/thumbs/";

            //the named mutex helps to avoid creating the same files in different threads,
            //and does not decrease performance significantly, because the code is blocked only for the specific file.
            using (var mutex = new Mutex(false, thumbFileName))
            {
                if (!await GeneratedThumbExistsAsync(thumbFilePath, thumbFileName))
                {
                    mutex.WaitOne();

                    //check, if the file was created, while we were waiting for the release of the mutex.
                    if (!await GeneratedThumbExistsAsync(thumbFilePath, thumbFileName))
                    {
                        byte[] pictureBinaryResized;
                        if (targetSize != 0 && pictureBinary.Length > 0)
                        {
                            //resizing required
                            using (var image = SKBitmap.Decode(pictureBinary))
                            {
                                var format = GetImageFormatByMimeType(picture.MimeType);
                                pictureBinaryResized = ImageResize(image, format, targetSize);
                            }
                        }
                        else
                        {
                            //create a copy of pictureBinary
                            pictureBinaryResized = pictureBinary.ToArray();
                        }

                        SaveThumbAsync(thumbFilePath, thumbFileName, picture.MimeType, pictureBinaryResized).Wait();
                    }
                    mutex.ReleaseMutex();
                }
            }

            return (await GetThumbUrlAsync(thumbFileName, storeLocation), picture);
        }

        protected override async Task<byte[]> LoadPictureFromFileAsync(int pictureId, string mimeType)
        {
            var lastPart = await GetFileExtensionFromMimeTypeAsync(mimeType);
            var fileName = $"{pictureId:0000000}_0.{lastPart}";

            var result = _amazonS3Helper.S3ReadAllBytesFromImages(pictureId, fileName, lastPart).Result;
            return result;
        }

        public override async Task<Picture> SetSeoFilenameAsync(int pictureId, string seoFilename)
        {
            var picture = await GetPictureByIdAsync((pictureId));
            if (picture == null)
                throw new ArgumentException("No picture found with the specified id");

            if (seoFilename != picture.SeoFilename)
            {
                var lastPart = await GetFileExtensionFromMimeTypeAsync(picture.MimeType);
                //update picture
                var pictureinbyte = _amazonS3Helper.S3ReadAllBytesFromThumbs(picture.Id, picture.SeoFilename, lastPart).Result;
                picture = await UpdatePictureAsync(picture.Id,
                    pictureinbyte,
                    picture.MimeType,
                    seoFilename,
                    picture.AltAttribute,
                    picture.TitleAttribute,
                    true,
                    false);
            }
            return picture;
        }

        public override async Task<byte[]> LoadPictureBinaryAsync(Picture picture)
        {
            return await LoadPictureBinaryAsync(picture, false);
        }

        protected override async Task<byte[]> LoadPictureBinaryAsync(Picture picture, bool fromDb)
        {
            if (picture == null)
                throw new ArgumentNullException(nameof(picture));

            var result = await LoadPictureFromFileAsync(picture.Id, picture.MimeType);

            if (result.Length == 0 /*&& !string.IsNullOrEmpty(picture.SeoFilename)*/)
            {
                var lastPart = await GetFileExtensionFromMimeTypeAsync(picture.MimeType);
                result = _amazonS3Helper.S3ReadAllBytesFromThumbs(picture.Id, picture.SeoFilename, lastPart).Result;
            }

            return result;
        }

        public override async Task<Picture> UpdatePictureAsync(int pictureId, byte[] pictureBinary, string mimeType,
            string seoFilename, string altAttribute = null, string titleAttribute = null, bool isNew = true, bool validateBinary = true)
        {
            mimeType = CommonHelper.EnsureNotNull(mimeType);
            mimeType = CommonHelper.EnsureMaximumLength(mimeType, 20);

            seoFilename = CommonHelper.EnsureMaximumLength(seoFilename, 100);

            if (validateBinary)
                pictureBinary = await ValidatePictureAsync(pictureBinary, mimeType, seoFilename);

            var picture = await GetPictureByIdAsync(pictureId);
            if (picture == null)
                return null;

            //delete old thumbs if a picture has been changed
            if (seoFilename != picture.SeoFilename)
                await DeletePictureThumbsAsync(picture);

            picture.MimeType = mimeType;
            picture.SeoFilename = seoFilename;
            picture.AltAttribute = altAttribute;
            picture.TitleAttribute = titleAttribute;
            picture.IsNew = isNew;

            await _pictureRepository.UpdateAsync(picture);
            //UpdatePictureBinary(picture, StoreInDb ? pictureBinary : new byte[0]);
            await UpdatePictureBinaryAsync(picture, await IsStoreInDbAsync() ? pictureBinary : Array.Empty<byte>());

            if (!await IsStoreInDbAsync())
            {
                await SavePictureInFileAsync(picture.Id, pictureBinary, mimeType);
                if (!string.IsNullOrEmpty(picture.SeoFilename))
                    await SaveThumbAsync(string.Empty, picture.SeoFilename, mimeType, pictureBinary);
            }
            //event notification
            await _eventPublisher.EntityUpdatedAsync(picture);

            return picture;
        }

        protected override async Task SavePictureInFileAsync(int pictureId, byte[] pictureBinary, string mimeType)
        {
            var lastPart = await GetFileExtensionFromMimeTypeAsync(mimeType);
            var fileName = $"{pictureId:0000000}_0.{lastPart}";

            var s3ImagePath = $"{_amazonS3Settings.AWSS3BucketName}/images";

            await SaveImageAtImageFolder(s3ImagePath, fileName, mimeType, pictureBinary);

        }

        protected override Task<string> GetPictureLocalPathAsync(string fileName)
        {
            return Task.FromResult(GetS3ImagePath(fileName));
        }

        public override async Task<string> GetThumbLocalPathAsync(Picture picture, int targetSize = 0, bool showDefaultPicture = true)
        {
            var (url, _) = await GetPictureUrlAsync(picture, targetSize, showDefaultPicture);
            if (string.IsNullOrEmpty(url))
                return string.Empty;

            return await GetThumbLocalPathAsync(_fileProvider.GetFileName(url));
        }

        #endregion

        #region Utilities

        protected virtual Task<string> GetAWSS3PathUrlAsync(string thumbFileName)
        {
            var path = (_amazonS3Settings.EnableCdn && !String.IsNullOrEmpty(_amazonS3Settings.CdnBaseUrl)) ? $"{_amazonS3Settings.CdnBaseUrl}/images/thumbs/{thumbFileName}" : $"{_amazonS3Settings.AWSS3RootUrl}{_amazonS3Settings.AWSS3BucketName}/images/thumbs/{thumbFileName}";
            return Task.FromResult(path);
        }

        protected override async Task<string> GetThumbUrlAsync(string thumbFileName, string storeLocation = null)
        {
            //return  $"{_amazonS3Settings.AWSS3RootUrl}{_amazonS3Settings.AWSS3BucketName}/images/thumbs/{thumbFileName}";
            var path = await GetAWSS3PathUrlAsync(thumbFileName);
            return path;
        }

        protected override Task<string> GetThumbLocalPathAsync(string thumbFileName)
        {
            return Task.FromResult($"{_amazonS3Settings.AWSS3RootUrl}{_amazonS3Settings.AWSS3BucketName}/images/thumbs/{thumbFileName}");
        }

        protected override async Task DeletePictureThumbsAsync(Picture picture)
        {
            try
            {
                using (var client = GetS3Client())
                {
                    var prefix = string.Format("{0}", picture.Id.ToString("0000000"));
                    var listRequest = new ListObjectsRequest
                    {
                        BucketName = _amazonS3Settings.AWSS3BucketName,
                        Prefix = $"images/thumbs/{prefix}"
                    };

                    ListObjectsResponse listResponse;
                    do
                    {
                        // Get a list of objects
                        listResponse = await client.ListObjectsAsync(listRequest);
                        foreach (S3Object obj in listResponse.S3Objects)
                        {
                            //delete
                            await S3DeleteImageFromThumbs(obj.Key, picture.MimeType);
                        }

                        // Set the marker property
                        listRequest.Marker = listResponse.NextMarker;
                    } while (listResponse.IsTruncated);
                }
            }
            catch (AmazonS3Exception ex)
            {
                if (ex.ErrorCode != null && (ex.ErrorCode.Equals("InvalidAccessKeyId") || ex.ErrorCode.Equals("InvalidSecurity")))
                {
                    await _logger.InsertLogAsync(Nop.Core.Domain.Logging.LogLevel.Error, ex.Message + " ## Check Crendentials", ex.StackTrace);
                }
                else
                {
                    await _logger.InsertLogAsync(Nop.Core.Domain.Logging.LogLevel.Error, $"Error occurred. Message:{ex.Message} when writing an object", ex.StackTrace);
                }
            }
            await _staticCacheManager.RemoveByPrefixAsync(NopAWSMediaDefaults.ThumbsExistsAwsPrefixCacheKey);
        }

        protected override async Task<bool> GeneratedThumbExistsAsync(string thumbFilePath, string thumbFileName)
        {
            try
            {
                //var key = string.Format(NopAWSMediaDefaults.ThumbExistsCacheKey, thumbFileName);
                var key = _cacheManager.PrepareKeyForDefaultCache(NopAWSMediaDefaults.ThumbExistsAwsCacheKey, thumbFileName);
                return await _cacheManager.GetAsync(key, async () =>
                {
                    using (var client = GetS3Client())
                    {
                        var inthumb = "images/thumbs";
                        var fileExist = string.IsNullOrEmpty(thumbFilePath) ?
                            await FileExists(_amazonS3Settings.AWSS3BucketName, thumbFileName) :
                            await FileExists(_amazonS3Settings.AWSS3BucketName, inthumb, thumbFileName);

                        return fileExist;
                    }
                });
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync(ex.Message, ex);
                return false;
            }
        }

        protected async Task<bool> SaveImageAtImageFolder(string filePath, string fileName, string mimeType, byte[] binary)
        {
            try
            {
                using (var client = GetS3Client())
                {
                    if (!string.IsNullOrEmpty(filePath) && binary.Length > 0)
                    {
                        using (var ms = new MemoryStream(binary))
                        {
                            var putRequest = new PutObjectRequest
                            {
                                BucketName = filePath,
                                Key = fileName,
                                InputStream = ms,
                                ContentType = mimeType,
                            };
                            putRequest.CannedACL = S3CannedACL.PublicRead;
                            putRequest.Headers.CacheControl = "public, max-age=1728000";
                            PutObjectResponse response = await client.PutObjectAsync(putRequest);
                            var result = response.ContentLength > 0 ? true : false;
                            await _staticCacheManager.RemoveByPrefixAsync(NopAWSMediaDefaults.ThumbsExistsAwsPrefixCacheKey);
                            return result;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (AmazonS3Exception ex)
            {
                if (ex.ErrorCode != null && (ex.ErrorCode.Equals("InvalidAccessKeyId") || ex.ErrorCode.Equals("InvalidSecurity")))
                {
                    await _logger.InsertLogAsync(Nop.Core.Domain.Logging.LogLevel.Error, ex.Message + " ## Check Crendentials", ex.StackTrace);
                }
                else
                {
                    await _logger.InsertLogAsync(Nop.Core.Domain.Logging.LogLevel.Error, $"Error occurred. Message:{ex.Message} when writing an object", ex.StackTrace);
                }
                return false;
            }
        }

        protected override async Task SaveThumbAsync(string thumbFilePath, string thumbFileName, string mimeType, byte[] binary)
        {
            try
            {
                using (var client = GetS3Client())
                {
                    var bucketName = GetS3ImageThumbPath();

                    if (!string.IsNullOrEmpty(bucketName))
                    {
                        var putRequest = new PutObjectRequest
                        {
                            BucketName = bucketName,
                            Key = thumbFileName,
                            InputStream = new MemoryStream(binary),
                            ContentType = mimeType,
                        };
                        putRequest.CannedACL = GetS3CannedACL(((CannedACLEnum)_amazonS3Settings.CannedACLId));
                        putRequest.Headers.CacheControl = "public, max-age=1728000";
                        PutObjectResponse response = client.PutObjectAsync(putRequest).Result;
                    }
                }
            }
            catch (AmazonS3Exception ex)
            {
                if (ex.ErrorCode != null && (ex.ErrorCode.Equals("InvalidAccessKeyId") || ex.ErrorCode.Equals("InvalidSecurity")))
                {
                    await _logger.InsertLogAsync(Nop.Core.Domain.Logging.LogLevel.Error, ex.Message + " ## Check Crendentials", ex.StackTrace);
                }
                else
                {
                    await _logger.InsertLogAsync(Nop.Core.Domain.Logging.LogLevel.Error, $"Error occurred. Message:{ex.Message} when writing an object", ex.StackTrace);
                }
            }
            await _staticCacheManager.RemoveByPrefixAsync(NopAWSMediaDefaults.ThumbsExistsAwsPrefixCacheKey);
        }

        protected async Task<bool> FileExists(string bucket, string folder, string filePrefix)
        {
            return await FileExists(bucket, $"{folder}/{filePrefix}");
        }

        protected async Task<bool> FileExists(string bucket, string filePrefix)
        {
            try
            {
                using (var client = GetS3Client())
                {
                    var request = new GetObjectRequest()
                    {
                        BucketName = bucket,
                        Key = filePrefix
                    };

                    var response = await client.GetObjectAsync(request, CancellationToken.None);

                    return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
                }
            }
            catch (Exception ex)
            {
                await _logger.InsertLogAsync(Nop.Core.Domain.Logging.LogLevel.Error, $"Error occurred. Message:{ex.Message} when get an object", ex.Message);
                return false;
            }
        }

        protected AmazonS3Client GetS3Client()
        {
            return new AmazonS3Client(_amazonS3Settings.AWSS3AccessKeyId, _amazonS3Settings.AWSS3SecretKey, GetRegionEndpoint((RegionEndpointEnum)_amazonS3Settings.RegionEndpointId));
        }
        protected S3CannedACL GetS3CannedACL(CannedACLEnum cannedACLEnum)
        {
            switch (cannedACLEnum)
            {
                case CannedACLEnum.NoACL:
                    return S3CannedACL.NoACL;

                case CannedACLEnum.Private:
                    return S3CannedACL.Private;

                case CannedACLEnum.PublicRead:
                    return S3CannedACL.PublicRead;

                case CannedACLEnum.PublicReadWrite:
                    return S3CannedACL.PublicReadWrite;

                case CannedACLEnum.AuthenticatedRead:
                    return S3CannedACL.AuthenticatedRead;

                case CannedACLEnum.AWSExecRead:
                    return S3CannedACL.AWSExecRead;

                case CannedACLEnum.BucketOwnerRead:
                    return S3CannedACL.BucketOwnerRead;

                case CannedACLEnum.BucketOwnerFullControl:
                    return S3CannedACL.BucketOwnerFullControl;

                case CannedACLEnum.LogDeliveryWrite:
                    return S3CannedACL.LogDeliveryWrite;
            }
            return S3CannedACL.PublicRead;
        }
        protected RegionEndpoint GetRegionEndpoint(RegionEndpointEnum regionEnum)
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

        protected string GetS3ImagePath(string fileName)
        {
            return $"{_amazonS3Settings.AWSS3RootUrl}{_amazonS3Settings.AWSS3BucketName}/images/{fileName}";
        }

        protected string GetS3ImageThumbPath()
        {
            if (!string.IsNullOrEmpty(_amazonS3Settings.AWSS3BucketName))
            {
                return $"{_amazonS3Settings.AWSS3BucketName}/images/thumbs";
            }
            else
            {
                return string.Empty;
            }
        }

        protected async Task S3DeleteImageFromThumbs(string fileName, string mimeType)
        {
            try
            {
                using (var client = GetS3Client())
                {
                    var deleteObjectRequest = new DeleteObjectRequest
                    {
                        BucketName = GetS3ImageThumbPath(),
                        Key = fileName,
                    };

                    await client.DeleteObjectAsync(deleteObjectRequest);
                }
            }
            catch (AmazonS3Exception ex)
            {
                if (ex.ErrorCode != null && (ex.ErrorCode.Equals("InvalidAccessKeyId") || ex.ErrorCode.Equals("InvalidSecurity")))
                {
                    await _logger.InsertLogAsync(Nop.Core.Domain.Logging.LogLevel.Error, ex.Message + " ## Check Crendentials", ex.StackTrace);
                }
                else
                {
                    await _logger.InsertLogAsync(Nop.Core.Domain.Logging.LogLevel.Error, $"Error occurred. Message:{ex.Message} when writing an object", ex.StackTrace);
                }
            }
        }

        #endregion
    }
}
