using IndustryX.Application.Interfaces;
using IndustryX.Domain.Entities;
using IndustryX.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IndustryX.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IRepository<Category> _categoryRepository;

        public CategoryService(IRepository<Category> categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<List<Category>> GetAllAsync()
        {
            return await _categoryRepository.GetQueryable().ToListAsync();
        }

        public async Task<Category?> GetByIdAsync(int id)
        {
            return await _categoryRepository.GetByIdAsync(id);
        }

        public async Task<(bool Success, string? Error)> CreateAsync(Category category)
        {
            if (await _categoryRepository.AnyAsync(c => c.Name == category.Name))
                return (false, "A category with the same name already exists.");

            if (await _categoryRepository.AnyAsync(c => c.Url == category.Url))
                return (false, "A category with the same URL already exists.");

            await _categoryRepository.AddAsync(category);
            await _categoryRepository.SaveAsync();
            return (true, null);
        }

        public async Task<(bool Success, string? Error)> UpdateAsync(Category category)
        {
            if (await _categoryRepository.AnyAsync(c => c.Name == category.Name && c.Id != category.Id))
                return (false, "Another category with the same name already exists.");

            if (await _categoryRepository.AnyAsync(c => c.Url == category.Url && c.Id != category.Id))
                return (false, "Another category with the same URL already exists.");

            _categoryRepository.Update(category);
            await _categoryRepository.SaveAsync();
            return (true, null);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
                return false;

            _categoryRepository.Delete(category);
            await _categoryRepository.SaveAsync();
            return true;
        }
    }
}
