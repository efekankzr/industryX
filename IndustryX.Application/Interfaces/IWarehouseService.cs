using IndustryX.Domain.Entities;

namespace IndustryX.Application.Interfaces
{
    public interface IWarehouseService
    {
        Task<IEnumerable<Warehouse>> GetAllAsync();
        Task<Warehouse?> GetByIdAsync(int id);
        Task<(bool HasMainProductWarehouse, bool HasMainRawMaterialWarehouse, bool HasMainSalesProductWarehouse)> CheckMainWarehousesAsync();
        Task SetMainWarehouseAsync(int warehouseId, string type);
        Task<(bool Success, string? ErrorMessage)> AddWithInitialStocksAsync(Warehouse warehouse);
        Task<(bool Success, string? ErrorMessage)> AddAsync(Warehouse warehouse);
        Task<(bool Success, string? ErrorMessage)> UpdateAsync(Warehouse warehouse);
        Task DeleteAsync(int id);
    }
}