using System.IO;
using System.Net;
using System.Threading.Tasks;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Media;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Seo;

namespace NopStation.Plugin.Misc.CloudinaryCdn.Services
{
    public partial class CloudinaryPictureService : PictureService
    {
        #region Fields

        private static Cloudinary _cloudinary;
        private static bool _isInitialized;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly CloudinaryCdnSettings _cloudinaryCdnSettings;
        private readonly ILocalizationService _localizationService;
        private readonly object _locker = new();

        #endregion

        #region Ctor

        public CloudinaryPictureService(IDownloadService downloadService,
            IHttpContextAccessor httpContextAccessor,
            ILogger logger,
            INopFileProvider fileProvider,
            IProductAttributeParser productAttributeParser,
            IProductAttributeService productAttributeService,
            IRepository<Picture> pictureRepository,
            IRepository<PictureBinary> pictureBinaryRepository,
            IRepository<ProductPicture> productPictureRepository,
            ISettingService settingService,
            IUrlRecordService urlRecordService,
            IWebHelper webHelper,
            MediaSettings mediaSettings,
            IStaticCacheManager staticCacheManager,
            CloudinaryCdnSettings cloudinaryCdnSettings,
            ILocalizationService localizationService)
            : base(downloadService,
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
            _cloudinaryCdnSettings = cloudinaryCdnSettings;
            _localizationService = localizationService;
            OneTimeInitAsync();
        }

        #endregion

        #region Utilities

        protected async void OneTimeInitAsync()
        {
            if (_isInitialized)
                return;

            if (string.IsNullOrEmpty(_cloudinaryCdnSettings.ApiKey))
            {
                await _logger.InformationAsync(await _localizationService.GetResourceAsync("Admin.NopStation.CloudinaryCdn.Configuration.Fields.ApiKey.Error"));
                return;
            }

            if (string.IsNullOrEmpty(_cloudinaryCdnSettings.ApiSecret))
            {
                await _logger.InformationAsync(await _localizationService.GetResourceAsync("Admin.NopStation.CloudinaryCdn.Configuration.Fields.ApiSecret.Error"));
                return;
            }

            if (string.IsNullOrEmpty(_cloudinaryCdnSettings.CloudName))
            {
                await _logger.InformationAsync(await _localizationService.GetResourceAsync("Admin.NopStation.CloudinaryCdn.Configuration.Fields.CloudName.Error"));
                return;
            }

            lock (_locker)
            {
                if (_isInitialized)
                    return;

                _cloudinary = new Cloudinary(new Account()
                {
                    ApiKey = _cloudinaryCdnSettings.ApiKey,
                    ApiSecret = _cloudinaryCdnSettings.ApiSecret,
                    Cloud = _cloudinaryCdnSettings.CloudName
                });

                _isInitialized = true;
            }
        }

        #endregion

        #region Methods

        protected override Task<string> GetThumbLocalPathAsync(string thumbFileName)
        {
            if (CloudinaryCdnHelper.IsAdminArea())
                return base.GetThumbLocalPathAsync(thumbFileName);

            var path = _cloudinaryCdnSettings.PrependCdnFolderName ? $"{_cloudinaryCdnSettings.CdnFolderName}/" : string.Empty;

            var index = thumbFileName.LastIndexOf('.');
            if (index >= 0)
                thumbFileName = thumbFileName.Substring(0, index);

            return Task.FromResult($"{path}images/thumbs/{thumbFileName}");
        }

        protected override async Task<string> GetThumbUrlAsync(string thumbFileName, string storeLocation = null)
        {
            if (CloudinaryCdnHelper.IsAdminArea())
                return await base.GetThumbUrlAsync(thumbFileName, storeLocation);

            var localPath = await GetThumbLocalPathAsync(thumbFileName);

            var index = thumbFileName.LastIndexOf('.');
            if (index >= 0)
                localPath += thumbFileName.Substring(index);

            return $"https://res.cloudinary.com/{_cloudinaryCdnSettings.CloudName}/image/upload/{localPath}";
        }

        protected override async Task DeletePictureThumbsAsync(Picture picture)
        {
            if (CloudinaryCdnHelper.IsAdminArea())
                await base.DeletePictureThumbsAsync(picture);

            //create a string containing the Blob name prefix
            var prefix = $"{picture.Id:0000000}";
            await _cloudinary.DeleteResourcesByPrefixAsync(prefix);

            await _staticCacheManager.RemoveByPrefixAsync(NopMediaDefaults.ThumbsExistsPrefix);
        }

        protected override async Task<bool> GeneratedThumbExistsAsync(string thumbFilePath, string thumbFileName)
        {
            if (CloudinaryCdnHelper.IsAdminArea())
                return await base.GeneratedThumbExistsAsync(thumbFilePath, thumbFileName);

            try
            {
                var key = _staticCacheManager.PrepareKeyForDefaultCache(NopMediaDefaults.ThumbExistsCacheKey, thumbFileName);

                return await _staticCacheManager.GetAsync(key, async () =>
                {
                    return (await _cloudinary.GetResourceAsync(thumbFilePath)).StatusCode == HttpStatusCode.OK;
                });
            }
            catch
            {
                return false;
            }
        }

        protected override async Task SaveThumbAsync(string thumbFilePath, string thumbFileName, string mimeType, byte[] binary)
        {
            if (CloudinaryCdnHelper.IsAdminArea())
                await base.SaveThumbAsync(thumbFilePath, thumbFileName, mimeType, binary);

            await using var ms = new MemoryStream(binary);
            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(thumbFileName, ms),
                PublicId = thumbFilePath,
                Overwrite = true
            };
            var uploadResult = _cloudinary.Upload(uploadParams);

            await _staticCacheManager.RemoveByPrefixAsync(NopMediaDefaults.ThumbsExistsPrefix);
        }

        #endregion
    }
}