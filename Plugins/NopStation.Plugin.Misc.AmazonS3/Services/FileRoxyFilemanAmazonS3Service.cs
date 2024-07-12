using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Media;
using Nop.Core.Infrastructure;
using Nop.Services.Media.RoxyFileman;
using NopStation.Plugin.Misc.AmazonS3.Models;
using SkiaSharp;
namespace NopStation.Plugin.Misc.AmazonS3.Services
{
    public class FileRoxyFilemanAmazonS3Service : IRoxyFilemanAmazonS3Service
    {
        protected string _fileRootPath;
        private const string CONFIGURATION_FILE = "Plugins/NopStation.Plugin.Misc.AmazonS3/Roxy_Fileman/conf.json";
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly INopFileProvider _fileProvider;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        protected readonly AmazonS3Settings _amazonS3Settings;
        private readonly MediaSettings _mediaSettings;
        private Dictionary<string, string> _lang;
        private readonly string _webRootPath = null;
        private readonly string _rootDirLocation = "wwwroot/files/temporary";
        private Dictionary<string, string> _settings;

        public FileRoxyFilemanAmazonS3Service(
            IWebHostEnvironment webHostEnvironment,
            IHttpContextAccessor httpContextAccessor,
            INopFileProvider fileProvider,
            IWebHelper webHelper,
            IWorkContext workContext,
            AmazonS3Settings amazonS3Settings,
            MediaSettings mediaSettings) 
            //: base(
            //    webHostEnvironment,
            //    httpContextAccessor,
            //    fileProvider,
            //    webHelper,
            //    workContext,
            //    mediaSettings)
        {
            _webHostEnvironment = webHostEnvironment;
            _httpContextAccessor = httpContextAccessor;
            _fileProvider = fileProvider;
            _webHelper = webHelper;
            _workContext = workContext;
            _amazonS3Settings = amazonS3Settings;
            _mediaSettings = mediaSettings;
            _fileRootPath = null;
            _lang = new Dictionary<string, string>();
        }

        public async Task ConfigureAsync()
        {
            await CreateConfigurationAsync();

            var existingText = await _fileProvider.ReadAllTextAsync(GetConfigurationFilePath(), Encoding.UTF8);
            var config = JsonConvert.DeserializeObject<Dictionary<string, string>>(existingText);
            _fileRootPath = _fileProvider.GetAbsolutePath(config["FILES_ROOT"]);
        }

