﻿using System.Linq.Expressions;

namespace IndustryX.Domain.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task AddAsync(T entity);
        void Update(T entity);
        Task UpdateAsync(T entity);
        void Delete(T entity);
        Task SaveAsync();
        IQueryable<T> GetQueryable();
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
        void DeleteRange(IEnumerable<T> entities);
    }
}
