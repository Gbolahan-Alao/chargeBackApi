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
        private readonly FileUploadDbContext _dbContext;

        public DatabaseService(FileUploadDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<UploadedFile>> GetAllUploadedFilesAsync(String merchantId)
        {
            return await _dbContext.UploadedFiles.Where(f => f.MerchantId == merchantId).OrderByDescending(file => file.DateLogged)
            .ToListAsync();
        }
        public async Task<IEnumerable<UploadedFile>> GetAllFilesAsync()
        {
            return await _dbContext.UploadedFiles.ToListAsync();

        }


        public async Task<List<UploadedFileInfo>> GetUploadedFileInfoAsync(String merchantId)
        {
           
            return await _dbContext.UploadedFileInfos.Where(f=>f.MerchantId==merchantId).OrderByDescending(file => file.UploadDate).ToListAsync();
        }

        public async Task<object> GetDashboardData()
        {
            try
            {
                var uploadedFiles = await _dbContext.UploadedFiles.ToListAsync();

                var totalCount = uploadedFiles.Count();
                var acceptedCount = uploadedFiles.Count(x => x.Action == "Accept");
                var rejectedCount = uploadedFiles.Count(x => x.Action == "Reject");
                var pendingCount = totalCount - acceptedCount - rejectedCount;

                var dashboardData = new
                {
                    TotalCount = totalCount,
                    AcceptedCount = acceptedCount,
                    RejectedCount = rejectedCount,
                    PendingCount = pendingCount
                };

                return dashboardData;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred: {ex.Message}");
            }
        }

        public async Task<object> GetMerchantDashboardData(String merchantId)
        {
            try
            {
                var uploadedFiles = await _dbContext.UploadedFiles.Where(f => f.MerchantId == merchantId)
            .ToListAsync();

                var totalCount = uploadedFiles.Count();
                var acceptedCount = uploadedFiles.Count(x => x.Action == "Accept");
                var rejectedCount = uploadedFiles.Count(x => x.Action == "Reject");
                var pendingCount = totalCount - acceptedCount - rejectedCount;

                var dashboardData = new
                {
                    TotalCount = totalCount,
                    AcceptedCount = acceptedCount,
                    RejectedCount = rejectedCount,
                    PendingCount = pendingCount
                };

                return dashboardData;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred: {ex.Message}");
            }
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
    }
}