        public async Task CreateConfigurationAsync()
        {
            //var filePath = GetFullPath(CONFIGURATION_FILE);
            var filePath = CONFIGURATION_FILE;
            //create file if not exists
            _fileProvider.CreateFile(filePath);

            //try to read existing configuration
            var existingText = _fileProvider.ReadAllText(filePath, Encoding.UTF8);
            var existingConfiguration = JsonConvert.DeserializeAnonymousType(existingText, new
            {
                FILES_ROOT = string.Empty,
                SESSION_PATH_KEY = string.Empty,
                THUMBS_VIEW_WIDTH = string.Empty,
                THUMBS_VIEW_HEIGHT = string.Empty,
                PREVIEW_THUMB_WIDTH = string.Empty,
                PREVIEW_THUMB_HEIGHT = string.Empty,
                MAX_IMAGE_WIDTH = string.Empty,
                MAX_IMAGE_HEIGHT = string.Empty,
                DEFAULTVIEW = string.Empty,
                FORBIDDEN_UPLOADS = string.Empty,
                ALLOWED_UPLOADS = string.Empty,
                FILEPERMISSIONS = string.Empty,
                DIRPERMISSIONS = string.Empty,
                LANG = string.Empty,
                DATEFORMAT = string.Empty,
                OPEN_LAST_DIR = string.Empty,
                INTEGRATION = string.Empty,
                RETURN_URL_PREFIX = string.Empty,
                DIRLIST = string.Empty,
                CREATEDIR = string.Empty,
                DELETEDIR = string.Empty,
                MOVEDIR = string.Empty,
                COPYDIR = string.Empty,
                RENAMEDIR = string.Empty,
                FILESLIST = string.Empty,
                UPLOAD = string.Empty,
                DOWNLOAD = string.Empty,
                DOWNLOADDIR = string.Empty,
                DELETEFILE = string.Empty,
                MOVEFILE = string.Empty,
                COPYFILE = string.Empty,
                RENAMEFILE = string.Empty,
                GENERATETHUMB = string.Empty
            });

            //check whether the path base has changed, otherwise there is no need to overwrite the configuration file
            var currentPathBase = _httpContextAccessor.HttpContext.Request.PathBase.ToString();
            if (existingConfiguration?.RETURN_URL_PREFIX?.Equals(currentPathBase) ?? false)
                return;

            //create configuration
            var configuration = new
            {
                FILES_ROOT = existingConfiguration?.FILES_ROOT ?? $"{_amazonS3Settings.AWSS3BucketName}/images",
                SESSION_PATH_KEY = existingConfiguration?.SESSION_PATH_KEY ?? string.Empty,
                THUMBS_VIEW_WIDTH = existingConfiguration?.THUMBS_VIEW_WIDTH ?? "140",
                THUMBS_VIEW_HEIGHT = existingConfiguration?.THUMBS_VIEW_HEIGHT ?? "120",
                PREVIEW_THUMB_WIDTH = existingConfiguration?.PREVIEW_THUMB_WIDTH ?? "300",
                PREVIEW_THUMB_HEIGHT = existingConfiguration?.PREVIEW_THUMB_HEIGHT ?? "200",
                MAX_IMAGE_WIDTH = existingConfiguration?.MAX_IMAGE_WIDTH ?? "1000",
                MAX_IMAGE_HEIGHT = existingConfiguration?.MAX_IMAGE_HEIGHT ?? "1000",
                DEFAULTVIEW = existingConfiguration?.DEFAULTVIEW ?? "list",
                FORBIDDEN_UPLOADS = existingConfiguration?.FORBIDDEN_UPLOADS ?? "zip js jsp jsb mhtml mht xhtml xht php phtml " +
                    "php3 php4 php5 phps shtml jhtml pl sh py cgi exe application gadget hta cpl msc jar vb jse ws wsf wsc wsh " +
                    "ps1 ps2 psc1 psc2 msh msh1 msh2 inf reg scf msp scr dll msi vbs bat com pif cmd vxd cpl htpasswd htaccess",
                ALLOWED_UPLOADS = existingConfiguration?.ALLOWED_UPLOADS ?? string.Empty,
                FILEPERMISSIONS = existingConfiguration?.FILEPERMISSIONS ?? "0644",
                DIRPERMISSIONS = existingConfiguration?.DIRPERMISSIONS ?? "0755",
                LANG = existingConfiguration?.LANG ?? (await _workContext.GetWorkingLanguageAsync()).UniqueSeoCode,
                DATEFORMAT = existingConfiguration?.DATEFORMAT ?? "dd/MM/yyyy HH:mm",
                OPEN_LAST_DIR = existingConfiguration?.OPEN_LAST_DIR ?? "yes",

                //no need user to configure
                INTEGRATION = "tinymce4",
                RETURN_URL_PREFIX = currentPathBase,
                DIRLIST = $"{currentPathBase}/Admin/RoxyFileman/ProcessRequest?a=DIRLIST",
                CREATEDIR = $"{currentPathBase}/Admin/RoxyFileman/ProcessRequest?a=CREATEDIR",
                DELETEDIR = $"{currentPathBase}/Admin/RoxyFileman/ProcessRequest?a=DELETEDIR",
                MOVEDIR = $"{currentPathBase}/Admin/RoxyFileman/ProcessRequest?a=MOVEDIR",
                COPYDIR = $"{currentPathBase}/Admin/RoxyFileman/ProcessRequest?a=COPYDIR",
                RENAMEDIR = $"{currentPathBase}/Admin/RoxyFileman/ProcessRequest?a=RENAMEDIR",
                FILESLIST = $"{currentPathBase}/Admin/RoxyFileman/ProcessRequest?a=FILESLIST",
                UPLOAD = $"{currentPathBase}/Admin/RoxyFileman/ProcessRequest?a=UPLOAD",
                DOWNLOAD = $"{currentPathBase}/Admin/RoxyFileman/ProcessRequest?a=DOWNLOAD",
                DOWNLOADDIR = $"{currentPathBase}/Admin/RoxyFileman/ProcessRequest?a=DOWNLOADDIR",
                DELETEFILE = $"{currentPathBase}/Admin/RoxyFileman/ProcessRequest?a=DELETEFILE",
                MOVEFILE = $"{currentPathBase}/Admin/RoxyFileman/ProcessRequest?a=MOVEFILE",
                COPYFILE = $"{currentPathBase}/Admin/RoxyFileman/ProcessRequest?a=COPYFILE",
                RENAMEFILE = $"{currentPathBase}/Admin/RoxyFileman/ProcessRequest?a=RENAMEFILE",
                GENERATETHUMB = $"{currentPathBase}/Admin/RoxyFileman/ProcessRequest?a=GENERATETHUMB"
            };

            //save the file
            var text = JsonConvert.SerializeObject(configuration, Formatting.Indented);
            _fileProvider.WriteAllText(filePath, text, Encoding.UTF8);
        }

        public async Task CopyDirectoryAsync(string source, string destination, bool isSuccess = true)
        {
            using (var client = GetS3Client())
            {
                var destinationRequest = new ListObjectsRequest
                {
                    BucketName = _amazonS3Settings.AWSS3BucketName,
                    Prefix = destination
                };
                var listObjectResultDestination = await client.ListObjectsAsync(destinationRequest, CancellationToken.None);

                var destinationSuffix = GetLastChildFromPath(source);
                var pathToCopy = destination + "/" + destinationSuffix + "/";
                var isDublicate = await listObjectResultDestination.S3Objects.Where(s => s.Key == pathToCopy).ToListAsync();

                if (isDublicate.Count > 0)
                {
                    throw new RoxyFilemanException("E_DirAlreadyExists");
                }

                var sourceRequest = new ListObjectsRequest
                {
                    BucketName = _amazonS3Settings.AWSS3BucketName,
                    Prefix = source
                };
                var listObjectResultSource = await client.ListObjectsAsync(sourceRequest, CancellationToken.None);

                for (int i = 0; i < listObjectResultSource.S3Objects.Count; i++)
                {
                    var keyValue = listObjectResultSource.S3Objects[i].Key;
                    string destinationKeyValue = pathToCopy + GetPathAfterRootPath(keyValue, source + "/");

                    var copyObjectRequest = new CopyObjectRequest
                    {
                        SourceBucket = _amazonS3Settings.AWSS3BucketName,
                        SourceKey = keyValue,
                        DestinationBucket = _amazonS3Settings.AWSS3BucketName,
                        DestinationKey = destinationKeyValue,
                        CannedACL = GetS3CannedACL(((CannedACLEnum)_amazonS3Settings.CannedACLId))
                    };

                    await client.CopyObjectAsync(copyObjectRequest);
                }
            }
            if (isSuccess)
            {
                await _httpContextAccessor.HttpContext.Response.WriteAsync(GetSuccessRes());
            }
        }

