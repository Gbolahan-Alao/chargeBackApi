using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace FileUpload.Services
{
    public class FileUploadService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly AppDbContext _dbContext;
        private readonly ExcelDataService _excelDataService;

        public FileUploadService(IWebHostEnvironment webHostEnvironment, AppDbContext dbContext, ExcelDataService excelDataService)
        {
            _webHostEnvironment = webHostEnvironment;
            _dbContext = dbContext;
            _excelDataService = excelDataService;
        }

        public async Task<(string, string)> UploadFileAsync(IFormFile file)
        {
            var fileName = Path.GetFileNameWithoutExtension(file.FileName);
            var fileExtension = Path.GetExtension(file.FileName);
            var uploadDateTime = DateTime.Now; 

            var uniqueFileName = $"{fileName}_{uploadDateTime:yyyyMMddHHmmss}{fileExtension}";

            
            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", uniqueFileName);
            if (File.Exists(filePath))
            {
                
                return (uniqueFileName, $"File {uniqueFileName} already exists in the uploads folder");
            }

            // Save the file to the wwwroot folder
            var uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }

            filePath = Path.Combine(uploadFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Read the file and extract data using ExcelDataService
            var uploadedFiles = _excelDataService.ExtractDataFromExcel(filePath);

            // Save extracted data to the database
            foreach (var uploadedFile in uploadedFiles)
            {
                _dbContext.UploadedFiles.Add(uploadedFile);
            }

            await _dbContext.SaveChangesAsync();

            // Return success message with filename
            return (uniqueFileName, $"{uniqueFileName} uploaded successfully");
        }
    }
}
