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

        public FileController(FileUploadService fileUploadService,  DatabaseService databaseService )
        {
            _fileUploadService = fileUploadService;
            _databaseService = databaseService;
            
        }

        [HttpGet("files")]
        public async Task<IActionResult> GetUploadedFiles([FromQuery] List<string> userRoles)
        {
            try
            {
                string merchantId = "";
                foreach (var userRole in userRoles)
                {
                    switch (userRole)
                    {
                        case "polFairmoney":
                            merchantId = "1234";
                            break;
                        case "polTeamapt":
                            merchantId = "5678";
                            break;
                        case "polPalmpay":
                            merchantId = "91011";
                            break;
                        default:
                            throw new Exception("Invalid user role. Please provide a valid role.");
                    }
                }
                var uploadedFiles = await _databaseService.GetAllUploadedFilesAsync(merchantId);
                return Ok(uploadedFiles);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("files-info")]
        public async Task<IActionResult> GetUploadedFileInfo([FromQuery] List<string> userRoles)
        {
            try
            {
                string merchantId = "";

                foreach (var userRole in userRoles)
                {
                    switch (userRole)
                    {
                        case "polFairmoney":
                            merchantId = "1234";
                            break;
                        case "polTeamapt":
                            merchantId = "5678";
                            break;
                        case "polPalmpay":
                            merchantId = "91011";
                            break;
                        default:
                            throw new Exception("Invalid user role. Please provide a valid role.");
                    }

                    var fileInfo = await _databaseService.GetUploadedFileInfoAsync(merchantId);

                    if (fileInfo != null)
                        return Ok(fileInfo);
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"An error occurred: {ex.Message}" });
            }
        }


        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file, string merchantId)
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
                
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("updateUserActions")]
        public async Task<IActionResult> UpdateUserActions([FromQuery] string merchantId, [FromQuery] string stan, [FromQuery] string rrn, [FromQuery] string action)
        {
            if (string.IsNullOrEmpty(merchantId) || string.IsNullOrEmpty(stan) || string.IsNullOrEmpty(rrn) || string.IsNullOrEmpty(action))
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
