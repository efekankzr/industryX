using IndustryX.Application.Interfaces;
using IndustryX.Domain.Entities;
using IndustryX.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IndustryX.Application.Services
{
    public class RawMaterialStockService : IRawMaterialStockService
    {
        private readonly IRepository<RawMaterialStock> _repository;

        public RawMaterialStockService(IRepository<RawMaterialStock> repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<RawMaterialStock>> GetAllWithIncludesAsync()
        {
            return await _repository
                .GetQueryable()
                .Include(s => s.RawMaterial)
                .Include(s => s.Warehouse)
                .ToListAsync();
        }

        public async Task<bool> IncreaseStockAsync(int id, decimal amount)
        {
            var stock = await _repository.GetByIdAsync(id);
            if (stock == null || amount <= 0)
                return false;

            stock.Stock += amount;
            _repository.Update(stock);
            await _repository.SaveAsync();
            return true;
        }

        public async Task<bool> DecreaseStockAsync(int id, decimal amount)
        {
            var stock = await _repository.GetByIdAsync(id);
            if (stock == null || amount <= 0 || stock.Stock < amount)
                return false;

            stock.Stock -= amount;
            _repository.Update(stock);
            await _repository.SaveAsync();
            return true;
        }

        public async Task<bool> SetCriticalStockAsync(int id, decimal critical)
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
