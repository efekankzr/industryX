using IndustryX.Domain.Entities;

namespace IndustryX.Application.Interfaces
{
    public interface IProductService
    {
        Task<List<ProductStock>> GetCriticalStocksAsync();
        Task<IEnumerable<Product>> GetAllAsync();
        Task<Product?> GetByIdAsync(int id);
        Task<(bool Success, string? ErrorMessage)> CreateAsync(Product product);
        Task<(bool Success, string? ErrorMessage)> UpdateAsync(Product product);
        Task DeleteAsync(int id);
    }
}
