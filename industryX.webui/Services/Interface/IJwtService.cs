using IndustryX.Domain.Entities;

namespace IndustryX.WebUI.Services.Interface
{
    public interface IJwtService
    {
        Task<string> GenerateToken(ApplicationUser user);
    }
}
