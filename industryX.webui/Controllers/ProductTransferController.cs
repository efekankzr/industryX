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

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Detail(int id)
        {
            var (transfer, deficits) = await _transferService.GetDetailAsync(id);

            if (transfer == null)
            {
                ShowAlert("Error", "Transfer not found.", "danger");
                return RedirectToAction(nameof(Report));
            }

            var viewModel = new ProductTransferDetailViewModel
            {
                Transfer = transfer,
                Deficits = deficits
            };

            return View(viewModel);
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

        [Authorize(Roles = "WarehouseManager")]
        public async Task<IActionResult> Index()
        {
            var transfers = await _transferService.GetAllAsync();
            return View(transfers);
        }

        [Authorize(Roles = "WarehouseManager")]
        public async Task<IActionResult> Create()
        {
            var userWarehouseId = await GetCurrentUserWarehouseIdAsync();
            if (userWarehouseId == null)
            {
                ShowAlert("Error", "You don't have an assigned warehouse.", "danger");
                return RedirectToAction(nameof(Index));
            }

            return View(await CreateFormModelAsync(userWarehouseId.Value));
        }

        [HttpPost]
        [Authorize(Roles = "WarehouseManager")]
        public async Task<IActionResult> Create(ProductTransferFormViewModel model)
        {
            var userWarehouseId = await GetCurrentUserWarehouseIdAsync();
            if (userWarehouseId == null)
            {
                ShowAlert("Error", "You don't have an assigned warehouse.", "danger");
                return RedirectToAction(nameof(Index));
            }

            model.SourceWarehouseId = userWarehouseId.Value;

            if (!ModelState.IsValid)
            {
                return View(await CreateFormModelAsync(userWarehouseId.Value, model));
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
                return View(await CreateFormModelAsync(userWarehouseId.Value, model));
            }

            ShowAlert("Success", "Transfer successfully created.", "success");
            return RedirectToAction(nameof(Index));
        }

        private async Task<ProductTransferFormViewModel> CreateFormModelAsync(int userWarehouseId, ProductTransferFormViewModel? model = null)
        {
            var allWarehouses = (await _warehouseService.GetAllAsync()).ToList();
            var filteredWarehouses = allWarehouses
                .Where(w => w.Id != userWarehouseId)
                .ToList();

            return new ProductTransferFormViewModel
            {
                ProductId = model?.ProductId ?? 0,
                SourceWarehouseId = userWarehouseId,
                DestinationWarehouseId = model?.DestinationWarehouseId ?? 0,
                TransferQuantityBox = model?.TransferQuantityBox ?? 0,
                Products = (await _productService.GetAllAsync()).ToList(),
                Warehouses = filteredWarehouses
            };
        }

        private async Task<int?> GetCurrentUserWarehouseIdAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var user = await _userService.GetByIdAsync(userId);
            return user?.WarehouseId;
        }

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
