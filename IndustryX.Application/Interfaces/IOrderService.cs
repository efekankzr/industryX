using IndustryX.Domain.Entities;

namespace IndustryX.Application.Interfaces
{
    public interface IOrderService
    {
        Task<(bool Success, string? Error)> CreateOrderFromCartAsync(string userId);
        Task<List<Order>> GetUserOrdersAsync(string userId);
        Task SaveOrderAsync(Order order);
        Task<bool> MarkAsPaidAsync(int orderId, string paymentProvider = "", string transactionId = "");
        Task<Order?> GetByIdAsync(int id);
    }
}
