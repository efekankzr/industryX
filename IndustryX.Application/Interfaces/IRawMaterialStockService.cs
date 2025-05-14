using IndustryX.Domain.Entities;

namespace IndustryX.Application.Interfaces
{
    public interface IRawMaterialStockService
    {
        Task<IEnumerable<RawMaterialStock>> GetAllWithIncludesAsync();
        Task<bool> IncreaseStockAsync(int id, decimal amount);
        Task<bool> DecreaseStockAsync(int id, decimal amount);
        Task<bool> SetCriticalStockAsync(int id, decimal critical);
    }
}
