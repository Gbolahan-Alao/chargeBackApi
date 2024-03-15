using System.Collections.Generic;
using System.Threading.Tasks;
using Fileuploads.Models;
using Fileuploads.Models;
using Microsoft.AspNetCore.Http;

namespace Fileuploads.Services.Interfaces
{
    public interface ITeamaptFileUploadService
    {
        Task<(string, string, int, int)> UploadFileAsync(IFormFile file, string merchantId);
        Task<IEnumerable<TeamaptUploadedFilesInfo>> GetUploadedFileInfoAsync();
        Task<byte[]> DownloadFileAsync(string fileName);
    }
}
