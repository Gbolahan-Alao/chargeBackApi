using FileUpload.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using FileUpload.Models;

namespace FileUpload.Services.Interfaces
{
    public interface IFileUploadService
    {
        Task<(string, string, int, int)> UploadFileAsync(IFormFile file);
        Task<IEnumerable<UploadedFileInfo>> GetUploadedFileInfoAsync();
        Task<byte[]> DownloadFileAsync(string fileName);
    }
}