        public async Task CopyFileAsync(string sourcePath, string destinationPath, bool returnSuccess = true, bool isRename = false)
        {
            try
            {
                var rootDir = $"{_amazonS3Settings.AWSS3RootUrl}{_amazonS3Settings.AWSS3BucketName}/";
                sourcePath = sourcePath.Contains("https://") ? sourcePath.Substring(rootDir.Length, sourcePath.Length - rootDir.Length) : sourcePath;
                destinationPath = destinationPath.Contains("https://") ? destinationPath.Substring(rootDir.Length, destinationPath.Length - rootDir.Length) : destinationPath;
                var fileName = "";
                if (isRename)
                {
                    fileName = GetFileName(destinationPath);
                    var paths = destinationPath.Split('/');
                    destinationPath = "";
                    for (var i = 0; i < paths.Length - 1; i++)
                    {
                        destinationPath += paths[i] + "/";
                    }
                    destinationPath = destinationPath.TrimEnd('/');
                    fileName = await MakeUniqueFilenameAwsAsync(destinationPath, fileName);
                }
                else
                {
                    fileName = GetFileName(sourcePath);
                    fileName = await MakeUniqueFilenameAwsAsync(destinationPath, fileName);

                }

                using (var client = GetS3Client())
                {
                    var copyRequest = new CopyObjectRequest
                    {
                        SourceBucket = _amazonS3Settings.AWSS3BucketName,
                        SourceKey = sourcePath,
                        DestinationBucket = _amazonS3Settings.AWSS3BucketName,
                        DestinationKey = destinationPath + "/" + fileName,
                        CannedACL = GetS3CannedACL(((CannedACLEnum)_amazonS3Settings.CannedACLId))
                    };
                    var response = await client.CopyObjectAsync(copyRequest);
                    if (returnSuccess)
                    {
                        await _httpContextAccessor.HttpContext.Response.WriteAsync(GetSuccessRes());
                    }
                }
            }
            catch
            {
                throw new RoxyFilemanException("E_CopyFile");
            }
        }

        public async Task CreateDirectoryAsync(string parentDirectoryPath, string name, bool returnSuccess = true)
        {
            var regexItem = new Regex("^[a-zA-Z0-9 ._-]*$");
            if (regexItem.IsMatch(name))
            {
                using (var client = GetS3Client())
                {
                    var path = parentDirectoryPath.TrimEnd('/') + "/" + name.TrimEnd('/') + "/";
                    var folderName = await MakeUniqueDirectoryAwsAsync(parentDirectoryPath, name + "/");
                    folderName = folderName.TrimEnd('/');
                    if (name != folderName)
                    {
                        throw new RoxyFilemanException("E_DirAlreadyExists");
                    }
                    var copyRequest = new PutObjectRequest
                    {
                        BucketName = _amazonS3Settings.AWSS3BucketName,
                        Key = parentDirectoryPath + "/" + folderName + "/",
                    };
                    var response = await client.PutObjectAsync(copyRequest);
                    if (returnSuccess)
                    {
                        await _httpContextAccessor.HttpContext.Response.WriteAsync(GetSuccessRes());
                    }
                }
            }
            else
            {
                throw new Exception("folder name can't contain special characters");
            }
        }

        public async Task<byte[]> CreateImageThumbnailAsync(string path, int width, int height, string contentType)
        {
            var key = GetS3Key(path);
            var request = new GetObjectRequest()
            {
                BucketName = _amazonS3Settings.AWSS3BucketName,
                Key = key
            };
            var response = await GetS3Client().GetObjectAsync(request);

            using var imageStream = response.ResponseStream;
            using var ms = new MemoryStream();

            imageStream.CopyTo(ms);

            return ResizeImage(ms.ToArray(), GetImageFormatByMimeType(contentType), width, height);
        }

        public async Task DeleteDirectoryAsync(string path, bool returnSuccess = true)
        {
            if (path == "images/uploaded")
            {
                throw new RoxyFilemanException("E_CannotDeleteDir");
            }

            var value = path.TrimEnd('/') + '/';
            try
            {
                using (var client = GetS3Client())
                {
                    var requestForDelete = new ListObjectsRequest
                    {
                        BucketName = _amazonS3Settings.AWSS3BucketName,
                        Prefix = value,

                    };
                    var deleteObjectResult = await client.ListObjectsAsync(requestForDelete, CancellationToken.None);
                    foreach (var aObject in deleteObjectResult.S3Objects)
                    {
                        var deleteRequest = new DeleteObjectRequest
                        {
                            BucketName = _amazonS3Settings.AWSS3BucketName,
                            Key = aObject.Key
                        };
                        await client.DeleteObjectAsync(deleteRequest);
                    }
                }
                if (returnSuccess)
                {
                    await _httpContextAccessor.HttpContext.Response.WriteAsync(GetSuccessRes());
                }
            }
            catch
            {
                throw new RoxyFilemanException("E_CannotDeleteDir");
            }
        }

