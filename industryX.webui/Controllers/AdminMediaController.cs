using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IndustryX.WebUI.Controllers
{
    [Authorize(Roles = "Admin, SalesManager")]
    public class AdminMediaController : BaseController
    {
        private readonly IWebHostEnvironment _env;

        public AdminMediaController(IWebHostEnvironment env)
        {
            _env = env;
        }

        public IActionResult Upload()
        {
            ViewBag.BannerOptions = new List<string> { "banner1.jpg", "banner2.jpg", "banner3.jpg" };
            ViewBag.SectionOptions = new List<string>
            {
                "bestseller.jpg", "popular.jpg", "new.jpg", "sale.jpg", "featured.jpg", "limited.jpg"
            };
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadBanner(IFormFile file, string fileName)
        {
            if (file == null || file.Length == 0 || string.IsNullOrWhiteSpace(fileName))
            {
                ShowAlert("Error", "Invalid file or filename.", "danger");
                return RedirectToAction(nameof(Upload));
            }

            using var image = await SixLabors.ImageSharp.Image.LoadAsync(file.OpenReadStream());
            if (image.Width != 1200 || image.Height != 400)
            {
                ShowAlert("Error", "Banner image must be exactly 1200x400 pixels.", "danger");
                return RedirectToAction(nameof(Upload));
            }

            var bannerDir = Path.Combine(_env.WebRootPath, "images", "banners");
            if (!Directory.Exists(bannerDir))
                Directory.CreateDirectory(bannerDir);

            var path = Path.Combine(bannerDir, fileName);

            if (System.IO.File.Exists(path))
                System.IO.File.Delete(path);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            ShowAlert("Success", $"Banner '{fileName}' updated successfully.", "success");
            return RedirectToAction(nameof(Upload));
        }

        [HttpPost]
        public async Task<IActionResult> UploadSection(IFormFile file, string fileName)
        {
            if (file == null || file.Length == 0 || string.IsNullOrWhiteSpace(fileName))
            {
                ShowAlert("Error", "Invalid file or filename.", "danger");
                return RedirectToAction(nameof(Upload));
            }

            using var image = await SixLabors.ImageSharp.Image.LoadAsync(file.OpenReadStream());
            if (image.Width != 600 || image.Height != 400)
            {
                ShowAlert("Error", "Section image must be exactly 600x400 pixels.", "danger");
                return RedirectToAction(nameof(Upload));
            }

            var sectionDir = Path.Combine(_env.WebRootPath, "images", "sections");
            if (!Directory.Exists(sectionDir))
                Directory.CreateDirectory(sectionDir);

            var path = Path.Combine(sectionDir, fileName);

            if (System.IO.File.Exists(path))
                System.IO.File.Delete(path);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            ShowAlert("Success", $"Section image '{fileName}' updated successfully.", "success");
            return RedirectToAction(nameof(Upload));
        }
    }
}
