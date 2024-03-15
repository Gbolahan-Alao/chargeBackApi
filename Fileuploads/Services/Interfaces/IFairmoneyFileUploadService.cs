using Fileuploads.Models;

namespace Fileuploads.Services.Interfaces
{
    public interface IFairmoneyFileUploadService
    {
        Task<(string, string, int, int)> UploadFileAsync(IFormFile file, string merchantId);
        Task<IEnumerable<FairmoneyUploadedFilesInfo>> GetUploadedFileInfoAsync();
        Task<byte[]> DownloadFileAsync(string fileName);
    }
}