        public async Task DeleteFileAsync(string path, string fileName = null, bool returnSuccess = true)
        {
            try
            {
                using (var client = GetS3Client())
                {

                    if (string.IsNullOrEmpty(fileName))
                    {
                        var files = path.Split('/');
                        fileName = GetFileName(path);
                        path = GetPathAfterBucket(path);
                    }
                    var copyRequest = new DeleteObjectRequest
                    {
                        BucketName = $"{_amazonS3Settings.AWSS3BucketName}",
                        Key = path + fileName
                    };
                    var response = await client.DeleteObjectAsync(copyRequest);
                    if (returnSuccess)
                    {
                        await _httpContextAccessor.HttpContext.Response.WriteAsync(GetSuccessRes());
                    }
                }
            }
            catch
            {
                throw new RoxyFilemanException("E_CreateDirFailed");
            }
        }

        public async Task DownloadDirectoryAsync(string path)
        {
            var urlPrefix = GetUrlPrefixBeforeChild(path);
            var targetDirectoryName = GetLastChildFromPath(path);
            var webClient = new WebClient();

            if (!Directory.Exists(_rootDirLocation))
            {
                Directory.CreateDirectory(_rootDirLocation);
            }
            using (var client = GetS3Client())
            {
                var downloadRequestList = new ListObjectsRequest
                {
                    BucketName = _amazonS3Settings.AWSS3BucketName,
                    Prefix = path + "/"
                };
                var response = await client.ListObjectsAsync(downloadRequestList, CancellationToken.None);
                var folders = await response.S3Objects.Where(s => s.Key.EndsWith('/')).ToListAsync();
                foreach (var item in folders)
                {
                    var folderSuffix = GetPathAfterRootPath(item.Key, urlPrefix);
                    Directory.CreateDirectory(_rootDirLocation + "/" + folderSuffix);
                }

                foreach (var item in response.S3Objects)
                {
                    if (!item.Key.EndsWith('/'))
                    {
                        var folderSuffix = GetPathAfterRootPath(item.Key, urlPrefix);
                        await webClient.DownloadFileTaskAsync(new Uri(_amazonS3Settings.AWSS3RootUrl + _amazonS3Settings.AWSS3BucketName + "/" + item.Key), _rootDirLocation + "/" + folderSuffix);

                    }
                }

                var zipName = targetDirectoryName + ".zip";
                var zipPath = _rootDirLocation + "/" + zipName;
                var sourceDirectoryName = _rootDirLocation + "/" + targetDirectoryName;
                if (_fileProvider.FileExists(zipPath))
                    _fileProvider.DeleteFile(zipPath);

                ZipFile.CreateFromDirectory(sourceDirectoryName, zipPath, CompressionLevel.Fastest, true);
                GetHttpContext().Response.Clear();
                GetHttpContext().Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{WebUtility.UrlEncode(zipName)}\"");
                GetHttpContext().Response.ContentType = MimeTypes.ApplicationForceDownload;
                await GetHttpContext().Response.SendFileAsync(zipPath);
                _fileProvider.DeleteDirectory(_rootDirLocation + "/" + targetDirectoryName);
                _fileProvider.DeleteFile(zipPath);
            }
        }

        public async Task DownloadFileAsync(string path)
        {
            using (var client = GetS3Client())
            {
                var rootDir = $"{_amazonS3Settings.AWSS3RootUrl}{_amazonS3Settings.AWSS3BucketName}/";
                var filePath = path.Substring(rootDir.Length, path.Length - rootDir.Length);
                var files = path.Split('/');
                var fileName = files[files.Length - 1];
                var script = "<script>" +
                   "fetch(\"" + path + "\")\r\n " +
                   ".then(resp => resp.blob())\r\n " +
                   " .then(blob => {\r\n const url = window.URL.createObjectURL(blob);\r\n " +
                   " const a = document.createElement(\'a\');\r\n " +
                   " a.style.display = \'none\';\r\n " +
                   " a.href = url;\r\n " +
                   " a.download = \"" + fileName + "\";\r\n " +
                   " document.body.appendChild(a);\r\n " +
                   " a.click();\r\n " +
                   " window.URL.revokeObjectURL(url);\r\n })\r\n .catch(() => console.log(\'download failled!\'));"
                   + "</script>";

               await _httpContextAccessor.HttpContext.Response.WriteAsync(script);
            }
        }

        public async Task<IEnumerable<object>> GetDirectoriesAsync(string type)
        {
            var dirs = await AwsListDirsAsync();
            await _httpContextAccessor.HttpContext.Response.WriteAsync("[");
            for (var i = 0; i < dirs.Count; i++)
            {
                var dir = dirs[i];
                await _httpContextAccessor.HttpContext.Response.WriteAsync("{\"p\":\"" + dir.Name.TrimEnd('/') + "\",\"f\":\"" + dir.Count + "\",\"d\":\"" + dir.Name + "\"}");

                if (i < dirs.Count - 1)
                    await _httpContextAccessor.HttpContext.Response.WriteAsync(",");
            }
            await _httpContextAccessor.HttpContext.Response.WriteAsync("]");
            return dirs;
        }

