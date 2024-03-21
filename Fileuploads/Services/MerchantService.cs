using System.Collections.Generic;
using System.Threading.Tasks;
using Fileuploads.Models;
using Microsoft.EntityFrameworkCore;

namespace Fileuploads.Services
{
    public interface IMerchantService
    {
        Task<List<Merchant>> GetMerchantsAsync();
        Task<Merchant> GetMerchantByIdAsync(Guid id);
        Task<Merchant> CreateMerchantAsync(Merchant merchant);
        Task<bool> DeleteMerchantAsync(Guid id);
    }

    public class MerchantService : IMerchantService
    {
        private readonly FileUploadDbContext _dbContext;

        public MerchantService(FileUploadDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<Merchant>> GetMerchantsAsync()
        {
            return await _dbContext.Merchants.OrderBy(m=>m.SerialNumber).ToListAsync();
        }

        public async Task<Merchant> GetMerchantByIdAsync(Guid id)
        {
            return await _dbContext.Merchants.FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<Merchant> CreateMerchantAsync(Merchant merchant)
        {
            merchant.Id = Guid.NewGuid(); 
            merchant.SerialNumber = await GenerateNextSerialNumberAsync();

            _dbContext.Merchants.Add(merchant);
            await _dbContext.SaveChangesAsync();
            return merchant;
        }

        public async Task<int> GenerateNextSerialNumberAsync()
        {
         
            var latestMerchant = await _dbContext.Merchants
                                                .OrderByDescending(m => m.SerialNumber)
                                                .FirstOrDefaultAsync();
            if (latestMerchant == null)
                return 1;
            return latestMerchant.SerialNumber + 1;
        }

        public async Task<bool> DeleteMerchantAsync(Guid id)
        {
            var merchantToDelete = await _dbContext.Merchants.FindAsync(id);
            if (merchantToDelete == null)
                return false; 

            _dbContext.Merchants.Remove(merchantToDelete);
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}
