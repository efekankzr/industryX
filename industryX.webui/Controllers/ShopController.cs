using System.Security.Claims;
using IndustryX.Application.Interfaces;
using IndustryX.Domain.Entities;
using IndustryX.Domain.ValueObjects;
using IndustryX.WebUI.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IndustryX.WebUI.Controllers
{
    public class ShopController : BaseController
    {
        private readonly ICartService _cartService;
        private readonly IOrderService _orderService;
        private readonly IWishlistService _wishlistService;
        private readonly ISalesProductService _salesProductService;
        private readonly IUserAddressService _userAddressService;

        public ShopController(
            ICartService cartService,
            IOrderService orderService,
            IWishlistService wishlistService,
            ISalesProductService salesProductService,
            IUserAddressService userAddressService)
        {
            _cartService = cartService;
            _orderService = orderService;
            _wishlistService = wishlistService;
            _salesProductService = salesProductService;
            _userAddressService = userAddressService;
        }

        [AllowAnonymous]
        public IActionResult Index() => View();

        [AllowAnonymous]
        public async Task<IActionResult> Details(string url)
        {
            var product = await _salesProductService.GetByUrlAsync(url);
            if (product == null)
            {
                ShowAlert("Not Found", "The product you're looking for does not exist.", "warning");
                return RedirectToAction("Index", "Home");
            }

            var model = new SalesProductDetailViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.SalePrice,
                Url = product.Url,
                ImagePaths = product.Images.Select(i => i.ImagePath).ToList()
            };

            return View(model);
        }

        [Authorize]
        public async Task<IActionResult> Cart()
        {
            var userId = GetUserId();
            var items = await _cartService.GetUserCartAsync(userId);

            var model = items.Select(ci => new CartItemViewModel
            {
                SalesProductId = ci.SalesProductId,
                ProductName = ci.SalesProduct.Name,
                ProductUrl = ci.SalesProduct.Url,
                ImageUrl = ci.SalesProduct.Images.FirstOrDefault()?.ImagePath,
                UnitPrice = ci.SalesProduct.SalePrice,
                Quantity = ci.Quantity,
                Total = ci.Quantity * ci.SalesProduct.SalePrice
            }).ToList();

            return View(model);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddToCart(int id, int quantity = 1)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            await _cartService.AddToCartAsync(userId, id, quantity);
            ShowAlert("Success", "Item added to cart.", "success");
            return RedirectToAction("Cart");
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int id)
        {
            var userId = GetUserId();
            await _cartService.RemoveFromCartAsync(userId, id);
            ShowAlert("Removed", "Item removed from cart.", "info");
            return RedirectToAction("Cart");
        }

        [Authorize]
        public async Task<IActionResult> Checkout()
        {
            var userId = GetUserId();
            var cartItems = await _cartService.GetUserCartAsync(userId);
            if (!cartItems.Any())
            {
                ShowAlert("Warning", "Your cart is empty.", "warning");
                return RedirectToAction("Cart");
            }

            var addresses = await _userAddressService.GetUserAddressesAsync(userId);

            var model = new CheckoutViewModel
            {
                CartItems = cartItems.Select(ci => new CartItemViewModel
                {
                    SalesProductId = ci.SalesProductId,
                    ProductName = ci.SalesProduct.Name,
                    ProductUrl = ci.SalesProduct.Url,
                    ImageUrl = ci.SalesProduct.Images.FirstOrDefault()?.ImagePath,
                    UnitPrice = ci.SalesProduct.SalePrice,
                    Quantity = ci.Quantity,
                    Total = ci.Quantity * ci.SalesProduct.SalePrice
                }).ToList(),
                Total = cartItems.Sum(x => x.Quantity * x.SalesProduct.SalePrice),
                SavedAddresses = addresses
            };

            return View(model);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            var userId = GetUserId();
            var cartItems = await _cartService.GetUserCartAsync(userId);
            if (!cartItems.Any())
            {
                ShowAlert("Warning", "Your cart is empty.", "warning");
                return RedirectToAction("Cart");
            }

            var shippingAddress = await ResolveAddressAsync(model.SelectedShippingAddressId, model.CustomShippingAddress, userId, "shipping");
            if (shippingAddress == null) return RedirectToAction("Checkout");

            var billingAddress = await ResolveAddressAsync(model.SelectedBillingAddressId, model.CustomBillingAddress, userId, "billing");
            if (billingAddress == null) return RedirectToAction("Checkout");

            var order = new Order
            {
                UserId = userId,
                CreatedAt = DateTime.Now,
                Status = OrderStatus.Pending,
                TotalPrice = cartItems.Sum(x => x.SalesProduct.SalePrice * x.Quantity),
                OrderItems = cartItems.Select(x => new OrderItem
                {
                    SalesProductId = x.SalesProductId,
                    Quantity = x.Quantity,
                    UnitPrice = x.SalesProduct.SalePrice
                }).ToList(),
                ShippingAddress = shippingAddress,
                BillingAddress = billingAddress
            };

            await _orderService.SaveOrderAsync(order);
            await _cartService.ClearCartAsync(userId);

            return RedirectToAction("Checkout", "Payment", new { orderId = order.Id });
        }

        [Authorize]
        public async Task<IActionResult> MyOrders()
        {
            var userId = GetUserId();
            var orders = await _orderService.GetUserOrdersAsync(userId);
            return View(orders);
        }

        [Authorize]
        public async Task<IActionResult> OrderDetail(int id)
        {
            var userId = GetUserId();
            var order = await _orderService.GetByIdAsync(id);
            if (order == null || order.UserId != userId)
            {
                ShowAlert("Warning", "Order not found.", "warning");
                return RedirectToAction("MyOrders");
            }

            return View(order);
        }

        [Authorize]
        public async Task<IActionResult> Wishlist()
        {
            var userId = GetUserId();
            var items = await _wishlistService.GetUserWishlistAsync(userId);
            return View(items);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddToWishlist(int id)
        {
            var userId = GetUserId();
            await _wishlistService.AddToWishlistAsync(userId, id);
            ShowAlert("Success", "Product added to wishlist.", "success");
            return RedirectToAction("Wishlist");
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> RemoveFromWishlist(int id)
        {
            var userId = GetUserId();
            await _wishlistService.RemoveFromWishlistAsync(userId, id);
            ShowAlert("Info", "Product removed from wishlist.", "info");
            return RedirectToAction("Wishlist");
        }

        // ----------------------------
        // Helpers
        // ----------------------------
        private string? GetUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        private async Task<OrderAddress?> ResolveAddressAsync(int? selectedId, AddressInputModel? customAddress, string userId, string type)
        {
            if (selectedId == -1)
            {
                if (customAddress == null || string.IsNullOrWhiteSpace(customAddress.FullAddress))
                {
                    ShowAlert("Error", $"Please provide a {type} address.", "danger");
                    return null;
                }

                return new OrderAddress
                {
                    FirstName = customAddress.FirstName,
                    LastName = customAddress.LastName,
                    Country = customAddress.Country,
                    City = customAddress.City,
                    District = customAddress.District,
                    FullAddress = customAddress.FullAddress
                };
            }

            var saved = await _userAddressService.GetByIdAsync(selectedId!.Value, userId);
            if (saved == null)
            {
                ShowAlert("Error", $"Saved {type} address not found.", "danger");
                return null;
            }

            return new OrderAddress
            {
                FirstName = saved.FirstName,
                LastName = saved.LastName,
                Country = saved.Country,
                City = saved.City,
                District = saved.District,
                FullAddress = saved.FullAddress
            };
        }
    }
}
