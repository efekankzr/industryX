using IndustryX.Application.Interfaces;
using IndustryX.Domain.Entities;
using IndustryX.Domain.Interfaces;

namespace IndustryX.Application.Services
{
    public class LaborCostService : ILaborCostService
    {
        private readonly IRepository<LaborCost> _repository;

        public LaborCostService(IRepository<LaborCost> repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<LaborCost>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<LaborCost?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task AddAsync(LaborCost laborCost)
        {
            await _repository.AddAsync(laborCost);
            await _repository.SaveAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity != null)
            {
                _repository.Delete(entity);
                await _repository.SaveAsync();
            }
        }
    }
}
