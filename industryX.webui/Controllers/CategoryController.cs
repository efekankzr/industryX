using IndustryX.Application.Interfaces;
using IndustryX.Domain.Entities;
using IndustryX.WebUI.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IndustryX.WebUI.Controllers
{
    [Authorize(Roles = "Admin,SalesManager")]
    public class CategoryController : BaseController
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _categoryService.GetAllAsync();
            return View(categories);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CategoryViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var category = new Category
            {
                Name = model.Name,
                Url = model.Url
            };

            var result = await _categoryService.CreateAsync(category);
            if (!result.Success)
            {
                ModelState.AddModelError("", result.Error!);
                return View(model);
            }

            ShowAlert("Success", "Category created successfully.", "success");
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int id)
        {
            var category = await _categoryService.GetByIdAsync(id);
            if (category == null)
            {
                ShowAlert("Error", "Category not found.", "danger");
                return RedirectToAction("Index");
            }

            var model = new CategoryViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Url = category.Url
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(CategoryViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var category = new Category
            {
                Id = model.Id,
                Name = model.Name,
                Url = model.Url
            };

            var result = await _categoryService.UpdateAsync(category);
            if (!result.Success)
            {
                ModelState.AddModelError("", result.Error!);
                return View(model);
            }

            ShowAlert("Success", "Category updated successfully.", "success");
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _categoryService.DeleteAsync(id);

            if (deleted)
                ShowAlert("Success", "Category deleted.", "success");
            else
                ShowAlert("Error", "Category not found or could not be deleted.", "danger");

            return RedirectToAction("Index");
        }
    }
}
