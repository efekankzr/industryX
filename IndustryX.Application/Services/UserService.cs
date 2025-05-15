using IndustryX.Application.Services.Interfaces;
using IndustryX.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IndustryX.Application.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IEnumerable<ApplicationUser>> GetAllAsync()
        {
            return await _userManager.Users.Include(u => u.Warehouse).ToListAsync();
        }

        public async Task<ApplicationUser?> GetByIdAsync(string id)
        {
            return await _userManager.Users.Include(u => u.Warehouse).FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<(bool Success, string? Error)> CreateAsync(ApplicationUser user, string password, string role, int? warehouseId = null)
        {
            user.WarehouseId = warehouseId;

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
                return (false, string.Join("; ", result.Errors.Select(e => e.Description)));

            await _userManager.AddToRoleAsync(user, role);
            return (true, null);
        }

        public async Task<(bool Success, string? Error)> UpdateAsync(ApplicationUser user, int? warehouseId = null)
        {
            user.WarehouseId = warehouseId;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return (false, string.Join("; ", result.Errors.Select(e => e.Description)));

            return (true, null);
        }

        public async Task<(bool Success, string? Error)> DeleteAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return (false, "User not found.");

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
                return (false, string.Join("; ", result.Errors.Select(e => e.Description)));

            return (true, null);
        }

        public async Task<IList<string>> GetRolesAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            return user == null ? new List<string>() : await _userManager.GetRolesAsync(user);
        }

        public async Task<IEnumerable<ApplicationUser>> GetByRoleAsync(string role)
        {
            return await _userManager.GetUsersInRoleAsync(role);
        }

        public async Task<int?> GetUserWarehouseIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            return user?.WarehouseId;
        }
    }
}