        public async Task<IEnumerable<object>> GetFilesAsync(string path, string type)
        {
            var files = await GetFilesFromADirectory(path);
            var response = _httpContextAccessor.HttpContext.Response;
            await response.WriteAsync("[");
            for (var i = 0; i < files.Length; i++)
            {
                var f = files[i];
                var filePath = (_amazonS3Settings.EnableCdn && !String.IsNullOrEmpty(_amazonS3Settings.CdnBaseUrl)) ? $"{_amazonS3Settings.CdnBaseUrl}/{f.Key}" : $"{_amazonS3Settings.AWSS3RootUrl}{_amazonS3Settings.AWSS3BucketName}/{f.Key}";
                await response.WriteAsync("{");
                await response.WriteAsync("\"p\":\"" + filePath + "\"");
                await response.WriteAsync(",\"t\":\"" + Math.Ceiling(LinuxTimestamp(f.LastModified)).ToString() + "\"");
                await response.WriteAsync(",\"s\":\"" + f.Size.ToString() + "\"");
                await response.WriteAsync(",\"w\":\"" + 0 + "\"");
                await response.WriteAsync(",\"h\":\"" + 0 + "\"");
                await response.WriteAsync("}");
                if (i < files.Length - 1)
                    await response.WriteAsync(",");
            }
            await response.WriteAsync("]");
            return files;
        }

        public async Task MoveDirectoryAsync(string source, string destination, bool isSuccess = true)
        {
            await CopyDirectoryAsync(source, destination, false);
            await DeleteDirectoryAsync(source, false);
            if (isSuccess)
            {
                await _httpContextAccessor.HttpContext.Response.WriteAsync(GetSuccessRes());
            }
        }

        public async Task MoveFileAsync(string sourcePath, string destinationPath)
        {
            try
            {
                var path = destinationPath.Split('/');
                destinationPath = "";
                for (var i = 0; i < path.Length - 1; i++)
                {
                    destinationPath += path[i];
                    if (i + 1 != path.Length - 1)
                    {
                        destinationPath += "/";
                    }
                }
                await CopyFileAsync(sourcePath, destinationPath, false);
                await DeleteFileAsync(sourcePath, null, false);
                await _httpContextAccessor.HttpContext.Response.WriteAsync(GetSuccessRes());
            }
            catch
            {
                throw new RoxyFilemanException("E_MoveFile");
            }
        }

        public async Task MoveFileAsync(string sourcePath, string destinationPath, string fileName, bool returnSuccess = true)
        {
            try
            {
                await CopyFileAsync(sourcePath.TrimEnd('/') + '/' + fileName, destinationPath, false);
                await DeleteFileAsync(sourcePath, fileName, false);
                if (returnSuccess)
                {
                    await _httpContextAccessor.HttpContext.Response.WriteAsync(GetSuccessRes());
                }
            }
            catch
            {
                throw new RoxyFilemanException("E_MoveFile");
            }
        }

        public async Task RenameDirectoryAsync(string parentDirectoryPath, string folderName)
        {
            var regexItem = new Regex("^[a-zA-Z0-9 _.-]*$");
            if (regexItem.IsMatch(folderName))
            {
                var newPath = "";
                var newPathArr = (parentDirectoryPath.TrimEnd('/')).Split('/');
                for (var i = 0; i < newPathArr.Length - 1; i++)
                {
                    newPath += newPathArr[i] + "/";
                }
                newPath = newPath.TrimEnd('/');
                var destination = newPath + "/" + folderName;

                using (var client = GetS3Client())
                {
                    var destinationRequest = new ListObjectsRequest
                    {
                        BucketName = _amazonS3Settings.AWSS3BucketName,
                        Prefix = destination + "/"
                    };
                    var listObjectResultDestination = await client.ListObjectsAsync(destinationRequest, CancellationToken.None);
                    var destinationSuffix = GetLastChildFromPath(destination);

                    var isDublicate = await listObjectResultDestination.S3Objects.Where(s => s.Key == destination + "/").ToListAsync();

                    if (isDublicate.Count > 0)
                    {
                        throw new RoxyFilemanException("E_DirAlreadyExists");
                    }

                    var putObjectRequestDestination = new PutObjectRequest
                    {
                        BucketName = _amazonS3Settings.AWSS3BucketName,
                        Key = destination + "/"
                    };
                    await client.PutObjectAsync(putObjectRequestDestination);

                    var sourceRequest = new ListObjectsRequest
                    {
                        BucketName = _amazonS3Settings.AWSS3BucketName,
                        Prefix = parentDirectoryPath + "/"
                    };

                    var listObjectResultSource = await client.ListObjectsAsync(sourceRequest, CancellationToken.None);

                    foreach (var item in listObjectResultSource.S3Objects)
                    {
                        if (item.Key != parentDirectoryPath + "/")
                        {
                            var destinationKeyValue = destination + "/" + GetPathAfterRootPath(item.Key, parentDirectoryPath + "/");
                            var copyObjectRequest = new CopyObjectRequest
                            {
                                SourceBucket = _amazonS3Settings.AWSS3BucketName,
                                SourceKey = item.Key,
                                DestinationBucket = _amazonS3Settings.AWSS3BucketName,
                                DestinationKey = destinationKeyValue,
                                CannedACL = GetS3CannedACL(((CannedACLEnum)_amazonS3Settings.CannedACLId))
                            };
                            await client.CopyObjectAsync(copyObjectRequest);
                        }
                    }

                }
                await DeleteDirectoryAsync(parentDirectoryPath, false);
                await _httpContextAccessor.HttpContext.Response.WriteAsync(GetSuccessRes());
            }
            else
            {
                throw new RoxyFilemanException("E_RenameDir");
            }
        }

