using IndustryX.Application.Interfaces;
using IndustryX.Domain.Entities;
using IndustryX.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IndustryX.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IRepository<Order> _orderRepository;
        private readonly ICartService _cartService;

        public OrderService(IRepository<Order> orderRepository, ICartService cartService)
        {
            _orderRepository = orderRepository;
            _cartService = cartService;
        }

        public async Task<(bool Success, string? Error)> CreateOrderFromCartAsync(string userId)
        {
            var cartItems = await _cartService.GetUserCartAsync(userId);
            if (!cartItems.Any())
                return (false, "Your cart is empty.");

            var order = new Order
            {
                UserId = userId,
                CreatedAt = DateTime.Now,
                TotalPrice = cartItems.Sum(ci => ci.SalesProduct.SalePrice * ci.Quantity),
                OrderItems = cartItems.Select(ci => new OrderItem
                {
                    SalesProductId = ci.SalesProductId,
                    Quantity = ci.Quantity,
                    UnitPrice = ci.SalesProduct.SalePrice
                }).ToList(),
                Status = OrderStatus.Pending,
                IsPaid = false
            };

            await _orderRepository.AddAsync(order);
            await _orderRepository.SaveAsync();
            await _cartService.ClearCartAsync(userId);

            return (true, null);
        }

        public async Task<List<Order>> GetUserOrdersAsync(string userId)
        {
            return await _orderRepository
                .GetQueryable()
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.SalesProduct)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task SaveOrderAsync(Order order)
        {
            await _orderRepository.AddAsync(order);
            await _orderRepository.SaveAsync();
        }

        public async Task<bool> MarkAsPaidAsync(int orderId, string paymentProvider = "", string transactionId = "")
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null || order.IsPaid)
                return false;

            order.IsPaid = true;
            order.PaymentProvider = paymentProvider;
            order.PaymentTransactionId = transactionId;
            order.PaymentDate = DateTime.Now;

            await _orderRepository.SaveAsync();
            return true;
        }

        public async Task<Order?> GetByIdAsync(int id)
        {
            return await _orderRepository
                .GetQueryable()
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.SalesProduct)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<List<Order>> GetAllAsync()
        {
            return await _orderRepository
                .GetQueryable()
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.SalesProduct)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> UpdateStatusAsync(int orderId, OrderStatus newStatus)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null) return false;

            order.Status = newStatus;
            await _orderRepository.SaveAsync();
            return true;
        }
    }
}
