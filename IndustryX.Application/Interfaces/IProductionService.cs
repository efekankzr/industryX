using IndustryX.Domain.Entities;

namespace IndustryX.Application.Interfaces
{
    public interface IProductionService
    {
        Task<IEnumerable<Production>> GetAllAsync();
        Task<Production?> GetByIdAsync(int id);

        Task<(bool Success, string? Error)> CreateAsync(Production production);

        Task<bool> StartProductionAsync(int id);
        Task<bool> PauseProductionAsync(int id);
        Task<bool> ResumeProductionAsync(int id);
        Task<bool> FinishProductionAsync(int id);
    }
}
