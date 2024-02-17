﻿using Fileuploads.Models;
using Fileuploads.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Fileuploads.Services.FileUploadServices
{
    public class PalmpayFileUploadService : IPalmpayUploadService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly AppDbContext _dbContext;
        private readonly ExcelDataService _excelDataService;

        public PalmpayFileUploadService(IWebHostEnvironment webHostEnvironment, AppDbContext dbContext, ExcelDataService excelDataService)
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

                var uploadsFolderPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "Palmpay");
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

                int skippedRowCount = 0;
                foreach (var uploadedFile in uploadedFiles)
                {

                    var palmpayUploadedFile = ConvertToPalmpayUploadedFile(uploadedFile);

                    var existingStan = _dbContext.PalmpayUploadedFile.FirstOrDefault(f => f.Stan == palmpayUploadedFile.Stan);
                    if (existingStan != null)
                    {
                        skippedRowCount++;
                    }
                    else
                    {
                        _dbContext.PalmpayUploadedFile.Add(palmpayUploadedFile);
                    }
                }


                using (var package = new ExcelPackage(new FileInfo(filePath)))
                {
                    var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                    if (worksheet != null)
                    {
                        // ... [existing code] ...

                        // Find the "Status" column index (column H) and delete it
                        var statusColumnIndex = 8; // Column H
                        worksheet.DeleteColumn(statusColumnIndex);

                        // Insert a new "Status" column
                        worksheet.InsertColumn(statusColumnIndex, 1);
                        worksheet.Cells[1, statusColumnIndex].Value = "Status";

                        for (int row = 2; row <= totalRows + 1; row++)
                        {
                            var stan = worksheet.Cells[row, 1].Text;
                            var existingStan = _dbContext.PalmpayUploadedFile.FirstOrDefault(f => f.Stan == stan);
                            if (existingStan != null)
                            {
                                worksheet.Cells[row, statusColumnIndex].Value = "Failed";
                                skippedRowCount++;
                            }
                            else
                            {
                                worksheet.Cells[row, statusColumnIndex].Value = "Success";
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


                PalmpayUploadedFilesInfo fileInfo = new PalmpayUploadedFilesInfo
                {
                    FileName = fullFileName,
                    TotalItems = totalRows,
                    TotalSuccessful = totalRows - skippedRowCount,
                    TotalFailed = skippedRowCount,
                    UploadDate = DateTime.UtcNow,
                    FileUrl = filePath
                };
                _dbContext.PalmpayUploadedFilesInfo.Add(fileInfo);
                await _dbContext.SaveChangesAsync();
                return (Path.GetFileName(filePath), $"{fileName}{fileExtension} uploaded successfully", skippedRowCount, totalRows);
            }

            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading file: {ex.Message}");
                Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");

                return (null, $"Error uploading file: {ex.Message}", 0, 0);
            }
        }

        public async Task<IEnumerable<PalmpayUploadedFilesInfo>> GetUploadedFileInfoAsync()
        {
            try
            {
                var palmpayUploadedFiles = await _dbContext.PalmpayUploadedFilesInfo.ToListAsync();
                return palmpayUploadedFiles;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting uploaded file info: {ex.Message}");
            }
        }

        private PalmpayUploadedFile ConvertToPalmpayUploadedFile(UploadedFile uploadedFile)
        {
            // Create a new TeamaptUploadedFile object and populate its properties
            var palmpayUploadedFile = new PalmpayUploadedFile
            {
                MaskedPan = uploadedFile.MaskedPan,
                Rrn = uploadedFile.Rrn,
                Stan = uploadedFile.Stan,
                TerminalId = uploadedFile.TerminalId,
                TransactionDate = uploadedFile.TransactionDate,
                Amount = uploadedFile.Amount,
                AccountToBeCredited = uploadedFile.AccountToBeCredited
            };

            return palmpayUploadedFile;
        }

        public async Task<byte[]> DownloadFileAsync(string fileName)
        {
            try
            {
                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "Palmpay", fileName);

                if (File.Exists(filePath))
                {
                    return await File.ReadAllBytesAsync(filePath);
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
