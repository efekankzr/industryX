using IndustryX.Application.Interfaces;
using IndustryX.Application.Services.Interfaces;
using IndustryX.Domain.Entities;
using IndustryX.WebUI.Models;
using Microsoft.AspNetCore.Mvc;

namespace IndustryX.WebUI.Controllers
{
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
            var vm = new ProductFormViewModel
            {
                Product = new Product(),
                RawMaterials = (await _rawMaterialService.GetAllAsync()).ToList()
            };
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ProductFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.RawMaterials = (await _rawMaterialService.GetAllAsync()).ToList();
                return View(vm);
            }

            var (success, error) = await _productService.CreateAsync(vm.Product);
            if (!success)
            {
                ShowAlert("Error", error!, "danger");
                vm.RawMaterials = (await _rawMaterialService.GetAllAsync()).ToList();
                return View(vm);
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

            foreach (var receipt in product.ProductReceipts)
            {
                receipt.Product = null;
                receipt.RawMaterial?.ProductReceipts?.Clear();
            }

            var vm = new ProductFormViewModel
            {
                Product = product,
                RawMaterials = (await _rawMaterialService.GetAllAsync()).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(ProductFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.RawMaterials = (await _rawMaterialService.GetAllAsync()).ToList();
                return View(vm);
            }

            var (success, error) = await _productService.UpdateAsync(vm.Product);
            if (!success)
            {
                ShowAlert("Error", error!, "danger");
                vm.RawMaterials = (await _rawMaterialService.GetAllAsync()).ToList();
                return View(vm);
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
    }
}
