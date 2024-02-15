//using FileUpload.Models;
//using Microsoft.AspNetCore.Hosting;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using OfficeOpenXml;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Threading.Tasks;

//namespace FileUpload.Controllers
//{
//    [ApiController]
//    [Route("[controller]")]
//    public class FileController : ControllerBase
//    {
//        private readonly IWebHostEnvironment _webHostEnvironment;
//        private readonly AppDbContext _dbContext;

//        public FileController(AppDbContext dbContext, IWebHostEnvironment webHostEnvironment)
//        {
//            _dbContext = dbContext;
//            _webHostEnvironment = webHostEnvironment;
//        }
//        static FileController()
//        {
//            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
//        }

//        [HttpPost("upload")]
//        public async Task<IActionResult> UploadFiles(IFormFileCollection files)
//        {
//            try
//            {
//                if (files == null || files.Count == 0)
//                {
//                    return BadRequest(new { message = "No files were uploaded." });
//                }

//                var uploadedData = new List<UploadedFile>();

//                // Ensure wwwroot/uploads directory exists
//                var uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
//                if (!Directory.Exists(uploadFolder))
//                {
//                    Directory.CreateDirectory(uploadFolder);
//                }

//                // Iterate through each uploaded file
//                foreach (var file in files)
//                {
//                    var filePath = Path.Combine(uploadFolder, file.FileName);

//                    // Check if the file already exists
//                    if (System.IO.File.Exists(filePath))
//                    {
//                        return BadRequest(new { message = $"File '{file.FileName}' already exists." });
//                    }

//                    // Save the file to the wwwroot/uploads directory
//                    using (var stream = new FileStream(filePath, FileMode.Create))
//                    {
//                        await file.CopyToAsync(stream);
//                    }

//                    // Extract data from the saved file and save it to the database
//                    uploadedData.AddRange(ExtractDataFromFile(filePath));
//                }

//                // Saving uploaded data to the database
//                _dbContext.UploadedFiles.AddRange(uploadedData);
//                await _dbContext.SaveChangesAsync();

//                return Ok(new { message = "Files uploaded and data saved to the database successfully." });
//            }
//            catch (Exception ex)
//            {
//                // Log the error details here if needed
//                return BadRequest(new { message = $"An error occurred: {ex.Message}", innerException = ex.InnerException?.Message });
//            }
//        }

//        [HttpPost("delete")]
//        public async Task<IActionResult> DeleteFiles(List<int> fileIds)
//        {
//            try
//            {
//                if (fileIds == null || fileIds.Count == 0)
//                {
//                    return BadRequest(new { message = "No file IDs provided." });
//                }

//                var filesToDelete = await _dbContext.UploadedFiles
//                    .Where(f => fileIds.Contains(f.Id))
//                    .ToListAsync();

//                _dbContext.UploadedFiles.RemoveRange(filesToDelete);
//                await _dbContext.SaveChangesAsync();

//                return Ok(new { message = "Files deleted from the database successfully." });
//            }
//            catch (Exception ex)
//            {
//                // Log the error details here if needed
//                return BadRequest(new { message = $"An error occurred: {ex.Message}", innerException = ex.InnerException?.Message });
//            }
//        }

//        [HttpPost("clear")]
//        public async Task<IActionResult> ClearUploadedData()
//        {
//            try
//            {
//                // Retrieve all uploaded files from the database
//                var uploadedFiles = await _dbContext.UploadedFiles.ToListAsync();

//                // Remove all uploaded files from the database context
//                _dbContext.UploadedFiles.RemoveRange(uploadedFiles);

//                // Save changes to the database
//                await _dbContext.SaveChangesAsync();

//                return Ok(new { message = "All uploaded data cleared from the database successfully." });
//            }
//            catch (Exception ex)
//            {
//                // Log the error details here if needed
//                return BadRequest(new { message = $"An error occurred: {ex.Message}", innerException = ex.InnerException?.Message });
//            }
//        }

//        // Method to extract data from a file
//        private IEnumerable<UploadedFile> ExtractDataFromFile(string filePath)
//        {
//            var uploadedData = new List<UploadedFile>();

//            using (var package = new ExcelPackage(new FileInfo(filePath)))
//            {
//                var worksheet = package.Workbook.Worksheets.FirstOrDefault();
//                if (worksheet != null)
//                {
//                    for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
//                    {
//                        uploadedData.Add(new UploadedFile
//                        {
//                            MaskedPan = worksheet.Cells[row, 1]?.Value?.ToString(),
//                            Rrn = worksheet.Cells[row, 2]?.Value?.ToString(),
//                            Stan = worksheet.Cells[row, 3]?.Value?.ToString(),
//                            TerminalId = worksheet.Cells[row, 4]?.Value?.ToString(),
//                            TransactionDate = DateTime.Parse(worksheet.Cells[row, 5]?.Value?.ToString()),
//                            Amount = decimal.Parse(worksheet.Cells[row, 6]?.Value?.ToString()),
//                            AccountToBeCredited = worksheet.Cells[row, 7]?.Value?.ToString()
//                        });
//                    }
//                }
//            }

//            return uploadedData;
//        }
//    }
//}
