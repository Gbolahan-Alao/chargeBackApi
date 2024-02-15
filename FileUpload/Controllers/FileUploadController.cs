using FileUpload.Models;
using FileUpload.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FileUpload.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FileController : ControllerBase
    {
        private readonly FileUploadService _fileUploadService;
        private readonly DatabaseService _databaseService;

        public FileController(FileUploadService fileUploadService, DatabaseService databaseService)
        {
            _fileUploadService = fileUploadService;
            _databaseService = databaseService;
        }


       

        [HttpGet("files")]
        public async Task<IActionResult> GetUploadedFiles()
        {
            try
            {
                var uploadedFiles = await _databaseService.GetAllUploadedFilesAsync();
                return Ok(uploadedFiles);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("file-info")]
        public async Task<IActionResult> GetUploadedFileInfo()
        {
            try
            {
                var fileInfo = await _databaseService.GetUploadedFileInfoAsync();
                return Ok(fileInfo);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"An error occurred: {ex.Message}" });
            }
        }
        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest("Invalid file");

                // Upload the file and get the response message and filename
                var (fileName, responseMessage, skippedRowsCount, totalRows) = await _fileUploadService.UploadFileAsync(file);

                // Return the response message with filename as part of the OK response
                return Ok(new { fileName, message = responseMessage, skippedRowsCount });
            }
            catch (Exception ex)
            {
                // Return error message in case of exception
                return StatusCode(500, $"Internal server error: {ex.Message}");
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

        [HttpDelete("clear")]
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
                // Call the method in DatabaseService to delete all file info
                var result = await _databaseService.DeleteAllFileInfoAsync();

                // Check if deletion was successful
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
