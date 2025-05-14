using IndustryX.Domain.Entities;

namespace IndustryX.Application.Interfaces
{
    public interface IProductStockService
    {
        Task<IEnumerable<ProductStock>> GetAllWithIncludesAsync();
        Task<bool> SetCriticalStockAsync(int id, int critical);
    }
}
