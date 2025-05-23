using IndustryX.Application.Interfaces;
using IndustryX.Domain.Entities;
using IndustryX.Persistence.Contexts;
using IndustryX.WebUI.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IndustryX.WebUI.Controllers
{
    [Authorize(Roles = "Admin,SalesManager")]
    public class SalesProductController : BaseController
    {
        private readonly ISalesProductService _salesProductService;
        private readonly ICategoryService _categoryService;
        private readonly IProductService _productService;
        private readonly IndustryXDbContext _context;

        public SalesProductController(
            ISalesProductService salesProductService,
            ICategoryService categoryService,
            IProductService productService,
            IndustryXDbContext context)
        {
            _salesProductService = salesProductService;
            _categoryService = categoryService;
            _productService = productService;
            _context = context;
        }

        // ----------------------- List -----------------------
        public async Task<IActionResult> Index()
        {
            var products = await _salesProductService.GetAllAsync();
            return View(products);
        }

        // ---------------------- Create ----------------------
        public async Task<IActionResult> Create()
        {
            var model = await BuildCreateViewModelAsync();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(SalesProductCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadCategoriesAndProducts(model);
                return View(model);
            }

            var salesProduct = new SalesProduct
            {
                Name = model.Name,
                Url = model.Url,
                SalePrice = model.SalePrice,
                Description = model.Description,
                IsActive = model.IsActive,
                IsPopular = model.IsPopular,
                IsBestSeller = model.IsBestSeller,
                ProductId = model.ProductId,
                SalesProductCategories = model.SelectedCategoryIds
                    .Select(id => new SalesProductCategory { CategoryId = id }).ToList(),
                Images = new List<SalesProductImage>()
            };

            await UploadImagesAsync(model.Images, salesProduct.Images);

            var result = await _salesProductService.CreateAsync(salesProduct);
            if (!result.Success)
            {
                ModelState.AddModelError("", result.Error!);
                await LoadCategoriesAndProducts(model);
                return View(model);
            }

            await _salesProductService.InitializeSalesProductStocksAsync(salesProduct.Id);
            ShowAlert("Success", "Sales product created successfully.", "success");

            return RedirectToAction(nameof(Index));
        }

        // ----------------------- Edit -----------------------
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _salesProductService.GetByIdAsync(id);
            if (product == null)
            {
                ShowAlert("Error", "Sales product not found.", "danger");
                return RedirectToAction(nameof(Index));
            }

            var model = await BuildEditViewModelAsync(product);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(SalesProductEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadCategoriesAndProducts(model);
                return View(model);
            }

            var product = await _salesProductService.GetByIdAsync(model.Id);
            if (product == null)
            {
                ShowAlert("Error", "Sales product not found.", "danger");
                return RedirectToAction(nameof(Index));
            }

            product.Name = model.Name;
            product.Url = model.Url;
            product.SalePrice = model.SalePrice;
            product.Description = model.Description;
            product.IsActive = model.IsActive;
            product.IsPopular = model.IsPopular;
            product.IsBestSeller = model.IsBestSeller;
            product.ProductId = model.ProductId;

            product.SalesProductCategories.Clear();
            model.SelectedCategoryIds.ForEach(id =>
                product.SalesProductCategories.Add(new SalesProductCategory { SalesProductId = model.Id, CategoryId = id }));

            await UploadImagesAsync(model.NewImages, product.Images);

            var result = await _salesProductService.UpdateAsync(product);
            if (!result.Success)
            {
                ModelState.AddModelError("", result.Error!);
                await LoadCategoriesAndProducts(model);
                return View(model);
            }

            ShowAlert("Success", "Sales product updated successfully.", "success");
            return RedirectToAction(nameof(Index));
        }

        // ---------------------- Delete ----------------------
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _salesProductService.DeleteAsync(id);
            ShowAlert(result ? "Success" : "Error",
                      result ? "Sales product deleted." : "Could not delete sales product.",
                      result ? "success" : "danger");

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteImage(int imageId)
        {
            var image = await _context.SalesProductImages
                .Include(i => i.SalesProduct)
                .FirstOrDefaultAsync(i => i.Id == imageId);

            if (image == null)
            {
                ShowAlert("Error", "Image not found.", "danger");
                return RedirectToAction(nameof(Index));
            }

            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", image.ImagePath.TrimStart('/'));
            if (System.IO.File.Exists(fullPath))
                System.IO.File.Delete(fullPath);

            _context.SalesProductImages.Remove(image);
            await _context.SaveChangesAsync();

            ShowAlert("Success", "Image deleted successfully.", "success");
            return RedirectToAction(nameof(Edit), new { id = image.SalesProductId });
        }

        // --------------------- Helpers ----------------------

        private async Task UploadImagesAsync(List<IFormFile>? files, ICollection<SalesProductImage> imageList)
        {
            if (files == null || !files.Any()) return;

            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/sales");
            Directory.CreateDirectory(uploadPath);

            foreach (var file in files.Where(f => f.Length > 0))
            {
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                    await file.CopyToAsync(stream);

                imageList.Add(new SalesProductImage { ImagePath = $"/uploads/sales/{fileName}" });
            }
        }

        private async Task<SalesProductCreateViewModel> BuildCreateViewModelAsync()
        {
            var categories = await _categoryService.GetAllAsync();
            var products = await _productService.GetAllAsync();

            return new SalesProductCreateViewModel
            {
                Categories = categories.Select(c => new CategoryItem { Id = c.Id, Name = c.Name }).ToList(),
                Products = products.Select(p => new ProductItem
                {
                    Id = p.Id,
                    DisplayName = $"{p.Name} - {p.Barcode}"
                }).ToList()
            };
        }

        private async Task<SalesProductEditViewModel> BuildEditViewModelAsync(SalesProduct salesProduct)
        {
            var categories = await _categoryService.GetAllAsync();
            var products = await _productService.GetAllAsync();

            return new SalesProductEditViewModel
            {
                Id = salesProduct.Id,
                Name = salesProduct.Name,
                Url = salesProduct.Url,
                SalePrice = salesProduct.SalePrice,
                Description = salesProduct.Description,
                IsActive = salesProduct.IsActive,
                IsPopular = salesProduct.IsPopular,
                IsBestSeller = salesProduct.IsBestSeller,
                ProductId = salesProduct.ProductId,
                SelectedCategoryIds = salesProduct.SalesProductCategories.Select(c => c.CategoryId).ToList(),
                ExistingImages = salesProduct.Images.Select(i => new ExistingImageItem
                {
                    Id = i.Id,
                    ImagePath = i.ImagePath
                }).ToList(),
                Categories = categories.Select(c => new CategoryItem
                {
                    Id = c.Id,
                    Name = c.Name,
                    IsSelected = salesProduct.SalesProductCategories.Any(spc => spc.CategoryId == c.Id)
                }).ToList(),
                Products = products.Select(p => new ProductItem
                {
                    Id = p.Id,
                    DisplayName = $"{p.Name} - {p.Barcode}"
                }).ToList()
            };
        }

        private async Task LoadCategoriesAndProducts(SalesProductCreateViewModel model)
        {
            var categories = await _categoryService.GetAllAsync();
            var products = await _productService.GetAllAsync();

            model.Categories = categories.Select(c => new CategoryItem
            {
                Id = c.Id,
                Name = c.Name,
                IsSelected = model.SelectedCategoryIds.Contains(c.Id)
            }).ToList();

            model.Products = products.Select(p => new ProductItem
            {
                Id = p.Id,
                DisplayName = $"{p.Name} - {p.Barcode}"
            }).ToList();
        }

        private async Task LoadCategoriesAndProducts(SalesProductEditViewModel model)
        {
            var categories = await _categoryService.GetAllAsync();
            var products = await _productService.GetAllAsync();

            model.Categories = categories.Select(c => new CategoryItem
            {
                Id = c.Id,
                Name = c.Name,
                IsSelected = model.SelectedCategoryIds.Contains(c.Id)
            }).ToList();

            model.Products = products.Select(p => new ProductItem
            {
                Id = p.Id,
                DisplayName = $"{p.Name} - {p.Barcode}"
            }).ToList();
        }
    }
}
