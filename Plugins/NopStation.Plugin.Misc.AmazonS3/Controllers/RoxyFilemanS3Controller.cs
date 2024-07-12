using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Services.Media.RoxyFileman;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Areas.Admin.Models.Media;
using NopStation.Plugin.Misc.AmazonS3.Services;

namespace NopStation.Plugin.Misc.AmazonS3.Controllers
{
    public class RoxyFilemanS3Controller : RoxyFilemanController
    {
        #region Fields

        private readonly IRoxyFilemanAmazonS3Service _roxyFilemanAmazonS3Service;
        private readonly AmazonS3Settings _amazonS3Settings;
        protected readonly IHttpContextAccessor _httpContextAccessor;

        #endregion

        public RoxyFilemanS3Controller(IPermissionService permissionService,
            IRoxyFilemanService roxyFilemanService,
            AmazonS3Settings amazonS3Settings,
            IHttpContextAccessor httpContextAccessor,
            IRoxyFilemanAmazonS3Service roxyFilemanAmazonS3Service,
            IWebHelper webHelper) : base(
                permissionService,
                roxyFilemanService,
                webHelper)
        {
            _roxyFilemanAmazonS3Service = roxyFilemanAmazonS3Service;
            _amazonS3Settings = amazonS3Settings;
            _httpContextAccessor = httpContextAccessor;
        }

        [Route("Admin/RoxyFileman/CreateConfiguration")]
        public async override Task CreateConfiguration()
        {
            await _roxyFilemanAmazonS3Service.CreateConfigurationAsync();
        }

        //[Route("Admin/RoxyFileman/ProcessRequest")]
        //[IgnoreAntiforgeryToken]
        //public override void ProcessRequest()
        //{
        //    ProcessRequestAsync().Wait();
        //}

        //[IgnoreAntiforgeryToken]
        //protected override async Task ProcessRequestAsync()
        //{
        //    var action = "DIRLIST";

        //    //custom code by nopCommerce team
        //    if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.HtmlEditorManagePictures))
        //        await HttpContext.Response.WriteAsync(_roxyFilemanAmazonS3Service.GetErrorResponse("You don't have required permission"));

        //    try
        //    {
        //        if (!StringValues.IsNullOrEmpty(HttpContext.Request.Query["a"]))
        //            action = (string)HttpContext.Request.Query["a"];

        //        //custom code by nopCommerce team
        //        //VerifyAction(action);
        //        if (_amazonS3Settings.AWSS3Enable)
        //        {
        //            #region Amazon s3

