using IndustryX.Application.Interfaces;
using IndustryX.Domain.Entities;
using IndustryX.Persistence.Contexts;
using IndustryX.WebUI.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IndustryX.WebUI.Controllers
{
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

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var salesProducts = await _salesProductService.GetAllAsync();
            return View(salesProducts);
        }

        [HttpGet]
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
                    .Select(catId => new SalesProductCategory { CategoryId = catId })
                    .ToList(),
                Images = new List<SalesProductImage>()
            };

            // Resim Yükleme
            await UploadImagesAsync(model.Images, salesProduct.Images);

            var result = await _salesProductService.CreateAsync(salesProduct);
            if (!result.Success)
            {
                ModelState.AddModelError("", result.Error!);
                await LoadCategoriesAndProducts(model);
                return View(model);
            }

            ShowAlert("Success", "Sales product created successfully.", "success");
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var salesProduct = await _salesProductService.GetByIdAsync(id);
            if (salesProduct == null)
            {
                ShowAlert("Error", "Sales product not found.", "danger");
                return RedirectToAction("Index");
            }

            var categories = await _categoryService.GetAllAsync();
            var products = await _productService.GetAllAsync();

            var model = new SalesProductEditViewModel
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

            var salesProduct = await _salesProductService.GetByIdAsync(model.Id);
            if (salesProduct == null)
            {
                ShowAlert("Error", "Sales product not found.", "danger");
                return RedirectToAction("Index");
            }

            salesProduct.Name = model.Name;
            salesProduct.Url = model.Url;
            salesProduct.SalePrice = model.SalePrice;
            salesProduct.Description = model.Description;
            salesProduct.IsActive = model.IsActive;
            salesProduct.IsPopular = model.IsPopular;
            salesProduct.IsBestSeller = model.IsBestSeller;
            salesProduct.ProductId = model.ProductId;

            salesProduct.SalesProductCategories.Clear();
            foreach (var catId in model.SelectedCategoryIds)
            {
                salesProduct.SalesProductCategories.Add(new SalesProductCategory
                {
                    SalesProductId = model.Id,
                    CategoryId = catId
                });
            }

            await UploadImagesAsync(model.NewImages, salesProduct.Images);

            var result = await _salesProductService.UpdateAsync(salesProduct);
            if (!result.Success)
            {
                ModelState.AddModelError("", result.Error!);
                await LoadCategoriesAndProducts(model);
                return View(model);
            }

            ShowAlert("Success", "Sales product updated successfully.", "success");
            return RedirectToAction("Index");
        }

        [HttpPost("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _salesProductService.DeleteAsync(id);
            if (success)
                ShowAlert("Success", "Sales product deleted.", "success");
            else
                ShowAlert("Error", "Sales product not found or could not be deleted.", "danger");

            return RedirectToAction("Index");
        }

        [HttpPost("delete-image/{imageId}")]
        public async Task<IActionResult> DeleteImage(int imageId)
        {
            var image = await _context.SalesProductImages
                .Include(i => i.SalesProduct)
                .FirstOrDefaultAsync(i => i.Id == imageId);

            if (image == null)
            {
                ShowAlert("Error", "Image not found.", "danger");
                return RedirectToAction("Index");
            }

            var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", image.ImagePath.TrimStart('/'));
            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }

            _context.SalesProductImages.Remove(image);
            await _context.SaveChangesAsync();

            ShowAlert("Success", "Image deleted successfully.", "success");
            return RedirectToAction("Edit", new { id = image.SalesProductId });
        }

        // --- Helpers ---

        private async Task UploadImagesAsync(List<IFormFile>? files, ICollection<SalesProductImage> imageList)
        {
            if (files == null || !files.Any())
                return;

            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/sales");
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            foreach (var formFile in files)
            {
                if (formFile.Length > 0)
                {
                    var ext = Path.GetExtension(formFile.FileName);
                    var fileName = $"{Guid.NewGuid()}{ext}";
                    var filePath = Path.Combine(uploadPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await formFile.CopyToAsync(stream);
                    }

                    imageList.Add(new SalesProductImage
                    {
                        ImagePath = $"/uploads/sales/{fileName}"
                    });
                }
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
