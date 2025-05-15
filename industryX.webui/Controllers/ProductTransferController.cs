using IndustryX.Application.Interfaces;
using IndustryX.Application.Services.Interfaces;
using IndustryX.Domain.Entities;
using IndustryX.WebUI.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IndustryX.WebUI.Controllers
{
    [Authorize(Roles = "Admin,Driver,WarehouseManager")]
    public class ProductTransferController : BaseController
    {
        private readonly IProductTransferService _transferService;
        private readonly IProductService _productService;
        private readonly IWarehouseService _warehouseService;

        public ProductTransferController(
            IProductTransferService transferService,
            IProductService productService,
            IWarehouseService warehouseService)
        {
            _transferService = transferService;
            _productService = productService;
            _warehouseService = warehouseService;
        }

        // ============================
        // REPORT (Admin)
        // ============================

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Report()
        {
            var warehouses = await _warehouseService.GetAllAsync();
            var transfers = await _transferService.GetFilteredAsync(null, null, null, null, null);

            return View(new ProductTransferFilterViewModel
            {
                Warehouses = warehouses.ToList(),
                Transfers = transfers.ToList()
            });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
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

        // ============================
        // LIST (WarehouseManager)
        // ============================

        [Authorize(Roles = "WarehouseManager")]
        public async Task<IActionResult> Index()
        {
            var transfers = await _transferService.GetAllAsync();
            return View(transfers);
        }

        // ============================
        // CREATE (WarehouseManager)
        // ============================

        [Authorize(Roles = "WarehouseManager")]
        public async Task<IActionResult> Create()
        {
            return View(await CreateFormModelAsync());
        }

        [HttpPost]
        [Authorize(Roles = "WarehouseManager")]
        public async Task<IActionResult> Create(ProductTransferFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(await CreateFormModelAsync(model));
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var (success, error) = await _transferService.CreateAsync(
                model.SourceWarehouseId,
                model.DestinationWarehouseId,
                model.ProductId,
                model.TransferQuantityBox,
                userId);

            if (!success)
            {
                ShowAlert("Error", error!, "danger");
                return View(await CreateFormModelAsync(model));
            }

            ShowAlert("Success", "Transfer successfully created.", "success");
            return RedirectToAction(nameof(Index));
        }

        private async Task<ProductTransferFormViewModel> CreateFormModelAsync(ProductTransferFormViewModel? model = null)
        {
            return new ProductTransferFormViewModel
            {
                ProductId = model?.ProductId ?? 0,
                SourceWarehouseId = model?.SourceWarehouseId ?? 0,
                DestinationWarehouseId = model?.DestinationWarehouseId ?? 0,
                TransferQuantityBox = model?.TransferQuantityBox ?? 0,
                Products = (await _productService.GetAllAsync()).ToList(),
                Warehouses = (await _warehouseService.GetAllAsync()).ToList()
            };
        }

        // ============================
        // BARCODE SCAN (Driver + Manager)
        // ============================

        [Authorize(Roles = "WarehouseManager, Driver")]
        public IActionResult ScanTransfer() => View();

        [HttpPost]
        [Authorize(Roles = "WarehouseManager, Driver")]
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

        // ============================
        // ACCEPT TRANSFER (Driver)
        // ============================

        [Authorize(Roles = "Driver")]
        public async Task<IActionResult> AcceptTransfer(string? barcode)
        {
            if (string.IsNullOrWhiteSpace(barcode))
                return View(new AcceptTransferViewModel());

            var transfer = await _transferService.GetByBarcodeAsync(barcode.Trim());
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

        [HttpPost]
        [Authorize(Roles = "Driver")]
        public async Task<IActionResult> AcceptTransfer(AcceptTransferViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var (success, error) = await _transferService.AcceptTransferAsync(
                model.TransferBarcode.Trim(),
                model.DeliveredBoxCount,
                userId);

            if (!success)
            {
                ShowAlert("Error", error!, "danger");
                return View(model);
            }

            ShowAlert("Success", "Transfer accepted.", "success");
            return RedirectToAction(nameof(Index));
        }

        // ============================
        // COMPLETE TRANSFER (WarehouseManager)
        // ============================

        [Authorize(Roles = "WarehouseManager")]
        public async Task<IActionResult> CompleteTransfer(string? barcode)
        {
            if (string.IsNullOrWhiteSpace(barcode))
                return View(new CompleteTransferViewModel());

            var transfer = await _transferService.GetByBarcodeAsync(barcode.Trim());
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

        [HttpPost]
        [Authorize(Roles = "WarehouseManager")]
        public async Task<IActionResult> CompleteTransfer(CompleteTransferViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var (success, error) = await _transferService.CompleteTransferAsync(
                model.Barcode.Trim(),
                model.ReceivedBoxCount,
                userId);

            if (!success)
            {
                ShowAlert("Error", error!, "danger");
                return View(model);
            }

            ShowAlert("Success", "Transfer completed and stock updated.", "success");
            return RedirectToAction(nameof(Index));
        }
    }
}
