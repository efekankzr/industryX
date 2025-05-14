using IndustryX.Domain.Entities;

namespace IndustryX.Application.Interfaces
{
    public interface ILaborCostService
    {
        Task<IEnumerable<LaborCost>> GetAllAsync();
        Task<LaborCost?> GetByIdAsync(int id);
        Task AddAsync(LaborCost laborCost);
        Task DeleteAsync(int id);
    }
}