using IndustryX.Domain.Entities;

namespace IndustryX.Application.Interfaces
{
    public interface IWishlistService
    {
        Task<List<WishlistItem>> GetUserWishlistAsync(string userId);
        Task AddToWishlistAsync(string userId, int salesProductId);
        Task RemoveFromWishlistAsync(string userId, int salesProductId);
    }

}
