using IndustryX.Application.Interfaces;
using IndustryX.Domain.Entities;
using IndustryX.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IndustryX.Application.Services
{
    public class SalesProductStockService : ISalesProductStockService
    {
        private readonly IRepository<SalesProductStock> _repository;
        private readonly IRepository<ProductStock> _productStockRepository;

        public SalesProductStockService(IRepository<SalesProductStock> repository, IRepository<ProductStock> productStockRepository)
        {
            _repository = repository;
            _productStockRepository = productStockRepository;
        }

        public async Task<IEnumerable<SalesProductStock>> GetAllWithIncludesAsync()
        {
            return await _repository
                .GetQueryable()
                .Include(s => s.SalesProduct)
                .Include(s => s.Warehouse)
                .ToListAsync();
        }
        
        public async Task<bool> IncreaseStockAsync(int id, int amount)
        {
            var salesStock = await _repository.GetQueryable()
                .Include(s => s.SalesProduct)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (salesStock == null || amount <= 0)
                return false;

            var productId = salesStock.SalesProduct.ProductId;
            var warehouseId = salesStock.WarehouseId;

            var productStock = await _productStockRepository.GetQueryable()
                .FirstOrDefaultAsync(ps => ps.ProductId == productId && ps.WarehouseId == warehouseId);

            if (productStock == null || productStock.Stock < amount)
                return false;

            int oldSalesQty = salesStock.Stock;
            decimal oldSalesPrice = salesStock.Price;

            int productQtyToUse = amount;
            decimal productUnitPrice = productStock.Price;

            int newSalesQty = oldSalesQty + productQtyToUse;

            decimal newPrice = newSalesQty > 0
                ? ((oldSalesQty * oldSalesPrice) + (productQtyToUse * productUnitPrice)) / newSalesQty
                : oldSalesPrice;

            productStock.Stock -= productQtyToUse;
            salesStock.Stock = newSalesQty;
            salesStock.Price = newPrice;

            _productStockRepository.Update(productStock);
            _repository.Update(salesStock);

            await _productStockRepository.SaveAsync();
            await _repository.SaveAsync();

            return true;
        }

        public async Task<bool> DecreaseStockAsync(int id, int amount)
        {
            var salesStock = await _repository.GetQueryable()
                .Include(s => s.SalesProduct)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (salesStock == null || amount <= 0 || salesStock.Stock < amount)
                return false;

            var productId = salesStock.SalesProduct.ProductId;
            var warehouseId = salesStock.WarehouseId;

            var productStock = await _productStockRepository.GetQueryable()
                .FirstOrDefaultAsync(ps => ps.ProductId == productId && ps.WarehouseId == warehouseId);

            if (productStock == null)
            {
                productStock = new ProductStock
                {
                    ProductId = productId,
                    WarehouseId = warehouseId,
                    Stock = 0,
                    Price = 0,
                    QruicalStock = 0
                };
                await _productStockRepository.AddAsync(productStock);
            }

            int existingQty = productStock.Stock;
            decimal existingPrice = productStock.Price;

            int returnedQty = amount;
            decimal returnedUnitPrice = salesStock.Price;

            int newProductQty = existingQty + returnedQty;

            decimal newPrice = newProductQty > 0
                ? ((existingQty * existingPrice) + (returnedQty * returnedUnitPrice)) / newProductQty
                : existingPrice;

            salesStock.Stock -= returnedQty;
            productStock.Stock = newProductQty;
            productStock.Price = newPrice;

            _repository.Update(salesStock);
            _productStockRepository.Update(productStock);

            await _repository.SaveAsync();
            await _productStockRepository.SaveAsync();

            return true;
        }

        public async Task<bool> SetCriticalStockAsync(int id, int critical)
        {
            var stock = await _repository.GetByIdAsync(id);
            if (stock == null || critical < 0)
                return false;

            stock.CriticalStock = critical;
            _repository.Update(stock);
            await _repository.SaveAsync();
            return true;
        }
    }
}
