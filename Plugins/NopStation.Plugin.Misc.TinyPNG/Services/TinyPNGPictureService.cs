using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Media;
using Nop.Core.Infrastructure;
using Nop.Data;
using NopStation.Plugin.Misc.TinyPNG.Domain;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Media;
using Nop.Services.Seo;
using Nop.Services.Logging;

namespace NopStation.Plugin.Misc.TinyPNG.Services
{
    public partial class TinyPNGPictureService : PictureService
    {
        #region Fields

        private readonly IRepository<PictureInfo> _pictureInfoRepository;
        private readonly ITinyPNGService _tinyPNGService;
        private readonly TinyPNGSettings _tinyPNGSettings;

        #endregion

        #region Ctor

        public TinyPNGPictureService(IDownloadService downloadService,
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
            IRepository<PictureInfo> pictureInfoRepository,
            ITinyPNGService tinyPNGService,
            TinyPNGSettings tinyPNGSettings) : base(
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
            _pictureInfoRepository = pictureInfoRepository;
            _tinyPNGService = tinyPNGService;
            _tinyPNGSettings = tinyPNGSettings;
        }

        #endregion

        #region Utilities

        protected async Task<byte[]> CompressPictureBinary(int pictureId, byte[] pictureBinary)
        {
            if (_tinyPNGSettings.TinyPNGEnable)
            {
                //check is already compressed or not
                var pictureInfo = await _pictureInfoRepository.Table.Where(x => x.PictureId == pictureId).FirstOrDefaultAsync();
                if (pictureInfo != null && pictureInfo.Compressed)
                {
                    //_logger.Information("already compressed picture Id: " + pictureId);
                    return pictureBinary;
                }

                //update file system binary
                var (imageByte, isCompressed) = await _tinyPNGService.TinifyImageAsync(pictureBinary);

                //track picture binary changes
                if (isCompressed)
                {
                    if (pictureInfo == null)
                        await _pictureInfoRepository.InsertAsync(new PictureInfo()
                        {
                            PictureId = pictureId,
                            BinaryLength = imageByte.Length,
                            Compressed = true,
                        });
                    else
                    {
                        pictureInfo.Compressed = true;
                        await _pictureInfoRepository.UpdateAsync(pictureInfo);
                    }

                    return imageByte;
                }
            }

            return pictureBinary;
        }

        #endregion

        #region Overwritten methods

        protected override async Task<PictureBinary> UpdatePictureBinaryAsync(Picture picture, byte[] binaryData)
        {
            if (picture == null)
                throw new ArgumentNullException(nameof(picture));

            var pictureBinary = await GetPictureBinaryByPictureIdAsync(picture.Id);

            var isNew = pictureBinary == null;

            if (isNew)
                pictureBinary = new PictureBinary
                {
                    PictureId = picture.Id
                };

            pictureBinary.BinaryData = binaryData;

            if (isNew)
                await _pictureBinaryRepository.InsertAsync(pictureBinary);
            else
            {
                //call to compress
                pictureBinary.BinaryData = await CompressPictureBinary(pictureBinary.PictureId, pictureBinary.BinaryData);

                await _pictureBinaryRepository.UpdateAsync(pictureBinary);
            }

            return pictureBinary;
        }

        protected override async Task SaveThumbAsync(string thumbFilePath, string thumbFileName, string mimeType, byte[] binary)
        {
            //ensure \thumb directory exists
            var thumbsDirectoryPath = _fileProvider.GetAbsolutePath(NopMediaDefaults.ImageThumbsPath);
            _fileProvider.CreateDirectory(thumbsDirectoryPath);

            //compress thumb
            var (imageByte, isCompressed) = await _tinyPNGService.TinifyImageAsync(binary);

            //save
            await _fileProvider.WriteAllBytesAsync(thumbFilePath, imageByte);
        }

        #endregion
    }
}
