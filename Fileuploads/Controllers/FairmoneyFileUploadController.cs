using Fileuploads.Services.FileUploadServices;
using Fileuploads.Services.DatabaseServices;
using Fileuploads.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Fileuploads.Controllers
{
    [Route("fairmoney")]
    [ApiController]
    public class FairmoneyFileUploadController : ControllerBase
    {
        private readonly FairmoneyFileUploadService _fairmoneyFileUploadService;
        private readonly FairmoneyDatabaseServices _databaseService;


        public FairmoneyFileUploadController(FairmoneyFileUploadService fairmoneyFileUploadService, FairmoneyDatabaseServices databaseService)
        {
            _fairmoneyFileUploadService = fairmoneyFileUploadService;
            _databaseService = databaseService;

        }

        [HttpGet("files")]
        public async Task<IActionResult> GetUploadedFiles()
        {
            try
            {
                var uploadedFiles = await _databaseService.GetAllFairmoneyUploadedFilesAsync();
                return Ok(uploadedFiles);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("files-info")]
        public async Task<IActionResult> GetUploadedFileInfo()
        {
            try
            {
                var fileInfo = await _databaseService.GetFairmoneyUploadedFileInfoAsync();
                return Ok(fileInfo);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFairmoneyFile(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0) return BadRequest("Invalid File");
                var (fileName, responseMessage, skippedRowsCount, totalRows) = await _fairmoneyFileUploadService.UploadFileAsync(file);
                return Ok(new { fileName, message = responseMessage, skippedRowsCount });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error:{ex.Message}");
            }
        }


        [HttpPost("download/{fileName}")]
        public async Task<IActionResult> DownloadFile(string fileName)
        {
            try
            {
                var fileBytes = await _fairmoneyFileUploadService.DownloadFileAsync(fileName);
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

        [HttpDelete("clear-all-data")]
        public async Task<IActionResult> ClearUploadedData()
        {
            var result = await _databaseService.ClearFairmoneyUploadedData();

            if (result.StartsWith("An error occurred"))
            {
                return BadRequest(new { message = result });
            }
            else
            {
                return Ok(new { message = result });
            }
        }


        [HttpDelete("delete-all")]
        public async Task<IActionResult> DeleteAllFileInfo()
        {
            try
            {
                var result = await _databaseService.DeleteAllFairmoneyFileInfoAsync();
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
