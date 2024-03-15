using FileUpload.Migrations;
using Fileuploads.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fileuploads.Services
{
    public class DatabaseService
    {
        private readonly AppDbContext _dbContext;

        public DatabaseService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<UploadedFile>> GetAllUploadedFilesAsync( string merchantId)
        {
            return await _dbContext.UploadedFiles.Where(f => f.MerchantId == merchantId)
            .ToListAsync();
        }

       

        public async Task<List<UploadedFileInfo>> GetUploadedFileInfoAsync(string merchantId)
        {
           
            return await _dbContext.UploadedFileInfos.Where(f=>f.MerchantId==merchantId).ToListAsync();
        }

      

        public async Task DeleteUploadedFilesAsync(List<int> fileIds)
        {
            var filesToDelete = await _dbContext.UploadedFiles
                .Where(f => fileIds.Contains(f.Id))
                .ToListAsync();

            _dbContext.UploadedFiles.RemoveRange(filesToDelete);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<string> ClearUploadedData()
        {
            try
            {
               
                var uploadedFiles = await _dbContext.UploadedFiles.ToListAsync();

              
                _dbContext.UploadedFiles.RemoveRange(uploadedFiles);

              
                await _dbContext.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT('UploadedFiles', RESEED, 0)");

            
                await _dbContext.SaveChangesAsync();

                return "All uploaded data cleared from the database successfully.";
            }
            catch (Exception ex)
            {
           
                return $"An error occurred: {ex.Message}";
            }
        }

        public async Task<string> ClearTeamaptUploadedData()
        {
            try
            {

                var uploadedFiles = await _dbContext.TeamaptUploadedFile.ToListAsync();


                _dbContext.TeamaptUploadedFile.RemoveRange(uploadedFiles);


                await _dbContext.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT('TeamaptUploadedFile', RESEED, 0)");


                await _dbContext.SaveChangesAsync();

                return "All uploaded data cleared from the Teamapt database successfully.";
            }
            catch (Exception ex)
            {

                return $"An error occurred: {ex.Message}";
            }
        }

        public async Task<string> DeleteAllFileInfoAsync()
        {
            try
            {
                
                var fileInfoList = await _dbContext.UploadedFileInfos.ToListAsync();

               
                _dbContext.UploadedFileInfos.RemoveRange(fileInfoList);
                await _dbContext.SaveChangesAsync();

                return "All file info deleted successfully.";
            }
            catch (Exception ex)
            {
                return $"An error occurred while deleting file info: {ex.Message}";
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
