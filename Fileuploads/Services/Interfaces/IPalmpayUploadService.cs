using Fileuploads.Models;

namespace Fileuploads.Services.Interfaces
{
    public interface IPalmpayUploadService
    {
        Task<(string, string, int, int)> UploadFileAsync(IFormFile file, string merchantId);
        Task<IEnumerable<PalmpayUploadedFilesInfo>> GetUploadedFileInfoAsync();
        Task<byte[]> DownloadFileAsync(string fileName);
    }
}
