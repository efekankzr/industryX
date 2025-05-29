using IndustryX.Application.Interfaces;
using IndustryX.Domain.Entities;
using IndustryX.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IndustryX.Application.Services
{
    public class VehicleService : IVehicleService
    {
        private readonly IRepository<Vehicle> _vehicleRepository;
        private readonly IRepository<VehicleUser> _vehicleUserRepository;
        private readonly IRepository<LocationLog> _locationLogRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public VehicleService(
            IRepository<Vehicle> vehicleRepository,
            IRepository<LocationLog> locationLogRepository,
            IRepository<VehicleUser> vehicleUserRepository,
            UserManager<ApplicationUser> userManager)
        {
            _vehicleRepository = vehicleRepository;
            _locationLogRepository = locationLogRepository;
            _vehicleUserRepository = vehicleUserRepository;
            _userManager = userManager;
        }

        public async Task<List<Vehicle>> GetAllAsync()
        {
            return await _vehicleRepository
                .GetQueryable()
                .Include(v => v.VehicleUsers)
                .ThenInclude(vu => vu.User)
                .ToListAsync();
        }

        public async Task<Vehicle?> GetByIdAsync(int id)
        {
            return await _vehicleRepository.GetByIdAsync(id);
        }

        public async Task AddAsync(Vehicle vehicle)
        {
            await _vehicleRepository.AddAsync(vehicle);
            await _vehicleRepository.SaveAsync();

            var locationLog = new LocationLog
            {
                VehicleId = vehicle.Id,
                Latitude = 0,
                Longitude = 0,
                Status = VehicleStatus.Stopped,
                UpdatedAt = DateTime.UtcNow
            };
            await _locationLogRepository.AddAsync(locationLog);
            await _locationLogRepository.SaveAsync();
        }

        public async Task UpdateAsync(Vehicle vehicle)
        {
            _vehicleRepository.Update(vehicle);
            await _vehicleRepository.SaveAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(id);
            if (vehicle != null)
            {
                _vehicleRepository.Delete(vehicle);
                await _vehicleRepository.SaveAsync();
            }
        }

        public async Task<bool> AssignDriverAsync(int vehicleId, string driverUserId)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId);
            if (vehicle == null)
                return false;

            var existing = await _vehicleUserRepository
                .GetQueryable()
                .FirstOrDefaultAsync(vu => vu.VehicleId == vehicleId);
            if (existing != null)
            {
                _vehicleUserRepository.Delete(existing);
                await _vehicleUserRepository.SaveAsync();
            }

            if (!string.IsNullOrEmpty(driverUserId))
            {
                await _vehicleUserRepository.AddAsync(new VehicleUser
                {
                    VehicleId = vehicleId,
                    UserId = driverUserId
                });
                await _vehicleUserRepository.SaveAsync();
            }
            return true;
        }

        public async Task RemoveDriverAsync(int vehicleId)
        {
            var existing = await _vehicleUserRepository
                .GetQueryable()
                .FirstOrDefaultAsync(vu => vu.VehicleId == vehicleId);
            if (existing != null)
            {
                _vehicleUserRepository.Delete(existing);
                await _vehicleUserRepository.SaveAsync();
            }
        }

        public async Task<List<ApplicationUser>> GetAvailableDriversAsync()
        {
            var drivers = await _userManager.GetUsersInRoleAsync("Driver");
            return drivers.ToList();
        }
    }
}