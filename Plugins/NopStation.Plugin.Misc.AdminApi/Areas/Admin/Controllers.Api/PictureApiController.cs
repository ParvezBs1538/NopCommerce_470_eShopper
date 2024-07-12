using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Http.Extensions;
using Nop.Services.Media;
using Nop.Services.Security;
using NopStation.Plugin.Misc.Core.Models.Api;

namespace NopStation.Plugin.Misc.AdminApi.Areas.Admin.Controllers.Api;

[Route("api/a/picture/[action]")]
public partial class PictureApiController : BaseAdminApiController
{
    #region Fields

    private readonly IPictureService _pictureService;
    private readonly IPermissionService _permissionService;

    #endregion

    #region Ctor

    public PictureApiController(IPictureService pictureService,
        IPermissionService permissionService)
    {
        _pictureService = pictureService;
        _permissionService = permissionService;
    }

    #endregion

    #region Methods

    [HttpPost]
    public virtual async Task<IActionResult> AsyncUpload()
    {
        //if (!await _permissionService.Authorize(StandardPermissionProvider.UploadPictures))
        //    return Json(new { success = false, error = "You do not have required permissions" }, "text/plain");

        var httpPostedFile = await Request.GetFirstOrDefaultFileAsync();
        if (httpPostedFile == null)
            return BadRequest("No file uploaded");

        const string qqFileNameParameter = "qqfilename";

        var qqFileName = Request.Form.ContainsKey(qqFileNameParameter)
            ? Request.Form[qqFileNameParameter].ToString()
            : string.Empty;

        var picture = await _pictureService.InsertPictureAsync(httpPostedFile, qqFileName);

        //when returning JSON the mime-type must be set to text/plain
        //otherwise some browsers will pop-up a "Save As" dialog.

        if (picture == null)
            return BadRequest("Wrong file format");

        return Ok(new GenericResponseModel<object>()
        {
            Data = new
            {
                Success = true,
                PictureId = picture.Id,
                ImageUrl = (await _pictureService.GetPictureUrlAsync(picture, 100)).Url
            }
        });
    }

    [HttpGet("{id}")]
    public virtual async Task<IActionResult> PictureById(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.HtmlEditorManagePictures))
            return AdminApiAccessDenied();

        var picture = await _pictureService.GetPictureByIdAsync(id);
        if (picture == null)
            return NotFound("No picture found with the specified id");

        return Ok(new GenericResponseModel<object>()
        {
            Data = new
            {
                PictureId = picture.Id,
                ImageUrl = (await _pictureService.GetPictureUrlAsync(picture, 100)).Url
            }
        });
    }

    #endregion
}