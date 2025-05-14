using IndustryX.WebUI.Models;
using Microsoft.AspNetCore.Mvc;

namespace IndustryX.WebUI.Controllers
{
    public class BaseController : Controller
    {
        protected void ShowAlert(string title, string message, string alertType = "info")
        {
            var alert = new AlertMessage
            {
                Title = title,
                Message = message,
                AlertType = alertType
            };

            TempData["Alert"] = System.Text.Json.JsonSerializer.Serialize(alert);
        }
    }
}
