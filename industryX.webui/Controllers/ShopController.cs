using System.Security.Claims;
using IndustryX.Application.Interfaces;
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

        public ShopController(
            ICartService cartService,
            IOrderService orderService,
            IWishlistService wishlistService,
            ISalesProductService salesProductService)
        {
            _cartService = cartService;
            _orderService = orderService;
            _wishlistService = wishlistService;
            _salesProductService = salesProductService;
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            return View(); // Ana sayfa
        }

        // ----------------------------
        // PRODUCT DETAILS
        // ----------------------------
        [AllowAnonymous]
        [HttpGet("productdetail/{url}")]
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
                ImagePaths = product.Images.Select(i => i.ImagePath).ToList(),
                Url = product.Url
            };

            return View(model);
        }

        // ----------------------------
        // CART
        // ----------------------------
        [Authorize]
        [HttpGet("cart")]
        public async Task<IActionResult> Cart()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var cartItems = await _cartService.GetUserCartAsync(userId);

            var model = cartItems.Select(ci => new CartItemViewModel
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
        [HttpPost("cart/remove/{id}")]
        public async Task<IActionResult> RemoveFromCart(int id)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            await _cartService.RemoveFromCartAsync(userId, id);
            ShowAlert("Removed", "Item removed from cart.", "info");
            return RedirectToAction("Cart");
        }

        [Authorize]
        [HttpPost("cart/add/{id}")]
        public async Task<IActionResult> AddToCart(int id)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            await _cartService.AddToCartAsync(userId, id);
            ShowAlert("Success", "Item added to cart.", "success");
            return RedirectToAction("Cart");
        }

        // ----------------------------
        // CHECKOUT
        // ----------------------------
        [Authorize]
        [HttpGet("checkout")]
        public async Task<IActionResult> Checkout()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var result = await _orderService.CreateOrderFromCartAsync(userId);
            if (!result.Success)
            {
                ShowAlert("Warning", result.Error ?? "Could not complete checkout.", "warning");
                return RedirectToAction("Cart");
            }

            ShowAlert("Success", "Your order has been placed.", "success");
            return RedirectToAction("Complete");
        }

        [Authorize]
        [HttpGet("complete")]
        public IActionResult Complete()
        {
            return View();
        }

        // ----------------------------
        // MY ORDERS
        // ----------------------------
        [Authorize]
        [HttpGet("my-orders")]
        public async Task<IActionResult> MyOrders()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var orders = await _orderService.GetUserOrdersAsync(userId);
            return View(orders);
        }

        // ----------------------------
        // WISHLIST
        // ----------------------------
        [Authorize]
        [HttpGet("wishlist")]
        public async Task<IActionResult> Wishlist()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var items = await _wishlistService.GetUserWishlistAsync(userId);
            return View(items);
        }

        [Authorize]
        [HttpPost("wishlist/add/{id}")]
        public async Task<IActionResult> AddToWishlist(int id)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            await _wishlistService.AddToWishlistAsync(userId, id);
            ShowAlert("Success", "Product added to your wishlist.", "success");
            return RedirectToAction("Wishlist");
        }

        [Authorize]
        [HttpPost("wishlist/remove/{id}")]
        public async Task<IActionResult> RemoveFromWishlist(int id)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            await _wishlistService.RemoveFromWishlistAsync(userId, id);
            ShowAlert("Success", "Product removed from your wishlist.", "info");
            return RedirectToAction("Wishlist");
        }

        // ----------------------------
        // HELPERS
        // ----------------------------
        private string? GetUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}
