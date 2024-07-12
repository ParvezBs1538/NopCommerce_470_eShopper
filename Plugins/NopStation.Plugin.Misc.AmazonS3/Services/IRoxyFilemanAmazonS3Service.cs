using System.Collections.Generic;
using System.Threading.Tasks;

namespace NopStation.Plugin.Misc.AmazonS3.Services
{
    public interface IRoxyFilemanAmazonS3Service
    {
        #region Configuration

        /// <summary>
        /// Initial service configuration
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task ConfigureAsync();

        /// <summary>
        /// Gets a configuration file path
        /// </summary>
        string GetConfigurationFilePath();

        /// <summary>
        /// Create configuration file for RoxyFileman
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task CreateConfigurationAsync();

        #endregion

        #region Directories

        /// <summary>
        /// Copy the directory
        /// </summary>
        /// <param name="sourcePath">Path to the source directory</param>
        /// <param name="destinationPath">Path to the destination directory</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task CopyDirectoryAsync(string source, string destination, bool isSuccess = true);

        /// <summary>
        /// Create the new directory
        /// </summary>
        /// <param name="parentDirectoryPath">Path to the parent directory</param>
        /// <param name="name">Name of the new directory</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task CreateDirectoryAsync(string parentDirectoryPath, string name, bool returnSuccess = true);

        /// <summary>
        /// Delete the directory
        /// </summary>
        /// <param name="path">Path to the directory</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task DeleteDirectoryAsync(string path, bool returnSuccess = true);

        /// <summary>
        /// Download the directory from the server as a zip archive
        /// </summary>
        /// <param name="path">Path to the directory</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task DownloadDirectoryAsync(string path);

        /// <summary>
        /// Get all available directories as a directory tree
        /// </summary>
        /// <param name="type">Type of the file</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task<IEnumerable<object>> GetDirectoriesAsync(string type);

        /// <summary>
        /// Move the directory
        /// </summary>
        /// <param name="sourcePath">Path to the source directory</param>
        /// <param name="destinationPath">Path to the destination directory</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task MoveDirectoryAsync(string source, string destination, bool isSuccess = true);

        /// <summary>
        /// Rename the directory
        /// </summary>
        /// <param name="sourcePath">Path to the source directory</param>
        /// <param name="newName">New name of the directory</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task RenameDirectoryAsync(string parentDirectoryPath, string folderName);

        #endregion

        #region Files

        /// <summary>
        /// Copy the file
        /// </summary>
        /// <param name="sourcePath">Path to the source file</param>
        /// <param name="destinationPath">Path to the destination file</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task CopyFileAsync(string sourcePath, string destinationPath, bool returnSuccess = true, bool isRename = false);

        /// <summary>
        /// Delete the file
        /// </summary>
        /// <param name="path">Path to the file</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task DeleteFileAsync(string path, string fileName = null, bool returnSuccess = true);

        /// <summary>
        /// Download the file from the server
        /// </summary>
        /// <param name="path">Path to the file</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task DownloadFileAsync(string path);

        /// <summary>
        /// Get files in the passed directory
        /// </summary>
        /// <param name="directoryPath">Path to the files directory</param>
        /// <param name="type">Type of the files</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task<IEnumerable<object>> GetFilesAsync(string path, string type);

        /// <summary>
        /// Move the file
        /// </summary>
        /// <param name="sourcePath">Path to the source file</param>
        /// <param name="destinationPath">Path to the destination file</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        /// 
        Task MoveFileAsync(string sourcePath, string destinationPath);

        Task MoveFileAsync(string sourcePath, string destinationPath, string fileName, bool returnSuccess = true);

        /// <summary>
        /// Rename the file
        /// </summary>
        /// <param name="sourcePath">Path to the source file</param>
        /// <param name="newName">New name of the file</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task RenameFileAsync(string originalPath, string newFilename);

        /// <summary>
        /// Upload files to a directory on passed path
        /// </summary>
        /// <param name="directoryPath">Path to directory to upload files</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task UploadFilesAsync(string path);

        #endregion

        #region Images

        /// <summary>
        /// Create the thumbnail of the image and write it to the response
        /// </summary>
        /// <param name="path">Path to the image</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task<byte[]> CreateImageThumbnailAsync(string path, int width, int height, string contentType);

        /// <summary>
        /// Flush all images on disk
        /// </summary>
        /// <param name="removeOriginal">Specifies whether to delete original images</param>
        /// <returns>A task that represents the asynchronous operation</returns>


        /// <summary>
        /// Flush images on disk
        /// </summary>
        /// <param name="directoryPath">Directory path to flush images</param>
        /// <returns>A task that represents the asynchronous operation</returns>

        #endregion

        #region Others

        /// <summary>
        /// Get the string to write an error response
        /// </summary>
        /// <param name="message">Additional message</param>
        /// <returns>String to write to the response</returns>
        string GetErrorResponse(string message = null);

        /// <summary>
        /// Get the language resource value
        /// </summary>
        /// <param name="key">Language resource key</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the language resource value
        /// </returns>
        //Task<string> GetLanguageResourceAsync(string key);

        /// <summary>
        /// Whether the request is made with ajax 
        /// </summary>
        /// <returns>True or false</returns>
        //bool IsAjaxRequest();

        #endregion
    }
}