        //            switch (action.ToUpper())
        //            {
        //                case "DIRLIST":
        //                    await _roxyFilemanAmazonS3Service.GetDirectoriesAsync(HttpContext.Request.Query["type"]);
        //                    break;
        //                case "FILESLIST":
        //                    await _roxyFilemanAmazonS3Service.GetFilesAsync(HttpContext.Request.Query["d"], HttpContext.Request.Query["type"]);
        //                    break;
        //                case "COPYDIR":
        //                    await _roxyFilemanAmazonS3Service.CopyDirectoryAsync(HttpContext.Request.Query["d"], HttpContext.Request.Query["n"]);
        //                    break;
        //                case "COPYFILE":
        //                    await _roxyFilemanAmazonS3Service.CopyFileAsync(HttpContext.Request.Query["f"], HttpContext.Request.Query["n"]);
        //                    break;
        //                case "CREATEDIR":
        //                    await _roxyFilemanAmazonS3Service.CreateDirectoryAsync(HttpContext.Request.Query["d"], HttpContext.Request.Query["n"]);
        //                    break;
        //                case "DELETEDIR":
        //                    await _roxyFilemanAmazonS3Service.DeleteDirectoryAsync(HttpContext.Request.Query["d"]);
        //                    break;
        //                case "DELETEFILE":
        //                    await _roxyFilemanAmazonS3Service.DeleteFileAsync(HttpContext.Request.Query["f"]);
        //                    break;
        //                case "DOWNLOAD":
        //                    await _roxyFilemanAmazonS3Service.DownloadFileAsync(HttpContext.Request.Query["f"]);
        //                    break;
        //                case "DOWNLOADDIR":
        //                    await _roxyFilemanAmazonS3Service.DownloadDirectoryAsync(HttpContext.Request.Query["d"]);
        //                    break;
        //                case "MOVEDIR":
        //                    await _roxyFilemanAmazonS3Service.MoveDirectoryAsync(HttpContext.Request.Query["d"], HttpContext.Request.Query["n"]);
        //                    break;
        //                case "MOVEFILE":
        //                    await _roxyFilemanAmazonS3Service.MoveFileAsync(HttpContext.Request.Query["f"], HttpContext.Request.Query["n"]);
        //                    break;
        //                case "RENAMEDIR":
        //                    await _roxyFilemanAmazonS3Service.RenameDirectoryAsync(HttpContext.Request.Query["d"], HttpContext.Request.Query["n"]);
        //                    break;
        //                case "RENAMEFILE":
        //                    await _roxyFilemanAmazonS3Service.RenameFileAsync(HttpContext.Request.Query["f"], HttpContext.Request.Query["n"]);
        //                    break;
        //                case "GENERATETHUMB":
        //                    int w = 140, h = 0;
        //                    int.TryParse(HttpContext.Request.Query["width"].ToString().Replace("px", ""), out w);
        //                    int.TryParse(HttpContext.Request.Query["height"].ToString().Replace("px", ""), out h);
        //                    await _roxyFilemanAmazonS3Service.CreateImageThumbnailAsync(HttpContext.Request.Query["f"], w, h);
        //                    break;
        //                case "UPLOAD":
        //                    await _roxyFilemanAmazonS3Service.UploadFilesAsync(HttpContext.Request.Form["d"]);//UploadFilesAsync(HttpContext.Request.Form["d"]);
        //                    break;
        //                default:
        //                    await HttpContext.Response.WriteAsync(_roxyFilemanAmazonS3Service.GetErrorResponse("This action is not implemented."));
        //                    break;
        //            }

        //            #endregion
        //        }
        //        else
        //        {
        //            #region Local
        //            await base.CreateConfiguration();
        //            await base.ProcessRequestAsync();

        //            #endregion
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        if (action == "UPLOAD" && !_roxyFilemanAmazonS3Service.IsAjaxRequest())
        //            await HttpContext.Response.WriteAsync($"<script>parent.fileUploaded({_roxyFilemanService.GetErrorResponse(await _roxyFilemanService.GetLanguageResourceAsync("E_UploadNoFiles"))});</script>");
        //        else
        //            await HttpContext.Response.WriteAsync(_roxyFilemanAmazonS3Service.GetErrorResponse(ex.Message));
        //    }
        //}

        [IgnoreAntiforgeryToken]
        public override async Task<IActionResult> DirectoriesList(string type)
        {
            //custom code by nopCommerce team
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.HtmlEditorManagePictures))
                await HttpContext.Response.WriteAsync(_roxyFilemanAmazonS3Service.GetErrorResponse("You don't have required permission"));

