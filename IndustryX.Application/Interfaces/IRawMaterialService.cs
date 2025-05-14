using IndustryX.Domain.Entities;

namespace IndustryX.Application.Interfaces
{
    public interface IRawMaterialService
    {
        Task<IEnumerable<RawMaterial>> GetAllAsync();
        Task<RawMaterial?> GetByIdAsync(int id);
        Task<(bool Success, string? ErrorMessage)> AddAsync(RawMaterial material);
        Task<(bool Success, string? ErrorMessage)> UpdateAsync(RawMaterial material);
        Task DeleteAsync(int id);
    }
}