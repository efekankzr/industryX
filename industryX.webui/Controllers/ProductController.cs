using IndustryX.Application.Interfaces;
using IndustryX.Domain.Entities;
using IndustryX.WebUI.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IndustryX.WebUI.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ProductController : BaseController
    {
        private readonly IProductService _productService;
        private readonly IRawMaterialService _rawMaterialService;

        public ProductController(
            IProductService productService,
            IRawMaterialService rawMaterialService)
        {
            _productService = productService;
            _rawMaterialService = rawMaterialService;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _productService.GetAllAsync();
            return View(products);
        }

        public async Task<IActionResult> Create()
        {
            var rawMaterials = await _rawMaterialService.GetAllAsync();

            if (!rawMaterials.Any())
            {
                ShowAlert("Warning", "You must add at least one raw material before creating a product.", "warning");
                return RedirectToAction("Index", "RawMaterial");
            }

            var model = new ProductCreateViewModel
            {
                RawMaterials = rawMaterials.Select(rm => new RawMaterialInputViewModel
                {
                    RawMaterialId = rm.Id,
                    RawMaterialName = rm.Name
                }).ToList()
            };

            return View(model);
        }
        
        [HttpPost]
        public async Task<IActionResult> Create(ProductCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadRawMaterialsIntoModel(model);
                return View(model);
            }

            var product = MapToProductEntity(model);

            var (success, error) = await _productService.CreateAsync(product);
            if (!success)
            {
                ShowAlert("Error", error!, "danger");
                await LoadRawMaterialsIntoModel(model);
                return View(model);
            }

            ShowAlert("Product Created", "Product successfully created.", "success");
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null)
            {
                ShowAlert("Not Found", "Product not found.", "danger");
                return RedirectToAction(nameof(Index));
            }

            var allMaterials = await _rawMaterialService.GetAllAsync();
            var viewModel = new ProductEditViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Barcode = product.Barcode,
                PiecesInBox = product.PiecesInBox,
                MaterialPrice = product.MaterialPrice,
                RawMaterials = allMaterials.Select(rm =>
                {
                    var receipt = product.ProductReceipts.FirstOrDefault(r => r.RawMaterialId == rm.Id);
                    return new RawMaterialInputViewModel
                    {
                        RawMaterialId = rm.Id,
                        RawMaterialName = rm.Name,
                        Include = receipt != null,
                        Quantity = receipt?.Quantity
                    };
                }).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(ProductEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadRawMaterialsIntoModel(model);
                return View(model);
            }

            var product = MapToProductEntity(model);

            var (success, error) = await _productService.UpdateAsync(product);
            if (!success)
            {
                ShowAlert("Error", error!, "danger");
                await LoadRawMaterialsIntoModel(model);
                return View(model);
            }

            ShowAlert("Updated", "Product successfully updated.", "success");
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null)
            {
                ShowAlert("Error", "Product not found.", "danger");
                return RedirectToAction(nameof(Index));
            }

            await _productService.DeleteAsync(id);
            ShowAlert("Deleted", "Product deleted successfully.", "warning");
            return RedirectToAction(nameof(Index));
        }

        // ---------------------------------------
        // Helpers
        // ---------------------------------------

        private async Task LoadRawMaterialsIntoModel(ProductBaseViewModel model)
        {
            var rawMaterials = await _rawMaterialService.GetAllAsync();
            model.RawMaterials = rawMaterials.Select(rm =>
            {
                var existing = model.RawMaterials.FirstOrDefault(x => x.RawMaterialId == rm.Id);
                return new RawMaterialInputViewModel
                {
                    RawMaterialId = rm.Id,
                    RawMaterialName = rm.Name,
                    Include = existing?.Include ?? false,
                    Quantity = existing?.Quantity
                };
            }).ToList();
        }

        private Product MapToProductEntity(ProductBaseViewModel model)
        {
            return new Product
            {
                Id = (model is ProductEditViewModel edit) ? edit.Id : 0,
                Name = model.Name,
                Barcode = model.Barcode,
                PiecesInBox = model.PiecesInBox,
                ProductReceipts = model.RawMaterials
                    .Where(r => r.Include && r.Quantity.HasValue)
                    .Select(r => new ProductReceipt
                    {
                        RawMaterialId = r.RawMaterialId,
                        Quantity = r.Quantity!.Value,
                        Include = true
                    }).ToList()
            };
        }
    }
}
