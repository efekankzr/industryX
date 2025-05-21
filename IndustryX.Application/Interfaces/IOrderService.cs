using IndustryX.Domain.Entities;

namespace IndustryX.Application.Interfaces
{
    public interface IOrderService
    {
        Task<(bool Success, string? Error)> CreateOrderFromCartAsync(string userId);
        Task<List<Order>> GetUserOrdersAsync(string userId);
    }
}
