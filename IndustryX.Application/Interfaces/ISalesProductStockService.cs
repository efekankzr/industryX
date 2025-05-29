using IndustryX.Domain.Entities;

namespace IndustryX.Application.Interfaces
{
    public interface ISalesProductStockService
    {
        Task<IEnumerable<SalesProductStock>> GetAllWithIncludesAsync();
        Task<bool> IncreaseStockAsync(int id, int amount);
        Task<bool> DecreaseStockAsync(int id, int amount);
        Task<bool> SetCriticalStockAsync(int id, int critical);
    }
}