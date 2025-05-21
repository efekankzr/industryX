using IndustryX.Domain.Entities;

namespace IndustryX.Application.Interfaces
{
    public interface ICartService
    {
        Task<List<CartItem>> GetUserCartAsync(string userId);
        Task AddToCartAsync(string userId, int salesProductId, int quantity = 1);
        Task RemoveFromCartAsync(string userId, int salesProductId);
        Task ClearCartAsync(string userId);
    }
}
