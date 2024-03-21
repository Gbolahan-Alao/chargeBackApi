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
        private readonly FileUploadDbContext _dbContext;
        private readonly ExcelDataService _excelDataService;
        private readonly UploadReceiptService _uploadReceiptService;

        public FileUploadService(IWebHostEnvironment webHostEnvironment, FileUploadDbContext dbContext, ExcelDataService excelDataService, UploadReceiptService uploadReceiptService)
        {
            _webHostEnvironment = webHostEnvironment;
            _dbContext = dbContext;
            _excelDataService = excelDataService;
            _uploadReceiptService = uploadReceiptService;
        }

        public async Task<(string, string, int, int)> UploadFileAsync(IFormFile file, String merchantId)
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

                var (uploadedFiles, totalRows) = _excelDataService.ExtractDataFromExcel(filePath, merchantId );
                var distinctUploadedFiles = uploadedFiles.GroupBy(f => f.Stan).Select(g => g.First());

                int skippedRowCount = 0;
                foreach (var uploadedFile in distinctUploadedFiles)
                {
                 
                    var existingStan = _dbContext.UploadedFiles.FirstOrDefault(f => f.Stan == uploadedFile.Stan);
                    if (existingStan != null)
                    {
                        skippedRowCount++;
                        uploadedFile.Status = "Failed"; 
                    }
                    else
                    {
                        uploadedFile.Status = "Success"; 
                        _dbContext.UploadedFiles.Add(uploadedFile);
                    }
                }
               UploadedFileInfo fileInfo = new UploadedFileInfo
                {
                    FileName = fullFileName,
                    TotalItems = totalRows,
                    TotalSuccessful = totalRows - skippedRowCount,
                    TotalFailed = skippedRowCount,
                    UploadDate = DateTime.UtcNow,
                    FileUrl = filePath,
                    MerchantId= merchantId,
                };
                _dbContext.UploadedFileInfos.Add(fileInfo);

               
                await _dbContext.SaveChangesAsync();
                AddAndUpdateStatusColumn(filePath);


                return (Path.GetFileName(filePath), $"{fileName}{fileExtension} uploaded successfully", skippedRowCount, totalRows);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading file: {ex.Message}");
                Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");

                return (null, $"Error uploading file: {ex.Message}", 0, 0);
            }
        }
        public async Task<IEnumerable<UploadedFileInfo>> GetUploadedFileInfoAsync(String merchantId)
        {
            try
            {
                var uploadsFolderPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                var allFiles = Directory.GetFiles(uploadsFolderPath);
                var filesForMerchant = allFiles
                    .Select(filePath => new UploadedFileInfo
                    {
                        FileName = Path.GetFileName(filePath),
                        TotalItems = GetTotalRowsInExcelFile(filePath),
                        UploadDate = File.GetCreationTimeUtc(filePath),
                        FileUrl = filePath,
                        MerchantId = merchantId
                    })
                    .OrderByDescending(file => file.UploadDate); 

                foreach (var uploadedFile in filesForMerchant)
                {
                    _dbContext.UploadedFileInfos.Add(uploadedFile);
                }

                await _dbContext.SaveChangesAsync();

                return filesForMerchant;
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
                           
                            statusColumnIndex = worksheet.Dimension.End.Column + 1;
                            worksheet.Cells[1, statusColumnIndex].Value = "Status";
                        }

                        for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                        {
                            var stanText = worksheet.Cells[row, stanColumnIndex].Text; 
                            var stan = Convert.ToInt32(stanText); 

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

        public async Task<bool> UpdateUserActionsAsync(String merchantId, string stan, string rrn, string action)
        {
            try
            {
                var uploadedFile = await _dbContext.UploadedFiles.FirstOrDefaultAsync(f => f.MerchantId == merchantId && f.Stan == stan && f.Rrn == rrn);

                if (uploadedFile == null)
                {
                    return false;
                }
                uploadedFile.Action = action;
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating user action: {ex.Message}");
                return false;
            }
        }

        public async Task<string> GetUserActionAsync(String merchantId, string stan, string rrn)
        {
            try
            {
                var uploadedFile = await _dbContext.UploadedFiles.FirstOrDefaultAsync(f => f.MerchantId == merchantId && f.Stan == stan && f.Rrn == rrn);
                if (uploadedFile != null)
                {
                    return uploadedFile.Action;
                }
                return null; // Return null if no matching record found
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching user action: {ex.Message}");
                return null;
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
