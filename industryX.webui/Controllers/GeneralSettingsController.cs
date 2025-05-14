using IndustryX.Application.Interfaces;
using IndustryX.Domain.Entities;
using IndustryX.WebUI.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace IndustryX.WebUI.Controllers
{
    public class GeneralSettingsController : BaseController
    {
        private readonly ILaborCostService _laborCostService;

        public GeneralSettingsController(ILaborCostService laborCostService)
        {
            _laborCostService = laborCostService;
        }

        public async Task<IActionResult> ListLabor()
        {
            var viewModel = new LaborCostFormViewModel
            {
                ExistingCosts = (await _laborCostService.GetAllAsync()).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AddLaborCost(LaborCostFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.ExistingCosts = (await _laborCostService.GetAllAsync()).ToList();
                return View("ListLabor", model);
            }

            var entity = new LaborCost
            {
                HourlyWage = model.HourlyWage,
                EffectiveDate = model.EffectiveDate
            };

            await _laborCostService.AddAsync(entity);
            ShowAlert("Success", "Hourly wage added.", "success");
            return RedirectToAction(nameof(ListLabor));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteLaborCost(int id)
        {
            await _laborCostService.DeleteAsync(id);
            ShowAlert("Deleted", "Hourly wage removed.", "warning");
            return RedirectToAction(nameof(ListLabor));
        }
    }
}
