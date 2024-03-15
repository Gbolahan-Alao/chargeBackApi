using Fileuploads.Models;
using Microsoft.AspNetCore.Mvc;

namespace Fileuploads.Controllers
{
    public class MerchantController : Controller
    {

        private static List<Merchant> _merchants = new List<Merchant>
    {
        new Merchant { Id = Guid.NewGuid(), SerialNumber = 1, Name = "Team APT" },
        new Merchant { Id = Guid.NewGuid(), SerialNumber = 2, Name = "Fair Money" },
        new Merchant { Id = Guid.NewGuid(), SerialNumber = 3, Name = "Palmpay" }
    };

        [HttpGet]
        public IActionResult GetMerchants()
        {
            return Ok(_merchants);
        }

        [HttpPost]
        public IActionResult AddMerchant(Merchant merchant)
        {
            merchant.Id = Guid.NewGuid();
            _merchants.Add(merchant);
            return CreatedAtAction(nameof(GetMerchantById), new { id = merchant.Id }, merchant);
        }

        [HttpGet("{id}")]
        public IActionResult GetMerchantById(Guid id)
        {
            var merchant = _merchants.FirstOrDefault(m => m.Id == id);
            if (merchant == null)
                return NotFound();

            return Ok(merchant);
        }
    }
}
