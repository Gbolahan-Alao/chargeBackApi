using Fileuploads.Models;
using Microsoft.EntityFrameworkCore;

namespace Fileuploads.Services.DatabaseServices
{
    public class FairmoneyDatabaseServices
    {
        private readonly AppDbContext _dbContext;
        public FairmoneyDatabaseServices(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<IEnumerable<FairmoneyUploadedFile>> GetAllFairmoneyUploadedFilesAsync()
        {
            return await _dbContext.FairmoneyUploadedFile.ToListAsync();
        }
        
        public async Task<List<FairmoneyUploadedFilesInfo>> GetFairmoneyUploadedFileInfoAsync()
        {
            return await _dbContext.FairmoneyUploadedFilesInfo.ToListAsync();
        }

        public async Task<string> ClearFairmoneyUploadedData()
        {
            try
            {
                var uploadedFiles = await _dbContext.FairmoneyUploadedFile.ToListAsync();

                _dbContext.FairmoneyUploadedFile.RemoveRange(uploadedFiles);

                await _dbContext.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT('FairmoneyUploadedFile', RESEED, 0)");

                await _dbContext.SaveChangesAsync();

                return "All uploaded data cleared from the Fairmoney database successfully.";
            }
            catch (Exception ex)
            {

                return $"An error occurred: {ex.Message}";
            }
        }

        public async Task<string> DeleteAllFairmoneyFileInfoAsync()
        {
            try
            {
                var fileInfoList = await _dbContext.FairmoneyUploadedFilesInfo.ToListAsync();

                _dbContext.FairmoneyUploadedFilesInfo.RemoveRange(fileInfoList);
               
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
