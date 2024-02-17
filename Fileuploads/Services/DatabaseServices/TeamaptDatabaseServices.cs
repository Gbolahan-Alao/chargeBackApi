using Fileuploads.Models;
using Microsoft.EntityFrameworkCore;

namespace Fileuploads.Services.DatabaseServices
{
    public class TeamaptDatabaseServices
    {
        private readonly AppDbContext _dbContext;
        public TeamaptDatabaseServices(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<IEnumerable<TeamaptUploadedFile>> GetAllTeamaptFilesAsync()
        {
            return await _dbContext.TeamaptUploadedFile.ToListAsync();
        }

        public async Task<List<TeamaptUploadedFilesInfo>> GetTeamaptUploadedFileInfoAsync()
        {
            return await _dbContext.TeamaptUploadedFilesInfo.ToListAsync();
        }

        public async Task<string> ClearTeamaptUploadedData()
        {
            try
            {
                var uploadedFiles = await _dbContext.TeamaptUploadedFile.ToListAsync();

                _dbContext.TeamaptUploadedFile.RemoveRange(uploadedFiles);

                await _dbContext.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT('TeamaptUploadedFile', RESEED, 0)");

                await _dbContext.SaveChangesAsync();

                return "All uploaded data cleared from theTeamapt database successfully.";
            }
            catch (Exception ex)
            {

                return $"An error occurred: {ex.Message}";
            }
        }

        public async Task<string> DeleteAllTeamaptFileInfoAsync()
        {
            try
            {
                var fileInfoList = await _dbContext.TeamaptUploadedFilesInfo.ToListAsync();

                _dbContext.TeamaptUploadedFilesInfo.RemoveRange(fileInfoList);

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
