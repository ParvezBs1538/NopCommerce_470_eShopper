using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Nop.Core;
using Nop.Core.Domain.Media;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Media;
using NopStation.Plugin.Widgets.SmartSliders.Domains;
using NReco.VideoConverter;
using SkiaSharp;

namespace NopStation.Plugin.Widgets.SmartSliders.Services
{
    public class VideoService : ISmartSliderVideoService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRepository<Download> _downloadRepository;
        private readonly IWebHelper _webHelper;
        private readonly MediaSettings _mediaSettings;
        private readonly INopFileProvider _fileProvider;
        private readonly IPictureService _pictureService;
        private readonly IDownloadService _downloadService;

        public VideoService(IHttpContextAccessor httpContextAccessor,
            IRepository<Download> downloadRepository,
            IWebHelper webHelper,
            MediaSettings mediaSettings,
            INopFileProvider fileProvider,
            IPictureService pictureService,
            IDownloadService downloadService)
        {
            _httpContextAccessor = httpContextAccessor;
            _downloadRepository = downloadRepository;
            _webHelper = webHelper;
            _mediaSettings = mediaSettings;
            _fileProvider = fileProvider;
            _pictureService = pictureService;
            _downloadService = downloadService;
        }

        #region Utilities

        #endregion

        public async Task<(string Url, string ThumbUrl, Download Video)> GetVideoUrlAsync(Download video,
            int targetSize = 0,
            bool showDefaultPicture = true,
            string storeLocation = null)
        {
            
        }
    }
}