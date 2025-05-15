using IndustryX.Application.Interfaces;
using IndustryX.Application.Services.Interfaces;
using IndustryX.WebUI.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IndustryX.WebUI.Controllers
{
    [Authorize]
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

        public async Task<IActionResult> Index()
        {
            var transfers = await _transferService.GetAllAsync();
            return View(transfers);
        }

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
            if (!User.IsInRole("WarehouseManager"))
            {
                ShowAlert("Unauthorized", "Only Warehouse Managers can create transfers.", "danger");
                return RedirectToAction("Index", "Home");
            }

            if (!ModelState.IsValid)
            {
                model.Products = (await _productService.GetAllAsync()).ToList();
                model.Warehouses = (await _warehouseService.GetAllAsync()).ToList();
                return View(model);
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
                ShowAlert("Error", error ?? "Transfer could not be created.", "danger");
                model.Products = (await _productService.GetAllAsync()).ToList();
                model.Warehouses = (await _warehouseService.GetAllAsync()).ToList();
                return View(model);
            }

            ShowAlert("Success", "Product transfer created and assigned to you.", "success");
            return RedirectToAction(nameof(Index));
        }

        public IActionResult AcceptTransfer()
        {
            return View(new AcceptTransferViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> AcceptTransfer(AcceptTransferViewModel model)
        {
            if (!User.IsInRole("Driver"))
            {
                ShowAlert("Unauthorized", "Only drivers can accept transfers.", "danger");
                return RedirectToAction("Index", "Home");
            }

            if (!ModelState.IsValid)
                return View(model);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var (success, error) = await _transferService.AcceptTransferAsync(
                model.TransferBarcode,
                model.DeliveredBoxCount,
                userId);

            if (!success)
            {
                ShowAlert("Error", error ?? "Transfer could not be accepted.", "danger");
                return View(model);
            }

            ShowAlert("Accepted", "Product transfer accepted successfully.", "success");
            return RedirectToAction(nameof(Index));
        }

        public IActionResult CompleteTransfer()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CompleteTransfer(string barcode, int receivedBoxCount)
        {
            if (!User.IsInRole("WarehouseManager"))
            {
                ShowAlert("Unauthorized", "Only warehouse managers can complete transfers.", "danger");
                return RedirectToAction("Index", "Home");
            }

            if (string.IsNullOrWhiteSpace(barcode) || receivedBoxCount <= 0)
            {
                ShowAlert("Error", "Please provide a valid barcode and box quantity.", "danger");
                return View();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var (success, error) = await _transferService.CompleteTransferAsync(barcode.Trim(), receivedBoxCount, userId);

            if (!success)
            {
                ShowAlert("Error", error ?? "Transfer could not be completed.", "danger");
                return View();
            }

            ShowAlert("Success", "Product transfer successfully completed and stock updated.", "success");
            return RedirectToAction(nameof(Index));
        }
    }
}
