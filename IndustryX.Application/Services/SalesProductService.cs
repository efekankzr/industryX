using IndustryX.Application.Interfaces;
using IndustryX.Domain.Entities;
using IndustryX.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IndustryX.Application.Services
{
    public class SalesProductService : ISalesProductService
    {
        private readonly IRepository<SalesProduct> _salesProductRepository;

        public SalesProductService(IRepository<SalesProduct> salesProductRepository)
        {
            _salesProductRepository = salesProductRepository;
        }

        public async Task<List<SalesProduct>> GetAllAsync()
        {
            return await _salesProductRepository
                .GetQueryable()
                .Include(sp => sp.Product)
                .Include(sp => sp.Images)
                .Include(sp => sp.SalesProductCategories)
                    .ThenInclude(spc => spc.Category)
                .ToListAsync();
        }

        public async Task<SalesProduct?> GetByIdAsync(int id)
        {
            return await _salesProductRepository
                .GetQueryable()
                .Include(sp => sp.Product)
                .Include(sp => sp.Images)
                .Include(sp => sp.SalesProductCategories)
                    .ThenInclude(spc => spc.Category)
                .FirstOrDefaultAsync(sp => sp.Id == id);
        }

        public async Task<(bool Success, string? Error)> CreateAsync(SalesProduct salesProduct)
        {
            if (await _salesProductRepository.AnyAsync(sp => sp.Name == salesProduct.Name))
                return (false, "A sales product with the same name already exists.");

            if (await _salesProductRepository.AnyAsync(sp => sp.Url == salesProduct.Url))
                return (false, "A sales product with the same URL already exists.");

            await _salesProductRepository.AddAsync(salesProduct);
            await _salesProductRepository.SaveAsync();
            return (true, null);
        }

        public async Task<(bool Success, string? Error)> UpdateAsync(SalesProduct salesProduct)
        {
            if (await _salesProductRepository.AnyAsync(sp => sp.Name == salesProduct.Name && sp.Id != salesProduct.Id))
                return (false, "Another sales product with the same name already exists.");

            if (await _salesProductRepository.AnyAsync(sp => sp.Url == salesProduct.Url && sp.Id != salesProduct.Id))
                return (false, "Another sales product with the same URL already exists.");

            _salesProductRepository.Update(salesProduct);
            await _salesProductRepository.SaveAsync();
            return (true, null);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var salesProduct = await _salesProductRepository.GetByIdAsync(id);
            if (salesProduct == null)
                return false;

            _salesProductRepository.Delete(salesProduct);
            await _salesProductRepository.SaveAsync();
            return true;
        }
    }
}
