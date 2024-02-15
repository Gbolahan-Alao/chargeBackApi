using FileUpload.Models;
using FileUpload.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FileUpload.Services
{
    public class FileUploadService : IFileUploadService
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
                    throw new Exception($"File '{fullFileName}' already exists. Please rename the file and try again.");
                }

            
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var (uploadedFiles, totalRows) = _excelDataService.ExtractDataFromExcel(filePath);

                var distinctUploadedFiles = uploadedFiles.GroupBy(f => f.Stan).Select(g => g.First());

                int skippedRowCount = 0;
                foreach (var uploadedFile in distinctUploadedFiles)
                {
                    var existingStan = _dbContext.UploadedFiles.FirstOrDefault(f => f.Stan == uploadedFile.Stan);
                    if (existingStan != null)
                    {
                        skippedRowCount++;
                    }
                    else
                    {
                        _dbContext.UploadedFiles.Add(uploadedFile);
                    }
                }

                using (var package = new ExcelPackage(new FileInfo(filePath)))
                {
                    var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                    if (worksheet != null)
                    {
                        worksheet.InsertColumn(8, 1); 
                        worksheet.Cells[1, 8].Value = "Status";

                    
                        for (int row = 2; row <= totalRows + 1; row++)
                        {
                            var stan = worksheet.Cells[row, 1].Text;
                            var existingStan = _dbContext.UploadedFiles.FirstOrDefault(f => f.Stan == stan);
                            if (existingStan != null)
                            {
                                worksheet.Cells[row, 8].Value = "Failed";
                            }
                            else
                            {
                                worksheet.Cells[row, 8].Value = "Success";
                            }
                        }

                        // Save changes to the Excel file
                        package.Save();
                    }
                    else
                    {
                        throw new InvalidOperationException("The Excel file does not contain any worksheets.");
                    }
                }

                UploadedFileInfo fileInfo = new UploadedFileInfo
                {
                    FileName = fullFileName,
                    TotalItems = totalRows,
                    UploadDate = DateTime.UtcNow,
                    FileUrl = filePath
                };

                // Add UploadedFileInfo to context and save
                _dbContext.UploadedFileInfos.Add(fileInfo);
                await _dbContext.SaveChangesAsync();

                // Return upload success message with filename, skipped rows count, and total rows
                return (Path.GetFileName(filePath), $"{fileName}{fileExtension} uploaded successfully", skippedRowCount, totalRows);
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
                        TotalItems = GetTotalRowsInExcelFile(filePath), 
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
                        return 0;
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that might occur during file processing
                throw new Exception($"Error getting total rows in Excel file: {ex.Message}");
            }
        }

        // Inside the DownloadFileAsync method
        // Inside the DownloadFileAsync method
        public async Task<byte[]> DownloadFileAsync(string fileName)
        {
            try
            {
                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", fileName);

                if (File.Exists(filePath))
                {
                    // Read the file bytes and return
                    var fileBytes = await File.ReadAllBytesAsync(filePath);

                    // Update status for each row based on the "Status" column value
                    using (var package = new ExcelPackage(new FileInfo(filePath)))
                    {
                        var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                        if (worksheet != null)
                        {
                            // Remove any existing "Status" columns
                            var existingStatusColumns = worksheet.Cells["1:1"].Where(cell => cell.Text == "Status").ToList();
                            foreach (var existingStatusColumn in existingStatusColumns)
                            {
                                int columnIndex = existingStatusColumn.Start.Column;
                                worksheet.DeleteColumn(columnIndex);
                            }

                            // Find the existing "Status" column index
                            int statusColumnIndex = worksheet.Dimension?.End?.Column + 1 ?? 1;

                            // Read the status for each row and set it in the downloaded file
                            for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                            {
                                var stan = worksheet.Cells[row, 1].Text;
                                var existingStan = _dbContext.UploadedFiles.FirstOrDefault(f => f.Stan == stan);
                                if (existingStan != null)
                                {
                                    worksheet.Cells[row, statusColumnIndex].Value = "Failed";
                                }
                                else
                                {
                                    worksheet.Cells[row, statusColumnIndex].Value = "Success";
                                }
                            }

                            // Set the header for the new "Status" column
                            worksheet.Cells[1, statusColumnIndex].Value = "Status";

                            // Save changes to the Excel file
                            package.Save();
                        }
                        else
                        {
                            throw new InvalidOperationException("The Excel file does not contain any worksheets.");
                        }
                    }

                    return fileBytes;
                }
                else
                {
                    throw new FileNotFoundException($"File {fileName} not found");
                }
            }
            catch (FileNotFoundException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error downloading file: {ex.Message}", ex);
            }
        }





    }
}
