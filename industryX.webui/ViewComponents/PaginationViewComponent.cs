using IndustryX.WebUI.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace IndustryX.WebUI.ViewComponents
{
    public class PaginationViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(PaginationViewModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model), "Pagination model cannot be null.");
            }

            if (model.PageSizeOptions == null || !model.PageSizeOptions.Any())
            {
                model.PageSizeOptions = new List<int> { 10, 20, 50, 100, 200 };
            }

            if (!model.PageSizeOptions.Contains(model.PageSize))
            {
                model.PageSize = 10;
            }

            return View(model);
        }
    }
}
