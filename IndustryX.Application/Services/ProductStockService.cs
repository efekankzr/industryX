using IndustryX.Application.Interfaces;
using IndustryX.Domain.Entities;
using IndustryX.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IndustryX.Application.Services
{
    public class ProductStockService : IProductStockService
    {
        private readonly IRepository<ProductStock> _repository;

        public ProductStockService(IRepository<ProductStock> repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<ProductStock>> GetAllWithIncludesAsync()
        {
            return await _repository
                .GetQueryable()
                .Include(s => s.Product)
                .Include(s => s.Warehouse)
                .ToListAsync();
        }

        public async Task<bool> SetCriticalStockAsync(int id, int critical)
        {
            var stock = await _repository.GetByIdAsync(id);
            if (stock == null || critical < 0)
                return false;

            stock.QruicalStock = critical;
            _repository.Update(stock);
            await _repository.SaveAsync();
            return true;
        }
    }
}
