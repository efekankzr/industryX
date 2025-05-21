using IndustryX.Application.Interfaces;
using IndustryX.Domain.Entities;
using IndustryX.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IndustryX.Application.Services
{
    public class WishlistService : IWishlistService
    {
        private readonly IRepository<WishlistItem> _wishlistRepository;

        public WishlistService(IRepository<WishlistItem> wishlistRepository)
        {
            _wishlistRepository = wishlistRepository;
        }

        public async Task<List<WishlistItem>> GetUserWishlistAsync(string userId)
        {
            return await _wishlistRepository
                .GetQueryable()
                .Include(w => w.SalesProduct)
                .ThenInclude(p => p.Images)
                .Where(w => w.UserId == userId)
                .ToListAsync();
        }

        public async Task AddToWishlistAsync(string userId, int salesProductId)
        {
            var exists = await _wishlistRepository
                .AnyAsync(w => w.UserId == userId && w.SalesProductId == salesProductId);

            if (!exists)
            {
                await _wishlistRepository.AddAsync(new WishlistItem
                {
                    UserId = userId,
                    SalesProductId = salesProductId
                });

                await _wishlistRepository.SaveAsync();
            }
        }

        public async Task RemoveFromWishlistAsync(string userId, int salesProductId)
        {
            var item = await _wishlistRepository
                .GetQueryable()
                .FirstOrDefaultAsync(w => w.UserId == userId && w.SalesProductId == salesProductId);

            if (item != null)
            {
                _wishlistRepository.Delete(item);
                await _wishlistRepository.SaveAsync();
            }
        }
    }
}
