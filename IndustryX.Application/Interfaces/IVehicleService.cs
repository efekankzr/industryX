using IndustryX.Domain.Entities;

namespace IndustryX.Application.Interfaces
{
    public interface IVehicleService
    {
        Task<List<Vehicle>> GetAllAsync();
        Task<Vehicle?> GetByIdAsync(int id);
        Task AddAsync(Vehicle vehicle);
        Task UpdateAsync(Vehicle vehicle);
        Task DeleteAsync(int id);
        Task<bool> AssignDriverAsync(int vehicleId, string driverUserId);
        Task RemoveDriverAsync(int vehicleId);
        Task<List<ApplicationUser>> GetAvailableDriversAsync();
        Task<Vehicle?> GetByIdWithLocationAsync(int id);
        Task<List<Vehicle>> GetAllWithLocationAsync();
        Task<Vehicle?> GetByDeviceIdAsync(string deviceId);
    }
}
