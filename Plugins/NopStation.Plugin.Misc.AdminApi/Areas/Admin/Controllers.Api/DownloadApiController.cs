using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Media;
using Nop.Core.Http.Extensions;
using Nop.Core.Infrastructure;
using Nop.Services.Logging;
using Nop.Services.Media;
using NopStation.Plugin.Misc.Core.Models.Api;

namespace NopStation.Plugin.Misc.AdminApi.Areas.Admin.Controllers.Api;

[Route("api/a/download/[action]")]
public partial class DownloadApiController : BaseAdminApiController
{
    #region Fields

    private readonly IDownloadService _downloadService;
    private readonly ILogger _logger;
    private readonly INopFileProvider _fileProvider;
    private readonly IWorkContext _workContext;

    #endregion

    #region Ctor

    public DownloadApiController(IDownloadService downloadService,
        ILogger logger,
        INopFileProvider fileProvider,
        IWorkContext workContext)
    {
        _downloadService = downloadService;
        _logger = logger;
        _fileProvider = fileProvider;
        _workContext = workContext;
    }

    #endregion

    #region Methods

    [HttpGet("{downloadGuid}")]
    public virtual async Task<IActionResult> DownloadFile(Guid downloadGuid)
    {
        var download = await _downloadService.GetDownloadByGuidAsync(downloadGuid);
        if (download == null)
            return BadRequest("No download record found with the specified id");

        //A warning (SCS0027 - Open Redirect) from the "Security Code Scan" analyzer may appear at this point. 
        //In this case, it is not relevant. Url may not be local.
        if (download.UseDownloadUrl)
            return Ok(new GenericResponseModel<string>() { Data = download.DownloadUrl });

        //use stored data
        if (download.DownloadBinary == null)
            return BadRequest($"Download data is not available any more. Download GD={download.Id}");

        var fileName = !string.IsNullOrWhiteSpace(download.Filename) ? download.Filename : download.Id.ToString();
        var contentType = !string.IsNullOrWhiteSpace(download.ContentType)
            ? download.ContentType
            : MimeTypes.ApplicationOctetStream;
        return new FileContentResult(download.DownloadBinary, contentType)
        {
            FileDownloadName = fileName + download.Extension
        };
    }

    [HttpPost("{downloadUrl}")]
    //do not validate request token (XSRF)
    public virtual async Task<IActionResult> SaveDownloadUrl(string downloadUrl)
    {
        //don't allow to save empty download object
        if (string.IsNullOrEmpty(downloadUrl))
        {
            return BadRequest("Please enter URL");
        }

        //insert
        var download = new Download
        {
            DownloadGuid = Guid.NewGuid(),
            UseDownloadUrl = true,
            DownloadUrl = downloadUrl,
            IsNew = true
        };
        await _downloadService.InsertDownloadAsync(download);

        return Created(download.Id);
    }

    [HttpPost]
    //do not validate request token (XSRF)
    public virtual async Task<IActionResult> AsyncUpload()
    {
        var httpPostedFile = await Request.GetFirstOrDefaultFileAsync();
        if (httpPostedFile == null)
        {
            return BadRequest("No file uploaded");
        }

        var fileBinary = await _downloadService.GetDownloadBitsAsync(httpPostedFile);

        var qqFileNameParameter = "qqfilename";
        var fileName = httpPostedFile.FileName;
        if (string.IsNullOrEmpty(fileName) && Request.Form.ContainsKey(qqFileNameParameter))
            fileName = Request.Form[qqFileNameParameter].ToString();
        //remove path (passed in IE)
        fileName = _fileProvider.GetFileName(fileName);

        var contentType = httpPostedFile.ContentType;

        var fileExtension = _fileProvider.GetFileExtension(fileName);
        if (!string.IsNullOrEmpty(fileExtension))
            fileExtension = fileExtension.ToLowerInvariant();

        var download = new Download
        {
            DownloadGuid = Guid.NewGuid(),
            UseDownloadUrl = false,
            DownloadUrl = string.Empty,
            DownloadBinary = fileBinary,
            ContentType = contentType,
            //we store filename without extension for downloads
            Filename = _fileProvider.GetFileNameWithoutExtension(fileName),
            Extension = fileExtension,
            IsNew = true
        };

        try
        {
            await _downloadService.InsertDownloadAsync(download);

            //when returning JSON the mime-type must be set to text/plain
            //otherwise some browsers will pop-up a "Save As" dialog.
            return Created(download.Id);
        }
        catch (Exception exc)
        {
            await _logger.ErrorAsync(exc.Message, exc, await _workContext.GetCurrentCustomerAsync());
            return BadRequest(exc.Message);
        }
    }

    #endregion
}