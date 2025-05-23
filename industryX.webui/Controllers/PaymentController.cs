using IndustryX.Application.Interfaces;
using IndustryX.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IndustryX.WebUI.Controllers
{
    [Authorize]
    public class PaymentController : BaseController
    {
        private readonly IOrderService _orderService;
        private readonly IIyzicoService _iyzicoService;

        public PaymentController(IOrderService orderService, IIyzicoService iyzicoService)
        {
            _orderService = orderService;
            _iyzicoService = iyzicoService;
        }

        public async Task<IActionResult> Checkout(int orderId)
        {
            var order = await _orderService.GetByIdAsync(orderId);
            if (order == null)
            {
                ShowAlert("Error", "Order not found.", "danger");
                return RedirectToAction("Index", "Home");
            }

            if (order.IsPaid)
            {
                ShowAlert("Info", "This order has already been paid.", "info");
                return RedirectToAction("MyOrders", "Shop");
            }

            ViewBag.CheckoutForm = await _iyzicoService.CreatePaymentRequestAsync(order);
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Callback([FromQuery] int orderId)
        {
            var success = await _orderService.MarkAsPaidAsync(orderId);
            return RedirectToAction(success ? "Success" : "Failure");
        }

        public IActionResult Success() => View();

        public IActionResult Failure() => View();
    }
}
