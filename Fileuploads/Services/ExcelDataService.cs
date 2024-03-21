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
        public (IEnumerable<UploadedFile>, int) ExtractDataFromExcel(string filePath, String merchantId)
        {
            var uploadedData = new List<UploadedFile>();
            int totalRows = 0;
            DateTime dateLogged = DateTime.UtcNow;

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                if (worksheet != null)
                {
                    totalRows = worksheet.Dimension.End.Row - 1; 

                    // Adding the actions column
                    worksheet.Cells[1, worksheet.Dimension.End.Column + 1].Value = "Actions";

                    for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                    {
                        uploadedData.Add(new UploadedFile
                        {
                            DateLogged = dateLogged,
                            MaskedPan = worksheet.Cells[row, 1]?.Value?.ToString(),
                            Rrn = worksheet.Cells[row, 2]?.Value?.ToString(),
                            Stan = worksheet.Cells[row, 3]?.Value?.ToString(),
                            TerminalId = worksheet.Cells[row, 4]?.Value?.ToString(),
                            TransactionDate = DateTime.Parse(worksheet.Cells[row, 5]?.Value?.ToString()),
                            Amount = decimal.Parse(worksheet.Cells[row, 6]?.Value?.ToString()),
                            AccountToBeCredited = worksheet.Cells[row, 7]?.Value?.ToString(),
                            MerchantId = merchantId,
                            Action = "None" // I set None as the initial value"
                        }) ;
                        worksheet.Cells[row, worksheet.Dimension.End.Column].Value = "N/A";
                    }
                }
            }

            return (uploadedData, totalRows);
        }
    }
}
