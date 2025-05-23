using IndustryX.Application.Interfaces;
using IndustryX.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IndustryX.WebUI.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RawMaterialController : BaseController
    {
        private readonly IRawMaterialService _materialService;

        public RawMaterialController(IRawMaterialService materialService)
        {
            _materialService = materialService;
        }

        // -------------------- List --------------------
        public async Task<IActionResult> Index()
        {
            var materials = await _materialService.GetAllAsync();
            return View(materials);
        }

        // -------------------- Create --------------------
        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(RawMaterial material)
        {
            if (!ModelState.IsValid)
                return View(material);

            var (success, error) = await _materialService.AddAsync(material);

            if (!success)
            {
                ShowAlert("Error", error!, "danger");
                return View(material);
            }

            ShowAlert("Success", "Raw material added successfully.", "success");
            return RedirectToAction(nameof(Index));
        }

        // -------------------- Edit --------------------
        public async Task<IActionResult> Edit(int id)
        {
            var material = await _materialService.GetByIdAsync(id);
            if (material == null)
            {
                ShowAlert("Not Found", "Raw material not found.", "danger");
                return RedirectToAction(nameof(Index));
            }

            return View(material);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(RawMaterial material)
        {
            if (!ModelState.IsValid)
                return View(material);

            var (success, error) = await _materialService.UpdateAsync(material);

            if (!success)
            {
                ShowAlert("Error", error!, "danger");
                return View(material);
            }

            ShowAlert("Success", "Raw material updated successfully.", "success");
            return RedirectToAction(nameof(Index));
        }

        // -------------------- Delete --------------------
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var material = await _materialService.GetByIdAsync(id);
            if (material == null)
            {
                ShowAlert("Error", "Raw material not found.", "danger");
                return RedirectToAction(nameof(Index));
            }

            await _materialService.DeleteAsync(id);
            ShowAlert("Deleted", "Raw material has been deleted.", "warning");
            return RedirectToAction(nameof(Index));
        }
    }
}
