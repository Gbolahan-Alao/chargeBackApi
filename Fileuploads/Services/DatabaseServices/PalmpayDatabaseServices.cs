using Fileuploads.Models;
using Microsoft.EntityFrameworkCore;

namespace Fileuploads.Services.DatabaseServices
{
    public class PalmpayDatabaseServices
    {
        private readonly AppDbContext _dbContext;
        public PalmpayDatabaseServices(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<IEnumerable<PalmpayUploadedFile>> GetAllPalmpayFilesAsync()
        {
            return await _dbContext.PalmpayUploadedFile.ToListAsync();
        }

        public async Task<List<PalmpayUploadedFilesInfo>> GetPalmpayUploadedFileInfoAsync()
        {
            return await _dbContext.PalmpayUploadedFilesInfo.ToListAsync();
        }

        public async Task<string> ClearPalmpayUploadedData()
        {
            try
            {
                var uploadedFiles = await _dbContext.PalmpayUploadedFile.ToListAsync();

                _dbContext.PalmpayUploadedFile.RemoveRange(uploadedFiles);

                await _dbContext.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT('PalmpayUploadedFile', RESEED, 0)");

                await _dbContext.SaveChangesAsync();

                return "All uploaded data cleared from the Palmpay database successfully.";
            }
            catch (Exception ex)
            {

                return $"An error occurred: {ex.Message}";
            }
        }

        public async Task<string> DeleteAllPalmpayFileInfoAsync()
        {
            try
            {
                var fileInfoList = await _dbContext.PalmpayUploadedFilesInfo.ToListAsync();

                _dbContext.PalmpayUploadedFilesInfo.RemoveRange(fileInfoList);

                await _dbContext.SaveChangesAsync();

                return "All file info deleted successfully.";
            }
            catch (Exception ex)
            {
                return $"An error occurred while deleting file info: {ex.Message}";
            }
        }
    }
}