            try
            {
                if (_amazonS3Settings.AWSS3Enable)
                {
                    var directories = await _roxyFilemanAmazonS3Service.GetDirectoriesAsync(HttpContext.Request.Query["type"]);
                    return Json(directories);
                }
                else
                {
                    var directories = _roxyFilemanService.GetDirectoryList(type);
                    return Json(directories);
                }
            }
            catch (Exception ex)
            {
                return JsonError(ex.Message);
            }
        }

        [IgnoreAntiforgeryToken]
        public override async Task<IActionResult> FilesList(string d, string type)
        {
            //custom code by nopCommerce team
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.HtmlEditorManagePictures))
                await HttpContext.Response.WriteAsync(_roxyFilemanAmazonS3Service.GetErrorResponse("You don't have required permission"));

            try
            {
                if (_amazonS3Settings.AWSS3Enable)
                {
                    var directories = await _roxyFilemanAmazonS3Service.GetFilesAsync(HttpContext.Request.Query["d"], HttpContext.Request.Query["type"]);
                    return Json(directories);
                }
                else
                {
                    var directories = _roxyFilemanService.GetFiles(d, type);
                    return Json(directories);
                }
            }
            catch (Exception ex)
            {
                return JsonError(ex.Message);
            }
        }

        public override async Task<IActionResult> DownloadFile(string f)
        {
            //custom code by nopCommerce team
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.HtmlEditorManagePictures))
                await HttpContext.Response.WriteAsync(_roxyFilemanAmazonS3Service.GetErrorResponse("You don't have required permission"));

            try
            {
                if (_amazonS3Settings.AWSS3Enable)
                {
                    await _roxyFilemanAmazonS3Service.DownloadFileAsync(HttpContext.Request.Query["f"]);
                    return JsonOk();
                }
                else
                {
                    var (stream, name) = _roxyFilemanService.GetFileStream(f);

                    if (!new FileExtensionContentTypeProvider().TryGetContentType(f, out var contentType))
                        contentType = MimeTypes.ApplicationOctetStream;

                    return File(stream, contentType, name);
                }
            }
            catch (Exception ex)
            {
                return JsonError(ex.Message);
            }
        }

        [IgnoreAntiforgeryToken]
        public override async Task<IActionResult> CopyDirectory(string d, string n)
        {
            //custom code by nopCommerce team
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.HtmlEditorManagePictures))
                await HttpContext.Response.WriteAsync(_roxyFilemanAmazonS3Service.GetErrorResponse("You don't have required permission"));

            try
            {

                if (_amazonS3Settings.AWSS3Enable)
                {
                    await _roxyFilemanAmazonS3Service.CopyDirectoryAsync(HttpContext.Request.Query["d"], HttpContext.Request.Query["n"]);
                    return JsonOk();
                }
                else
                {
                    _roxyFilemanService.CopyDirectory(d, n);
                    return JsonOk();
                }
            }
            catch (Exception ex)
            {
                return JsonError(ex.Message);
            }
        }

        [IgnoreAntiforgeryToken]
        public override async Task<IActionResult> CopyFile(string f, string n)
        {
            //custom code by nopCommerce team
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.HtmlEditorManagePictures))
                await HttpContext.Response.WriteAsync(_roxyFilemanAmazonS3Service.GetErrorResponse("You don't have required permission"));

            try
            {
                if (_amazonS3Settings.AWSS3Enable)
                {
                    await _roxyFilemanAmazonS3Service.CopyFileAsync(HttpContext.Request.Query["f"], HttpContext.Request.Query["n"]);
                    return JsonOk();
                }
                else
                {
                    _roxyFilemanService.CopyFile(f, n);
                    return JsonOk();
                }
            }
            catch (Exception ex)
            {
                return JsonError(ex.Message);
            }
        }

        [IgnoreAntiforgeryToken]
        public override async Task<IActionResult> CreateDirectory(string d, string n)
        {
            //custom code by nopCommerce team
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.HtmlEditorManagePictures))
                await HttpContext.Response.WriteAsync(_roxyFilemanAmazonS3Service.GetErrorResponse("You don't have required permission"));

            try
            {
                if (_amazonS3Settings.AWSS3Enable)
                {
                    await _roxyFilemanAmazonS3Service.CreateDirectoryAsync(HttpContext.Request.Query["d"], HttpContext.Request.Query["n"]);
                    return JsonOk();
                }
                else
                {
                    _roxyFilemanService.CreateDirectory(d, n);
                    return JsonOk();
                }

            }
            catch (Exception ex)
            {
                return JsonError(ex.Message);
            }
        }

        [IgnoreAntiforgeryToken]
        public override async Task<IActionResult> DeleteDirectory(string d)
        {
            //custom code by nopCommerce team
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.HtmlEditorManagePictures))
                await HttpContext.Response.WriteAsync(_roxyFilemanAmazonS3Service.GetErrorResponse("You don't have required permission"));

            try
            {
                if (_amazonS3Settings.AWSS3Enable)
                {
                    await _roxyFilemanAmazonS3Service.DeleteDirectoryAsync(HttpContext.Request.Query["d"]);
                    return JsonOk();
                }
                else
                {
                    _roxyFilemanService.DeleteDirectory(d);
                    return JsonOk();
                }
            }
            catch (Exception ex)
            {
                return JsonError(ex.Message);
            }
        }

        [IgnoreAntiforgeryToken]
        public override async Task<IActionResult> DeleteFile(string f)
        {
            //custom code by nopCommerce team
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.HtmlEditorManagePictures))
                await HttpContext.Response.WriteAsync(_roxyFilemanAmazonS3Service.GetErrorResponse("You don't have required permission"));

            try
            {
                if (_amazonS3Settings.AWSS3Enable)
                {
                    await _roxyFilemanAmazonS3Service.DeleteFileAsync(HttpContext.Request.Query["f"]);
                    return JsonOk();
                }
                else
                {
                    _roxyFilemanService.DeleteFile(f);
                    return JsonOk();
                }
            }
            catch (Exception ex)
            {
                return JsonError(ex.Message);
            }
        }

        public override async Task<IActionResult> DownloadDirectory(string d)
        {
            //custom code by nopCommerce team
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.HtmlEditorManagePictures))
                await HttpContext.Response.WriteAsync(_roxyFilemanAmazonS3Service.GetErrorResponse("You don't have required permission"));

            if (_amazonS3Settings.AWSS3Enable)
            {
                await _roxyFilemanAmazonS3Service.DownloadDirectoryAsync(HttpContext.Request.Query["d"]);
                return JsonOk();
            }
            else
            {
                var fileContents = _roxyFilemanService.DownloadDirectory(d);
                return File(fileContents, MimeTypes.ApplicationZip);
            }

        }

        [IgnoreAntiforgeryToken]
        public override async Task<IActionResult> MoveDirectory(string d, string n)
        {
            //custom code by nopCommerce team
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.HtmlEditorManagePictures))
                await HttpContext.Response.WriteAsync(_roxyFilemanAmazonS3Service.GetErrorResponse("You don't have required permission"));
            try
            {
                if (_amazonS3Settings.AWSS3Enable)
                {
                    await _roxyFilemanAmazonS3Service.MoveDirectoryAsync(HttpContext.Request.Query["d"], HttpContext.Request.Query["n"]);
                    return JsonOk();
                }
                else
                {
                    _roxyFilemanService.MoveDirectory(d, n);
                    return JsonOk();
                }

            }
            catch (Exception ex)
            {
                return JsonError(ex.Message);
            }
        }

        [IgnoreAntiforgeryToken]
        public override async Task<IActionResult> MoveFile(string f, string n)
        {
            //custom code by nopCommerce team
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.HtmlEditorManagePictures))
                await HttpContext.Response.WriteAsync(_roxyFilemanAmazonS3Service.GetErrorResponse("You don't have required permission"));
            try
            {
                if (_amazonS3Settings.AWSS3Enable)
                {
                    await _roxyFilemanAmazonS3Service.MoveFileAsync(HttpContext.Request.Query["f"], HttpContext.Request.Query["n"]);
                    return JsonOk();
                }
                else
                {
                    _roxyFilemanService.MoveFile(f, n);
                    return JsonOk();
                }

            }
            catch (Exception ex)
            {
                return JsonError(ex.Message);
            }
        }

        [IgnoreAntiforgeryToken]
        public override async Task<IActionResult> RenameDirectory(string d, string n)
        {
            //custom code by nopCommerce team
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.HtmlEditorManagePictures))
                await HttpContext.Response.WriteAsync(_roxyFilemanAmazonS3Service.GetErrorResponse("You don't have required permission"));
            try
            {
                if (_amazonS3Settings.AWSS3Enable)
                {
                    await _roxyFilemanAmazonS3Service.RenameDirectoryAsync(HttpContext.Request.Query["d"], HttpContext.Request.Query["n"]);
                    return JsonOk();
                }
                else
                {
                    _roxyFilemanService.RenameDirectory(d, n);
                    return JsonOk();
                }

            }
            catch (Exception ex)
            {
                return JsonError(ex.Message);
            }
        }

        [IgnoreAntiforgeryToken]
        public override async Task<IActionResult> RenameFile(string f, string n)
        {
            //custom code by nopCommerce team
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.HtmlEditorManagePictures))
                await HttpContext.Response.WriteAsync(_roxyFilemanAmazonS3Service.GetErrorResponse("You don't have required permission"));
            try
            {
                if (_amazonS3Settings.AWSS3Enable)
                {
                    await _roxyFilemanAmazonS3Service.RenameFileAsync(HttpContext.Request.Query["f"], HttpContext.Request.Query["n"]);
                    return JsonOk();
                }
                else
                {
                    _roxyFilemanService.RenameFile(f, n);
                    return JsonOk();
                }

            }
            catch (Exception ex)
            {
                return JsonError(ex.Message);
            }
        }

        public override async Task<IActionResult> CreateImageThumbnail(string f)
        {
            //custom code by nopCommerce team
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.HtmlEditorManagePictures))
                await HttpContext.Response.WriteAsync(_roxyFilemanAmazonS3Service.GetErrorResponse("You don't have required permission"));
            try
            {
                var provider = new FileExtensionContentTypeProvider();
                if (!provider.TryGetContentType(f, out var contentType))
                    contentType = MimeTypes.ImageJpeg;

                if (_amazonS3Settings.AWSS3Enable)
                {
                    int w = 140, h = 0;
                    int.TryParse(HttpContext.Request.Query["width"].ToString().Replace("px", ""), out w);
                    int.TryParse(HttpContext.Request.Query["height"].ToString().Replace("px", ""), out h);
                    var fileContents = await _roxyFilemanAmazonS3Service.CreateImageThumbnailAsync(HttpContext.Request.Query["f"], w, h, contentType);

                    return File(fileContents, contentType);
                }
                else
                {
                    var fileContents = _roxyFilemanService.CreateImageThumbnail(f, contentType);

                    return File(fileContents, contentType);
                }
            }
            catch (Exception ex)
            {
                return JsonError(ex.Message);
            }
        }

        [IgnoreAntiforgeryToken]
        [HttpPost]
        public override async Task<IActionResult> UploadFiles([FromForm] RoxyFilemanUploadModel uploadModel)
        {
            //custom code by nopCommerce team
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.HtmlEditorManagePictures))
                await HttpContext.Response.WriteAsync(_roxyFilemanAmazonS3Service.GetErrorResponse("You don't have required permission"));
            try
            {
                if (_amazonS3Settings.AWSS3Enable)
                {
                    await _roxyFilemanAmazonS3Service.UploadFilesAsync(HttpContext.Request.Form["d"]);
                    return JsonOk();
                }
                else
                {
                    if (HttpContext.Request.Form.Files.Count == 0)
                        throw new RoxyFilemanException("E_UploadNoFiles");

                    await _roxyFilemanService.UploadFilesAsync(uploadModel.D, HttpContext.Request.Form.Files);
                    return JsonOk();
                }
            }
            catch (Exception ex)
            {
                if (!_webHelper.IsAjaxRequest(Request))
                {
                    var roxyError = new { res = "error", msg = ex.Message };
                    return Content($"<script>parent.fileUploaded({JsonConvert.SerializeObject(roxyError)});</script>");
                }

                return JsonError(ex.Message);
            }
        }
    }
}
