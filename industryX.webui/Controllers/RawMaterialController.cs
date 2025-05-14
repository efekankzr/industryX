using IndustryX.Application.Interfaces;
using IndustryX.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace IndustryX.WebUI.Controllers
{
    public class RawMaterialController : BaseController
    {
        private readonly IRawMaterialService _materialService;

        public RawMaterialController(IRawMaterialService materialService)
        {
            _materialService = materialService;
        }

        public async Task<IActionResult> Index()
        {
            var materials = await _materialService.GetAllAsync();
            return View(materials);
        }

        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(RawMaterial material)
        {
            if (!ModelState.IsValid) return View(material);

            var (success, error) = await _materialService.AddAsync(material);
            if (!success)
            {
                ShowAlert("Error", error!, "danger");
                return View(material);
            }

            ShowAlert("Raw Material Added", "Material saved successfully.", "success");
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var material = await _materialService.GetByIdAsync(id);
            if (material == null)
            {
                ShowAlert("Not Found", "Material not found.", "danger");
                return RedirectToAction(nameof(Index));
            }

            return View(material);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(RawMaterial material)
        {
            if (!ModelState.IsValid) return View(material);

            var (success, error) = await _materialService.UpdateAsync(material);
            if (!success)
            {
                ShowAlert("Error", error!, "danger");
                return View(material);
            }

            ShowAlert("Updated", "Raw material updated.", "success");
            return RedirectToAction(nameof(Index));
        }

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
            ShowAlert("Deleted", "Raw material deleted.", "warning");
            return RedirectToAction(nameof(Index));
        }
    }
}
