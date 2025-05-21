using IndustryX.Application.Interfaces;
using IndustryX.Domain.Entities;
using IndustryX.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IndustryX.Application.Services
{
    public class UserAddressService : IUserAddressService
    {
        private readonly IRepository<UserAddress> _addressRepository;

        public UserAddressService(IRepository<UserAddress> addressRepository)
        {
            _addressRepository = addressRepository;
        }

        public async Task<List<UserAddress>> GetUserAddressesAsync(string userId)
        {
            return await _addressRepository
                .GetQueryable()
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<UserAddress?> GetByIdAsync(int id, string userId)
        {
            return await _addressRepository
                .GetQueryable()
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);
        }

        public async Task<(bool Success, string? Error)> CreateAsync(UserAddress address)
        {
            if (string.IsNullOrWhiteSpace(address.FullAddress))
                return (false, "Address cannot be empty.");

            await _addressRepository.AddAsync(address);
            await _addressRepository.SaveAsync();
            return (true, null);
        }

        public async Task<(bool Success, string? Error)> UpdateAsync(UserAddress address, string userId)
        {
            var existing = await GetByIdAsync(address.Id, userId);
            if (existing == null)
                return (false, "Address not found or access denied.");

            existing.Title = address.Title;
            existing.Country = address.Country;
            existing.City = address.City;
            existing.District = address.District;
            existing.FullAddress = address.FullAddress;

            _addressRepository.Update(existing);
            await _addressRepository.SaveAsync();
            return (true, null);
        }

        public async Task<bool> DeleteAsync(int id, string userId)
        {
            var address = await GetByIdAsync(id, userId);
            if (address == null) return false;

            _addressRepository.Delete(address);
            await _addressRepository.SaveAsync();
            return true;
        }

        public async Task<bool> SetAsDefaultAsync(int id, string userId)
        {
            var currentDefault = await _addressRepository.GetQueryable()
                .FirstOrDefaultAsync(a => a.UserId == userId && a.IsDefault);

            if (currentDefault != null)
            {
                currentDefault.IsDefault = false;
                _addressRepository.Update(currentDefault);
            }

            var toSet = await GetByIdAsync(id, userId);
            if (toSet == null) return false;

            toSet.IsDefault = true;
            _addressRepository.Update(toSet);
            await _addressRepository.SaveAsync();
            return true;
        }
    }
}
