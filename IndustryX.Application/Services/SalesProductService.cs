using IndustryX.Application.Interfaces;
using IndustryX.Domain.Entities;
using IndustryX.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IndustryX.Application.Services
{
    public class SalesProductService : ISalesProductService
    {
        private readonly IRepository<SalesProduct> _salesProductRepository;
        private readonly IRepository<Warehouse> _warehouseRepository;
        private readonly IRepository<SalesProductStock> _salesProductStockRepository;

        public SalesProductService(
            IRepository<SalesProduct> salesProductRepository, 
            IRepository<SalesProductStock> salesProductStockRepository, 
            IRepository<Warehouse> warehouseRepository)
        {
            _salesProductRepository = salesProductRepository;
            _salesProductStockRepository = salesProductStockRepository;
            _warehouseRepository = warehouseRepository;
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

        public async Task InitializeSalesProductStocksAsync(int salesProductId)
        {
            var warehouses = await _warehouseRepository.GetAllAsync();

            foreach (var warehouse in warehouses)
            {
                bool alreadyExists = await _salesProductStockRepository.AnyAsync(
                    s => s.SalesProductId == salesProductId && s.WarehouseId == warehouse.Id);

                if (!alreadyExists)
                {
                    var stock = new SalesProductStock
                    {
                        SalesProductId = salesProductId,
                        WarehouseId = warehouse.Id,
                        Stock = 0,
                        CriticalStock = 0,
                        Price = 0
                    };
                    await _salesProductStockRepository.AddAsync(stock);
                }
            }

            await _salesProductStockRepository.SaveAsync();
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

        public async Task<List<SalesProduct>> GetActiveListAsync()
        {
            return await _salesProductRepository.GetQueryable()
                .Where(p => p.IsActive)
                .Include(p => p.Images)
                .Include(p => p.SalesProductCategories)
                    .ThenInclude(spc => spc.Category)
                .ToListAsync();
        }

        public async Task<SalesProduct?> GetByUrlAsync(string url)
        {
            return await _salesProductRepository.GetQueryable()
                .Include(p => p.Images)
                .Include(p => p.Product)
                .Include(p => p.SalesProductCategories)
                    .ThenInclude(spc => spc.Category)
                .FirstOrDefaultAsync(p => p.Url == url && p.IsActive);
        }

        public async Task<List<SalesProduct>> GetBestSellersAsync()
        {
            return await _salesProductRepository.GetQueryable()
                .Include(p => p.Images)
                .Where(p => p.IsActive && p.IsBestSeller)
                .ToListAsync();
        }

        public async Task<List<SalesProduct>> GetPopularAsync()
        {
            return await _salesProductRepository.GetQueryable()
                .Include(p => p.Images)
                .Where(p => p.IsActive && p.IsPopular)
                .ToListAsync();
        }
    }
}
