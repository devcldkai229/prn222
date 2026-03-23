using MealPrep.DAL.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealPrep.DAL.Repositories
{
    public class EfRepository<T> : IRepository<T> where T : class
    {
        private readonly AppDbContext _db;
        private readonly DbSet<T> _set;

        public EfRepository(AppDbContext db)
        {
            _db = db;
            _set = db.Set<T>();
        }

        public Task<List<T>> GetAllAsync() => _set.ToListAsync();
        public Task<T?> GetByIdAsync(int id) => _set.FindAsync(id).AsTask();

        public Task AddAsync(T entity) => _set.AddAsync(entity).AsTask();
        public void Update(T entity) => _set.Update(entity);
        public void Remove(T entity) => _set.Remove(entity);

        public Task<int> SaveChangesAsync() => _db.SaveChangesAsync();

        public IQueryable<T> Query() => _set.AsQueryable();
    }
}
