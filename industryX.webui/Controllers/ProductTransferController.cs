using IndustryX.Application.Interfaces;
using IndustryX.Application.Services.Interfaces;
using IndustryX.Domain.Entities;
using IndustryX.WebUI.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IndustryX.WebUI.Controllers
{
    public class ProductTransferController : BaseController
    {
        private readonly IProductTransferService _transferService;
        private readonly IProductService _productService;
        private readonly IWarehouseService _warehouseService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProductTransferController(
            IProductTransferService transferService,
            IProductService productService,
            IWarehouseService warehouseService,
            UserManager<ApplicationUser> userManager)
        {
            _transferService = transferService;
            _productService = productService;
            _warehouseService = warehouseService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new ProductTransferFormViewModel
            {
                Products = (await _productService.GetAllAsync()).ToList(),
                Warehouses = (await _warehouseService.GetAllAsync()).ToList()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ProductTransferFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Products = (await _productService.GetAllAsync()).ToList();
                model.Warehouses = (await _warehouseService.GetAllAsync()).ToList();
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                ShowAlert("Error", "User not found.", "danger");
                return RedirectToAction("Create");
            }

            var transfer = new ProductTransfer
            {
                ProductId = model.ProductId,
                SourceWarehouseId = model.SourceWarehouseId,
                DestinationWarehouseId = model.DestinationWarehouseId,
                TransferQuantityBox = model.TransferQuantityBox,
                InitiatedByUserId = user.Id,
                CreatedAt = DateTime.Now,
                TransferBarcode = Guid.NewGuid().ToString().Substring(0, 8).ToUpper()
            };

            var (success, error) = await _transferService.CreateTransferAsync(transfer);
            if (!success)
            {
                ShowAlert("Error", error ?? "Failed to create transfer.", "danger");
                model.Products = (await _productService.GetAllAsync()).ToList();
                model.Warehouses = (await _warehouseService.GetAllAsync()).ToList();
                return View(model);
            }

            ShowAlert("Transfer Created", "Product transfer has been initiated.", "success");
            return RedirectToAction("Index");
        }
    }
}
