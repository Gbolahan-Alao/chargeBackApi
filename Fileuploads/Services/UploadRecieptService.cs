using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Fileuploads.Services
{
    public class UploadReceiptService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly FileUploadDbContext _dbContext;

        public UploadReceiptService(IWebHostEnvironment webHostEnvironment, FileUploadDbContext dbContext)
        {
            _webHostEnvironment = webHostEnvironment;
            _dbContext = dbContext;
        }

        public async Task<string> UploadReceiptAsync(string merchantId, string stan, string rrn, IFormFile receipt)
        {
            try
            {
                var uploadedFile = await _dbContext.UploadedFiles.FirstOrDefaultAsync(f => f.MerchantId == merchantId && f.Stan == stan && f.Rrn == rrn);

                if (uploadedFile == null)
                {
                    throw new Exception("Invalid dispute or dispute is not rejected.");
                }

                var uploadsFolderPath = Path.Combine(_webHostEnvironment.WebRootPath, "receipts");
                var fileName = $"{stan}-{rrn}{Path.GetExtension(receipt.FileName)}";
                var filePath = Path.Combine(uploadsFolderPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await receipt.CopyToAsync(stream);
                }

                uploadedFile.ReceiptFilePath = filePath;
                await _dbContext.SaveChangesAsync();

                return filePath; 
            }
            catch (Exception ex)
            {
                throw new Exception($"Error uploading receipt: {ex.Message}");
            }
        }

        public async Task<(byte[] FileBytes, string ContentType)> DownloadReceiptAsync(string merchantId, string stan, string rrn)
        {
            try
            {
                var uploadedFile = await _dbContext.UploadedFiles.FirstOrDefaultAsync(f => f.MerchantId == merchantId && f.Stan == stan && f.Rrn == rrn);

                if (uploadedFile == null || string.IsNullOrEmpty(uploadedFile.ReceiptFilePath))
                {
                    throw new Exception("Receipt not found.");
                }

                var filePath = uploadedFile.ReceiptFilePath;

                // Determine the content type based on the file extension
                var contentType = GetContentType(filePath);

                // Read the file content into a byte array
                var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);

                return (fileBytes, contentType);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error downloading receipt: {ex.Message}");
            }
        }

        private string GetContentType(string filePath)
        {
            var fileExtension = Path.GetExtension(filePath).ToLowerInvariant();
            switch (fileExtension)
            {
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".png":
                    return "image/png";
                case ".gif":
                    return "image/gif";
                case ".pdf":
                    return "application/pdf";
                default:
                    return "application/octet-stream"; // Default content type
                                                       // Add more cases as needed for other file types
            }
        }


    }
}
