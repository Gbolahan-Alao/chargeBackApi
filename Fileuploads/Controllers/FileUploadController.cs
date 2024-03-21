using FileUpload.Migrations;
using Fileuploads.Models;
using Fileuploads.Services;
using Fileuploads.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fileuploads.Controllers
{
    [ApiController]
    [Route("api")]
    public class FileController : ControllerBase
    {
        private readonly FileUploadService _fileUploadService;
       
        private readonly DatabaseService _databaseService;
        private readonly UploadReceiptService _receiptService;

        public FileController(FileUploadService fileUploadService,  DatabaseService databaseService, UploadReceiptService receiptService )
        {
            _fileUploadService = fileUploadService;
            _databaseService = databaseService;
            _receiptService = receiptService;
            
        }

        [HttpGet("all-files")]
        public async Task<IActionResult> GetAllUploadedFiles()
        {
            try
            {
                var uploadedFiles = await _databaseService.GetAllFilesAsync();
                return Ok(uploadedFiles);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("files")]
        public async Task<IActionResult> GetUploadedFiles([FromQuery] String merchantId)
        {
            try
            {
                var uploadedFiles = await _databaseService.GetAllUploadedFilesAsync(merchantId);
                return Ok(uploadedFiles);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("dashboard-data")]
        public async Task<IActionResult> GetDashboardData()
        {
            try
            {
                var dashboardData = await _databaseService.GetDashboardData();
                return Ok(dashboardData);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("merchant-dashboard-data")]
        public async Task<IActionResult> GetMerchantDashboardData([FromQuery] String merchantId)
        {
            try
            {
                var dashboardData = await _databaseService.GetMerchantDashboardData(merchantId);
                return Ok(dashboardData);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("files-info")]
        public async Task<IActionResult> GetUploadedFileInfo([FromQuery] String merchantId)
        {
            try
            {
                var fileInfo = await _databaseService.GetUploadedFileInfoAsync(merchantId);

                if (fileInfo != null)
                {
                    return Ok(fileInfo);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"An error occurred: {ex.Message}" });
            }
        }



        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file, String merchantId)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest("Invalid file");

                var (fileName, responseMessage, skippedRowsCount, totalRows) = await _fileUploadService.UploadFileAsync(file, merchantId);

                return Ok(new { fileName, message = responseMessage, skippedRowsCount });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading file: {ex.Message}");
                Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}"); // Print inner exception
                return BadRequest($"Error uploading file: {ex.Message}");
            }
        }

        [HttpPost("updateUserActions")]
        public async Task<IActionResult> UpdateUserActions([FromQuery] String merchantId, [FromQuery] string stan, [FromQuery] string rrn, [FromQuery] string action)
        {
            try
            {
                if (merchantId == String.Empty || string.IsNullOrEmpty(stan) || string.IsNullOrEmpty(rrn) || string.IsNullOrEmpty(action))
                {
                    return BadRequest("Merchant ID, STAN, RRN, and action are required parameters.");
                }
                var result = await _fileUploadService.UpdateUserActionsAsync(merchantId, stan, rrn, action);

                if (result)
                {
                    return Ok("User action updated successfully.");
                }
                else
                {
                    return NotFound("No matching record found or failed to update user action.");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("getUserAction")]
        public async Task<IActionResult> GetUserAction([FromQuery] String merchantId, [FromQuery] string stan, [FromQuery] string rrn)
        {
            try
            {
                if (merchantId == String.Empty || string.IsNullOrEmpty(stan) || string.IsNullOrEmpty(rrn))
                {
                    return BadRequest("Merchant ID, STAN, and RRN are required parameters.");
                }
                var action = await _fileUploadService.GetUserActionAsync(merchantId, stan, rrn);
                if (action != null)
                {
                    return Ok(action);
                }
                else
                {
                    return NotFound("No matching record found.");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpPost("uploadReceipt")]
        public async Task<IActionResult> UploadReceipt(string merchantId, string stan, string rrn, IFormFile receipt)
        {
            try
            {
                var filePath = await _receiptService.UploadReceiptAsync(merchantId, stan, rrn, receipt);
                return Ok(filePath);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error uploading receipt: {ex.Message}");
            }
        }

        [HttpGet("download-receipt/{merchantId}/{stan}/{rrn}")]
        public async Task<IActionResult> DownloadReceipt(string merchantId, string stan, string rrn)
        {
            try
            {
                var (fileBytes, contentType) = await _receiptService.DownloadReceiptAsync(merchantId, stan, rrn);
                var fileName = $"{stan}-{rrn}.pdf"; // Assuming the file name is constructed from stan and rrn
                return File(fileBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                // Handle the exception, e.g., return an error response
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error downloading receipt: {ex.Message}");
            }
        }




        [HttpPost("download/{fileName}")]
        public async Task<IActionResult> DownloadFile(string fileName)
        {
            try
            {
                var fileBytes = await _fileUploadService.DownloadFileAsync(fileName);
                return File(fileBytes, "application/octet-stream", fileName);
            }
            catch (FileNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"An error occurred: {ex.Message}" });
            }
        }


        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteFiles(List<int> fileIds)
        {
            try
            {
                await _databaseService.DeleteUploadedFilesAsync(fileIds);
                return Ok(new { message = "Files deleted from the database successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpDelete("clear-all-data")]
        public async Task<IActionResult> ClearUploadedData()
        {
            var result = await _databaseService.ClearUploadedData();

            if (result.StartsWith("An error occurred"))
            {
                return BadRequest(new { message = result });
            }
            else
            {
                return Ok(new { message = result });
            }
        }

        

        [HttpPost("delete-all")]
        public async Task<IActionResult> DeleteAllFileInfo()
        {
            try
            {
               
                var result = await _databaseService.DeleteAllFileInfoAsync();

               
                if (result.StartsWith("An error occurred"))
                {
                    return BadRequest(new { message = result });
                }
                else
                {
                    return Ok(new { message = result });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }

        }
    }
}
