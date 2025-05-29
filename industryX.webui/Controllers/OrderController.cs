using IndustryX.Application.Interfaces;
using IndustryX.Domain.Entities;
using IndustryX.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IndustryX.WebUI.Controllers
{
    [Authorize(Roles = "Admin, SalesManager")]
    public class OrderController : BaseController
    {
        private readonly IOrderService _orderService;
        private readonly IEmailService _emailService;

        public OrderController(IOrderService orderService, IEmailService emailService)
        {
            _orderService = orderService;
            _emailService = emailService;
        }

        public async Task<IActionResult> Index(string? status, string? customer, DateTime? startDate, DateTime? endDate, int page = 1, int pageSize = 10)
        {
            var orders = await _orderService.GetAllAsync();

            if (!string.IsNullOrWhiteSpace(status))
                orders = orders.Where(o => o.Status.ToString() == status).ToList();

            if (!string.IsNullOrWhiteSpace(customer))
                orders = orders.Where(o => o.User.Firstname.Contains(customer, StringComparison.OrdinalIgnoreCase)).ToList();

            if (startDate.HasValue)
                orders = orders.Where(o => o.CreatedAt >= startDate.Value).ToList();

            if (endDate.HasValue)
                orders = orders.Where(o => o.CreatedAt <= endDate.Value).ToList();

            int totalCount = orders.Count;
            var pagedOrders = orders
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            ViewBag.PageSize = pageSize;
            ViewBag.Status = status;
            ViewBag.Customer = customer;
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");

            return View(pagedOrders);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var order = await _orderService.GetByIdAsync(id);
            if (order == null)
            {
                ShowAlert("Not Found", $"Order #{id} could not be found.", "warning");
                return RedirectToAction(nameof(Index));
            }

            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> AdvanceStatus(int id)
        {
            var order = await _orderService.GetByIdAsync(id);
            if (order == null)
            {
                ShowAlert("Error", $"Order #{id} not found.", "danger");
                return RedirectToAction(nameof(Index));
            }

            if (order.Status is OrderStatus.Delivered or OrderStatus.Cancelled)
            {
                ShowAlert("Info", "Order is already completed or cancelled.", "info");
                return RedirectToAction(nameof(Index));
            }

            var nextStatus = order.Status switch
            {
                OrderStatus.Pending => OrderStatus.Shipped,
                OrderStatus.Shipped => OrderStatus.Delivered,
                _ => order.Status
            };

            var result = await _orderService.UpdateStatusAsync(id, nextStatus);
            if (!result)
            {
                ShowAlert("Error", "Failed to update order status.", "danger");
                return RedirectToAction(nameof(Index));
            }

            ShowAlert("Success", $"Order advanced to '{nextStatus}'", "success");

            // Notify user via email when shipped
            if (nextStatus == OrderStatus.Shipped && !string.IsNullOrWhiteSpace(order.User?.Email))
            {
                var subject = "Your Order Has Been Shipped";
                var body = $@"
                    <p>Dear {order.User.Firstname} {order.User.Surname},</p>
                    <p>Your order <strong>#{order.Id}</strong> placed on {order.CreatedAt:dd MMM yyyy HH:mm} has been shipped.</p>
                    <p>You can expect delivery soon. Thank you for shopping with us!</p>
                    <p><strong>IndustryX</strong></p>";

                await _emailService.SendEmailAsync(order.User.Email, subject, body);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
