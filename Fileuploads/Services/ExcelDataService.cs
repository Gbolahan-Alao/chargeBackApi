using Fileuploads.Models;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Fileuploads.Services
{
    public class ExcelDataService
    {
        public (IEnumerable<UploadedFile>, int) ExtractDataFromExcel(string filePath)
        {
            var uploadedData = new List<UploadedFile>();
            int totalRows = 0;

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                if (worksheet != null)
                {
                    totalRows = worksheet.Dimension.End.Row - 1; // Exclude header row
                    for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                    {
                        uploadedData.Add(new UploadedFile
                        {
                            MaskedPan = worksheet.Cells[row, 1]?.Value?.ToString(),
                            Rrn = worksheet.Cells[row, 2]?.Value?.ToString(),
                            Stan = worksheet.Cells[row, 3]?.Value?.ToString(),
                            TerminalId = worksheet.Cells[row, 4]?.Value?.ToString(),
                            TransactionDate = DateTime.Parse(worksheet.Cells[row, 5]?.Value?.ToString()),
                            Amount = decimal.Parse(worksheet.Cells[row, 6]?.Value?.ToString()),
                            AccountToBeCredited = worksheet.Cells[row, 7]?.Value?.ToString()
                        });
                    }
                }
            }

            return (uploadedData, totalRows);
        }
    }
}
