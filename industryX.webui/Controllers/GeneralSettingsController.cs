using IndustryX.Application.Interfaces;
using IndustryX.Domain.Entities;
using IndustryX.WebUI.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IndustryX.WebUI.Controllers
{
    [Authorize(Roles = "Admin")]
    public class GeneralSettingsController : BaseController
    {
        private readonly ILaborCostService _laborCostService;

        public GeneralSettingsController(ILaborCostService laborCostService)
        {
            _laborCostService = laborCostService;
        }

        // ----------------------------
        // LIST ALL LABOR COST RECORDS
        // ----------------------------
        public async Task<IActionResult> ListLabor()
        {
            var viewModel = new LaborCostFormViewModel
            {
                ExistingCosts = (await _laborCostService.GetAllAsync()).ToList()
            };

            return View(viewModel);
        }

        // ----------------------------
        // ADD NEW LABOR COST
        // ----------------------------
        [HttpPost]
        public async Task<IActionResult> AddLaborCost(LaborCostFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.ExistingCosts = (await _laborCostService.GetAllAsync()).ToList();
                return View("ListLabor", model);
            }

            var newCost = new LaborCost
            {
                HourlyWage = model.HourlyWage,
                EffectiveDate = model.EffectiveDate
            };

            await _laborCostService.AddAsync(newCost);

            ShowAlert("Success", "Hourly wage added successfully.", "success");
            return RedirectToAction(nameof(ListLabor));
        }

        // ----------------------------
        // DELETE LABOR COST
        // ----------------------------
        [HttpPost]
        public async Task<IActionResult> DeleteLaborCost(int id)
        {
            await _laborCostService.DeleteAsync(id);
            ShowAlert("Deleted", "Hourly wage removed.", "warning");
            return RedirectToAction(nameof(ListLabor));
        }
    }
}
