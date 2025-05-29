using System.Security.Claims;
using IndustryX.Domain.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IndustryX.WebUI.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserApiController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserApiController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet("me")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetMe()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Console.WriteLine("JWT içinden alınan kullanıcı ID: " + userId);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Token geçersiz ya da çözümlemede hata oldu.");

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return NotFound("Kullanıcı bulunamadı.");

            foreach (var claim in User.Claims)
            {
                Console.WriteLine($"Claim: {claim.Type} = {claim.Value}");
            }

            return Ok(new
            {
                user.Id,
                user.Email,
                user.Firstname,
                user.Surname
            });
        }
    }
}
