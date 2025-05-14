using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace IndustryX.WebUI.Controllers
{
    public class ErrorController : Controller
    {
        public IActionResult Index()
        {
            var feature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            var errorMessage = feature?.Error?.Message;
            var errorPath = feature?.Path;

            ViewData["ErrorMessage"] = errorMessage;
            ViewData["ErrorPath"] = errorPath;

            return View();
        }
    }
}
