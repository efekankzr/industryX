using IndustryX.Application.Interfaces;
using IndustryX.Domain.Entities;
using IndustryX.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IndustryX.Application.Services
{
    public class WarehouseService : IWarehouseService
    {
        private readonly IRepository<Warehouse> _warehouseRepository;
        private readonly IRepository<ProductStock> _productStockRepository;
        private readonly IRepository<RawMaterialStock> _rawMaterialStockRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<RawMaterial> _rawMaterialRepository;
        private readonly IRepository<SalesProduct> _salesProductRepository;
        private readonly IRepository<SalesProductStock> _salesProductStockRepository;

        public WarehouseService(
            IRepository<Warehouse> warehouseRepository,
            IRepository<ProductStock> productStockRepository,
            IRepository<RawMaterialStock> rawMaterialStockRepository,
            IRepository<Product> productRepository,
            IRepository<RawMaterial> rawMaterialRepository,
            IRepository<SalesProduct> salesProductRepository,
            IRepository<SalesProductStock> salesProductStockRepository)
        {
            _warehouseRepository = warehouseRepository;
            _productStockRepository = productStockRepository;
            _rawMaterialStockRepository = rawMaterialStockRepository;
            _productRepository = productRepository;
            _rawMaterialRepository = rawMaterialRepository;
            _salesProductRepository = salesProductRepository;
            _salesProductStockRepository = salesProductStockRepository;
        }

        public async Task<(bool HasMainProductWarehouse, bool HasMainRawMaterialWarehouse, bool HasMainSalesProductWarehouse)> CheckMainWarehousesAsync()
        {
            var query = _warehouseRepository.GetQueryable();

            bool hasMainProduct = await query.AnyAsync(w => w.IsMainForProduct);
            bool hasMainRaw = await query.AnyAsync(w => w.IsMainForRawMaterial);
            bool hasMainSales = await query.AnyAsync(w => w.IsMainForSalesProduct);

            return (hasMainProduct, hasMainRaw, hasMainSales);
        }

        public async Task<IEnumerable<Warehouse>> GetAllAsync() => await _warehouseRepository.GetAllAsync();

        public async Task<Warehouse?> GetByIdAsync(int id) => await _warehouseRepository.GetByIdAsync(id);

        public async Task SetMainWarehouseAsync(int warehouseId, string type)
        {
            var allWarehouses = await _warehouseRepository.GetAllAsync();

            foreach (var warehouse in allWarehouses)
            {
                bool shouldBeMain = warehouse.Id == warehouseId;
                bool hasChanged = false;

                if (type == "product" && warehouse.IsMainForProduct != shouldBeMain)
                {
                    warehouse.IsMainForProduct = shouldBeMain;
                    hasChanged = true;
                }
                else if (type == "raw" && warehouse.IsMainForRawMaterial != shouldBeMain)
                {
                    warehouse.IsMainForRawMaterial = shouldBeMain;
                    hasChanged = true;
                }
                else if (type == "sales" && warehouse.IsMainForSalesProduct != shouldBeMain)
                {
                    warehouse.IsMainForSalesProduct = shouldBeMain;
                    hasChanged = true;
                }

                if (hasChanged)
                    await _warehouseRepository.UpdateAsync(warehouse);
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> AddWithInitialStocksAsync(Warehouse warehouse)
        {
            var nameExists = await _warehouseRepository.AnyAsync(w => w.Name.ToLower() == warehouse.Name.ToLower());
            if (nameExists)
                return (false, "A warehouse with the same name already exists.");

            await _warehouseRepository.AddAsync(warehouse);
            await _warehouseRepository.SaveAsync();

            var products = await _productRepository.GetAllAsync();
            var rawMaterials = await _rawMaterialRepository.GetAllAsync();
            var salesProducts = await _salesProductRepository.GetAllAsync();

            foreach (var product in products)
            {
                await _productStockRepository.AddAsync(new ProductStock
                {
                    ProductId = product.Id,
                    WarehouseId = warehouse.Id,
                    Stock = 0,
                    QruicalStock = 0,
                    Price = 0
                });
            }

            foreach (var material in rawMaterials)
            {
                await _rawMaterialStockRepository.AddAsync(new RawMaterialStock
                {
                    RawMaterialId = material.Id,
                    WarehouseId = warehouse.Id,
                    Stock = 0,
                    QruicalStock = 0
                });
            }

            foreach (var sp in salesProducts)
            {
                await _salesProductStockRepository.AddAsync(new SalesProductStock
                {
                    SalesProductId = sp.Id,
                    WarehouseId = warehouse.Id,
                    Stock = 0,
                    CriticalStock = 0,
                    Price = 0
                });
            }

            await _salesProductStockRepository.SaveAsync();
            await _productStockRepository.SaveAsync();
            await _rawMaterialStockRepository.SaveAsync();

            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> AddAsync(Warehouse warehouse)
        {
            var nameExists = await _warehouseRepository.AnyAsync(w => w.Name.ToLower() == warehouse.Name.ToLower());
            if (nameExists)
                return (false, "A warehouse with the same name already exists.");

            await _warehouseRepository.AddAsync(warehouse);
            await _warehouseRepository.SaveAsync();
            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateAsync(Warehouse warehouse)
        {
            var nameExists = await _warehouseRepository.AnyAsync(w => w.Id != warehouse.Id && w.Name.ToLower() == warehouse.Name.ToLower());
            if (nameExists)
                return (false, "Another warehouse with the same name already exists.");

            _warehouseRepository.Update(warehouse);
            await _warehouseRepository.SaveAsync();
            return (true, null);
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _warehouseRepository.GetByIdAsync(id);
            if (entity != null)
            {
                _warehouseRepository.Delete(entity);
                await _warehouseRepository.SaveAsync();
            }
        }
    }
}