using System.Security.Claims;
using IndustryX.Application.Interfaces;
using IndustryX.Domain.Entities;
using IndustryX.WebUI.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IndustryX.WebUI.Controllers
{
    [Authorize(Roles = "Admin,Driver,WarehouseManager")]
    public class ProductTransferController : BaseController
    {
        private readonly IProductTransferService _transferService;
        private readonly IProductService _productService;
        private readonly IWarehouseService _warehouseService;
        private readonly IUserService _userService;

        public ProductTransferController(
            IProductTransferService transferService,
            IProductService productService,
            IWarehouseService warehouseService,
            IUserService userService)
        {
            _transferService = transferService;
            _productService = productService;
            _warehouseService = warehouseService;
            _userService = userService;
        }

        // ----------- Admin - Report & Direct Transfer -----------

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Report()
        {
            var viewModel = new ProductTransferFilterViewModel
            {
                Warehouses = (await _warehouseService.GetAllAsync()).ToList(),
                Transfers = (await _transferService.GetFilteredAsync(null, null, null, null, null)).ToList()
            };
            return View(viewModel);
        }

        [HttpPost, Authorize(Roles = "Admin")]
        public async Task<IActionResult> Report(ProductTransferFilterViewModel model)
        {
            model.Warehouses = (await _warehouseService.GetAllAsync()).ToList();
            model.Transfers = (await _transferService.GetFilteredAsync(
                model.SourceWarehouseId,
                model.DestinationWarehouseId,
                model.Status,
                model.StartDate,
                model.EndDate)).ToList();

            return View(model);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Detail(int id)
        {
            var (transfer, deficits) = await _transferService.GetDetailAsync(id);
            if (transfer == null)
            {
                ShowAlert("Error", "Transfer not found.", "danger");
                return RedirectToAction(nameof(Report));
            }

            return View(new ProductTransferDetailViewModel
            {
                Transfer = transfer,
                Deficits = deficits
            });
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DirectTransfer()
        {
            var model = new ProductTransferFormViewModel
            {
                Products = (await _productService.GetAllAsync()).ToList(),
                Warehouses = (await _warehouseService.GetAllAsync()).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DirectTransfer(ProductTransferFormViewModel model)
        {
            if (!ModelState.IsValid || model.SourceWarehouseId == model.DestinationWarehouseId)
            {
                ShowAlert("Error", "Invalid transfer parameters.", "danger");
                model.Products = (await _productService.GetAllAsync()).ToList();
                model.Warehouses = (await _warehouseService.GetAllAsync()).ToList();
                return View(model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var (success, error) = await _transferService.DirectTransferAsync(
                model.SourceWarehouseId,
                model.DestinationWarehouseId,
                model.ProductId,
                model.TransferQuantityBox,
                userId);

            if (!success)
            {
                ShowAlert("Error", error!, "danger");
                model.Products = (await _productService.GetAllAsync()).ToList();
                model.Warehouses = (await _warehouseService.GetAllAsync()).ToList();
                return View(model);
            }

            ShowAlert("Success", "Stock transferred directly.", "success");
            return RedirectToAction(nameof(Report));
        }

        
        // ----------- WarehouseManager - Create & View -----------

        [Authorize(Roles = "WarehouseManager")]
        public async Task<IActionResult> Index()
        {
            var transfers = await _transferService.GetAllAsync();
            return View(transfers);
        }

        [Authorize(Roles = "WarehouseManager")]
        public async Task<IActionResult> Create()
        {
            var warehouseId = await GetCurrentUserWarehouseIdAsync();
            if (warehouseId == null)
            {
                ShowAlert("Error", "You don't have an assigned warehouse.", "danger");
                return RedirectToAction(nameof(Index));
            }

            return View(await CreateFormModelAsync(warehouseId.Value));
        }

        [HttpPost, Authorize(Roles = "WarehouseManager")]
        public async Task<IActionResult> Create(ProductTransferFormViewModel model)
        {
            var warehouseId = await GetCurrentUserWarehouseIdAsync();
            if (warehouseId == null)
            {
                ShowAlert("Error", "You don't have an assigned warehouse.", "danger");
                return RedirectToAction(nameof(Index));
            }

            model.SourceWarehouseId = warehouseId.Value;

            if (!ModelState.IsValid)
                return View(await CreateFormModelAsync(warehouseId.Value, model));

            var (success, error) = await _transferService.CreateAsync(
                model.SourceWarehouseId,
                model.DestinationWarehouseId,
                model.ProductId,
                model.TransferQuantityBox,
                GetUserId());

            if (!success)
            {
                ShowAlert("Error", error!, "danger");
                return View(await CreateFormModelAsync(warehouseId.Value, model));
            }

            ShowAlert("Success", "Transfer successfully created.", "success");
            return RedirectToAction(nameof(Index));
        }

        
        // ----------- Barcode Flow - Scan, Accept, Complete -----------

        [Authorize(Roles = "WarehouseManager, Driver")]
        public IActionResult ScanTransfer() => View();

        [HttpPost, Authorize(Roles = "WarehouseManager, Driver")]
        public async Task<IActionResult> ScanTransfer(string barcode)
        {
            if (string.IsNullOrWhiteSpace(barcode))
            {
                ShowAlert("Error", "Please enter a barcode.", "danger");
                return View();
            }

            var transfer = await _transferService.GetByBarcodeAsync(barcode.Trim());
            if (transfer == null)
            {
                ShowAlert("Error", "Transfer not found.", "danger");
                return View();
            }

            if (transfer.Status == TransferStatus.Created && User.IsInRole("Driver"))
                return RedirectToAction(nameof(AcceptTransfer), new { barcode });

            if (transfer.Status == TransferStatus.InTransit && User.IsInRole("WarehouseManager"))
                return RedirectToAction(nameof(CompleteTransfer), new { barcode });

            ShowAlert("Unauthorized", "No permission or invalid status.", "danger");
            return View();
        }

        [Authorize(Roles = "Driver")]
        public async Task<IActionResult> AcceptTransfer(string? barcode)
        {
            var transfer = await _transferService.GetByBarcodeAsync(barcode?.Trim());
            if (transfer == null || transfer.Status != TransferStatus.Created)
            {
                ShowAlert("Error", "No valid transfer found.", "danger");
                return View(new AcceptTransferViewModel());
            }

            return View(new AcceptTransferViewModel
            {
                TransferBarcode = transfer.TransferBarcode,
                ProductName = transfer.Product.Name,
                QuantityBox = transfer.TransferQuantityBox,
                SourceWarehouse = transfer.SourceWarehouse.Name,
                DestinationWarehouse = transfer.DestinationWarehouse.Name
            });
        }

        [HttpPost, Authorize(Roles = "Driver")]
        public async Task<IActionResult> AcceptTransfer(AcceptTransferViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var (success, error) = await _transferService.AcceptTransferAsync(
                model.TransferBarcode.Trim(),
                model.DeliveredBoxCount,
                GetUserId());

            if (!success)
            {
                ShowAlert("Error", error!, "danger");
                return View(model);
            }

            ShowAlert("Success", "Transfer accepted.", "success");
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "WarehouseManager")]
        public async Task<IActionResult> CompleteTransfer(string? barcode)
        {
            var transfer = await _transferService.GetByBarcodeAsync(barcode?.Trim());
            if (transfer == null || transfer.Status != TransferStatus.InTransit)
            {
                ShowAlert("Error", "No valid transfer found.", "danger");
                return View(new CompleteTransferViewModel());
            }

            return View(new CompleteTransferViewModel
            {
                Barcode = transfer.TransferBarcode,
                ProductName = transfer.Product.Name,
                TotalBoxCount = transfer.TransferQuantityBox,
                SourceWarehouse = transfer.SourceWarehouse.Name,
                DestinationWarehouse = transfer.DestinationWarehouse.Name
            });
        }

        [HttpPost, Authorize(Roles = "WarehouseManager")]
        public async Task<IActionResult> CompleteTransfer(CompleteTransferViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var (success, error) = await _transferService.CompleteTransferAsync(
                model.Barcode.Trim(),
                model.ReceivedBoxCount,
                GetUserId());

            if (!success)
            {
                ShowAlert("Error", error!, "danger");
                return View(model);
            }

            ShowAlert("Success", "Transfer completed and stock updated.", "success");
            return RedirectToAction(nameof(Index));
        }

        
        // ----------- Helpers -----------

        private async Task<ProductTransferFormViewModel> CreateFormModelAsync(int? excludeWarehouseId = null, ProductTransferFormViewModel? model = null)
        {
            var warehouses = (await _warehouseService.GetAllAsync()).ToList();
            var filteredWarehouses = excludeWarehouseId.HasValue
                ? warehouses.Where(w => w.Id != excludeWarehouseId.Value).ToList()
                : warehouses;

            return new ProductTransferFormViewModel
            {
                ProductId = model?.ProductId ?? 0,
                SourceWarehouseId = model?.SourceWarehouseId ?? 0,
                DestinationWarehouseId = model?.DestinationWarehouseId ?? 0,
                TransferQuantityBox = model?.TransferQuantityBox ?? 0,
                Products = (await _productService.GetAllAsync()).ToList(),
                Warehouses = filteredWarehouses
            };
        }

        private async Task<int?> GetCurrentUserWarehouseIdAsync()
        {
            var user = await _userService.GetByIdAsync(GetUserId());
            return user?.WarehouseId;
        }

        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        }
    }
}
