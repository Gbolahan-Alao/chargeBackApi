using Fileuploads.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace Fileuploads.Services.Interfaces
{
    public interface IFileUploadService
    {
        Task<(string, string, int, int)> UploadFileAsync(IFormFile file, string merchantId);
        Task<IEnumerable<UploadedFileInfo>> GetUploadedFileInfoAsync();
        Task<byte[]> DownloadFileAsync(string fileName);
    }
}
