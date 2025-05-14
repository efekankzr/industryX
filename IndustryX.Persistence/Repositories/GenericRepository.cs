using IndustryX.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using IndustryX.Persistence.Contexts;
using System.Linq.Expressions;

namespace IndustryX.Persistence.Repositories
{
    public class GenericRepository<T> : IRepository<T> where T : class
    {
        private readonly IndustryXDbContext _context;
        private readonly DbSet<T> _dbSet;

        public GenericRepository(IndustryXDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task<T?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);

        public async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();

        public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);

        public void Update(T entity) => _dbSet.Update(entity);

        public void Delete(T entity) => _dbSet.Remove(entity);

        public async Task SaveAsync() => await _context.SaveChangesAsync();

        public IQueryable<T> GetQueryable() => _dbSet.AsQueryable();
        public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
        {
            return await _context.Set<T>().AnyAsync(predicate);
        }
    }
}
