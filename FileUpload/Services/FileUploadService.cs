using FileUpload.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System;
using System.IO;
using System.Linq;
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

        public async Task<(string, string, int, int)> UploadFileAsync(IFormFile file)
        {
            try
            {
                if (file == null || file.Length <= 0)
                {
                    throw new Exception("File is empty or null.");
                }

                var fileName = Path.GetFileNameWithoutExtension(file.FileName);
                var fileExtension = Path.GetExtension(file.FileName);
                var fullFileName = $"{fileName}{fileExtension}";

                var uploadsFolderPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                var filePath = Path.Combine(uploadsFolderPath, fullFileName);

                if (File.Exists(filePath))
                {
                    // Generate a unique filename by appending a timestamp
                    var uniqueFileName = $"{fileName}_{DateTime.UtcNow.Ticks}{fileExtension}";
                    filePath = Path.Combine(uploadsFolderPath, uniqueFileName);
                }

                // Save the file to the uploads folder
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Extract data from the uploaded Excel file
                var (uploadedFiles, totalRows) = _excelDataService.ExtractDataFromExcel(filePath);

                int skippedRowsCount = 0;

                UploadedFileInfo fileInfo = new UploadedFileInfo
                {
                    FileName = fullFileName,
                    TotalItems = totalRows,
                    UploadDate = DateTime.UtcNow,
                    FileUrl = filePath
                };

                foreach (var uploadedFile in uploadedFiles)
                {
                    var existingStan = _dbContext.UploadedFiles.FirstOrDefault(f => f.Stan == uploadedFile.Stan);
                    if (existingStan != null)
                    {
                        skippedRowsCount++;
                        continue;
                    }

                    _dbContext.UploadedFiles.Add(uploadedFile);
                }


                _dbContext.UploadedFileInfos.Add(fileInfo);

                // Save changes to the database
                await _dbContext.SaveChangesAsync(); // Ensure changes are saved to the database

                // Return upload success message with filename, skipped rows count, and total rows
                return (Path.GetFileName(filePath), $"{fileName}{fileExtension} uploaded successfully", skippedRowsCount, totalRows);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading file: {ex.Message}");
                Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");

                return (null, $"Error uploading file: {ex.Message}", 0, 0);
            }
        }








        public async Task<IEnumerable<UploadedFileInfo>> GetUploadedFileInfoAsync()
        {
            try
            {
                var uploadsFolderPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                var uploadedFiles = Directory.GetFiles(uploadsFolderPath)
                    .Select(filePath => new UploadedFileInfo
                    {
                        FileName = Path.GetFileName(filePath),
                        TotalItems = GetTotalRowsInExcelFile(filePath), // Use the method to get total rows
                        UploadDate = File.GetCreationTimeUtc(filePath),
                        FileUrl = filePath
                    })
                    .ToList();

                return uploadedFiles;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting uploaded file info: {ex.Message}");
            }
        }





        private int GetTotalRowsInExcelFile(string filePath)
        {
            try
            {
                using (var package = new ExcelPackage(new FileInfo(filePath)))
                {
                    var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                    if (worksheet != null && worksheet.Dimension != null)
                    {
                        // Count all rows in the worksheet
                        return worksheet.Dimension.Rows;
                    }
                    else
                    {
                        return 14;
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that might occur during file processing
                throw new Exception($"Error getting total rows in Excel file: {ex.Message}");
            }
        }






        public async Task<(byte[], string, int)> DownloadFileAsync(string fileName)
        {
            try
            {
                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", fileName);

                if (File.Exists(filePath))
                {
                    var fileBytes = await File.ReadAllBytesAsync(filePath);

                    using (var package = new ExcelPackage(new MemoryStream(fileBytes)))
                    {
                        var worksheet = package.Workbook.Worksheets.FirstOrDefault();

                        if (worksheet != null)
                        {
                            int skippedRowsCount = 0; 
                            int rowCount = worksheet.Dimension.End.Row;

                          
                            worksheet.Cells[1, 8].Value = "Status";


                            for (int row = 2; row <= rowCount; row++)
                            {
                                
                                string stanValue = worksheet.Cells[row, 3]?.Value?.ToString();

                             
                                var existingStan = _dbContext.UploadedFiles.FirstOrDefault(f => f.Stan == stanValue);
                                if (existingStan != null)
                                {
                                  
                                    worksheet.Cells[row, 8].Value = "Success";
                                }
                                else
                                {
                                    worksheet.Cells[row, 8].Value = "Failed";
                                    skippedRowsCount++; // Increment skipped rows count
                                }
                            }

                            
                            fileBytes = package.GetAsByteArray();

                            return (fileBytes, fileName, skippedRowsCount);
                        }
                        else
                        {
                            throw new InvalidOperationException("The Excel file does not contain any worksheets.");
                        }
                    }
                }
                else
                {
                    throw new FileNotFoundException($"File {fileName} not found");
                }
            }
            catch (FileNotFoundException ex)
            {
                throw ex; 
            }
            catch (InvalidOperationException ex)
            {
                throw ex; 
            }
            catch (Exception ex)
            {
                throw new Exception($"Error downloading file: {ex.Message}");
            }
        }
    }
}