        public async Task RenameFileAsync(string originalPath, string newFilename)
        {
            var regexItem = new Regex("^[a-zA-Z0-9 _.-]*$");
            if (regexItem.IsMatch(newFilename))
            {
                var path = originalPath.Split('/');
                var destianationPath = "";
                for (var i = 4; i < path.Length - 1; i++)
                {
                    destianationPath += path[i] + "/";
                }
                destianationPath += newFilename;
                await CopyFileAsync(originalPath, destianationPath, false, true);
                await DeleteFileAsync(originalPath, null, false);
                await _httpContextAccessor.HttpContext.Response.WriteAsync(GetSuccessRes());
            }
            else
            {
                throw new RoxyFilemanException("E_RenameFile");
            }
        }

        public async Task UploadFilesAsync(string path)
        {
            var res = GetSuccessRes();
            var hasErrors = false;
            try
            {
                for (var i = 0; i < _httpContextAccessor.HttpContext.Request.Form.Files.Count; i++)
                {
                    var file = _httpContextAccessor.HttpContext.Request.Form.Files[i];
                    var filename = await MakeUniqueFilenameAwsAsync(path, file.FileName);

                    path = _amazonS3Settings.AWSS3BucketName + "/" + path;
                    using (var client = GetS3Client())
                    {
                        using (var newMemoryStream = new MemoryStream())
                        {
                            file.CopyTo(newMemoryStream);
                            var putRequest = new PutObjectRequest
                            {
                                BucketName = path,
                                Key = filename,
                                InputStream = newMemoryStream,
                                ContentType = file.ContentType,
                            };
                            putRequest.CannedACL = GetS3CannedACL(((CannedACLEnum)_amazonS3Settings.CannedACLId));
                            var response = await client.PutObjectAsync(putRequest);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                res = GetErrorRes(ex.Message);
            }
            if (IsAjaxUpload())
            {
                if (hasErrors)
                    res = GetErrorRes(await LangResAsync("E_UploadNotAll"));
                await _httpContextAccessor.HttpContext.Response.WriteAsync(res);
            }
            else
            {
                await _httpContextAccessor.HttpContext.Response.WriteAsync("<script>");
                await _httpContextAccessor.HttpContext.Response.WriteAsync("parent.fileUploaded(" + res + ");");
                await _httpContextAccessor.HttpContext.Response.WriteAsync("</script>");
            }
        }

        protected virtual (int width, int height) ValidateImageMeasures(SKBitmap image, int maxWidth = 0, int maxHeight = 0)
        {
            ArgumentNullException.ThrowIfNull(image);

            float width = Math.Min(image.Width, maxWidth);
            float height = Math.Min(image.Height, maxHeight);

            var targetSize = Math.Max(width, height);

            if (image.Height > image.Width)
            {
                // portrait
                width = image.Width * (targetSize / image.Height);
                height = targetSize;
            }
            else
            {
                // landscape or square
                width = targetSize;
                height = image.Height * (targetSize / image.Width);
            }

            return ((int)width, (int)height);
        }

        protected virtual byte[] ResizeImage(byte[] data, SKEncodedImageFormat format, int maxWidth, int maxHeight)
        {
            using var sourceStream = new SKMemoryStream(data);
            using var inputData = SKData.Create(sourceStream);
            using var image = SKBitmap.Decode(inputData);

            var (width, height) = ValidateImageMeasures(image, maxWidth, maxHeight);

            var toBitmap = new SKBitmap(width, height, image.ColorType, image.AlphaType);

            if (!image.ScalePixels(toBitmap, SKFilterQuality.None))
                throw new Exception("Image scaling");

            var newImage = SKImage.FromBitmap(toBitmap);
            var imageData = newImage.Encode(format, _mediaSettings.DefaultImageQuality);

            newImage.Dispose();
            return imageData.ToArray();
        }

        private AmazonS3Client GetS3Client()
        {
            var regionId = _amazonS3Settings.RegionEndpointId.ToString();
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

        protected virtual SKEncodedImageFormat GetImageFormatByMimeType(string mimeType)
        {
            var format = SKEncodedImageFormat.Jpeg;
            if (string.IsNullOrEmpty(mimeType))
                return format;

            var parts = mimeType.ToLowerInvariant().Split('/');
            var lastPart = parts[^1];

            switch (lastPart)
            {
                case "webp":
                    format = SKEncodedImageFormat.Webp;
                    break;
                case "png":
                case "gif":
                case "bmp":
                case "x-icon":
                    format = SKEncodedImageFormat.Png;
                    break;
                default:
                    break;
            }

            return format;
        }

        private double LinuxTimestamp(DateTime d)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0).ToLocalTime();
            var timeSpan = (d.ToLocalTime() - epoch);

            return timeSpan.TotalSeconds;
        }

        private string GetPathAfterBucket(string path)
        {
            var rootDir = $"{_amazonS3Settings.AWSS3RootUrl}{_amazonS3Settings.AWSS3BucketName}/";
            path = path.Substring(rootDir.Length, path.Length - rootDir.Length);
            if (path.EndsWith('/'))
                return path;

            var files = path.Split('/');
            var fileLength = files[files.Length - 1].Length;
            path = path.Substring(0, path.Length - fileLength);
            return path;
        }

        private async Task<List<AwsDirectoryModel>> AwsListDirsAsync()
        {
            var list = new List<AwsDirectoryModel>();
            using (var client = GetS3Client())
            {
                var listRequest = new ListObjectsRequest
                {
                    BucketName = _amazonS3Settings.AWSS3BucketName,
                    Prefix = "images/uploaded/",

                };
                var response = await client.ListObjectsAsync(listRequest, CancellationToken.None);

                do
                {
                    var folders = response.S3Objects.Where(x =>
                        x.Key.EndsWith(@"/")).ToList();

                    folders.ForEach(x => list.Add(new AwsDirectoryModel()
                    {
                        Count = response.S3Objects.Where(w => w.Key.Contains(x.Key)).ToList().Count,
                        Name = x.Key
                    }));

                    if (response.IsTruncated)
                    {
                        listRequest.Marker = response.NextMarker;
                    }
                    else
                    {
                        listRequest = null;
                    }
                } while (listRequest != null);
            }

            return list;
        }

        private string GetFileName(string path)
        {
            var files = path.Split('/');
            var fileName = files[files.Length - 1];
            return fileName;
        }

        private string GetSuccessRes()
        {
            return GetSuccessRes("");
        }

        private string GetSuccessRes(string msg)
        {
            return GetResultStr("ok", msg);
        }

        private string GetResultStr(string type, string msg)
        {
            return "{\"res\":\"" + type + "\",\"msg\":\"" + msg.Replace("\"", "\\\"") + "\"}";
        }

        private string GetLastChildFromPath(string path)
        {
            var pathArr = (path.TrimEnd('/')).Split('/');
            return pathArr[pathArr.Length - 1];
        }

        private string GetPathAfterRootPath(string keyValue, string source)
        {
            return keyValue.Substring(source.Length);
        }

        private async Task<string> MakeUniqueDirectoryAwsAsync(string parentDirectoryPath, string foldername)
        {
            var ret = foldername;
            var i = 0;
            while (await DirectoryExits(parentDirectoryPath + "/" + ret))
            {
                i++;
                ret = foldername.TrimEnd('/') + " - Copy " + i.ToString();
            }
            return ret;
        }

        private async Task<bool> DirectoryExits(string path)
        {
            var files = (await GetDirectories(path)).ToList();

            if (files.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private async Task<S3Object[]> GetDirectories(string path)
        {
            S3Object[] dirs;
            using (var client = GetS3Client())
            {
                var request = new ListObjectsRequest
                {
                    BucketName = _amazonS3Settings.AWSS3BucketName,
                    Prefix = path,
                    //Delimiter = "/"
                };

                var response = await client.ListObjectsAsync(request, CancellationToken.None);
                dirs = response.S3Objects.Where(s => s.Key.EndsWith("/")).ToArray();
            }

            return dirs;
        }

        private async Task<string> MakeUniqueFilenameAwsAsync(string path, string filename)
        {
            var ret = filename;
            var i = 0;
            while (await FileExists(path, ret))
            {
                i++;
                ret = Path.GetFileNameWithoutExtension(filename) + "-Copy" + i.ToString() + Path.GetExtension(filename);
            }

            return ret;
        }

        private async Task<bool> FileExists(string path, string filePrefix)
        {
            var files = (await GetFilesFromADirectory(path)).Where(s => s.Key.EndsWith(filePrefix)).ToList();

            if (files.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private async Task<S3Object[]> GetFilesFromADirectory(string path)
        {
            S3Object[] files;
            using (var client = GetS3Client())
            {
                var request = new ListObjectsRequest
                {
                    BucketName = _amazonS3Settings.AWSS3BucketName,
                    Prefix = path + "/",
                    Delimiter = "/"
                };

                var response = await client.ListObjectsAsync(request, CancellationToken.None);
                files = response.S3Objects.Where(s => !s.Key.EndsWith("/")).ToArray();
            }

            return files;
        }

        private string GetS3Key(string path)
        {
            return path.Substring(GetS3BucketFullPath().Length, path.Length - GetS3BucketFullPath().Length).TrimStart('/');
        }

        private string GetS3BucketFullPath()
        {
            return $"{_amazonS3Settings.AWSS3RootUrl}/{_amazonS3Settings.AWSS3BucketName}";
        }

        protected string GetErrorRes(string msg)
        {
            return GetResultStr("error", msg);
        }

        protected bool IsAjaxUpload()
        {
            return (_httpContextAccessor.HttpContext.Request.Query["method"].ToString() != null && _httpContextAccessor.HttpContext.Request.Query["method"].ToString() == "ajax");
        }

        protected async Task<string> LangResAsync(string name)
        {
            var ret = name;
            if (_lang == null)
                _lang = ParseJSON(await GetLangFileAsync());
            if (_lang.ContainsKey(name))
                ret = _lang[name];

            return ret;
        }

        private async Task<string> GetLangFileAsync()
        {
            var filename = "../lang/" + await GetSettingAsync("LANG") + ".json";
            var path = Path.Combine(_webRootPath, filename);

            if (!File.Exists(path))
                filename = "../lang/en.json";
            return filename;
        }

        protected Dictionary<string, string> ParseJSON(string file)
        {
            var ret = new Dictionary<string, string>();
            var json = "";
            try
            {
                file = Path.Combine(_webRootPath, file);
                json = File.ReadAllText(file, Encoding.UTF8);
            }
            catch { }

            json = json.Trim();
            if (json != "")
            {
                if (json.StartsWith("{"))
                    json = json.Substring(1, json.Length - 2);
                json = json.Trim();
                json = json.Substring(1, json.Length - 2);
                var lines = Regex.Split(json, "\"\\s*,\\s*\"");
                foreach (var line in lines)
                {
                    var tmp = Regex.Split(line, "\"\\s*:\\s*\"");
                    try
                    {
                        if (tmp[0] != "" && !ret.ContainsKey(tmp[0]))
                        {
                            ret.Add(tmp[0], tmp[1]);
                        }
                    }
                    catch { }
                }
            }
            return ret;
        }

        private string GetUrlPrefixBeforeChild(string path)
        {
            var url = "";
            var pathArr = path.Split('/');
            for (var i = 0; i < pathArr.Length - 1; i++)
            {
                url += pathArr[i] + "/";
            }
            return url;
        }

        public string GetConfigurationFilePath()
        {
            return GetFullPath(NopRoxyFilemanDefaults.ConfigurationFile);
        }

        /// <summary>
        /// Get the absolute path by virtual path
        /// </summary>
        /// <param name="virtualPath">Virtual path</param>
        /// <returns>Path</returns>
        protected virtual string GetFullPath(string virtualPath)
        {
            virtualPath ??= string.Empty;
            if (!virtualPath.StartsWith("/"))
                virtualPath = "/" + virtualPath;
            virtualPath = virtualPath.TrimEnd('/');

            return _fileProvider.Combine(_webHostEnvironment.WebRootPath, virtualPath);
        }

        /// <summary>
        /// Get the string to write to the response
        /// </summary>
        /// <param name="type">Type of the response</param>
        /// <param name="message">Additional message</param>
        /// <returns>String to write to the response</returns>
        protected virtual string GetResponse(string type, string message)
        {
            return $"{{\"res\":\"{type}\",\"msg\":\"{message?.Replace("\"", "\\\"")}\"}}";
        }
        public string GetErrorResponse(string message = null)
        {
            return GetResponse("error", message);
        }
        /// <summary>
        /// Get the http context
        /// </summary>
        /// <returns>Http context</returns>
        protected virtual HttpContext GetHttpContext()
        {
            return _httpContextAccessor.HttpContext;
        }
        /// <summary>
        /// Get a value of the configuration setting
        /// </summary>
        /// <param name="key">Setting key</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the setting value
        /// </returns>
        protected virtual async Task<string> GetSettingAsync(string key)
        {
            if (_settings == null)
                _settings = await ParseJsonAsync(GetFullPath(NopRoxyFilemanDefaults.ConfigurationFile));

            if (_settings.TryGetValue(key, out var value))
                return value;

            return null;
        }
        /// <summary>
        /// Parse the JSON file
        /// </summary>
        /// <param name="file">Path to the file</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the collection of keys and values from the parsed file
        /// </returns>
        protected virtual async Task<Dictionary<string, string>> ParseJsonAsync(string file)
        {
            var result = new Dictionary<string, string>();
            var json = string.Empty;
            try
            {
                json = (await _fileProvider.ReadAllTextAsync(file, Encoding.UTF8))?.Trim();
            }
            catch
            {
                //ignore any exception
            }

            if (string.IsNullOrEmpty(json))
                return result;

            if (json.StartsWith("{"))
                json = json[1..^1];

            json = json.Trim();
            json = json[1..^1];

            var lines = Regex.Split(json, "\"\\s*,\\s*\"");
            foreach (var line in lines)
            {
                var tmp = Regex.Split(line, "\"\\s*:\\s*\"");
                try
                {
                    if (!string.IsNullOrEmpty(tmp[0]) && !result.ContainsKey(tmp[0]))
                        result.Add(tmp[0], tmp[1]);
                }
                catch
                {
                    //ignore any exception
                }
            }

            return result;
        }
        //public Task<string> GetLanguageResourceAsync(string key)
        //{
        //    if (_languageResources == null)
        //        _languageResources = await ParseJsonAsync(await GetLanguageFileAsync());

        //    if (_languageResources.TryGetValue(key, out var value))
        //        return value;

        //    return key;
        //}

        //public bool IsAjaxRequest()
        //{
        //    throw new NotImplementedException();
        //}
    }
}
