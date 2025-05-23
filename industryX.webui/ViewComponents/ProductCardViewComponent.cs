using Microsoft.AspNetCore.Mvc;
using IndustryX.WebUI.ViewModels;

namespace IndustryX.WebUI.ViewComponents
{
    public class ProductCardViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(ProductCardViewModel model)
        {
            return View(model);
        }
    }
}
