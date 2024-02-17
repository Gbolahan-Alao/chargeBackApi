using Fileuploads.Models;
using Fileuploads.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Fileuploads.Services
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

                // Extract data from Excel and check for duplicates
                var (uploadedFiles, totalRows) = _excelDataService.ExtractDataFromExcel(filePath);
                var distinctUploadedFiles = uploadedFiles.GroupBy(f => f.Stan).Select(g => g.First());

                int skippedRowCount = 0;
                foreach (var uploadedFile in distinctUploadedFiles)
                {
                    // Check if the Stan already exists in the database
                    var existingStan = _dbContext.UploadedFiles.FirstOrDefault(f => f.Stan == uploadedFile.Stan);
                    if (existingStan != null)
                    {
                        skippedRowCount++;
                        uploadedFile.Status = "Failed"; // Set status to failed for existing records
                    }
                    else
                    {
                        uploadedFile.Status = "Success"; // Set status to success for new records
                        _dbContext.UploadedFiles.Add(uploadedFile);
                    }
                }
                
                // Save changes to the database
                await _dbContext.SaveChangesAsync();
                AddAndUpdateStatusColumn(filePath);
                // Add status column and update status in the Excel file


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
                    });

                foreach (var uploadedFile in uploadedFiles)
                {
                    _dbContext.UploadedFileInfos.Add(uploadedFile);
                }
                await _dbContext.SaveChangesAsync();

                return uploadedFiles;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting/uploading file info: {ex.Message}", ex);
            }
        }



        public async Task<byte[]> DownloadFileAsync(string fileName)
        {
            try
            {
                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", fileName);

                if (File.Exists(filePath))
                {
                    // Read the file bytes
                    var fileBytes = await File.ReadAllBytesAsync(filePath);

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

        private void AddAndUpdateStatusColumn(string filePath)
        {
            try
            {
                using (var package = new ExcelPackage(new FileInfo(filePath)))
                {
                    var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                    if (worksheet != null)
                    {
                        // Find the column index for "Stan"
                        int stanColumnIndex = -1;
                        for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
                        {
                            if (worksheet.Cells[1, col].Text == "Stan")
                            {
                                stanColumnIndex = col;
                                break;
                            }
                        }

                        if (stanColumnIndex == -1)
                        {
                            throw new InvalidOperationException("Column 'Stan' not found in the Excel file.");
                        }

                        // Check if the "Status" column already exists
                        int statusColumnIndex = -1;
                        for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
                        {
                            if (worksheet.Cells[1, col].Text == "Status")
                            {
                                statusColumnIndex = col;
                                break;
                            }
                        }

                        if (statusColumnIndex == -1)
                        {
                            // If "Status" column does not exist, add it
                            statusColumnIndex = worksheet.Dimension.End.Column + 1;
                            worksheet.Cells[1, statusColumnIndex].Value = "Status";
                        }

                        // Read the status for each row and set it in the downloaded file
                        for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                        {
                            var stanText = worksheet.Cells[row, stanColumnIndex].Text; // Retrieve value from "Stan" column
                            var stan = Convert.ToInt32(stanText); // Convert to int

                            using (var dbContextTransaction = _dbContext.Database.BeginTransaction())
                            {
                                try
                                {
                                    var existingStan = _dbContext.UploadedFiles.FirstOrDefault(f => f.Stan == Convert.ToString(stan));
                                    if (existingStan != null)
                                    {
                                        worksheet.Cells[row, statusColumnIndex].Value = "Success";
                                    }
                                    else
                                    {
                                        worksheet.Cells[row, statusColumnIndex].Value = "Failed";
                                    }

                                    // Save changes to the Excel file
                                   

                                    dbContextTransaction.Commit();
                                }
                                catch (Exception ex)
                                {
                                    dbContextTransaction.Rollback();
                                    throw new Exception($"Error updating status for Stan {stan}: {ex.Message}");
                                }
                            }

                        }
                        package.Save();
                    }
                    else
                    {
                        throw new InvalidOperationException("The Excel file does not contain any worksheets.");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error adding/updating status column: {ex.Message}", ex);
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
                throw new Exception($"Error getting total rows in Excel file: {ex.Message}");
            }
        }
    }
}
