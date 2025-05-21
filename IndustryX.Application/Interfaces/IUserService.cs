using IndustryX.Domain.Entities;

namespace IndustryX.Application.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<ApplicationUser>> GetAllAsync();
        Task<ApplicationUser?> GetByIdAsync(string id);
        Task<(bool Success, string? Error)> CreateAsync(ApplicationUser user, string password, string role, int? warehouseId = null);
        Task<(bool Success, string? Error)> UpdateAsync(ApplicationUser user, int? warehouseId = null);
        Task<(bool Success, string? Error)> DeleteAsync(string id);
        Task<IEnumerable<ApplicationUser>> GetByRoleAsync(string role);
        Task<int?> GetUserWarehouseIdAsync(string userId);
        Task<IList<string>> GetRolesAsync(string userId);
    }
}
