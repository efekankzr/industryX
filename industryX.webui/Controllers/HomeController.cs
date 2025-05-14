using IndustryX.WebUI.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace IndustryX.webui.Controllers
{
    public class HomeController : BaseController
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
