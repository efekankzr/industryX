using System.Security.Claims;
using IndustryX.Application.Interfaces;
using IndustryX.WebUI.ViewModels.ApiViewModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IndustryX.WebUI.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class TransferApiController : ControllerBase
    {
        private readonly IProductTransferService _transferService;

        public TransferApiController(IProductTransferService transferService)
        {
            _transferService = transferService;
        }

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);
        private bool UserIsInRole(string role) => User.IsInRole(role);

        [HttpPost("accept")]
        public async Task<IActionResult> AcceptTransfer([FromBody] TransferActionDto dto)
        {
            if (!UserIsInRole("Driver")) return Forbid();

            var userId = GetUserId();
            var (success, error) = await _transferService.AcceptTransferAsync(
                dto.Barcode.Trim(),
                dto.DeliveredBoxCount,
                userId);

            if (!success)
                return BadRequest(error ?? "Transfer kabul edilemedi.");

            return Ok("Transfer başarıyla kabul edildi.");
        }

        [HttpPost("complete")]
        public async Task<IActionResult> CompleteTransfer([FromBody] TransferActionDto dto)
        {
            if (!UserIsInRole("WarehouseManager")) return Forbid();

            var userId = GetUserId();
            var (success, error) = await _transferService.CompleteTransferAsync(
                dto.Barcode.Trim(),
                dto.DeliveredBoxCount,
                userId);

            if (!success)
                return BadRequest(error ?? "Transfer tamamlanamadı.");

            return Ok("Transfer başarıyla tamamlandı.");
        }
    }
}
