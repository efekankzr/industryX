using IndustryX.Application.Interfaces;
using IndustryX.Domain.Entities;
using IndustryX.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IndustryX.Application.Services
{
    public class CartService : ICartService
    {
        private readonly IRepository<CartItem> _cartRepository;

        public CartService(IRepository<CartItem> cartRepository)
        {
            _cartRepository = cartRepository;
        }

        public async Task<List<CartItem>> GetUserCartAsync(string userId)
        {
            return await _cartRepository
                .GetQueryable()
                .Include(c => c.SalesProduct)
                .Where(c => c.UserId == userId)
                .ToListAsync();
        }

        public async Task AddToCartAsync(string userId, int salesProductId, int quantity = 1)
        {
            var existing = await _cartRepository
                .GetQueryable()
                .FirstOrDefaultAsync(c => c.UserId == userId && c.SalesProductId == salesProductId);

            if (existing != null)
            {
                existing.Quantity += quantity;
                _cartRepository.Update(existing);
            }
            else
            {
                var cartItem = new CartItem
                {
                    UserId = userId,
                    SalesProductId = salesProductId,
                    Quantity = quantity
                };
                await _cartRepository.AddAsync(cartItem);
            }

            await _cartRepository.SaveAsync();
        }

        public async Task RemoveFromCartAsync(string userId, int salesProductId)
        {
            var item = await _cartRepository
                .GetQueryable()
                .FirstOrDefaultAsync(c => c.UserId == userId && c.SalesProductId == salesProductId);

            if (item != null)
            {
                _cartRepository.Delete(item);
                await _cartRepository.SaveAsync();
            }
        }

        public async Task ClearCartAsync(string userId)
        {
            var items = await _cartRepository
                .GetQueryable()
                .Where(c => c.UserId == userId)
                .ToListAsync();

            foreach (var item in items)
            {
                _cartRepository.Delete(item);
            }

            await _cartRepository.SaveAsync();
        }
    }
}
