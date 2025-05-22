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

            var htmlContent = await _iyzicoService.CreatePaymentRequestAsync(order);
            ViewBag.CheckoutForm = htmlContent;

            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Callback([FromQuery] int orderId)
        {
            var result = await _orderService.MarkAsPaidAsync(orderId);
            if (result)
            {
                return RedirectToAction("Success");
            }

            return RedirectToAction("Failure");
        }

        public IActionResult Success()
        {
            return View();
        }

        public IActionResult Failure()
        {
            return View();
        }
    }
}
