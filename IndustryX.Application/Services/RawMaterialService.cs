using IndustryX.Application.Interfaces;
using IndustryX.Domain.Entities;
using IndustryX.Domain.Interfaces;

namespace IndustryX.Application.Services
{
    public class RawMaterialService : IRawMaterialService
    {
        private readonly IRepository<RawMaterial> _repository;
        private readonly IRepository<Warehouse> _warehouseRepository;
        private readonly IRepository<RawMaterialStock> _rawMaterialStockRepository;

        public RawMaterialService(
            IRepository<RawMaterial> repository,
            IRepository<Warehouse> warehouseRepository,
            IRepository<RawMaterialStock> rawMaterialStockRepository)
        {
            _repository = repository;
            _warehouseRepository = warehouseRepository;
            _rawMaterialStockRepository = rawMaterialStockRepository;
        }

        public async Task<IEnumerable<RawMaterial>> GetAllAsync() => await _repository.GetAllAsync();

        public async Task<RawMaterial?> GetByIdAsync(int id) => await _repository.GetByIdAsync(id);

        public async Task<(bool Success, string? ErrorMessage)> AddAsync(RawMaterial material)
        {
            var nameExists = await _repository.AnyAsync(r => r.Name.ToLower() == material.Name.ToLower());
            if (nameExists)
                return (false, "A raw material with the same name already exists.");

            var barcodeExists = await _repository.AnyAsync(r => r.Barcode == material.Barcode);
            if (barcodeExists)
                return (false, "A raw material with the same barcode already exists.");

            await _repository.AddAsync(material);
            await _repository.SaveAsync();

            var warehouses = await _warehouseRepository.GetAllAsync();
            foreach (var warehouse in warehouses)
            {
                var stock = new RawMaterialStock
                {
                    RawMaterialId = material.Id,
                    WarehouseId = warehouse.Id,
                    Stock = 0,
                    QruicalStock = 0
                };
                await _rawMaterialStockRepository.AddAsync(stock);
            }
            await _rawMaterialStockRepository.SaveAsync();

            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateAsync(RawMaterial material)
        {
            var nameExists = await _repository.AnyAsync(r => r.Id != material.Id && r.Name.ToLower() == material.Name.ToLower());
            if (nameExists)
                return (false, "Another raw material with the same name already exists.");

            var barcodeExists = await _repository.AnyAsync(r => r.Id != material.Id && r.Barcode == material.Barcode);
            if (barcodeExists)
                return (false, "Another raw material with the same barcode already exists.");

            _repository.Update(material);
            await _repository.SaveAsync();
            return (true, null);
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity != null)
            {
                _repository.Delete(entity);
                await _repository.SaveAsync();
            }
        }
    }
}