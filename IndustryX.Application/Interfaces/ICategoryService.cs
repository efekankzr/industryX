using IndustryX.Domain.Entities;

namespace IndustryX.Application.Interfaces
{
    public interface ICategoryService
    {
        Task<List<Category>> GetAllAsync();
        Task<Category?> GetByIdAsync(int id);
        Task<(bool Success, string? Error)> CreateAsync(Category category);
        Task<(bool Success, string? Error)> UpdateAsync(Category category);
        Task<bool> DeleteAsync(int id);
    }
}
