using IndustryX.Domain.Entities;

namespace IndustryX.Application.Interfaces
{
    public interface IUserAddressService
    {
        Task<List<UserAddress>> GetUserAddressesAsync(string userId);
        Task<UserAddress?> GetByIdAsync(int id, string userId);
        Task<(bool Success, string? Error)> CreateAsync(UserAddress address);
        Task<(bool Success, string? Error)> UpdateAsync(UserAddress address, string userId);
        Task<bool> DeleteAsync(int id, string userId);
        Task<bool> SetAsDefaultAsync(int id, string userId);
    }
}
