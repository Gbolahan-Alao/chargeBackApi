using Fileuploads.Models;
using Fileuploads.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Fileuploads.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MerchantsController : ControllerBase
    {
        private readonly IMerchantService _merchantService;

        public MerchantsController(IMerchantService merchantService)
        {
            _merchantService = merchantService;
        }

        [HttpGet]
        public async Task<IActionResult> GetMerchants()
        {
            var merchants = await _merchantService.GetMerchantsAsync();
            return Ok(merchants);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMerchantById(Guid id)
        {
            var merchant = await _merchantService.GetMerchantByIdAsync(id);
            if (merchant == null)
                return NotFound();

            return Ok(merchant);
        }

        [HttpPost]
        public async Task<IActionResult> CreateMerchant(Merchant merchant)
        {
            var createdMerchant = await _merchantService.CreateMerchantAsync(merchant);
            return CreatedAtAction(nameof(GetMerchantById), new { id = createdMerchant.Id }, createdMerchant);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMerchant(Guid id)
        {
            var result = await _merchantService.DeleteMerchantAsync(id);
            if (!result)
                return NotFound(); 

            return NoContent(); 
        }
    }
}
