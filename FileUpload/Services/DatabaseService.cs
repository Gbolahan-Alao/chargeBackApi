using FileUpload.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileUpload.Services
{
    public class DatabaseService
    {
        private readonly AppDbContext _dbContext;

        public DatabaseService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<UploadedFile>> GetAllUploadedFilesAsync()
        {
            return await _dbContext.UploadedFiles.ToListAsync();
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
    }
}
